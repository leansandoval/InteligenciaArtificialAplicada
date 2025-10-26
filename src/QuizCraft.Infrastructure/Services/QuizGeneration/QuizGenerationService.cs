using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace QuizCraft.Infrastructure.Services.QuizGeneration
{
    /// <summary>
    /// Servicio principal para generación automática de quizzes
    /// </summary>
    public class QuizGenerationService : IQuizGenerationService
    {
        private readonly IAIService _aiService;
        private readonly IAIDocumentProcessor _documentProcessor;
        private readonly ILogger<QuizGenerationService> _logger;

        public QuizGenerationService(
            IAIService aiService,
            IAIDocumentProcessor documentProcessor,
            ILogger<QuizGenerationService> logger)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuizGenerationResult> GenerateFromDocumentAsync(
            Stream documentStream, 
            string fileName, 
            QuizGenerationSettings settings)
        {
            try
            {
                _logger.LogInformation("Starting quiz generation from document: {FileName}", fileName);

                // Validar que el archivo es soportado
                if (!IsFileSupported(fileName))
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Tipo de archivo no soportado: {Path.GetExtension(fileName)}"
                    };
                }

                // Verificar disponibilidad del servicio de IA
                if (!await _aiService.IsServiceAvailableAsync())
                {
                    _logger.LogWarning("AI service not available");
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "El servicio de IA no está disponible en este momento"
                    };
                }

                // TODO: Implementar extracción de texto de documentos
                _logger.LogWarning("Document text extraction not implemented yet for file: {FileName}", fileName);
                return new QuizGenerationResult
                {
                    Success = false,
                    ErrorMessage = "La extracción de texto de documentos no está implementada aún."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during quiz generation from document");
                return new QuizGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error inesperado durante la generación: {ex.Message}"
                };
            }
        }

        public async Task<QuizGenerationResult> GenerateFromTextAsync(string content, QuizGenerationSettings settings)
        {
            try
            {
                _logger.LogInformation("Starting quiz generation from text content (length: {Length})", content.Length);

                if (string.IsNullOrWhiteSpace(content))
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "El contenido de texto no puede estar vacío"
                    };
                }

                if (content.Length < 50)
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "El contenido de texto es demasiado corto para generar un quiz"
                    };
                }

                // Verificar disponibilidad del servicio de IA
                if (!await _aiService.IsServiceAvailableAsync())
                {
                    _logger.LogWarning("AI service not available");
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "El servicio de IA no está disponible en este momento"
                    };
                }

                // Estimar tokens
                var estimatedTokens = await _aiService.EstimateTokenCostAsync(content);
                _logger.LogInformation("Estimated tokens for quiz generation: {Tokens}", estimatedTokens);

                // Generar quiz usando IA
                var response = await _aiService.GenerateQuizFromTextAsync(content, settings);

                if (!response.Success)
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Error generando quiz: {response.ErrorMessage}",
                        ProcessingMethod = "Gemini AI",
                        TokensUsed = response.TokenUsage?.TotalTokens ?? 0,
                        EstimatedCost = response.TokenUsage?.EstimatedCost ?? 0
                    };
                }

                // Parsear respuesta JSON
                var quizResult = ParseAIResponse(response.Content);
                if (!quizResult.Success)
                {
                    return quizResult;
                }

                // Completar información del resultado
                quizResult.ProcessingMethod = "Gemini AI";
                quizResult.TokensUsed = response.TokenUsage?.TotalTokens ?? estimatedTokens;
                quizResult.EstimatedCost = response.TokenUsage?.EstimatedCost ?? 0;
                if (response.TokenUsage != null)
                    quizResult.TokenUsage = response.TokenUsage;
                quizResult.SourceContent = "Texto directo";

                _logger.LogInformation("Quiz generation from text completed successfully. Generated {Count} questions", 
                    quizResult.Questions.Count);

                return quizResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during quiz generation from text");
                return new QuizGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error inesperado durante la generación: {ex.Message}"
                };
            }
        }

        public bool IsFileSupported(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var supportedExtensions = GetSupportedFileTypes();
            
            return supportedExtensions.Contains(extension);
        }

        public List<string> GetSupportedFileTypes()
        {
            return new List<string>
            {
                ".txt", ".md", ".pdf", ".docx", ".doc", 
                ".rtf", ".odt", ".html", ".htm"
            };
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            return await _aiService.IsServiceAvailableAsync();
        }

        public async Task<TokenUsageInfo> EstimateTokenUsageAsync(string content, QuizGenerationSettings settings)
        {
            try
            {
                // Crear un prompt simulado para estimar tokens más precisamente
                var mockPrompt = $@"
                Genera {settings.NumberOfQuestions} preguntas de quiz basadas en este contenido:
                {content}
                
                Configuración:
                - Dificultad: {settings.DifficultyLevel}
                - Tipos: {string.Join(", ", settings.QuestionTypes)}
                - Explicaciones: {settings.IncludeExplanations}
                ";

                var tokenInfo = await _aiService.EstimateTokenUsageAsync(mockPrompt);
                
                // Ajustar estimación basada en la configuración del quiz
                var adjustedTokens = (int)(tokenInfo.TotalTokens * 1.5); // Factor de respuesta de quiz
                
                return new TokenUsageInfo
                {
                    PromptTokens = tokenInfo.PromptTokens,
                    CompletionTokens = adjustedTokens,
                    TotalTokens = tokenInfo.PromptTokens + adjustedTokens,
                    EstimatedCost = tokenInfo.EstimatedCost * 1.5m,
                    RequestTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating token usage for quiz generation");
                
                // Fallback: estimación básica
                var basicEstimate = content.Length / 4; // ~4 chars per token
                return new TokenUsageInfo
                {
                    PromptTokens = basicEstimate,
                    CompletionTokens = basicEstimate * 2,
                    TotalTokens = basicEstimate * 3,
                    EstimatedCost = basicEstimate * 0.001m,
                    RequestTime = DateTime.UtcNow
                };
            }
        }

        private QuizGenerationResult ParseAIResponse(string? jsonContent)
        {
            try
            {
                _logger.LogInformation("Parseando respuesta de IA. Longitud del JSON: {Length}", jsonContent?.Length ?? 0);
                _logger.LogDebug("Contenido JSON recibido: {Json}", jsonContent);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "La respuesta de IA estaba vacía"
                    };
                }
                
                using var document = JsonDocument.Parse(jsonContent);
                
                if (!document.RootElement.TryGetProperty("questions", out var questionsArray))
                {
                    _logger.LogWarning("No se encontró array 'questions' en la respuesta de IA");
                    return new QuizGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "Formato de respuesta inválido: no se encontró array de preguntas"
                    };
                }

                var questions = new List<GeneratedQuizQuestion>();
                var questionCount = questionsArray.GetArrayLength();
                _logger.LogInformation("Encontradas {Count} preguntas en la respuesta de IA", questionCount);

                foreach (var questionElement in questionsArray.EnumerateArray())
                {
                    try
                    {
                        var question = ParseQuestionFromJson(questionElement);
                        if (question != null)
                        {
                            questions.Add(question);
                            _logger.LogDebug("Pregunta parseada exitosamente: {QuestionText}", question.QuestionText);
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo parsear una pregunta - resultado null");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing individual question from AI response");
                    }
                }

                _logger.LogInformation("Parseado completado. Preguntas válidas: {ValidCount} de {TotalCount}", 
                    questions.Count, questionCount);

                return new QuizGenerationResult
                {
                    Success = true,
                    Questions = questions,
                    ProcessingTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI response JSON: {Content}", jsonContent);
                return new QuizGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error al procesar respuesta de IA: {ex.Message}"
                };
            }
        }

        private GeneratedQuizQuestion? ParseQuestionFromJson(JsonElement questionElement)
        {
            try
            {
                var question = new GeneratedQuizQuestion
                {
                    QuestionText = GetStringValue(questionElement, "questionText") ?? GetStringValue(questionElement, "question") ?? "",
                    QuestionType = ParseQuestionType(GetStringValue(questionElement, "questionType") ?? GetStringValue(questionElement, "type") ?? "MultipleChoice"),
                    DifficultyLevel = ParseDifficultyLevel(GetStringValue(questionElement, "difficultyLevel") ?? GetStringValue(questionElement, "difficulty") ?? "Medio"),
                    CorrectAnswer = GetStringValue(questionElement, "correctAnswer") ?? GetStringValue(questionElement, "correct") ?? "",
                    Explanation = GetStringValue(questionElement, "explanation") ?? "",
                    Points = GetIntValue(questionElement, "points") ?? 1,
                    SourceReference = GetStringValue(questionElement, "sourceReference") ?? "",
                    ConfidenceScore = GetDoubleValue(questionElement, "confidenceScore") ?? 0.8,
                    IsApproved = false
                };

                // Parsear opciones de respuesta
                if (questionElement.TryGetProperty("answerOptions", out var optionsArray) || 
                    questionElement.TryGetProperty("options", out optionsArray) ||
                    questionElement.TryGetProperty("answers", out optionsArray))
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
                if (questionElement.TryGetProperty("tags", out var tagsArray))
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

                // Validar que la pregunta es válida
                if (string.IsNullOrEmpty(question.QuestionText))
                    return null;

                if (question.QuestionType == QuestionType.MultipleChoice && !question.AnswerOptions.Any())
                    return null;

                if ((question.QuestionType == QuestionType.FillInTheBlank || question.QuestionType == QuestionType.ShortAnswer) 
                    && string.IsNullOrEmpty(question.CorrectAnswer))
                    return null;

                return question;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing question from JSON element");
                return null;
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

        private Core.Enums.NivelDificultad ParseDifficultyLevel(string? difficulty)
        {
            return difficulty?.ToLower() switch
            {
                "fácil" or "facil" or "easy" => Core.Enums.NivelDificultad.Facil,
                "difícil" or "dificil" or "hard" => Core.Enums.NivelDificultad.Dificil,
                _ => Core.Enums.NivelDificultad.Intermedio
            };
        }

        private string? GetStringValue(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.GetDecimal().ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => property.ToString()
            };
        }

        private int? GetIntValue(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetInt32(),
                JsonValueKind.String => int.TryParse(property.GetString(), out var result) ? result : null,
                _ => null
            };
        }

        private double? GetDoubleValue(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetDouble(),
                JsonValueKind.String => double.TryParse(property.GetString(), out var result) ? result : null,
                _ => null
            };
        }
    }
}