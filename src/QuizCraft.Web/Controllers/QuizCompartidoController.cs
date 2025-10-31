using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para compartir e importar quizzes
/// </summary>
[Authorize]
public class QuizCompartidoController : Controller
{
    private readonly IQuizCompartidoService _quizCompartidoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<QuizCompartidoController> _logger;

    public QuizCompartidoController(
        IQuizCompartidoService quizCompartidoService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<QuizCompartidoController> logger)
    {
        _quizCompartidoService = quizCompartidoService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Lista de quizzes compartidos e importados
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
                
                var url = Url.Action("Importar", "QuizCompartido", 
                    new { codigo = codigo }, Request.Scheme);
                
                ViewBag.MostrarExito = true;
                ViewBag.CodigoCompartido = codigo;
                ViewBag.UrlCompartir = url;
                
                _logger.LogInformation("=== INDEX: ViewBag configurado - MostrarExito=true, Codigo={Codigo}, URL={Url}", 
                    codigo, url);
            }
            else
            {
                _logger.LogInformation("=== INDEX: NO se cumplió condición - mostrarExito={MostrarExito}, codigo nulo/vacío={CodigoVacio}", 
                    mostrarExito, string.IsNullOrEmpty(codigo));
            }

            var compartidos = await _quizCompartidoService.ObtenerQuizzesCompartidosAsync(userId);
            var importados = await _quizCompartidoService.ObtenerQuizzesImportadosAsync(userId);

            var viewModel = new QuizzesCompartidosViewModel
            {
                QuizzesCompartidos = compartidos.Select(qc => new QuizCompartidoListItem
                {
                    Id = qc.Id,
                    QuizId = qc.QuizId,
                    Codigo = qc.Codigo,
                    TituloQuiz = qc.TituloQuiz,
                    NombreMateria = qc.NombreMateria,
                    Dificultad = qc.Dificultad,
                    NumeroPreguntas = qc.NumeroPreguntas,
                    FechaCreacion = qc.FechaCreacion,
                    FechaExpiracion = qc.FechaExpiracion,
                    VecesUsado = qc.VecesUsado,
                    MaximoUsos = qc.MaximoUsos,
                    EstaExpirado = qc.EstaExpirado,
                    EstaAgotado = qc.EstaAgotado,
                    EstaActivo = qc.EstaActivo
                }).ToList(),
                QuizzesImportados = importados.Select(qi => new QuizImportadoListItem
                {
                    QuizId = qi.QuizId,
                    TituloQuiz = qi.TituloQuiz,
                    NombreMateria = qi.NombreMateria,
                    Dificultad = qi.Dificultad,
                    NumeroPreguntas = qi.NumeroPreguntas,
                    NombrePropietarioOriginal = qi.NombrePropietarioOriginal,
                    FechaImportacion = qi.FechaImportacion
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar quizzes compartidos");
            TempData["Error"] = "Error al cargar la información";
            return RedirectToAction("Index", "Quiz");
        }
    }

    /// <summary>
    /// Formulario para compartir un quiz
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Compartir(int quizId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizId);
            if (quiz == null || quiz.CreadorId != userId)
            {
                TempData["Error"] = "Quiz no encontrado o no tienes permisos";
                return RedirectToAction("Index", "Quiz");
            }

            var viewModel = new CompartirQuizViewModel
            {
                QuizId = quizId,
                TituloQuiz = quiz.Titulo,
                PermiteModificaciones = true
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar compartición de quiz {QuizId}", quizId);
            TempData["Error"] = "Error al preparar la compartición";
            return RedirectToAction("Index", "Quiz");
        }
    }

    /// <summary>
    /// Procesar compartición de quiz
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compartir(CompartirQuizViewModel model)
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

            var opciones = new CompartirQuizOptions
            {
                FechaExpiracion = model.FechaExpiracion,
                MaximoUsos = model.MaximoUsos,
                PermiteModificaciones = model.PermiteModificaciones
            };

            var resultado = await _quizCompartidoService.CompartirQuizAsync(
                model.QuizId, userId, opciones);

            _logger.LogInformation("=== COMPARTIR POST: Resultado del servicio - IsSuccess={IsSuccess}, Data={Data}, Error={Error}", 
                resultado.IsSuccess, resultado.Data, resultado.ErrorMessage);

            if (resultado.IsSuccess)
            {
                var url = Url.Action("Importar", "QuizCompartido", 
                    new { codigo = resultado.Data }, Request.Scheme);
                
                _logger.LogInformation("=== COMPARTIR POST: Redirigiendo a Index con codigo={Codigo}, mostrarExito=true", 
                    resultado.Data);
                
                // Redirigir al Index con el código en la URL
                return RedirectToAction("Index", "QuizCompartido", new { 
                    codigo = resultado.Data,
                    mostrarExito = true 
                });
            }

            _logger.LogWarning("=== COMPARTIR POST: Fallo - {ErrorMessage}", resultado.ErrorMessage);
            ModelState.AddModelError("", resultado.ErrorMessage);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al compartir quiz {QuizId}", model.QuizId);
            ModelState.AddModelError("", "Error al compartir el quiz");
            return View(model);
        }
    }

    /// <summary>
    /// Formulario para importar un quiz
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
            ViewBag.Materias = new SelectList(materias, "Id", "Nombre");
            
            var viewModel = new ImportarQuizViewModel();

            // Si se proporciona un código, obtener información del quiz
            if (!string.IsNullOrEmpty(codigo))
            {
                viewModel.CodigoCompartido = codigo.ToUpper();
                var infoResult = await _quizCompartidoService.ObtenerInfoQuizCompartidoAsync(codigo);
                
                if (infoResult.IsSuccess)
                {
                    viewModel.InfoQuiz = infoResult.Data;
                }
                else
                {
                    TempData["Error"] = infoResult.ErrorMessage;
                }
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar importación de quiz");
            TempData["Error"] = "Error al preparar la importación";
            return RedirectToAction("Index", "Quiz");
        }
    }

    /// <summary>
    /// Procesar importación de quiz
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Importar(ImportarQuizViewModel model)
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
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(userId);
                ViewBag.Materias = new SelectList(materias, "Id", "Nombre");
                return View(model);
            }

            var resultado = await _quizCompartidoService.ImportarQuizAsync(
                model.CodigoCompartido.ToUpper(), userId, model.MateriaId);

            if (resultado.IsSuccess)
            {
                TempData["Success"] = "Quiz importado exitosamente";
                return RedirectToAction("Details", "Quiz", new { id = resultado.Data });
            }

            ModelState.AddModelError("", resultado.ErrorMessage);
            
            var materiasReload = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(userId);
            ViewBag.Materias = new SelectList(materiasReload, "Id", "Nombre");
            
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar quiz con código {Codigo}", model.CodigoCompartido);
            ModelState.AddModelError("", "Error al importar el quiz");
            
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(userId);
                ViewBag.Materias = new SelectList(materias, "Id", "Nombre");
            }
            
            return View(model);
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
                return RedirectToAction("Login", "Account");
            }

            var resultado = await _quizCompartidoService.RevocarComparticionAsync(id, userId);

            if (resultado.IsSuccess)
            {
                TempData["Success"] = "Compartición revocada exitosamente";
            }
            else
            {
                TempData["Error"] = resultado.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar compartición {Id}", id);
            TempData["Error"] = "Error al revocar la compartición";
            return RedirectToAction(nameof(Index));
        }
    }
}
