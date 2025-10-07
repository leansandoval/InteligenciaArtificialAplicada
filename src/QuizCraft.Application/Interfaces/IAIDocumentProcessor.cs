using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces
{
    /// <summary>
    /// Interface para el procesador de documentos con IA
    /// </summary>
    public interface IAIDocumentProcessor
    {
        /// <summary>
        /// Procesa un documento usando IA para generar flashcards
        /// </summary>
        /// <param name="documentStream">Stream del documento</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="settings">Configuración de IA</param>
        /// <returns>Resultado con las flashcards generadas</returns>
        Task<QuizCraft.Application.Models.FlashcardGenerationResult> ProcessAsync(
            Stream documentStream, 
            string fileName, 
            QuizCraft.Application.Models.AIGenerationSettings settings);

        /// <summary>
        /// Verifica si el servicio de IA está disponible
        /// </summary>
        /// <returns>True si está disponible, false en caso contrario</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Verifica si hay créditos de IA disponibles
        /// </summary>
        /// <returns>True si hay créditos disponibles</returns>
        Task<bool> HasCreditsAvailableAsync();

        /// <summary>
        /// Estima el costo en tokens para procesar un contenido
        /// </summary>
        /// <param name="content">Contenido a procesar</param>
        /// <returns>Estimación de tokens y costo</returns>
        Task<TokenUsageInfo> EstimateTokenCostAsync(string content);
    }
}