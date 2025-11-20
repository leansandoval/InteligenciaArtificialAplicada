using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para la gestión de repasos programados
/// </summary>
[Authorize]
public class RepasoController : Controller
{
    private readonly IRepasoProgramadoService _repasoProgramadoService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RepasoController> _logger;

    public RepasoController(
        IRepasoProgramadoService repasoProgramadoService,
        UserManager<ApplicationUser> userManager,
        ILogger<RepasoController> logger)
    {
        _repasoProgramadoService = repasoProgramadoService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Página principal de repasos programados
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await _repasoProgramadoService.ObtenerRepasosPorUsuarioAsync(userId);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la página de repasos programados");
            TempData["Error"] = "Ocurrió un error al cargar los repasos programados.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Formulario para crear un nuevo repaso programado
    /// </summary>
    public async Task<IActionResult> Crear()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new CrearRepasoProgramadoViewModel();
            await CargarOpcionesSelectAsync(viewModel, userId);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el formulario de crear repaso");
            TempData["Error"] = "Ocurrió un error al cargar el formulario.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Crear nuevo repaso programado
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearRepasoProgramadoViewModel viewModel)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Validar que la fecha programada sea futura
                if (viewModel.FechaProgramada <= DateTime.Now)
                {
                    ModelState.AddModelError("FechaProgramada", "La fecha programada debe ser futura.");
                }
                
                // Validar que se haya seleccionado al menos un tipo de contenido
                if (!viewModel.QuizId.HasValue && !viewModel.MateriaId.HasValue)
                {
                    ModelState.AddModelError("", "Debes seleccionar un quiz específico o una materia para flashcards.");
                }
                
                // Validar que no se seleccionen Quiz y Materia al mismo tiempo
                if (viewModel.QuizId.HasValue && viewModel.MateriaId.HasValue)
                {
                    ModelState.AddModelError("", "No puedes seleccionar un Quiz y una Materia al mismo tiempo. Elige solo un tipo de repaso.");
                }
                
