using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;

namespace QuizCraft.Infrastructure.Services
{
    public class OpenAIConfigurationService : IOpenAIConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIConfigurationService> _logger;
        private OpenAISettings? _cachedSettings;

        public OpenAIConfigurationService(
            IConfiguration configuration,
            ILogger<OpenAIConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OpenAISettings> GetSettingsAsync()
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            _cachedSettings = new OpenAISettings();
            var configSection = _configuration.GetSection("OpenAI");
            
            // Bind manual para evitar problemas de dependencias
            _cachedSettings.ApiKey = configSection["ApiKey"] ?? string.Empty;
            _cachedSettings.Model = configSection["Model"] ?? "gpt-4o";
            _cachedSettings.Organization = configSection["Organization"] ?? string.Empty;
            
            if (int.TryParse(configSection["MaxTokens"], out var maxTokens))
                _cachedSettings.MaxTokens = maxTokens;
            else
                _cachedSettings.MaxTokens = 1500;
                
            if (double.TryParse(configSection["Temperature"], out var temperature))
                _cachedSettings.Temperature = temperature;
            else
                _cachedSettings.Temperature = 0.7;
                
            if (int.TryParse(configSection["MaxRequestsPerDay"], out var maxRequests))
                _cachedSettings.MaxRequestsPerDay = maxRequests;
            else
                _cachedSettings.MaxRequestsPerDay = 1000;
                
            if (int.TryParse(configSection["MaxTokensPerUser"], out var maxTokensPerUser))
                _cachedSettings.MaxTokensPerUser = maxTokensPerUser;
            else
                _cachedSettings.MaxTokensPerUser = 5000;

            // Validar configuración básica
            if (string.IsNullOrEmpty(_cachedSettings.ApiKey))
            {
                _logger.LogWarning("OpenAI API Key not configured");
            }

            if (string.IsNullOrEmpty(_cachedSettings.Model))
            {
                _cachedSettings.Model = "gpt-4o";
                _logger.LogInformation("Using default OpenAI model: {Model}", _cachedSettings.Model);
            }

            return await Task.FromResult(_cachedSettings);
        }

        public async Task<bool> IsConfiguredAsync()
        {
            var settings = await GetSettingsAsync();
            return !string.IsNullOrEmpty(settings.ApiKey);
        }

        public async Task<string> GetApiKeyAsync()
        {
            var settings = await GetSettingsAsync();
            
            // 1. Primero buscar en configuración
            if (!string.IsNullOrEmpty(settings.ApiKey))
                return settings.ApiKey;

            // 2. Buscar en variables de entorno como fallback
            var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrEmpty(envKey))
            {
                _logger.LogInformation("Using OpenAI API Key from environment variable");
                return envKey;
            }

            throw new InvalidOperationException("OpenAI API Key not configured. Please set it in appsettings.json or environment variable OPENAI_API_KEY");
        }

        public async Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                var apiKey = await GetApiKeyAsync();
                var settings = await GetSettingsAsync();

                // Validaciones básicas
                if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith("sk-"))
                {
                    _logger.LogError("Invalid OpenAI API Key format");
                    return false;
                }

                if (settings.MaxTokens <= 0 || settings.MaxTokens > 4096)
                {
                    _logger.LogWarning("Invalid MaxTokens value: {MaxTokens}. Using default: 1500", settings.MaxTokens);
                    settings.MaxTokens = 1500;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OpenAI configuration");
                return false;
            }
        }
    }
}