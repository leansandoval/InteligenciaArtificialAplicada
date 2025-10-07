using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using QuizCraft.Core.Enums;
using System.Text.Json;

namespace QuizCraft.Infrastructure.Services
{
    public class AIDocumentProcessor : IAIDocumentProcessor
    {
        private readonly IOpenAIService _openAIService;
        private readonly IDocumentTextExtractor _textExtractor;
        private readonly ILogger<AIDocumentProcessor> _logger;

        public AIDocumentProcessor(
            IOpenAIService openAIService,
            IDocumentTextExtractor textExtractor,
            ILogger<AIDocumentProcessor> logger)
        {
            _openAIService = openAIService;
            _textExtractor = textExtractor;
            _logger = logger;
        }

        public async Task<QuizCraft.Application.Models.FlashcardGenerationResult> ProcessAsync(
            Stream documentStream, 
            string fileName, 
            QuizCraft.Application.Models.AIGenerationSettings settings)
        {
            try
            {
                // Verificar disponibilidad del servicio de IA
                if (!await _openAIService.IsServiceAvailableAsync())
                {
                    _logger.LogWarning("AI service not available");
                    return new QuizCraft.Application.Models.FlashcardGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "El servicio de IA no está disponible. Verifique la configuración de OpenAI.",
                        ProcessingMethod = "AI (Error)",
                        ProcessingTime = DateTime.UtcNow
                    };
                }

                // Extraer texto del documento
                var documentContent = await _textExtractor.ExtractTextAsync(documentStream, fileName);
                var extractedText = documentContent?.RawText;

                if (string.IsNullOrEmpty(extractedText))
                {
                    return new QuizCraft.Application.Models.FlashcardGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "No se pudo extraer texto del documento",
                        ProcessingMethod = "AI (Error)",
                        ProcessingTime = DateTime.UtcNow
                    };
                }

                // Limitar el texto si es muy largo (para evitar costos excesivos)
                if (extractedText.Length > 10000)
                {
                    extractedText = extractedText.Substring(0, 10000) + "...";
                    _logger.LogInformation("Text truncated to 10,000 characters to manage costs");
                }

                // Estimar costo y verificar límites
                var estimatedTokens = await _openAIService.EstimateTokenCostAsync(extractedText);
                _logger.LogInformation("Estimated tokens for AI processing: {Tokens}", estimatedTokens);

                // Procesar con OpenAI
                var response = await _openAIService.GenerateFlashcardsFromTextAsync(extractedText, settings);
                
                if (!response.Success)
                {
                    _logger.LogWarning("AI processing failed: {Error}", response.ErrorMessage);
                    return new QuizCraft.Application.Models.FlashcardGenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Error en el procesamiento con IA: {response.ErrorMessage}",
                        ProcessingMethod = "AI (Error)",
                        ProcessingTime = DateTime.UtcNow
                    };
                }

                // Parsear respuesta JSON de OpenAI
                var flashcards = ParseOpenAIResponse(response.Content, fileName);
                
                return new QuizCraft.Application.Models.FlashcardGenerationResult
                {
                    Success = true,
                    Flashcards = flashcards,
                    ProcessingMethod = "AI (OpenAI)",
                    TokensUsed = response.TokenUsage.TotalTokens,
                    EstimatedCost = response.TokenUsage.EstimatedCost,
                    ProcessingTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI document processing");
                return new QuizCraft.Application.Models.FlashcardGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error en el procesamiento: {ex.Message}",
                    ProcessingMethod = "AI (Error)",
                    ProcessingTime = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> HasCreditsAvailableAsync()
        {
            // Implementación básica - en producción verificaría límites de tokens/créditos
            return await _openAIService.IsServiceAvailableAsync();
        }

        public async Task<int> GetAvailableTokensAsync()
        {
            // Implementación básica - en producción consultaría límites reales
            var tokenInfo = await _openAIService.GetTokenUsageInfoAsync();
            return 5000; // Placeholder - implementar lógica real de límites
        }

        private List<FlashcardData> ParseOpenAIResponse(string jsonResponse, string sourceFileName)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonResponse);
                var flashcardsArray = jsonDoc.RootElement.GetProperty("flashcards");
                
                var flashcards = new List<FlashcardData>();
                
                foreach (var flashcardElement in flashcardsArray.EnumerateArray())
                {
                    var pregunta = flashcardElement.GetProperty("pregunta").GetString() ?? string.Empty;
                    var respuesta = flashcardElement.GetProperty("respuesta").GetString() ?? string.Empty;
                    
                    if (string.IsNullOrEmpty(pregunta) || string.IsNullOrEmpty(respuesta))
                        continue;

                    var explicacion = flashcardElement.TryGetProperty("explicacion", out var expElement) 
                        ? expElement.GetString() 
                        : string.Empty;
                    
                    var dificultadStr = flashcardElement.TryGetProperty("dificultad", out var difElement)
                        ? difElement.GetString()
                        : "Medium";
                    
                    var categoria = flashcardElement.TryGetProperty("categoria", out var catElement)
                        ? catElement.GetString()
                        : "General";

                    // Combinar respuesta y explicación si existe
                    var respuestaCompleta = string.IsNullOrEmpty(explicacion) 
                        ? respuesta 
                        : $"{respuesta}\n\n💡 {explicacion}";

                    flashcards.Add(new FlashcardData
                    {
                        Pregunta = pregunta,
                        Respuesta = respuestaCompleta,
                        Dificultad = dificultadStr,
                        Etiquetas = new List<string> { categoria, "IA-Generated" },
                        FuenteOriginal = sourceFileName,
                        ConfianzaPuntuacion = 0.9, // Alta confianza para contenido generado por IA
                        Categoria = categoria
                    });
                }
                
                _logger.LogInformation("Successfully parsed {Count} flashcards from AI response", flashcards.Count);
                return flashcards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing OpenAI response: {Response}", jsonResponse);
                return new List<FlashcardData>();
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            return await _openAIService.IsServiceAvailableAsync();
        }

        public async Task<TokenUsageInfo> EstimateTokenCostAsync(string content)
        {
            return await _openAIService.EstimateTokenUsageAsync(content);
        }
    }
}