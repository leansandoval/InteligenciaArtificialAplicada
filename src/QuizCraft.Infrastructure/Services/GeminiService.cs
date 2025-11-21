using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace QuizCraft.Infrastructure.Services
{
    /// <summary>
    /// Servicio de Google Gemini para generación de contenido con IA
    /// </summary>
    public class GeminiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly GeminiSettings _settings;
        private readonly GeminiRateLimiter _rateLimiter;

        public GeminiService(
            IOptions<GeminiSettings> options,
            ILogger<GeminiService> logger,
            ILoggerFactory loggerFactory,
            HttpClient httpClient)
        {
            _logger = logger;
            _settings = options.Value ?? throw new InvalidOperationException("Gemini settings not configured");
            _httpClient = httpClient;

            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                throw new InvalidOperationException("Gemini API Key must be configured");
            }

            // Inicializar rate limiter con logger del factory
            var rateLimiterLogger = loggerFactory.CreateLogger<GeminiRateLimiter>();
            
            _rateLimiter = new GeminiRateLimiter(
                rateLimiterLogger,
                _settings.RequestsPerMinute,
                _settings.RequestsPerDay,
                _settings.TokensPerMinute
            );

            _logger.LogInformation("Gemini Service initialized with model: {Model}",
                _settings.Model);
        }

        public async Task<AIResponse> GenerateFlashcardsFromTextAsync(string content, QuizCraft.Application.Interfaces.AIGenerationSettings settings)
        {
            try
            {
                _logger.LogInformation("Generating flashcards with Gemini for content length: {Length}", content.Length);

                var prompt = BuildFlashcardPrompt(content, settings);
                var response = await GenerateTextAsync(prompt);

                if (!response.Success)
                {
                    _logger.LogWarning("GenerateTextAsync failed for flashcards. ErrorMessage: '{ErrorMessage}', Content: '{Content}'", 
                        response.ErrorMessage, response.Content);
                    
                    // Asegurar que ErrorMessage esté configurado
                    if (string.IsNullOrEmpty(response.ErrorMessage) && !string.IsNullOrEmpty(response.Content))
                    {
                        response.ErrorMessage = response.Content;
                    }
                    
                    return response;
                }

                // Parsear la respuesta y convertirla al formato esperado
                var flashcardResponse = ParseFlashcardResponse(response.Content, settings);

                var serializedContent = flashcardResponse.Success
                    ? JsonSerializer.Serialize(new { flashcards = flashcardResponse.Flashcards }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    : flashcardResponse.ErrorMessage;

                _logger.LogDebug("Serialized content being sent to AIDocumentProcessor: {Content}", serializedContent);

                return new AIResponse
                {
                    Success = flashcardResponse.Success,
                    Content = serializedContent,
                    TokenUsage = response.TokenUsage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flashcards with Gemini");
                var errorMsg = $"Error al comunicarse con Gemini: {ex.Message}";
                return new AIResponse
                {
                    Success = false,
                    Content = errorMsg,
                    ErrorMessage = errorMsg,
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }
        }

        public async Task<AIResponse> GenerateQuizFromTextAsync(string content, QuizGenerationSettings settings)
        {
            try
            {
                _logger.LogInformation("Generating quiz with Gemini for content length: {Length}", content.Length);

                var prompt = BuildQuizPrompt(content, settings);
                
                // Estimar tokens antes de enviar para controlar costos
                // El prompt ya incluye el content, no duplicar
                var estimatedTokens = EstimateTokens(prompt);
                _logger.LogInformation("Estimated tokens for quiz generation: {Tokens}", estimatedTokens);
                
                if (estimatedTokens > 12000) // Límite aumentado para documentos extensos
                {
                    _logger.LogWarning("Content too large. Estimated tokens: {Tokens}", estimatedTokens);
                    var errorMsg = "El contenido es demasiado largo. El documento debe tener menos de 10,000 caracteres.";
                    return new AIResponse
                    {
                        Success = false,
                        Content = errorMsg,
                        ErrorMessage = errorMsg,
                        TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                    };
                }
                
                // Configuración personalizada para quiz con más tokens
                var quizSettings = new AISettings
                {
                    MaxTokens = 8000, // Aumentado significativamente para respuestas completas de quiz
                    Temperature = 0.7f // Mantener creatividad controlada
                };
                
                var response = await GenerateTextAsync(prompt, quizSettings);

                if (!response.Success)
                {
                    _logger.LogWarning("GenerateTextAsync failed. ErrorMessage: '{ErrorMessage}', Content: '{Content}'", 
                        response.ErrorMessage, response.Content);
                    
                    // Asegurar que ErrorMessage esté configurado
                    if (string.IsNullOrEmpty(response.ErrorMessage) && !string.IsNullOrEmpty(response.Content))
                    {
                        response.ErrorMessage = response.Content;
                    }
                    
                    return response;
                }

                // Parsear la respuesta y convertirla al formato esperado
                var quizResponse = ParseQuizResponse(response.Content, settings);

                var serializedContent = quizResponse.Success
                    ? JsonSerializer.Serialize(new { questions = quizResponse.Questions }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    : quizResponse.ErrorMessage;

                _logger.LogDebug("Serialized quiz content: {Content}", serializedContent);

                return new AIResponse
                {
                    Success = quizResponse.Success,
                    Content = serializedContent,
                    TokenUsage = response.TokenUsage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz with Gemini");
                var errorMsg = $"Error al comunicarse con Gemini: {ex.Message}";
                return new AIResponse
                {
                    Success = false,
                    Content = errorMsg,
                    ErrorMessage = errorMsg,
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }
        }

        public async Task<AIResponse> GenerateTextAsync(string prompt, AISettings? customSettings = null)
        {
            var estimatedTokens = EstimateTokens(prompt) + (customSettings?.MaxTokens ?? _settings.MaxTokens);
            
            // Aplicar rate limiting antes de hacer la petición
            try
            {
                var waitTime = await _rateLimiter.WaitIfNeededAsync(estimatedTokens);
                if (waitTime > 0)
                {
                    _logger.LogInformation("Waited {WaitTime}ms due to rate limiting", waitTime);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Límite diario alcanzado
                _logger.LogError(ex, "Rate limit exceeded");
                return new AIResponse
                {
                    Success = false,
                    Content = ex.Message,
                    ErrorMessage = ex.Message,
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }

            // Intentar con reintentos en caso de error 429
            for (int attempt = 0; attempt <= _settings.MaxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        _logger.LogInformation("Retry attempt {Attempt}/{Max} for Gemini API", 
                            attempt, _settings.MaxRetries);
                    }

                    var response = await ExecuteGeminiRequestAsync(prompt, customSettings);
                    
                    // Si fue exitoso, retornar
                    if (response.Success || !_settings.EnableRetryOnRateLimit)
                    {
                        return response;
                    }

                    // Si es error 429 y quedan reintentos, esperar y reintentar
                    if (response.ErrorMessage?.Contains("429") == true && attempt < _settings.MaxRetries)
                    {
                        var delaySeconds = _settings.RetryDelaySeconds * Math.Pow(2, attempt); // Backoff exponencial
                        _logger.LogWarning(
                            "Rate limit error (429). Waiting {Delay}s before retry {Attempt}/{Max}",
                            delaySeconds, attempt + 1, _settings.MaxRetries);
                        
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        continue;
                    }

                    // Si es otro error, retornar inmediatamente
                    return response;
                }
                catch (Exception ex)
                {
                    if (attempt >= _settings.MaxRetries)
                    {
                        _logger.LogError(ex, "Error making request to Gemini after {Attempts} attempts", 
                            attempt + 1);
                        
                        return new AIResponse
                        {
                            Success = false,
                            Content = $"Error al comunicarse con Gemini después de {attempt + 1} intentos: {ex.Message}",
                            ErrorMessage = ex.Message,
                            TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                        };
                    }

                    // Esperar antes de reintentar
                    var delaySeconds = _settings.RetryDelaySeconds * Math.Pow(2, attempt);
                    _logger.LogWarning(ex, "Request failed, retrying in {Delay}s", delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            // No debería llegar aquí, pero por si acaso
            return new AIResponse
            {
                Success = false,
                Content = "Error inesperado al comunicarse con Gemini",
                ErrorMessage = "Max retries exceeded",
                TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
            };
        }

        /// <summary>
        /// Ejecuta la petición HTTP a Gemini sin lógica de reintentos
        /// </summary>
        private async Task<AIResponse> ExecuteGeminiRequestAsync(string prompt, AISettings? customSettings = null)
        {
            _logger.LogInformation("Sending request to Gemini with model {Model}", _settings.Model);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = customSettings?.Temperature ?? _settings.Temperature,
                    topP = _settings.TopP,
                    topK = _settings.TopK,
                    maxOutputTokens = customSettings?.MaxTokens ?? _settings.MaxTokens
                }
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.BaseUrl}/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);

                // Parsear el error para obtener información detallada
                string errorMessage = ParseGeminiError(response.StatusCode, errorContent);

                return new AIResponse
                {
                    Success = false,
                    Content = errorMessage,
                    ErrorMessage = $"{(int)response.StatusCode}:{errorMessage}",
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (!geminiResponse.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                _logger.LogError("Gemini returned no candidates");
                var errorMsg = "Error: No se recibió respuesta válida de Gemini";
                return new AIResponse
                {
                    Success = false,
                    Content = errorMsg,
                    ErrorMessage = errorMsg,
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var contentElement) ||
                !contentElement.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() == 0)
            {
                _logger.LogError("Gemini response format invalid");
                var errorMsg = "Error: Formato de respuesta de Gemini inválido";
                return new AIResponse
                {
                    Success = false,
                    Content = errorMsg,
                    ErrorMessage = errorMsg,
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }

            var text = parts[0].GetProperty("text").GetString() ?? "";

            _logger.LogInformation("Gemini response received successfully");

            return new AIResponse
            {
                Success = true,
                Content = text,
                TokenUsage = new TokenUsageInfo
                {
                    CompletionTokens = EstimateTokens(text),
                    PromptTokens = EstimateTokens(prompt),
                    TotalTokens = EstimateTokens(prompt + text),
                    EstimatedCost = 0.00m, // Gemini es gratuito
                    RequestTime = DateTime.UtcNow
                }
            };
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            try
            {
                _logger.LogInformation("Validating Gemini configuration");

                var response = await GenerateTextAsync("Test");

                _logger.LogInformation("Gemini configuration validated successfully");
                return response.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini configuration validation failed. " +
                    "Model: {Model}. " +
                    "Error: {Error}", _settings.Model, ex.Message);
                return false;
            }
        }

        public async Task<TokenUsageInfo> GetTokenUsageInfoAsync()
        {
            await Task.Delay(1); // Para mantener la interfaz async

            return new TokenUsageInfo
            {
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                EstimatedCost = 0.00m, // Gemini es gratuito
                RequestTime = DateTime.UtcNow
            };
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                await Task.Delay(1);
                return !string.IsNullOrEmpty(_settings.ApiKey) && _settings.IsEnabled;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> EstimateTokenCostAsync(string content)
        {
            await Task.Delay(1);
            return EstimateTokens(content);
        }

        public async Task<TokenUsageInfo> EstimateTokenUsageAsync(string content)
        {
            await Task.Delay(1);

            var tokens = EstimateTokens(content);
            return new TokenUsageInfo
            {
                PromptTokens = tokens,
                CompletionTokens = 0,
                TotalTokens = tokens,
                EstimatedCost = 0.00m, // Gemini es gratuito
                RequestTime = DateTime.UtcNow
            };
        }

        private string BuildFlashcardPrompt(string content, QuizCraft.Application.Interfaces.AIGenerationSettings settings)
        {
            var prompt = $@"
            {(_settings as GeminiFlashcardGenerationSettings)?.SystemPrompt ?? "Eres un experto en educación que crea flashcards de alta calidad."}

            CONTENIDO A PROCESAR:
            {content}

            INSTRUCCIONES:
            - Genera máximo {settings.MaxCardsPerDocument} flashcards basadas en el contenido
            - Nivel de dificultad: {settings.Difficulty}
            - Idioma: {settings.Language}
            - {(settings.IncludeExplanations ? "Incluye explicaciones breves" : "Solo pregunta y respuesta")}
            - Área de enfoque: {settings.FocusArea ?? "General"}

            FORMATO DE RESPUESTA (JSON):
            {{
            ""flashcards"": [
                {{
                ""pregunta"": ""Pregunta clara y específica"",
                ""respuesta"": ""Respuesta precisa y completa"",
                ""dificultad"": ""{settings.Difficulty}"",
                ""explicacion"": ""Explicación adicional (opcional)"",
                ""etiquetas"": [""tag1"", ""tag2""],
                ""categoria"": ""Categoría del contenido""
                }}
            ]
            }}

            Responde ÚNICAMENTE con el JSON válido, sin texto adicional.";

            return prompt;
        }

        private string BuildQuizPrompt(string content, QuizGenerationSettings settings)
        {
            var questionTypes = string.Join(", ", settings.QuestionTypes);
            var difficultyText = settings.DifficultyLevel switch
            {
                Core.Enums.NivelDificultad.Facil => "Fácil",
                Core.Enums.NivelDificultad.Dificil => "Difícil",
                _ => "Intermedio"
            };

            var explanations = settings.IncludeExplanations ? "con explicaciones detalladas" : "sin explicaciones";
            var subject = !string.IsNullOrEmpty(settings.Subject) ? $"Materia: {settings.Subject}. " : "";
            var instructions = !string.IsNullOrEmpty(settings.CustomInstructions) ? $"Instrucciones adicionales: {settings.CustomInstructions}. " : "";

            var prompt = $"Crea {settings.NumberOfQuestions} preguntas de quiz {explanations} basadas en el siguiente contenido:\n\n" +
                        content + "\n\n" +
                        $"Configuración del Quiz:\n" +
                        $"- Nivel de dificultad: {difficultyText}\n" +
                        $"- Tipos de pregunta: {questionTypes}\n" +
                        $"- Idioma: {settings.Language}\n" +
                        subject + instructions + "\n" +
                        "Formato JSON requerido:\n" +
                        "{\n" +
                        "  \"questions\": [\n" +
                        "    {\n" +
                        "      \"questionText\": \"Pregunta clara y específica sobre un concepto clave\",\n" +
                        "      \"questionType\": \"MultipleChoice\",\n" +
                        "      \"difficultyLevel\": \"Intermedio\",\n" +
                        "      \"answerOptions\": [\n" +
                        "        {\"text\": \"Respuesta correcta basada directamente en el contenido\", \"isCorrect\": true, \"explanation\": \"Explicación de por qué es correcta\"},\n" +
                        "        {\"text\": \"Distractor plausible: concepto relacionado pero incorrecto\", \"isCorrect\": false, \"explanation\": \"Explicación de por qué es incorrecta\"},\n" +
                        "        {\"text\": \"Otro distractor convincente: definición parcialmente correcta\", \"isCorrect\": false, \"explanation\": \"Explicación de por qué es incorrecta\"},\n" +
                        "        {\"text\": \"Distractor final: término similar del mismo tema\", \"isCorrect\": false, \"explanation\": \"Explicación de por qué es incorrecta\"}\n" +
                        "      ],\n" +
                        "      \"explanation\": \"Explicación completa de la respuesta correcta con contexto del contenido\",\n" +
                        "      \"points\": 1,\n" +
                        "      \"tags\": [\"concepto-clave\"],\n" +
                        "      \"sourceReference\": \"Referencia específica al contenido fuente\",\n" +
                        "      \"confidenceScore\": 0.9\n" +
                        "    }\n" +
                        "  ]\n" +
                        "}\n\n" +
                        "REGLAS CRÍTICAS PARA DISTRACTORES (Opciones Incorrectas):\n" +
                        "1. NUNCA uses frases genéricas como 'Opción incorrecta A/B/C' o 'Respuesta falsa'\n" +
                        "2. Los distractores DEBEN ser:\n" +
                        "   - Plausibles: que parezcan correctos a primera vista\n" +
                        "   - Relacionados: conceptos del mismo tema pero incorrectos en este contexto\n" +
                        "   - Específicos: usa términos y definiciones reales del contenido\n" +
                        "   - Educativos: que ayuden a reforzar el aprendizaje al identificar por qué son incorrectos\n" +
                        "3. Ejemplos de BUENOS distractores:\n" +
                        "   - Fechas cercanas pero incorrectas (1492 vs 1498)\n" +
                        "   - Conceptos relacionados del mismo campo (mitosis vs meiosis)\n" +
                        "   - Definiciones parcialmente correctas pero incompletas\n" +
                        "   - Términos similares del mismo dominio (Java vs JavaScript)\n" +
                        "4. Usa variaciones apropiadas como:\n" +
                        "   - 'Ninguna de las anteriores' (cuando otras opciones cubren bien el tema)\n" +
                        "   - 'Todas las anteriores' (cuando tiene sentido lógico)\n" +
                        "   - 'Depende del contexto' (en casos con matices)\n" +
                        "5. Los distractores deben ser similares en:\n" +
                        "   - Longitud (ni muy cortos ni muy largos comparados con la correcta)\n" +
                        "   - Complejidad del lenguaje\n" +
                        "   - Nivel de detalle\n\n" +
                        "REGLAS GENERALES:\n" +
                        "- Para MultipleChoice: SIEMPRE incluye exactamente 4 opciones (1 correcta, 3 incorrectas)\n" +
                        "- Para TrueFalse: SIEMPRE incluye exactamente 2 opciones (Verdadero/Falso)\n" +
                        "- SOLO UNA opción debe tener isCorrect: true\n" +
                        "- Cada opción debe tener una explicación clara\n" +
                        "- NO uses numeración (A), B), etc.) en el texto de las opciones\n" +
                        "- Las preguntas deben enfocarse en conceptos importantes, no trivialidades\n" +
                        "- Devuelve SOLO JSON válido, sin texto adicional ni formato markdown\n" +
                        "- Asegúrate de que el JSON sea parseable y siga exactamente la estructura especificada";

            return prompt;
        }

        /// <summary>
        /// Limpia las marcas de código markdown de la respuesta de Gemini
        /// </summary>
        /// <param name="response">Respuesta cruda de Gemini que puede contener ```json ... ```</param>
        /// <returns>JSON limpio sin marcas de markdown</returns>
        private string CleanMarkdownCodeBlocks(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return response;

            _logger.LogDebug("Original response: {Response}", response);

            // Eliminar bloques de código markdown (```json y ```)
            var cleaned = response.Trim();

            // Si empieza con ```json, ```JSON, o simplemente ```, quitarlo
            if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(7).TrimStart();
                _logger.LogDebug("Removed ```json prefix");
            }
            else if (cleaned.StartsWith("```JSON", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(7).TrimStart();
                _logger.LogDebug("Removed ```JSON prefix");
            }
            else if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned.Substring(3).TrimStart();
                _logger.LogDebug("Removed ``` prefix");
            }

            // Si termina con ```, quitarlo
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3).TrimEnd();
                _logger.LogDebug("Removed ``` suffix");
            }

            _logger.LogDebug("Cleaned response: {CleanedResponse}", cleaned);
            return cleaned.Trim();
        }

        private string TryFixIncompleteJson(string json)
        {
            try
            {
                // Si el JSON está incompleto, intentar cerrarlo
                var trimmed = json.Trim();
                
                // Contar llaves y corchetes
                int openBraces = trimmed.Count(c => c == '{');
                int closeBraces = trimmed.Count(c => c == '}');
                int openBrackets = trimmed.Count(c => c == '[');
                int closeBrackets = trimmed.Count(c => c == ']');
                
                _logger.LogInformation("JSON balance check - Braces: {OpenBraces}/{CloseBraces}, Brackets: {OpenBrackets}/{CloseBrackets}", 
                    openBraces, closeBraces, openBrackets, closeBrackets);
                
                // Si está incompleto, intentar cerrarlo
                if (openBraces > closeBraces || openBrackets > closeBrackets)
                {
                    _logger.LogWarning("JSON appears incomplete, attempting to fix");
                    
                    // Cerrar strings abiertas primero
                    int quoteCount = trimmed.Count(c => c == '"');
                    if (quoteCount % 2 != 0)
                    {
                        trimmed += "\"";
                        _logger.LogInformation("Added closing quote");
                    }
                    
                    // Cerrar arrays abiertos
                    while (openBrackets > closeBrackets)
                    {
                        trimmed += "\n]";
                        closeBrackets++;
                        _logger.LogInformation("Added closing bracket");
                    }
                    
                    // Cerrar objetos abiertos
                    while (openBraces > closeBraces)
                    {
                        trimmed += "\n}";
                        closeBraces++;
                        _logger.LogInformation("Added closing brace");
                    }
                }
                
                return trimmed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error attempting to fix incomplete JSON, returning original");
                return json;
            }
        }

        private QuizCraft.Application.Models.FlashcardGenerationResult ParseFlashcardResponse(string jsonResponse, QuizCraft.Application.Interfaces.AIGenerationSettings settings)
        {
            try
            {
                // Limpiar las marcas de código markdown de la respuesta de Gemini
                var cleanedResponse = CleanMarkdownCodeBlocks(jsonResponse);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var responseObj = JsonSerializer.Deserialize<JsonElement>(cleanedResponse, options);

                if (!responseObj.TryGetProperty("flashcards", out var flashcardsArray))
                {
                    _logger.LogError("Response does not contain flashcards array");
                    return new QuizCraft.Application.Models.FlashcardGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "Formato de respuesta inválido: no se encontró array de flashcards"
                    };
                }

                var flashcards = new List<FlashcardData>();

                foreach (var item in flashcardsArray.EnumerateArray())
                {
                    try
                    {
                        var flashcard = new FlashcardData
                        {
                            Pregunta = item.GetProperty("pregunta").GetString() ?? "",
                            Respuesta = item.GetProperty("respuesta").GetString() ?? "",
                            Dificultad = item.TryGetProperty("dificultad", out var diff) ? diff.GetString() ?? "Medium" : "Medium",
                            Categoria = item.TryGetProperty("categoria", out var cat) ? cat.GetString() ?? "General" : "General",
                            ConfianzaPuntuacion = 0.95, // Gemini tiene alta confianza
                            FuenteOriginal = "Gemini AI Generation"
                        };

                        if (item.TryGetProperty("etiquetas", out var tags))
                        {
                            foreach (var tag in tags.EnumerateArray())
                            {
                                var tagValue = tag.GetString();
                                if (!string.IsNullOrEmpty(tagValue))
                                {
                                    flashcard.Etiquetas.Add(tagValue);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(flashcard.Pregunta) && !string.IsNullOrEmpty(flashcard.Respuesta))
                        {
                            flashcards.Add(flashcard);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing individual flashcard");
                    }
                }

                return new QuizCraft.Application.Models.FlashcardGenerationResult
                {
                    Success = true,
                    Flashcards = flashcards,
                    ProcessingMethod = "Gemini AI",
                    TokensUsed = EstimateTokens(jsonResponse),
                    EstimatedCost = 0.00m,
                    ProcessingTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini response: {Response}", jsonResponse);
                return new QuizCraft.Application.Models.FlashcardGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error al procesar respuesta de Gemini: {ex.Message}"
                };
            }
        }

        private QuizGenerationResult ParseQuizResponse(string jsonResponse, QuizGenerationSettings settings)
        {
            try
            {
                _logger.LogInformation("Parsing Gemini quiz response");

                var cleanedResponse = CleanMarkdownCodeBlocks(jsonResponse);
                
                // Intentar reparar JSON incompleto
                cleanedResponse = TryFixIncompleteJson(cleanedResponse);
                
                var document = JsonDocument.Parse(cleanedResponse);

                if (!document.RootElement.TryGetProperty("questions", out var questionsArray))
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "Formato de respuesta inválido: no se encontró array de preguntas"
                    };
                }

                var questions = new List<GeneratedQuizQuestion>();

                foreach (var item in questionsArray.EnumerateArray())
                {
                    try
                    {
                        var question = new GeneratedQuizQuestion
                        {
                            QuestionText = item.GetProperty("questionText").GetString() ?? "",
                            QuestionType = ParseQuestionType(item.TryGetProperty("questionType", out var type) ? type.GetString() : "MultipleChoice"),
                            DifficultyLevel = ParseDifficulty(item.TryGetProperty("difficultyLevel", out var diff) ? diff.GetString() : "Intermedio"),
                            CorrectAnswer = item.TryGetProperty("correctAnswer", out var correct) ? correct.GetString() ?? "" : "",
                            Explanation = item.TryGetProperty("explanation", out var expl) ? expl.GetString() ?? "" : "",
                            Points = item.TryGetProperty("points", out var pts) ? pts.GetInt32() : 1,
                            SourceReference = item.TryGetProperty("sourceReference", out var srcRef) ? srcRef.GetString() ?? "" : "",
                            ConfidenceScore = item.TryGetProperty("confidenceScore", out var conf) ? conf.GetDouble() : 0.9,
                            IsApproved = false // Requiere revisión manual
                        };

                        // Parsear opciones de respuesta
                        if (item.TryGetProperty("answerOptions", out var optionsArray))
                        {
                            var order = 1;
                            foreach (var option in optionsArray.EnumerateArray())
                            {
                                var answerOption = new QuizAnswerOption
                                {
                                    Text = option.GetProperty("text").GetString() ?? "",
                                    IsCorrect = option.TryGetProperty("isCorrect", out var isCorrect) && isCorrect.GetBoolean(),
                                    Explanation = option.TryGetProperty("explanation", out var optExpl) ? optExpl.GetString() ?? "" : "",
                                    Order = order++
                                };

                                if (!string.IsNullOrEmpty(answerOption.Text))
                                {
                                    question.AnswerOptions.Add(answerOption);
                                }
                            }
                        }

                        // Parsear etiquetas
                        if (item.TryGetProperty("tags", out var tagsArray))
                        {
                            foreach (var tag in tagsArray.EnumerateArray())
                            {
                                var tagValue = tag.GetString();
                                if (!string.IsNullOrEmpty(tagValue))
                                {
                                    question.Tags.Add(tagValue);
                                }
                            }
                        }

                        // Validar que la pregunta tiene contenido válido
                        if (!string.IsNullOrEmpty(question.QuestionText) && 
                            (question.AnswerOptions.Any() || !string.IsNullOrEmpty(question.CorrectAnswer)))
                        {
                            questions.Add(question);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing individual quiz question");
                    }
                }

                return new QuizGenerationResult
                {
                    Success = true,
                    Questions = questions,
                    ProcessingMethod = "Gemini AI",
                    TokensUsed = EstimateTokens(jsonResponse),
                    EstimatedCost = 0.00m,
                    ProcessingTime = DateTime.UtcNow,
                    SourceContent = settings.Subject ?? "Contenido procesado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini quiz response: {Response}", jsonResponse);
                return new QuizGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error al procesar respuesta de quiz de Gemini: {ex.Message}"
                };
            }
        }

        private QuestionType ParseQuestionType(string? questionType)
        {
            return questionType?.ToLower() switch
            {
                "multiplechoice" => QuestionType.MultipleChoice,
                "truefalse" => QuestionType.TrueFalse,
                "fillintheblank" => QuestionType.FillInTheBlank,
                "shortanswer" => QuestionType.ShortAnswer,
                "matching" => QuestionType.Matching,
                _ => QuestionType.MultipleChoice
            };
        }

        private Core.Enums.NivelDificultad ParseDifficulty(string? difficulty)
        {
            return difficulty?.ToLower() switch
            {
                "fácil" or "easy" or "facil" => Core.Enums.NivelDificultad.Facil,
                "difícil" or "hard" or "dificil" => Core.Enums.NivelDificultad.Dificil,
                _ => Core.Enums.NivelDificultad.Intermedio
            };
        }

        /// <summary>
        /// Parsea los errores de Gemini para proporcionar mensajes útiles al usuario
        /// </summary>
        private string ParseGeminiError(System.Net.HttpStatusCode statusCode, string errorContent)
        {
            try
            {
                var error = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (error.TryGetProperty("error", out var errorObj))
                {
                    var message = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "";
                    var code = errorObj.TryGetProperty("code", out var codeElement) ? codeElement.GetInt32() : (int)statusCode;

                    // Error 429: Cuota excedida
                    if (code == 429 || statusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "❌ Has excedido el límite de uso gratuito de la API de Gemini.\n\n" +
                               "📊 Para verificar tu uso actual: https://ai.dev/usage\n" +
                               "📖 Más información sobre límites: https://ai.google.dev/gemini-api/docs/rate-limits\n\n" +
                               "⏰ Espera unos minutos e intenta nuevamente, o considera actualizar a un plan de pago.";
                    }

                    // Error 401: API Key inválida
                    if (code == 401 || statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return "❌ Tu API Key de Gemini no es válida o ha expirado.\n\n" +
                               "🔑 Verifica tu API Key en: https://aistudio.google.com/app/apikey\n" +
                               "⚙️ Actualiza la configuración en appsettings.json";
                    }

                    // Error 403: Permisos insuficientes
                    if (code == 403 || statusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return "❌ No tienes permisos para usar este modelo de Gemini.\n\n" +
                               "📖 Verifica que tu API Key tenga acceso al modelo gemini-2.0-flash-exp";
                    }

                    // Otros errores: mostrar mensaje de la API
                    if (!string.IsNullOrEmpty(message))
                    {
                        return $"Error de Gemini ({code}): {message}";
                    }
                }
            }
            catch (JsonException)
            {
                // Si no se puede parsear, usar mensaje genérico
            }

            return $"Error de la API de Gemini ({(int)statusCode}). Por favor, verifica tu configuración e intenta nuevamente.";
        }

        private int EstimateTokens(string text)
        {
            // Estimación aproximada: 1 token ≈ 4 caracteres para español
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        /// <summary>
        /// Obtiene estadísticas actuales de uso del rate limiter
        /// </summary>
        public RateLimitStats GetRateLimitStats()
        {
            return _rateLimiter.GetStats();
        }
    }
}