using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Application.Models
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o";
        public int MaxTokens { get; set; } = 1500;
        public string Organization { get; set; } = string.Empty;
        public double Temperature { get; set; } = 0.7;
        public int MaxRequestsPerDay { get; set; } = 1000;
        public int MaxTokensPerUser { get; set; } = 5000;
    }

    public class AIGenerationSettings
    {
        public int MaxCardsPerDocument { get; set; } = 20;
        public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        public string Language { get; set; } = "Spanish";
        public bool IncludeExplanations { get; set; } = true;
        public string FocusArea { get; set; } = "General"; // Math, History, Science, etc.
        public bool GenerateQuestions { get; set; } = true;
        public bool GenerateDefinitions { get; set; } = true;
        public int MinQuestionLength { get; set; } = 10;
        public int MaxQuestionLength { get; set; } = 200;
    }

    public class FlashcardGenerationPrompt
    {
        public string Content { get; set; } = string.Empty;
        public AIGenerationSettings Settings { get; set; } = new();
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

    public class OpenAIResponse
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