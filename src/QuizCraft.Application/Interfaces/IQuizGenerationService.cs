using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces
{
    /// <summary>
    /// Servicio para generación automática de quizzes usando IA
    /// </summary>
    public interface IQuizGenerationService
    {
        /// <summary>
        /// Genera un quiz a partir de un documento
        /// </summary>
        /// <param name="documentStream">Stream del documento</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="settings">Configuración de generación</param>
        /// <returns>Resultado con el quiz generado</returns>
        Task<QuizGenerationResult> GenerateFromDocumentAsync(
            Stream documentStream, 
            string fileName, 
            QuizGenerationSettings settings);

        /// <summary>
        /// Genera un quiz a partir de texto directo
        /// </summary>
        /// <param name="content">Contenido de texto</param>
        /// <param name="settings">Configuración de generación</param>
        /// <returns>Resultado con el quiz generado</returns>
        Task<QuizGenerationResult> GenerateFromTextAsync(
            string content, 
            QuizGenerationSettings settings);

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

        /// <summary>
        /// Verifica si el servicio de IA está disponible
        /// </summary>
        /// <returns>True si está disponible</returns>
        Task<bool> IsServiceAvailableAsync();

        /// <summary>
        /// Estima el costo en tokens para generar un quiz
        /// </summary>
        /// <param name="content">Contenido a procesar</param>
        /// <param name="settings">Configuración de generación</param>
        /// <returns>Información de estimación de tokens</returns>
        Task<TokenUsageInfo> EstimateTokenUsageAsync(string content, QuizGenerationSettings settings);
    }
}