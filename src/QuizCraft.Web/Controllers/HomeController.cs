using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Web.Models;
using QuizCraft.Web.ViewModels.Home;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador principal de la aplicación QuizCraft
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// FUNC_MostrarPaginaInicio - Página principal de QuizCraft
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var model = new HomeIndexViewModel
            {
                EsUsuarioAutenticado = User.Identity?.IsAuthenticated == true
            };

            // Si el usuario está autenticado, cargar sus datos
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Obtener estadísticas del usuario
                    var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(user.Id);
                    var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(user.Id);
                    var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(user.Id);

                    model.NombreUsuario = user.NombreCompleto;
                    model.TotalMaterias = materias.Count();
                    model.TotalFlashcards = flashcards.Count();
                    model.TotalQuizzes = quizzes.Count();
                    model.QuizzesRecientes = quizzes.OrderByDescending(q => q.FechaCreacion).Take(5).ToList();
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la página de inicio");
            return View(new HomeIndexViewModel());
        }
    }

    /// <summary>
    /// FUNC_MostrarPaginaHome - Página de inicio para todos los usuarios
    /// </summary>
    public async Task<IActionResult> Home()
    {
        try
        {
            var model = new HomeIndexViewModel
            {
                EsUsuarioAutenticado = User.Identity?.IsAuthenticated == true
            };

            // Si el usuario está autenticado, cargar sus datos
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Obtener estadísticas del usuario
                    var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(user.Id);
                    var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(user.Id);
                    var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(user.Id);

                    model.NombreUsuario = user.NombreCompleto;
                    model.TotalMaterias = materias.Count();
                    model.TotalFlashcards = flashcards.Count();
                    model.TotalQuizzes = quizzes.Count();
                    model.QuizzesRecientes = quizzes.OrderByDescending(q => q.FechaCreacion).Take(5).ToList();
                }
            }

            return View("Index", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la página de inicio");
            return View("Index", new HomeIndexViewModel());
        }
    }

    /// <summary>
    /// FUNC_MostrarDashboard - Dashboard del usuario autenticado
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener estadísticas dinámicas del usuario
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(user.Id);
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(user.Id);
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(user.Id);
            
            ViewBag.TotalMaterias = materias.Count();
            ViewBag.TotalFlashcards = flashcards.Count();
            ViewBag.TotalQuizzes = quizzes.Count();
            ViewBag.NombreUsuario = user.NombreCompleto;

            var model = new DashboardViewModel
            {
                NombreUsuario = user.NombreCompleto,
                FechaUltimoAcceso = user.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm") ?? "Nunca",
                
                TotalMaterias = materias.Count(),
                TotalFlashcards = flashcards.Count(),
                TotalQuizzes = quizzes.Count(),
                FlashcardsParaRevisar = flashcards.Count(f => f.ProximaRevision <= DateTime.Now),
                QuizzesRecientes = quizzes.OrderByDescending(q => q.FechaCreacion).Take(5).ToList(),
                MateriasRecientes = materias.OrderByDescending(m => m.FechaCreacion).Take(3).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el dashboard del usuario {UserId}", 
                _userManager.GetUserId(User));
            
            // En caso de error, usar valores por defecto
            ViewBag.TotalMaterias = 0;
            ViewBag.TotalFlashcards = 0;
            ViewBag.TotalQuizzes = 0;
            ViewBag.NombreUsuario = User.Identity?.Name ?? "Usuario";
            
            return View(new DashboardViewModel
            {
                NombreUsuario = User.Identity?.Name ?? "Usuario",
                FechaUltimoAcceso = "Error",
                TotalMaterias = 0,
                TotalFlashcards = 0,
                TotalQuizzes = 0,
                FlashcardsParaRevisar = 0,
                QuizzesRecientes = new List<Core.Entities.Quiz>(),
                MateriasRecientes = new List<Core.Entities.Materia>()
            });
        }
    }

    /// <summary>
    /// FUNC_MostrarAcercaDe - Página de información sobre QuizCraft
    /// </summary>
    public IActionResult About()
    {
        return View();
    }

    /// <summary>
    /// FUNC_MostrarPoliticaPrivacidad - Política de privacidad
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// FUNC_MostrarEstadisticas - Página de estadísticas del usuario
    /// </summary>
    [Authorize]
    public IActionResult Statistics()
    {
        // Por ahora, redirigir al Dashboard
        // En el futuro se puede crear una vista específica de estadísticas
        return RedirectToAction("Dashboard");
    }

    /// <summary>
    /// FUNC_MostrarPaginaError - Página de error personalizada
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
