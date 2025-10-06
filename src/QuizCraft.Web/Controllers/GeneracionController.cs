using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.Interfaces;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Core.Interfaces;

namespace QuizCraft.Web.Controllers
{
    [Authorize]
    public class GeneracionController : Controller
    {
        private readonly IFlashcardGenerationService _generationService;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IFlashcardRepository _flashcardRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GeneracionController> _logger;

        public GeneracionController(
            IFlashcardGenerationService generationService,
            IMateriaRepository materiaRepository,
            IFlashcardRepository flashcardRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<GeneracionController> logger)
        {
            _generationService = generationService;
            _materiaRepository = materiaRepository;
            _flashcardRepository = flashcardRepository;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Página principal para seleccionar modo de generación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            
            var materias = await _materiaRepository.GetMateriasByUsuarioIdAsync(user.Id);

            ViewBag.Materias = materias;
            ViewBag.SupportedFileTypes = _generationService.GetSupportedFileTypes();
            
            return View();
        }

        /// <summary>
        /// Procesar documento y generar flashcards
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarFlashcards(
            IFormFile documento,
            int materiaId,
            string modo,
            [FromForm] GenerationConfigModel config)
        {
            try
            {
                // Validaciones
                if (documento == null || documento.Length == 0)
                {
                    return Json(new { success = false, message = "Debe seleccionar un archivo" });
                }

                if (!_generationService.IsFileSupported(documento.FileName))
                {
                    return Json(new { success = false, message = "Tipo de archivo no soportado" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }
                
                var materia = await _materiaRepository.GetByIdAsync(materiaId);
                
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    return Json(new { success = false, message = "Materia no encontrada" });
                }

                // Determinar modo y configuración
                var generationMode = modo.ToLower() switch
                {
                    "traditional" => GenerationMode.Traditional,
                    "ai" => GenerationMode.AI,
                    _ => GenerationMode.Traditional
                };

                var settings = CreateGenerationSettings(generationMode, config);

                // Procesar documento
                using var stream = documento.OpenReadStream();
                var result = await _generationService.GenerateFromDocumentAsync(
                    stream, documento.FileName, generationMode, settings);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }

                // Crear flashcards en la base de datos
                var flashcardsCreadas = new List<object>();
                foreach (var generatedCard in result.Flashcards)
                {
                    var flashcard = new Flashcard
                    {
                        Pregunta = generatedCard.Pregunta,
                        Respuesta = generatedCard.Respuesta,
                        MateriaId = materiaId,
                        FechaCreacion = DateTime.Now,
                        Dificultad = DeterminarDificultad(generatedCard.Confidence)
                    };

                    await _flashcardRepository.AddAsync(flashcard);
                    
                    flashcardsCreadas.Add(new
                    {
                        id = flashcard.Id,
                        pregunta = flashcard.Pregunta,
                        respuesta = flashcard.Respuesta,
                        confidence = generatedCard.Confidence,
                        source = generatedCard.Source
                    });
                }

                _logger.LogInformation("Generadas {Count} flashcards para usuario {UserId} en materia {MateriaId}", 
                    result.TotalGenerated, user.Id, materiaId);

                return Json(new
                {
                    success = true,
                    message = $"Se generaron {result.TotalGenerated} flashcards exitosamente",
                    flashcards = flashcardsCreadas,
                    processingTime = result.ProcessingTime.TotalSeconds,
                    modeUsed = result.ModeUsed.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar flashcards");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener configuración predeterminada para un modo
        /// </summary>
        [HttpGet]
        public IActionResult GetDefaultConfig(string modo)
        {
            object config = modo.ToLower() switch
            {
                "traditional" => new
                {
                    maxCards = 50,
                    splitByParagraph = true,
                    detectQuestions = true,
                    useStructural = true,
                    minTextLength = 10,
                    maxTextLength = 500,
                    filterShort = true
                },
                "ai" => new
                {
                    maxCards = 20,
                    difficulty = "Medium",
                    includeExplanations = true,
                    minConfidence = 70,
                    focusArea = "",
                    minTextLength = 10,
                    maxTextLength = 800
                },
                _ => new { }
            };

            return Json(config);
        }

        /// <summary>
        /// Vista previa de flashcards generadas antes de guardar
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VistaPrevia(
            IFormFile documento,
            string modo,
            [FromForm] GenerationConfigModel config)
        {
            try
            {
                if (documento == null || documento.Length == 0)
                {
                    return Json(new { success = false, message = "Debe seleccionar un archivo" });
                }

                var generationMode = modo.ToLower() switch
                {
                    "traditional" => GenerationMode.Traditional,
                    "ai" => GenerationMode.AI,
                    _ => GenerationMode.Traditional
                };

                var settings = CreateGenerationSettings(generationMode, config);
                // Limitar a máximo 5 para vista previa
                settings.MaxCardsPerDocument = Math.Min(5, settings.MaxCardsPerDocument);

                using var stream = documento.OpenReadStream();
                var result = await _generationService.GenerateFromDocumentAsync(
                    stream, documento.FileName, generationMode, settings);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }

                var preview = result.Flashcards.Select(f => new
                {
                    pregunta = f.Pregunta,
                    respuesta = f.Respuesta,
                    confidence = f.Confidence,
                    source = f.Source
                }).ToList();

                return Json(new
                {
                    success = true,
                    flashcards = preview,
                    totalPossible = result.TotalGenerated,
                    modeUsed = result.ModeUsed.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en vista previa");
                return Json(new { success = false, message = "Error al generar vista previa" });
            }
        }

        private GenerationSettings CreateGenerationSettings(GenerationMode mode, GenerationConfigModel config)
        {
            return mode switch
            {
                GenerationMode.Traditional => new TraditionalGenerationSettings
                {
                    MaxCardsPerDocument = config.MaxCards ?? 50,
                    MinTextLength = config.MinTextLength ?? 10,
                    MaxTextLength = config.MaxTextLength ?? 500,
                    SplitByParagraph = config.SplitByParagraph ?? true,
                    DetectQuestionPatterns = config.DetectQuestions ?? true,
                    UseStructuralElements = config.UseStructural ?? true,
                    FilterShortContent = config.FilterShort ?? true,
                    CustomSeparator = config.CustomSeparator
                },
                GenerationMode.AI => new AIGenerationSettings
                {
                    MaxCardsPerDocument = config.MaxCards ?? 20,
                    MinTextLength = config.MinTextLength ?? 10,
                    MaxTextLength = config.MaxTextLength ?? 800,
                    Difficulty = config.Difficulty ?? "Medium",
                    IncludeExplanations = config.IncludeExplanations ?? true,
                    MinConfidence = config.MinConfidence ?? 70,
                    FocusArea = config.FocusArea
                },
                _ => throw new ArgumentException("Modo no válido")
            };
        }

        private NivelDificultad DeterminarDificultad(int confidence)
        {
            return confidence switch
            {
                >= 90 => NivelDificultad.Facil,
                >= 70 => NivelDificultad.Intermedio,
                _ => NivelDificultad.Dificil
            };
        }
    }

    /// <summary>
    /// Modelo para recibir configuración del frontend
    /// </summary>
    public class GenerationConfigModel
    {
        // Configuración común
        public int? MaxCards { get; set; }
        public int? MinTextLength { get; set; }
        public int? MaxTextLength { get; set; }

        // Configuración tradicional
        public bool? SplitByParagraph { get; set; }
        public bool? DetectQuestions { get; set; }
        public bool? UseStructural { get; set; }
        public bool? FilterShort { get; set; }
        public string? CustomSeparator { get; set; }

        // Configuración IA
        public string? Difficulty { get; set; }
        public bool? IncludeExplanations { get; set; }
        public int? MinConfidence { get; set; }
        public string? FocusArea { get; set; }
    }
}