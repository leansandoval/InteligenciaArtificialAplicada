using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;

namespace QuizCraft.Infrastructure.Services
{
    /// <summary>
    /// Servicio de configuración para Google Gemini
    /// </summary>
    public class GeminiConfigurationService : IAIConfigurationService
    {
        private readonly GeminiSettings _geminiSettings;
        private readonly IConfiguration _configuration;

        public GeminiConfigurationService(
            IOptions<GeminiSettings> geminiOptions, 
            IConfiguration configuration)
        {
            _geminiSettings = geminiOptions.Value;
            _configuration = configuration;
        }

        public async Task<AISettings> GetSettingsAsync()
        {
            await Task.Delay(1); // Para mantener la interfaz async
            
            // Mapear la configuración de Gemini a AISettings para compatibilidad
            return new AISettings
            {
                ApiKey = _geminiSettings.ApiKey,
                Model = _geminiSettings.Model,
                MaxTokens = _geminiSettings.MaxTokens,
                Temperature = _geminiSettings.Temperature,
                MaxRequestsPerDay = _geminiSettings.MaxRequestsPerDay,
                MaxTokensPerUser = _geminiSettings.MaxTokensPerUser,
                Organization = "Google Gemini" // Identificador para Gemini
            };
        }

        public async Task<bool> IsConfiguredAsync()
        {
            await Task.Delay(1);
            
            return !string.IsNullOrEmpty(_geminiSettings.ApiKey) &&
                   _geminiSettings.ApiKey != "TU_CLAVE_GEMINI_AQUI" &&
                   !string.IsNullOrEmpty(_geminiSettings.Model) &&
                   _geminiSettings.IsEnabled;
        }

        public async Task<string> GetApiKeyAsync()
        {
            await Task.Delay(1);
            return _geminiSettings.ApiKey;
        }

        public async Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                await Task.Delay(1);
                
                // Validar que todos los campos requeridos estén presentes
                if (string.IsNullOrEmpty(_geminiSettings.ApiKey) || 
                    _geminiSettings.ApiKey == "TU_CLAVE_GEMINI_AQUI")
                    return false;
                
                if (string.IsNullOrEmpty(_geminiSettings.Model))
                    return false;

                // Validar formato del BaseUrl
                if (!Uri.TryCreate(_geminiSettings.BaseUrl, UriKind.Absolute, out var uri))
                    return false;

                // Validar que sea un endpoint de Google
                if (!uri.Host.Contains("googleapis.com"))
                    return false;

                // Validar que esté habilitado
                if (!_geminiSettings.IsEnabled)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene la configuración específica de Gemini
        /// </summary>
        public GeminiSettings GetGeminiSettings()
        {
            return _geminiSettings;
        }

        /// <summary>
        /// Verifica si Gemini está habilitado y configurado correctamente
        /// </summary>
        public async Task<bool> IsGeminiEnabledAsync()
        {
            return await IsConfiguredAsync();
        }
    }
}