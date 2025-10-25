using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces
{
    /// <summary>
    /// Servicio de configuración para servicios de IA
    /// </summary>
    public interface IAIConfigurationService
    {
        /// <summary>
        /// Obtiene la configuración actual de IA
        /// </summary>
        Task<AISettings> GetSettingsAsync();
        
        /// <summary>
        /// Verifica si el servicio está configurado correctamente
        /// </summary>
        Task<bool> IsConfiguredAsync();
        
        /// <summary>
        /// Obtiene la clave API configurada
        /// </summary>
        Task<string> GetApiKeyAsync();
        
        /// <summary>
        /// Valida la configuración actual
        /// </summary>
        Task<bool> ValidateConfigurationAsync();
    }
}