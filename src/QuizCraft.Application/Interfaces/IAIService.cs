using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicios de Inteligencia Artificial
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Genera flashcards a partir de texto usando IA
        /// </summary>
        /// <param name="content">Contenido de texto</param>
        /// <param name="settings">Configuración de generación</param>
        /// <returns>Respuesta de la IA con flashcards</returns>
        Task<AIResponse> GenerateFlashcardsFromTextAsync(string content, AIGenerationSettings settings);

        /// <summary>
        /// Genera un quiz a partir de texto usando IA
        /// </summary>
        /// <param name="content">Contenido de texto</param>
        /// <param name="settings">Configuración de generación de quiz</param>
        /// <returns>Respuesta de la IA con preguntas de quiz</returns>
        Task<AIResponse> GenerateQuizFromTextAsync(string content, QuizGenerationSettings settings);

        /// <summary>
        /// Genera texto usando IA con un prompt personalizado
        /// </summary>
        /// <param name="prompt">Prompt o instrucción</param>
        /// <param name="customSettings">Configuración personalizada opcional</param>
        /// <returns>Respuesta de la IA</returns>
        Task<AIResponse> GenerateTextAsync(string prompt, AISettings? customSettings = null);

        /// <summary>
        /// Verifica si el servicio de IA está disponible
        /// </summary>
        /// <returns>True si está disponible</returns>
        Task<bool> IsServiceAvailableAsync();

        /// <summary>
        /// Estima el costo en tokens para un contenido
        /// </summary>
        /// <param name="content">Contenido a evaluar</param>
        /// <returns>Estimación de tokens</returns>
        Task<int> EstimateTokenCostAsync(string content);

        /// <summary>
        /// Estima el uso de tokens para una operación específica
        /// </summary>
        /// <param name="content">Contenido a procesar</param>
        /// <returns>Información detallada de uso de tokens</returns>
        Task<TokenUsageInfo> EstimateTokenUsageAsync(string content);

        /// <summary>
        /// Obtiene información sobre el uso actual de tokens
        /// </summary>
        /// <returns>Información de uso de tokens</returns>
        Task<TokenUsageInfo> GetTokenUsageInfoAsync();
    }
}
