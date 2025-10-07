using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;

namespace QuizCraft.Infrastructure.Services.DocumentProcessing
{
    /// <summary>
    /// Servicio principal para generación automática de flashcards
    /// Implementa el patrón Strategy para alternar entre procesamiento tradicional e IA
    /// </summary>
    public class FlashcardGenerationService : IFlashcardGenerationService
    {
        private readonly ITraditionalDocumentProcessor _traditionalProcessor;
        private readonly IAIDocumentProcessor? _aiProcessor;
        private readonly ILogger<FlashcardGenerationService> _logger;

        // Tipos de archivo soportados
        private readonly List<string> _supportedExtensions = new()
        {
            ".txt", ".pdf", ".docx", ".pptx"
        };

        public FlashcardGenerationService(
            ITraditionalDocumentProcessor traditionalProcessor,
            ILogger<FlashcardGenerationService> logger,
            IAIDocumentProcessor? aiProcessor = null)
        {
            _traditionalProcessor = traditionalProcessor;
            _aiProcessor = aiProcessor;
            _logger = logger;
        }

        public async Task<QuizCraft.Application.Models.FlashcardGenerationResult> GenerateFromDocumentAsync(
            Stream documentStream, 
            string fileName, 
            GenerationMode mode, 
            GenerationSettings settings)
        {
            _logger.LogInformation("Iniciando generación de flashcards para {FileName} con modo {Mode}", 
                fileName, mode);

            try
            {
                // Validar archivo
                if (!IsFileSupported(fileName))
                {
                    return new QuizCraft.Application.Models.FlashcardGenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Tipo de archivo no soportado: {Path.GetExtension(fileName)}",
                        ProcessingMethod = mode.ToString()
                    };
                }

                // Validar stream
                if (documentStream == null || !documentStream.CanRead)
                {
                    return new QuizCraft.Application.Models.FlashcardGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "El documento no se puede leer",
                        ProcessingMethod = mode.ToString()
                    };
                }

                // Procesar según el modo seleccionado
                return mode switch
                {
                    GenerationMode.Traditional => await ProcessWithTraditionalMode(documentStream, fileName, settings),
                    GenerationMode.AI => await ProcessWithAIMode(documentStream, fileName, settings),
                    _ => throw new ArgumentException($"Modo de generación no válido: {mode}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la generación de flashcards para {FileName}", fileName);
                return new QuizCraft.Application.Models.FlashcardGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error interno: {ex.Message}",
                    ProcessingMethod = mode.ToString()
                };
            }
        }

        public bool IsFileSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _supportedExtensions.Contains(extension);
        }

        public List<string> GetSupportedFileTypes()
        {
            return _supportedExtensions.ToList();
        }

        private async Task<QuizCraft.Application.Models.FlashcardGenerationResult> ProcessWithTraditionalMode(
            Stream documentStream, 
            string fileName, 
            GenerationSettings settings)
        {
            if (settings is not TraditionalGenerationSettings traditionalSettings)
            {
                // Crear configuración por defecto si no es del tipo correcto
                traditionalSettings = new TraditionalGenerationSettings
                {
                    MaxCardsPerDocument = settings.MaxCardsPerDocument,
                    MinTextLength = settings.MinTextLength,
                    MaxTextLength = settings.MaxTextLength,
                    Language = settings.Language,
                    IncludeSourceReference = settings.IncludeSourceReference
                };
            }

            if (!_traditionalProcessor.CanProcess(fileName))
            {
                return new QuizCraft.Application.Models.FlashcardGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"El procesador tradicional no puede manejar archivos {Path.GetExtension(fileName)}",
                    ProcessingMethod = "Traditional"
                };
            }

            return await _traditionalProcessor.ProcessAsync(documentStream, fileName, traditionalSettings);
        }

        private async Task<QuizCraft.Application.Models.FlashcardGenerationResult> ProcessWithAIMode(
            Stream documentStream, 
            string fileName, 
            GenerationSettings settings)
        {
            // Verificar si el procesador de IA está disponible
            if (_aiProcessor == null)
            {
                _logger.LogWarning("Procesador de IA no está configurado, fallback a modo tradicional");
                return await ProcessWithTraditionalMode(documentStream, fileName, settings);
            }

            // Verificar créditos/tokens disponibles
            if (!await _aiProcessor.HasCreditsAvailableAsync())
            {
                _logger.LogInformation("No hay créditos de IA disponibles, fallback a modo tradicional");
                return await ProcessWithTraditionalMode(documentStream, fileName, settings);
            }

            if (settings is not QuizCraft.Application.Interfaces.AIGenerationSettings aiSettings)
            {
                // Crear configuración por defecto si no es del tipo correcto
                aiSettings = new QuizCraft.Application.Interfaces.AIGenerationSettings
                {
                    MaxCardsPerDocument = settings.MaxCardsPerDocument,
                    Language = settings.Language,
                    Difficulty = "Medium",
                    IncludeExplanations = true
                };
            }

            try
            {
                // Convertir de Interfaces.AIGenerationSettings a Models.AIGenerationSettings
                var modelsAiSettings = new QuizCraft.Application.Models.AIGenerationSettings
                {
                    MaxCardsPerDocument = aiSettings.MaxCardsPerDocument,
                    Language = aiSettings.Language,
                    Difficulty = aiSettings.Difficulty,
                    IncludeExplanations = aiSettings.IncludeExplanations
                };
                
                return await _aiProcessor.ProcessAsync(documentStream, fileName, modelsAiSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en procesamiento con IA, fallback a modo tradicional");
                // Fallback a modo tradicional en caso de error
                return await ProcessWithTraditionalMode(documentStream, fileName, settings);
            }
        }
    }
}