namespace QuizCraft.Application.Models
{
    /// <summary>
    /// Configuración para Google Gemini AI Service
    /// </summary>
    public class GeminiSettings
    {
        /// <summary>
        /// Clave de API de Google Gemini (gratuita con cuenta de Google)
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Modelo de Gemini a utilizar (ej: gemini-2.0-flash-exp, gemini-pro)
        /// </summary>
        public string Model { get; set; } = "gemini-2.0-flash-exp";

        /// <summary>
        /// Máximo número de tokens en la respuesta
        /// </summary>
        public int MaxTokens { get; set; } = 1500;

        /// <summary>
        /// Temperatura para la generación (0.0 - 2.0)
        /// 0.0 = más determinista, 2.0 = más creativo
        /// </summary>
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Top-P para núcleo sampling (0.0 - 1.0)
        /// </summary>
        public float TopP { get; set; } = 0.95f;

        /// <summary>
        /// Top-K para selección de tokens (número entero)
        /// </summary>
        public int TopK { get; set; } = 40;

        /// <summary>
        /// Máximo número de requests por día por usuario
        /// </summary>
        public int MaxRequestsPerDay { get; set; } = 1000;

        /// <summary>
        /// Máximo número de tokens por usuario por día
        /// </summary>
        public int MaxTokensPerUser { get; set; } = 50000;

        /// <summary>
        /// Timeout en segundos para las requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Número máximo de reintentos en caso de error temporal
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Indica si el servicio está habilitado
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// URL base de la API de Gemini
        /// </summary>
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";
    }

    /// <summary>
    /// Configuración específica para la generación de flashcards con Google Gemini
    /// </summary>
    public class GeminiFlashcardGenerationSettings : GeminiSettings
    {
        /// <summary>
        /// Prompt del sistema para generar flashcards
        /// </summary>
        public string SystemPrompt { get; set; } = 
            "Eres un experto en educación que crea flashcards de alta calidad. " +
            "Genera preguntas claras y respuestas precisas basadas en el contenido proporcionado. " +
            "Mantén un nivel educativo apropiado y asegúrate de que las preguntas sean específicas y las respuestas completas.";

        /// <summary>
        /// Máximo número de flashcards a generar por documento
        /// </summary>
        public int MaxFlashcardsPerDocument { get; set; } = 15;

        /// <summary>
        /// Incluir explicaciones adicionales en las flashcards
        /// </summary>
        public bool IncludeExplanations { get; set; } = true;

        /// <summary>
        /// Formato de respuesta esperado (JSON, Markdown, etc.)
        /// </summary>
        public string ResponseFormat { get; set; } = "JSON";

        /// <summary>
        /// Niveles de dificultad disponibles
        /// </summary>
        public List<string> AvailableDifficultyLevels { get; set; } = new()
        { 
            "Fácil", "Intermedio", "Difícil" 
        };
    }
}