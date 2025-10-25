using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using Microsoft.Extensions.Logging;

namespace QuizCraft.Infrastructure.Services
{
    /// <summary>
    /// Servicio mock para IA cuando no hay configuración de Gemini disponible
    /// </summary>
    public class MockAIService : IAIService
    {
        private readonly ILogger<MockAIService> _logger;

        public MockAIService(ILogger<MockAIService> logger)
        {
            _logger = logger;
        }

        public async Task<AIResponse> GenerateFlashcardsFromTextAsync(string content, QuizCraft.Application.Interfaces.AIGenerationSettings settings)
        {
            await Task.Delay(100); // Simular latencia
            
            _logger.LogWarning("Mock AI Service: Generación de flashcards deshabilitada - Configure Gemini para habilitar IA");
            
            return new AIResponse
            {
                Success = false,
                Content = "Servicio de IA no configurado. Configure Gemini en appsettings.json para habilitar la generación automática de flashcards.",
                TokenUsage = new TokenUsageInfo
                {
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    EstimatedCost = 0,
                    RequestTime = DateTime.UtcNow
                }
            };
        }

        public async Task<AIResponse> GenerateQuizFromTextAsync(string content, QuizGenerationSettings settings)
        {
            await Task.Delay(100); // Simular latencia
            
            _logger.LogWarning("Mock AI Service: Generación de quizzes deshabilitada - Configure Gemini para habilitar IA");
            
            return new AIResponse
            {
                Success = false,
                Content = "Servicio de IA no configurado. Configure Gemini en appsettings.json para habilitar la generación automática de quizzes.",
                TokenUsage = new TokenUsageInfo
                {
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    EstimatedCost = 0,
                    RequestTime = DateTime.UtcNow
                }
            };
        }

        public async Task<AIResponse> GenerateTextAsync(string prompt, AISettings? customSettings = null)
        {
            await Task.Delay(100); // Simular latencia
            
            _logger.LogWarning("Mock AI Service: Generación de texto deshabilitada - Configure Gemini para habilitar IA");
            
            return new AIResponse
            {
                Success = false,
                Content = "Servicio de IA no configurado. Configure Gemini en appsettings.json para habilitar la generación de contenido.",
                TokenUsage = new TokenUsageInfo
                {
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    EstimatedCost = 0,
                    RequestTime = DateTime.UtcNow
                }
            };
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            await Task.Delay(1);
            return false;
        }

        public async Task<TokenUsageInfo> GetTokenUsageInfoAsync()
        {
            await Task.Delay(1);
            
            return new TokenUsageInfo
            {
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                EstimatedCost = 0,
                RequestTime = DateTime.UtcNow
            };
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            await Task.Delay(1);
            return false;
        }

        public async Task<int> EstimateTokenCostAsync(string content)
        {
            await Task.Delay(1);
            return 0;
        }

        public async Task<TokenUsageInfo> EstimateTokenUsageAsync(string content)
        {
            await Task.Delay(1);
            
            return new TokenUsageInfo
            {
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                EstimatedCost = 0,
                RequestTime = DateTime.UtcNow
            };
        }
    }
}