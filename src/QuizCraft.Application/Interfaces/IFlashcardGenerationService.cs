using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces
{
    /// <summary>
    /// Servicio principal para generación automática de flashcards
    /// </summary>
    public interface IFlashcardGenerationService
    {
        /// <summary>
        /// Genera flashcards a partir de un documento
        /// </summary>
        /// <param name="documentStream">Stream del documento</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="mode">Modo de generación (Tradicional o IA)</param>
        /// <param name="settings">Configuración de generación</param>
        /// <returns>Resultado con las flashcards generadas</returns>
        Task<QuizCraft.Application.Models.FlashcardGenerationResult> GenerateFromDocumentAsync(
            Stream documentStream, 
            string fileName, 
            GenerationMode mode,
            GenerationSettings settings);

        /// <summary>
        /// Valida si un archivo es soportado para generación
        /// </summary>
        /// <param name="fileName">Nombre del archivo</param>
        /// <returns>True si es soportado</returns>
        bool IsFileSupported(string fileName);

        /// <summary>
        /// Obtiene los tipos de archivo soportados
        /// </summary>
        /// <returns>Lista de extensiones soportadas</returns>
        List<string> GetSupportedFileTypes();
    }

    /// <summary>
    /// Procesador específico para documentos sin IA
    /// </summary>
    public interface ITraditionalDocumentProcessor
    {
        /// <summary>
        /// Procesa un documento usando técnicas tradicionales
        /// </summary>
        Task<QuizCraft.Application.Models.FlashcardGenerationResult> ProcessAsync(
            Stream documentStream, 
            string fileName, 
            TraditionalGenerationSettings settings);

        /// <summary>
        /// Determina si puede procesar el tipo de archivo
        /// </summary>
        bool CanProcess(string fileName);
    }

    /// <summary>
    /// Extractor de texto base para diferentes tipos de documentos
    /// </summary>
    public interface IDocumentTextExtractor
    {
        /// <summary>
        /// Extrae texto estructurado de un documento
        /// </summary>
        Task<DocumentContent> ExtractTextAsync(Stream documentStream, string fileName);

        /// <summary>
        /// Determina si puede extraer texto del tipo de archivo
        /// </summary>
        bool CanExtract(string fileName);
    }

    /// <summary>
    /// Contenido estructurado extraído de un documento
    /// </summary>
    public class DocumentContent
    {
        public string RawText { get; set; } = string.Empty;
        public List<DocumentSection> Sections { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public string DocumentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sección de un documento con estructura
    /// </summary>
    public class DocumentSection
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Level { get; set; } // Para jerarquía de títulos
        public SectionType Type { get; set; }
        public int Position { get; set; } // Posición en el documento
    }

    /// <summary>
    /// Tipos de secciones en documentos
    /// </summary>
    public enum SectionType
    {
        Title,
        Subtitle,
        Paragraph,
        BulletPoint,
        Table,
        Image,
        Code,
        Quote
    }

    /// <summary>
    /// Servicio principal para interactuar con servicios de IA (Gemini)
    /// </summary>
    public interface IAIService
    {
        Task<QuizCraft.Application.Models.AIResponse> GenerateFlashcardsFromTextAsync(string content, AIGenerationSettings settings);
        Task<QuizCraft.Application.Models.AIResponse> GenerateQuizFromTextAsync(string content, QuizCraft.Application.Models.QuizGenerationSettings settings);
        Task<bool> ValidateApiKeyAsync();
        Task<QuizCraft.Application.Models.TokenUsageInfo> GetTokenUsageInfoAsync();
        Task<bool> IsServiceAvailableAsync();
        Task<int> EstimateTokenCostAsync(string content);
        Task<QuizCraft.Application.Models.TokenUsageInfo> EstimateTokenUsageAsync(string content);
        Task<QuizCraft.Application.Models.AIResponse> GenerateTextAsync(string prompt, QuizCraft.Application.Models.AISettings? customSettings = null);
    }

    /// <summary>
    /// Servicio de configuración para servicios de IA
    /// </summary>
    public interface IAIConfigurationService
    {
        Task<QuizCraft.Application.Models.AISettings> GetSettingsAsync();
        Task<bool> IsConfiguredAsync();
        Task<string> GetApiKeyAsync();
        Task<bool> ValidateConfigurationAsync();
    }
}