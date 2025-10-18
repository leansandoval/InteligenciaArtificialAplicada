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

        public Task<AISettings> GetSettingsAsync()
        {
            // Mapear la configuración de Gemini a AISettings para compatibilidad
            var settings = new AISettings
            {
                ApiKey = _geminiSettings.ApiKey,
                Model = _geminiSettings.Model,
                MaxTokens = _geminiSettings.MaxTokens,
                Temperature = _geminiSettings.Temperature,
                MaxRequestsPerDay = _geminiSettings.MaxRequestsPerDay,
                MaxTokensPerUser = _geminiSettings.MaxTokensPerUser
            };
            
            return Task.FromResult(settings);
        }

        public Task<bool> IsConfiguredAsync()
        {
            var isConfigured = !string.IsNullOrEmpty(_geminiSettings.ApiKey) &&
                   _geminiSettings.ApiKey != "TU_CLAVE_GEMINI_AQUI" &&
                   !string.IsNullOrEmpty(_geminiSettings.Model) &&
                   _geminiSettings.IsEnabled;
            
            return Task.FromResult(isConfigured);
        }

        public Task<string> GetApiKeyAsync()
        {
            return Task.FromResult(_geminiSettings.ApiKey);
        }

        public Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                // Validar que todos los campos requeridos estén presentes
                if (string.IsNullOrEmpty(_geminiSettings.ApiKey) || 
                    _geminiSettings.ApiKey == "TU_CLAVE_GEMINI_AQUI")
                    return Task.FromResult(false);
                
                if (string.IsNullOrEmpty(_geminiSettings.Model))
                    return Task.FromResult(false);

                // Validar formato del BaseUrl
                if (!Uri.TryCreate(_geminiSettings.BaseUrl, UriKind.Absolute, out var uri))
                    return Task.FromResult(false);

                // Validar que sea un endpoint de Google
                if (!uri.Host.Contains("googleapis.com"))
                    return Task.FromResult(false);

                // Validar que esté habilitado
                if (!_geminiSettings.IsEnabled)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
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