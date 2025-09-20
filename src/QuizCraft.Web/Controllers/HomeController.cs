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
            var model = new HomeIndexViewModel();

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.NombreUsuario = user.NombreCompleto;
                    model.EsUsuarioAutenticado = true;
                    
                    // Por ahora, usar valores de ejemplo hasta que arreglemos el UnitOfWork
                    model.TotalMaterias = 0;
                    model.TotalFlashcards = 0;
                    model.QuizzesRecientes = new List<Core.Entities.Quiz>();
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

            var model = new DashboardViewModel
            {
                NombreUsuario = user.NombreCompleto,
                FechaUltimoAcceso = user.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm") ?? "Nunca",
                
                // Por ahora, usar valores de ejemplo hasta que arreglemos el UnitOfWork
                TotalMaterias = 0,
                TotalFlashcards = 0,
                FlashcardsParaRevisar = 0,
                QuizzesRecientes = new List<Core.Entities.Quiz>(),
                MateriasRecientes = new List<Core.Entities.Materia>()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el dashboard del usuario {UserId}", 
                _userManager.GetUserId(User));
            return RedirectToAction(nameof(Index));
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
    /// FUNC_MostrarPaginaError - Página de error personalizada
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
