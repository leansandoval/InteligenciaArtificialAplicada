using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Application.Models
{
    public class AISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-pro";
        public int MaxTokens { get; set; } = 1500;
        public double Temperature { get; set; } = 0.7;
        public int MaxRequestsPerDay { get; set; } = 1000;
        public int MaxTokensPerUser { get; set; } = 5000;
    }

    public class FlashcardGenerationPrompt
    {
        public string Content { get; set; } = string.Empty;
        public QuizCraft.Application.Interfaces.AIGenerationSettings Settings { get; set; } = new();
        public string CustomInstructions { get; set; } = string.Empty;
    }

    public class TokenUsageInfo
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime RequestTime { get; set; }
    }

    public class AIResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = string.Empty;
        public TokenUsageInfo TokenUsage { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    public class FlashcardGenerationResult
    {
        public bool Success { get; set; }
        public List<FlashcardData> Flashcards { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public string ProcessingMethod { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime ProcessingTime { get; set; }
    }

    public class FlashcardData
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public string Dificultad { get; set; } = "Medium";
        public List<string> Etiquetas { get; set; } = new();
        public string FuenteOriginal { get; set; } = string.Empty;
        public double ConfianzaPuntuacion { get; set; } = 0.5;
        public string Categoria { get; set; } = "General";
    }
}