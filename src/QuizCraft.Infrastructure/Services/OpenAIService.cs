using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;

namespace QuizCraft.Infrastructure.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IOpenAIConfigurationService _configService;
        private readonly ILogger<OpenAIService> _logger;
        private const string OPENAI_API_BASE_URL = "https://api.openai.com/v1";

        public OpenAIService(
            HttpClient httpClient,
            IOpenAIConfigurationService configService,
            ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
        }

        public async Task<OpenAIResponse> GenerateFlashcardsFromTextAsync(string content, QuizCraft.Application.Models.AIGenerationSettings settings)
        {
            try
            {
                var prompt = BuildFlashcardGenerationPrompt(content, settings);
                return await GenerateTextAsync(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flashcards from text");
                return new OpenAIResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OpenAIResponse> GenerateTextAsync(string prompt, OpenAISettings? customSettings = null)
        {
            try
            {
                var settings = customSettings ?? await _configService.GetSettingsAsync();
                var apiKey = await _configService.GetApiKeyAsync();

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                
                if (!string.IsNullOrEmpty(settings.Organization))
                {
                    _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", settings.Organization);
                }

                var requestBody = new
                {
                    model = settings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = "Eres un asistente educativo especializado en crear flashcards efectivas para el aprendizaje." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = settings.MaxTokens,
                    temperature = settings.Temperature,
                    response_format = new { type = "json_object" }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to OpenAI API with {TokenCount} estimated tokens", EstimateTokenCount(prompt));

                var response = await _httpClient.PostAsync($"{OPENAI_API_BASE_URL}/chat/completions", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var openAIResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var content = openAIResponse
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    var usage = openAIResponse.GetProperty("usage");
                    var tokenUsage = new TokenUsageInfo
                    {
                        PromptTokens = usage.GetProperty("prompt_tokens").GetInt32(),
                        CompletionTokens = usage.GetProperty("completion_tokens").GetInt32(),
                        TotalTokens = usage.GetProperty("total_tokens").GetInt32(),
                        RequestTime = DateTime.UtcNow,
                        EstimatedCost = CalculateCost(usage.GetProperty("total_tokens").GetInt32(), settings.Model)
                    };

                    _logger.LogInformation("OpenAI request successful. Tokens used: {TotalTokens}, Estimated cost: ${Cost:F4}", 
                        tokenUsage.TotalTokens, tokenUsage.EstimatedCost);

                    return new OpenAIResponse
                    {
                        Success = true,
                        Content = content ?? string.Empty,
                        TokenUsage = tokenUsage,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogError("OpenAI API request failed with status {StatusCode}: {Response}", 
                        response.StatusCode, responseContent);

                    return new OpenAIResponse
                    {
                        Success = false,
                        ErrorMessage = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                return new OpenAIResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            try
            {
                var testPrompt = "Responde solo 'OK' si recibes este mensaje.";
                var testSettings = await _configService.GetSettingsAsync();
                testSettings.MaxTokens = 10; // Minimal tokens for test

                var response = await GenerateTextAsync(testPrompt, testSettings);
                return response.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TokenUsageInfo> GetTokenUsageInfoAsync()
        {
            // En una implementación real, esto podría obtener estadísticas de uso
            // Por ahora, retornamos información básica
            return await Task.FromResult(new TokenUsageInfo
            {
                RequestTime = DateTime.UtcNow
            });
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            var isConfigured = await _configService.IsConfiguredAsync();
            if (!isConfigured) return false;

            // Para evitar costos innecesarios en cada verificación, solo validamos la configuración
            return await _configService.ValidateConfigurationAsync();
        }

        public Task<int> EstimateTokenCostAsync(string content)
        {
            return Task.FromResult(EstimateTokenCount(content));
        }

        public Task<TokenUsageInfo> EstimateTokenUsageAsync(string content)
        {
            var tokenCount = EstimateTokenCount(content);
            var cost = CalculateCost(tokenCount, "gpt-4o"); // Modelo por defecto
            
            return Task.FromResult(new TokenUsageInfo
            {
                PromptTokens = tokenCount,
                CompletionTokens = 0,
                TotalTokens = tokenCount,
                EstimatedCost = cost
            });
        }

        private string BuildFlashcardGenerationPrompt(string content, QuizCraft.Application.Models.AIGenerationSettings settings)
        {
            var prompt = new StringBuilder();
            
            prompt.AppendLine("Genera flashcards educativas a partir del siguiente contenido.");
            prompt.AppendLine($"Número máximo de flashcards: {settings.MaxCardsPerDocument}");
            prompt.AppendLine($"Nivel de dificultad: {settings.Difficulty}");
            prompt.AppendLine($"Idioma: {settings.Language}");
            
            if (!string.IsNullOrEmpty(settings.FocusArea) && settings.FocusArea != "General")
            {
                prompt.AppendLine($"Área de enfoque: {settings.FocusArea}");
            }

            prompt.AppendLine("\nRequisitos:");
            prompt.AppendLine("- Cada flashcard debe tener una pregunta clara y una respuesta precisa");
            prompt.AppendLine("- Las preguntas deben ser variadas (definiciones, conceptos, ejemplos, aplicaciones)");
            prompt.AppendLine($"- Longitud de preguntas entre {settings.MinQuestionLength} y {settings.MaxQuestionLength} caracteres");
            
            if (settings.IncludeExplanations)
            {
                prompt.AppendLine("- Incluir explicaciones adicionales cuando sea útil para el aprendizaje");
            }

            prompt.AppendLine("\nFormato de respuesta (JSON):");
            prompt.AppendLine(@"{
  ""flashcards"": [
    {
      ""pregunta"": ""¿Cuál es la pregunta?"",
      ""respuesta"": ""La respuesta clara y precisa"",
      ""explicacion"": ""Explicación adicional opcional"",
      ""dificultad"": ""Easy"",
      ""categoria"": ""Categoría del contenido""
    }
  ],
  ""resumen"": {
    ""total_generadas"": 5,
    ""nivel_dificultad"": ""Medium"",
    ""area_enfoque"": ""Historia""
  }
}");

            prompt.AppendLine("\nContenido a procesar:");
            prompt.AppendLine("```");
            prompt.AppendLine(content);
            prompt.AppendLine("```");

            return prompt.ToString();
        }

        private int EstimateTokenCount(string text)
        {
            // Estimación aproximada: ~4 caracteres por token para español
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        private decimal CalculateCost(int totalTokens, string model)
        {
            // Precios aproximados (actualizar según pricing de OpenAI)
            var costPer1KTokens = model.ToLower() switch
            {
                "gpt-4" or "gpt-4o" => 0.03m, // $0.03 per 1K tokens
                "gpt-3.5-turbo" => 0.002m,     // $0.002 per 1K tokens
                _ => 0.002m
            };

            return (totalTokens / 1000m) * costPer1KTokens;
        }
    }
}