                // Si no hay errores de validación, proceder
                if (ModelState.IsValid)
                {
                    var exito = await _repasoProgramadoService.CrearRepasoProgramadoAsync(viewModel, userId);
                    
                    if (exito)
                    {
                        var mensaje = "Repaso programado creado exitosamente";
                        
                        // Mensaje específico según el tipo de contenido
                        if (viewModel.QuizId.HasValue)
                        {
                            mensaje += " - Se enfocará en el quiz seleccionado";
                        }
                        else if (viewModel.MateriaId.HasValue)
                        {
                            mensaje += " - Se enfocará en flashcards de la materia seleccionada";
                        }
                        
                        TempData["Success"] = mensaje;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Ocurrió un error al crear el repaso programado.");
                    }
                }
            }

            await CargarOpcionesSelectAsync(viewModel, userId);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear repaso programado");
            TempData["Error"] = "Ocurrió un error al crear el repaso programado.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Eliminar repaso programado
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var exito = await _repasoProgramadoService.EliminarRepasoProgramadoAsync(id, userId);
            
            if (exito)
            {
                TempData["Success"] = "Repaso programado eliminado exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo eliminar el repaso programado.";
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar repaso programado");
            TempData["Error"] = "Ocurrió un error al eliminar el repaso programado.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Iniciar un repaso programado (realizar quiz/flashcards)
    /// </summary>
    public async Task<IActionResult> IniciarRepaso(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var repaso = await _repasoProgramadoService.ObtenerRepasoPorIdAsync(id, userId);
            if (repaso == null)
            {
                TempData["Error"] = "Repaso no encontrado.";
                return RedirectToAction("Index");
            }

            if (repaso.Completado)
            {
                TempData["Info"] = "Este repaso ya está completado.";
                return RedirectToAction("Index");
            }

            // Guardar el ID del repaso en TempData para usar después del quiz/flashcard
            TempData["RepasoId"] = id;
            TempData["RepasoTitulo"] = repaso.Titulo;

            // Redireccionar según el tipo de contenido
            if (repaso.QuizId.HasValue)
            {
                // Si tiene quiz, ir al quiz en modo pregunta por pregunta
                return RedirectToAction("TomarQuiz", "Quiz", new { id = repaso.QuizId });
            }
            else if (repaso.FlashcardId.HasValue || repaso.MateriaId.HasValue)
            {
                // Si tiene flashcard o materia, ir directamente al repaso de flashcards
                // sin pasar por configuración (ya que es un repaso programado)
                if (repaso.FlashcardId.HasValue)
                {
                    // Repaso de una flashcard específica
                    return RedirectToAction("IniciarRepasoDirecto", "Flashcard", new { 
                        flashcardId = repaso.FlashcardId,
                        repasoId = id
                    });
                }
                else if (repaso.MateriaId.HasValue)
                {
                    // Repaso de flashcards de una materia
                    return RedirectToAction("IniciarRepasoDirecto", "Flashcard", new { 
                        materiaId = repaso.MateriaId,
                        repasoId = id
                    });
                }
            }
            
            // Si llegamos aquí, no hay contenido específico
            TempData["Error"] = "No hay contenido específico asociado a este repaso.";
            return RedirectToAction("Completar", new { id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar repaso {RepasoId}", id);
            TempData["Error"] = "Error interno del servidor al iniciar el repaso.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Completar repaso desde quiz o flashcard con resultados
    /// </summary>
    public async Task<IActionResult> CompletarConResultados(int? repasoId = null, double? puntaje = null, string? resultado = null)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener el repasoId de TempData si no se proporciona
            var idRepaso = repasoId ?? (TempData["RepasoId"] as int?);
            if (!idRepaso.HasValue)
            {
                TempData["Error"] = "No se pudo identificar el repaso a completar.";
                return RedirectToAction("Index");
            }

            var repaso = await _repasoProgramadoService.ObtenerRepasoPorIdAsync(idRepaso.Value, userId);
            if (repaso == null)
            {
                TempData["Error"] = "Repaso no encontrado.";
                return RedirectToAction("Index");
            }

            // Crear ViewModel para mostrar resultados y completar
            var viewModel = new CompletarRepasoViewModel
            {
                Id = repaso.Id,
                Titulo = repaso.Titulo,
                Descripcion = repaso.Descripcion,
                FechaProgramada = repaso.FechaProgramada,
                TipoRepaso = repaso.TipoRepaso,
                MateriaNombre = repaso.Materia?.Nombre,
                QuizTitulo = repaso.Quiz?.Titulo,
                FlashcardPregunta = repaso.Flashcard?.Pregunta,
                Puntaje = puntaje
            };

            // Agregar información de resultados para mostrar
            ViewBag.MostrarResultados = true;
            ViewBag.PuntajeObtenido = puntaje;
            ViewBag.ResultadoTexto = resultado;
            ViewBag.RegresoDeActividad = true;

            return View("Completar", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar resultados del repaso {RepasoId}", repasoId);
            TempData["Error"] = "Error interno del servidor al procesar los resultados.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Formulario para completar un repaso
    /// </summary>
    public async Task<IActionResult> Completar(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var repaso = await _repasoProgramadoService.ObtenerRepasoPorIdAsync(id, userId);
            if (repaso == null)
            {
                TempData["Error"] = "Repaso no encontrado.";
                return RedirectToAction("Index");
            }

            if (repaso.Completado)
            {
                TempData["Warning"] = "Este repaso ya ha sido completado.";
                return RedirectToAction("Index");
            }

            var viewModel = new CompletarRepasoViewModel
            {
                Id = repaso.Id,
                Titulo = repaso.Titulo,
                Descripcion = repaso.Descripcion,
                FechaProgramada = repaso.FechaProgramada,
                TipoRepaso = repaso.TipoRepaso,
                MateriaNombre = repaso.Materia?.Nombre,
                QuizTitulo = repaso.Quiz?.Titulo,
                FlashcardPregunta = repaso.Flashcard?.Pregunta,
                ProgramarProximo = repaso.FrecuenciaRepeticion.HasValue && repaso.FrecuenciaRepeticion != Core.Enums.FrecuenciaRepaso.Unica
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el formulario de completar repaso");
            TempData["Error"] = "Ocurrió un error al cargar el repaso.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Completar un repaso
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Completar(CompletarRepasoViewModel viewModel)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var exito = await _repasoProgramadoService.CompletarRepasoAsync(viewModel, userId);
                
                if (exito)
                {
                    // Obtener el repaso completado para redirección
                    var repaso = await _repasoProgramadoService.ObtenerRepasoPorIdAsync(viewModel.Id, userId);
                    
                    TempData["Success"] = "¡Repaso completado exitosamente!";
                    
                    // Redireccionar a la vista principal con la pestaña de completados activa
                    return RedirectToAction("Index", new { tab = "completados" });
                }
                else
                {
                    ModelState.AddModelError("", "Ocurrió un error al completar el repaso.");
                }
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar repaso");
            TempData["Error"] = "Ocurrió un error al completar el repaso.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Vista de repasos vencidos
    /// </summary>
    public async Task<IActionResult> Vencidos()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var repasosVencidos = await _repasoProgramadoService.ObtenerRepasosVencidosAsync(userId);
            return View(repasosVencidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar repasos vencidos");
            TempData["Error"] = "Ocurrió un error al cargar los repasos vencidos.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Vista de repasos próximos
    /// </summary>
    public async Task<IActionResult> Proximos()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var repasosProximos = await _repasoProgramadoService.ObtenerRepasosProximosAsync(userId);
            return View(repasosProximos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar repasos próximos");
            TempData["Error"] = "Ocurrió un error al cargar los repasos próximos.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// API endpoint para obtener quizzes por materia (para JavaScript)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerQuizzesPorMateria(int materiaId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var quizzes = await _repasoProgramadoService.ObtenerQuizzesDisponiblesAsync(userId, materiaId);
            return Json(new { success = true, data = quizzes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener quizzes por materia");
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// API endpoint para obtener flashcards por materia (para JavaScript)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerFlashcardsPorMateria(int materiaId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var flashcards = await _repasoProgramadoService.ObtenerFlashcardsDisponiblesAsync(userId, materiaId);
            return Json(new { success = true, data = flashcards });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener flashcards por materia");
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Método auxiliar para cargar las opciones de los selects
    /// </summary>
    private async Task CargarOpcionesSelectAsync(CrearRepasoProgramadoViewModel viewModel, string userId)
    {
        viewModel.MateriasDisponibles = await _repasoProgramadoService.ObtenerMateriasDisponiblesAsync(userId);
        viewModel.QuizzesDisponibles = await _repasoProgramadoService.ObtenerQuizzesDisponiblesAsync(userId);
        viewModel.FlashcardsDisponibles = await _repasoProgramadoService.ObtenerFlashcardsDisponiblesAsync(userId);
    }

    /// <summary>
    /// Obtiene el nombre de una materia por su ID
    /// </summary>
    private async Task<string> ObtenerNombreMateria(int materiaId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return "Materia";
        
        var materias = await _repasoProgramadoService.ObtenerMateriasDisponiblesAsync(userId);
        return materias.FirstOrDefault(m => m.Value == materiaId)?.Text ?? "Materia";
    }

    /// <summary>
    /// Obtiene el nombre de un quiz por su ID
    /// </summary>
    private async Task<string> ObtenerNombreQuiz(int quizId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return "Quiz";
        
        var quizzes = await _repasoProgramadoService.ObtenerQuizzesDisponiblesAsync(userId);
        return quizzes.FirstOrDefault(q => q.Value == quizId)?.Text ?? "Quiz";
    }
}