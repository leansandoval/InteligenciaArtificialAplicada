using QuizCraft.Core.Entities;

namespace QuizCraft.Application.Interfaces
{
    /// <summary>
    /// Resultado de la generación de flashcards
    /// </summary>
    public class FlashcardGenerationResult
    {
        public bool Success { get; set; }
        public List<GeneratedFlashcard> Flashcards { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalGenerated { get; set; }
        public GenerationMode ModeUsed { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    /// <summary>
    /// Datos de flashcard generada antes de crear la entidad
    /// </summary>
    public class GeneratedFlashcard
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public string? Explicacion { get; set; }
        public int Confidence { get; set; } = 100; // Para IA: confianza del modelo
        public string Source { get; set; } = string.Empty; // Origen en el documento
    }

    /// <summary>
    /// Modo de generación de flashcards
    /// </summary>
    public enum GenerationMode
    {
        Traditional,    // Sin IA - Procesamiento tradicional
        AI             // Con IA - Usando modelos de lenguaje
    }

    /// <summary>
    /// Configuración base para la generación
    /// </summary>
    public abstract class GenerationSettings
    {
        public int MaxCardsPerDocument { get; set; } = 50;
        public int MinTextLength { get; set; } = 10;
        public int MaxTextLength { get; set; } = 500;
        public string Language { get; set; } = "Spanish";
        public bool IncludeSourceReference { get; set; } = true;
    }

    /// <summary>
    /// Configuración para procesamiento tradicional
    /// </summary>
    public class TraditionalGenerationSettings : GenerationSettings
    {
        public bool SplitByParagraph { get; set; } = true;
        public bool DetectQuestionPatterns { get; set; } = true;
        public bool UseStructuralElements { get; set; } = true;
        public List<string> QuestionKeywords { get; set; } = new() 
        { 
            "¿", "Qué es", "Definir", "Explicar", "Cuál es", "Cómo", "Por qué" 
        };
        public string? CustomSeparator { get; set; }
        public bool FilterShortContent { get; set; } = true;
    }

    /// <summary>
    /// Configuración para procesamiento con IA
    /// </summary>
    public class AIGenerationSettings : GenerationSettings
    {
        public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        public bool IncludeExplanations { get; set; } = true;
        public string? FocusArea { get; set; } // Math, History, Science, etc.
        public int MinConfidence { get; set; } = 70;
        public string Model { get; set; } = "gpt-3.5-turbo";
    }
}