using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para compartir e importar flashcards
/// </summary>
[Authorize]
public class FlashcardCompartidaController : Controller
{
    private readonly IFlashcardCompartidaService _flashcardCompartidaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<FlashcardCompartidaController> _logger;

    public FlashcardCompartidaController(
        IFlashcardCompartidaService flashcardCompartidaService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<FlashcardCompartidaController> logger)
    {
        _flashcardCompartidaService = flashcardCompartidaService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Lista de flashcards compartidas e importadas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? codigo = null, bool mostrarExito = false)
    {
        _logger.LogInformation("=== INDEX: Inicio - codigo={Codigo}, mostrarExito={MostrarExito}", codigo, mostrarExito);
        
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Si se pasó un código, generar la URL y pasar a la vista
            if (mostrarExito && !string.IsNullOrEmpty(codigo))
            {
                _logger.LogInformation("=== INDEX: Generando URL para código {Codigo}", codigo);
                
                var url = Url.Action("Importar", "FlashcardCompartida", 
                    new { codigo }, Request.Scheme);
                
                _logger.LogInformation("=== INDEX: URL generada: {Url}", url);
                
                ViewBag.CodigoCompartido = codigo;
                ViewBag.UrlCompartir = url;
            }

            var compartidas = await _flashcardCompartidaService.ObtenerFlashcardsCompartidasAsync(userId);
            var importadas = await _flashcardCompartidaService.ObtenerFlashcardsImportadasAsync(userId);

            var viewModel = new FlashcardsCompartidasViewModel
            {
                FlashcardsCompartidas = compartidas.Select(fc => new FlashcardCompartidaListItem
                {
                    Id = fc.Id,
                    FlashcardId = fc.FlashcardId,
                    Codigo = fc.Codigo,
                    Pregunta = fc.Pregunta,
                    NombreMateria = fc.NombreMateria,
                    Dificultad = fc.Dificultad,
                    FechaCreacion = fc.FechaCreacion,
                    FechaExpiracion = fc.FechaExpiracion,
                    VecesUsado = fc.VecesUsado,
                    MaximoUsos = fc.MaximoUsos,
                    EstaExpirado = fc.EstaExpirado,
                    EstaAgotado = fc.EstaAgotado,
                    EstaActivo = fc.EstaActivo
                }).ToList(),
                
                FlashcardsImportadas = importadas.Select(fi => new FlashcardImportadaListItem
                {
                    FlashcardId = fi.FlashcardId,
                    Pregunta = fi.Pregunta,
                    NombreMateria = fi.NombreMateria,
                    Dificultad = fi.Dificultad,
                    NombrePropietarioOriginal = fi.NombrePropietarioOriginal,
                    FechaImportacion = fi.FechaImportacion,
                    PermiteModificaciones = fi.PermiteModificaciones
                }).ToList()
            };

            _logger.LogInformation("=== INDEX: Compartidas: {Compartidas}, Importadas: {Importadas}", 
                viewModel.FlashcardsCompartidas.Count, viewModel.FlashcardsImportadas.Count);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar flashcards compartidas");
            TempData["Error"] = "Error al cargar las flashcards compartidas";
            return RedirectToAction("Index", "Flashcard");
        }
    }

    /// <summary>
    /// Formulario para compartir una flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Compartir(int flashcardId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(flashcardId);
            if (flashcard == null || flashcard.Materia.UsuarioId != userId)
            {
                TempData["Error"] = "Flashcard no encontrada o no tienes permisos";
                return RedirectToAction("Index", "Flashcard");
            }

            var viewModel = new CompartirFlashcardViewModel
            {
                FlashcardId = flashcardId,
                PreguntaFlashcard = flashcard.Pregunta,
                PermiteModificaciones = true
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar compartición de flashcard {FlashcardId}", flashcardId);
            TempData["Error"] = "Error al preparar la compartición";
            return RedirectToAction("Index", "Flashcard");
        }
    }

    /// <summary>
    /// Procesar compartición de flashcard
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compartir(CompartirFlashcardViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opciones = new CompartirFlashcardOptions
            {
                FechaExpiracion = model.FechaExpiracion,
                MaximoUsos = model.MaximoUsos,
                PermiteModificaciones = model.PermiteModificaciones
            };

            var resultado = await _flashcardCompartidaService.CompartirFlashcardAsync(
                model.FlashcardId, userId, opciones);

            if (!resultado.IsSuccess)
            {
                TempData["Error"] = resultado.ErrorMessage;
                return View(model);
            }

            TempData["Success"] = "Flashcard compartida exitosamente";
            return RedirectToAction(nameof(Index), new { codigo = resultado.Data, mostrarExito = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al compartir flashcard");
            TempData["Error"] = "Error al compartir la flashcard";
            return View(model);
        }
    }

    /// <summary>
    /// Formulario para importar una flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Importar(string? codigo)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(userId);

            var viewModel = new ImportarFlashcardViewModel
            {
                Codigo = codigo ?? string.Empty,
                MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Color = m.Color ?? "#007bff",
                    Icono = m.Icono ?? "fas fa-book"
                }).ToList()
            };

            // Si se proporcionó un código, obtener información de la flashcard
            if (!string.IsNullOrWhiteSpace(codigo))
            {
                var infoResult = await _flashcardCompartidaService.ObtenerInfoFlashcardCompartidaAsync(codigo);
                if (infoResult.IsSuccess)
                {
                    viewModel.InfoFlashcard = infoResult.Data;
                }
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar formulario de importación");
            TempData["Error"] = "Error al cargar el formulario";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesar importación de flashcard
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Importar(ImportarFlashcardViewModel model)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Recargar materias
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(userId);
                model.MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Color = m.Color ?? "#007bff",
                    Icono = m.Icono ?? "fas fa-book"
                }).ToList();

                return View(model);
            }

            var resultado = await _flashcardCompartidaService.ImportarFlashcardAsync(
                model.Codigo, userId, model.MateriaDestinoId);

            if (!resultado.IsSuccess)
            {
                TempData["Error"] = resultado.ErrorMessage;
                return RedirectToAction(nameof(Importar), new { codigo = model.Codigo });
            }

            TempData["Success"] = "Flashcard importada exitosamente";
            return RedirectToAction("Details", "Flashcard", new { id = resultado.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar flashcard");
            TempData["Error"] = "Error al importar la flashcard";
            return RedirectToAction(nameof(Importar), new { codigo = model.Codigo });
        }
    }

    /// <summary>
    /// Revocar una compartición
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revocar(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var resultado = await _flashcardCompartidaService.RevocarComparticionAsync(id, userId);

            if (!resultado.IsSuccess)
            {
                return Json(new { success = false, message = resultado.ErrorMessage });
            }

            return Json(new { success = true, message = "Compartición revocada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar compartición {Id}", id);
            return Json(new { success = false, message = "Error al revocar la compartición" });
        }
    }
}
