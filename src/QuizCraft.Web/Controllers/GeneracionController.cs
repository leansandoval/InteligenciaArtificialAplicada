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
        private readonly IAIDocumentProcessor _aiProcessor;
        private readonly IUnitOfWork _unitOfWork;

        public GeneracionController(
            IFlashcardGenerationService generationService,
            IMateriaRepository materiaRepository,
            IFlashcardRepository flashcardRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<GeneracionController> logger,
            IAIDocumentProcessor aiProcessor,
            IUnitOfWork unitOfWork)
        {
            _generationService = generationService;
            _materiaRepository = materiaRepository;
            _flashcardRepository = flashcardRepository;
            _userManager = userManager;
            _logger = logger;
            _aiProcessor = aiProcessor;
            _unitOfWork = unitOfWork;
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
            var startTime = DateTime.UtcNow;
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

                // Guardar las flashcards generadas en TempData para revisión
                var flashcardsParaRevision = result.Flashcards.Select(card => new
                {
                    pregunta = card.Pregunta,
                    respuesta = card.Respuesta,
                    confidence = card.ConfianzaPuntuacion.ToString("F2"), // Convertir a string
                    source = card.FuenteOriginal,
                    dificultad = DeterminarDificultad((int)(card.ConfianzaPuntuacion * 100)).ToString()
                }).ToList();

                // Serializar para TempData (convertir tipos no soportados a string)
                TempData["FlashcardsGeneradas"] = System.Text.Json.JsonSerializer.Serialize(flashcardsParaRevision);
                TempData["MateriaId"] = materiaId.ToString();
                TempData["FileName"] = documento.FileName;
                TempData["ProcessingTime"] = Math.Round((DateTime.UtcNow - startTime).TotalSeconds, 2).ToString();
                TempData["ProcessingMethod"] = result.ProcessingMethod;

                _logger.LogInformation("Generadas {Count} flashcards para revisión de usuario {UserId} en materia {MateriaId}", 
                    result.Flashcards.Count, user.Id, materiaId);

                return Json(new
                {
                    success = true,
                    message = $"Se generaron {result.Flashcards.Count} flashcards exitosamente",
                    flashcardCount = result.Flashcards.Count,
                    processingTime = Math.Round((DateTime.UtcNow - startTime).TotalSeconds, 2),
                    redirectUrl = Url.Action("ReviewFlashcards", "Generacion")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar flashcards");
                var processingTimeSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
                return Json(new { 
                    success = false, 
                    message = "Error interno del servidor",
                    processingTime = Math.Round(processingTimeSeconds, 2)
                });
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

        /// <summary>
        /// Verificar disponibilidad del servicio de IA
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VerificarDisponibilidadIA()
        {
            try
            {
                var disponible = await _aiProcessor.IsAvailableAsync();
                var tieneCreditos = await _aiProcessor.HasCreditsAvailableAsync();

                return Json(new
                {
                    disponible,
                    tieneCreditos,
                    mensaje = disponible && tieneCreditos
                        ? "IA disponible"
                        : !disponible
                            ? "Servicio de IA no configurado"
                            : "Sin créditos disponibles"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando disponibilidad de IA");
                return Json(new
                {
                    disponible = false,
                    tieneCreditos = false,
                    mensaje = "Error verificando disponibilidad"
                });
            }
        }

        /// <summary>
        /// Vista para revisar las flashcards generadas antes de guardarlas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ReviewFlashcards()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Verificar que hay flashcards para revisar
            if (TempData["FlashcardsGeneradas"] == null)
            {
                TempData["ErrorMessage"] = "No hay flashcards para revisar.";
                return RedirectToAction("Index");
            }

            // Deserializar las flashcards generadas
            var flashcardsJson = TempData["FlashcardsGeneradas"]?.ToString();
            if (string.IsNullOrEmpty(flashcardsJson))
            {
                TempData["ErrorMessage"] = "No se pudieron recuperar las flashcards generadas.";
                return RedirectToAction("Index");
            }

            var flashcards = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(flashcardsJson);

            // Obtener información adicional (convertir desde string)
            var materiaIdStr = TempData["MateriaId"]?.ToString();
            if (!int.TryParse(materiaIdStr, out int materiaId))
            {
                TempData["ErrorMessage"] = "Error al recuperar la información de la materia.";
                return RedirectToAction("Index");
            }

            var materia = await _materiaRepository.GetByIdAsync(materiaId);
            var fileName = TempData["FileName"]?.ToString();
            
            var processingTimeStr = TempData["ProcessingTime"]?.ToString();
            if (!double.TryParse(processingTimeStr, out double processingTime))
            {
                processingTime = 0.0;
            }
            
            var processingMethod = TempData["ProcessingMethod"]?.ToString();

            // Crear el modelo de vista
            var model = new ReviewFlashcardsViewModel
            {
                FlashcardsGeneradas = flashcardsJson,
                MateriaId = materiaId,
                MateriaNombre = materia?.Nombre ?? "Desconocida",
                FileName = fileName,
                ProcessingTime = processingTime,
                ProcessingMethod = processingMethod,
                FlashcardCount = flashcards?.Count ?? 0
            };

            return View(model);
        }

        /// <summary>
        /// Guarda las flashcards seleccionadas en la base de datos
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveFlashcards([FromBody] SaveFlashcardsRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Validar que la materia pertenece al usuario
                var materia = await _materiaRepository.GetByIdAsync(request.MateriaId);
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    return Json(new { success = false, message = "Materia no válida" });
                }

                _logger.LogInformation("Iniciando guardado de {Count} flashcards para usuario {UserId} en materia {MateriaId}",
                    request.SelectedFlashcards.Count, user.Id, request.MateriaId);

                int flashcardsGuardadas = 0;
                foreach (var flashcardData in request.SelectedFlashcards)
                {
                    var flashcard = new Flashcard
                    {
                        Pregunta = flashcardData.Pregunta,
                        Respuesta = flashcardData.Respuesta,
                        MateriaId = request.MateriaId,
                        FechaCreacion = DateTime.Now,
                        Dificultad = Enum.Parse<NivelDificultad>(flashcardData.Dificultad, true)
                    };

                    await _flashcardRepository.AddAsync(flashcard);
                    flashcardsGuardadas++;
                    _logger.LogDebug("Agregada flashcard {Index}: {Pregunta}", flashcardsGuardadas, flashcard.Pregunta);
                }

                // IMPORTANTE: Guardar los cambios en la base de datos
                _logger.LogInformation("Llamando a SaveChangesAsync para persistir {Count} flashcards", flashcardsGuardadas);
                var recordsAffected = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync completado. Registros afectados: {RecordsAffected}", recordsAffected);

                _logger.LogInformation("Guardadas {Count} flashcards exitosamente para usuario {UserId} en materia {MateriaId}", 
                    flashcardsGuardadas, user.Id, request.MateriaId);

                return Json(new
                {
                    success = true,
                    message = $"Se guardaron {flashcardsGuardadas} flashcards exitosamente",
                    savedCount = flashcardsGuardadas,
                    recordsAffected = recordsAffected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar flashcards");
                return Json(new { success = false, message = "Error interno del servidor: " + ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo de vista para la revisión de flashcards
    /// </summary>
    public class ReviewFlashcardsViewModel
    {
        public string FlashcardsGeneradas { get; set; } = string.Empty;
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public double ProcessingTime { get; set; }
        public string? ProcessingMethod { get; set; }
        public int FlashcardCount { get; set; }
    }

    /// <summary>
    /// Request para guardar flashcards seleccionadas
    /// </summary>
    public class SaveFlashcardsRequest
    {
        public int MateriaId { get; set; }
        public List<FlashcardToSave> SelectedFlashcards { get; set; } = new();
    }

    public class FlashcardToSave
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public string Dificultad { get; set; } = string.Empty;
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