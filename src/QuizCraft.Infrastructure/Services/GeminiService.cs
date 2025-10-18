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

        public GeminiService(
            IOptions<GeminiSettings> options,
            ILogger<GeminiService> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _settings = options.Value ?? throw new InvalidOperationException("Gemini settings not configured");
            _httpClient = httpClient;

            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                throw new InvalidOperationException("Gemini API Key must be configured");
            }

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
                return new AIResponse
                {
                    Success = false,
                    Content = $"Error al comunicarse con Gemini: {ex.Message}",
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }
        }

        public async Task<AIResponse> GenerateTextAsync(string prompt, AISettings? customSettings = null)
        {
            try
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

                    return new AIResponse
                    {
                        Success = false,
                        Content = $"Error de la API de Gemini: {response.StatusCode}",
                        TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (!geminiResponse.TryGetProperty("candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    _logger.LogError("Gemini returned no candidates");
                    return new AIResponse
                    {
                        Success = false,
                        Content = "Error: No se recibió respuesta válida de Gemini",
                        TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                    };
                }

                var candidate = candidates[0];
                if (!candidate.TryGetProperty("content", out var contentElement) ||
                    !contentElement.TryGetProperty("parts", out var parts) ||
                    parts.GetArrayLength() == 0)
                {
                    _logger.LogError("Gemini response format invalid");
                    return new AIResponse
                    {
                        Success = false,
                        Content = "Error: Formato de respuesta de Gemini inválido",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making request to Gemini");
                return new AIResponse
                {
                    Success = false,
                    Content = $"Error al comunicarse con Gemini: {ex.Message}",
                    TokenUsage = new TokenUsageInfo { RequestTime = DateTime.UtcNow }
                };
            }
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

        private Core.Enums.NivelDificultad ParseDifficulty(string? difficulty)
        {
            return difficulty?.ToLower() switch
            {
                "fácil" or "easy" or "facil" => Core.Enums.NivelDificultad.Facil,
                "difícil" or "hard" or "dificil" => Core.Enums.NivelDificultad.Dificil,
                _ => Core.Enums.NivelDificultad.Intermedio
            };
        }

        private int EstimateTokens(string text)
        {
            // Estimación aproximada: 1 token ≈ 4 caracteres para español
            return (int)Math.Ceiling(text.Length / 4.0);
        }
    }
}