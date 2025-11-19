using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
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

            // Obtener actividad reciente
            var actividadesRecientes = new List<ActividadReciente>();
            
            // Agregar resultados de quizzes recientes
            var resultadosQuizRecientes = await _unitOfWork.ResultadoQuizRepository.GetResultadosRecientesAsync(user.Id, 3);
            foreach (var resultado in resultadosQuizRecientes)
            {
                actividadesRecientes.Add(new ActividadReciente
                {
                    TipoActividad = TipoActividad.Quiz,
                    ReferenciaId = resultado.QuizId,
                    Titulo = $"Quiz completado: {resultado.Quiz?.Titulo}",
                    Descripcion = $"Puntuación: {resultado.PorcentajeAcierto:F1}%",
                    FechaActividad = resultado.FechaFinalizacion,
                    MateriaNombre = resultado.Quiz?.Materia?.Nombre,
                    MateriaColor = resultado.Quiz?.Materia?.Color,
                    Puntuacion = resultado.PorcentajeAcierto
                });
            }
            
            // Agregar estadísticas de flashcards recientes
            var estadisticasRecientes = await _unitOfWork.EstadisticaEstudioRepository.GetActividadRecienteAsync(user.Id, 7);
            foreach (var estadistica in estadisticasRecientes.Where(e => e.FlashcardsRevisadas > 0 && e.MateriaId.HasValue))
            {
                actividadesRecientes.Add(new ActividadReciente
                {
                    TipoActividad = TipoActividad.Flashcard,
                    ReferenciaId = estadistica.MateriaId.Value,
                    Titulo = "Sesión de Flashcards",
                    Descripcion = $"{estadistica.FlashcardsRevisadas} tarjetas revisadas",
                    FechaActividad = estadistica.FechaCreacion,
                    MateriaNombre = estadistica.Materia?.Nombre,
                    MateriaColor = estadistica.Materia?.Color,
                    FlashcardsRevisadas = estadistica.FlashcardsRevisadas
                });
            }
            
            // Ordenar por fecha más reciente y tomar las últimas 3
            actividadesRecientes = actividadesRecientes
                .OrderByDescending(a => a.FechaActividad)
                .Take(3)
                .ToList();

            // Calcular progreso general
            var progreso = CalcularProgreso(flashcards.ToList(), quizzes.ToList(), resultadosQuizRecientes.ToList());

            var model = new DashboardViewModel
            {
                NombreUsuario = user.NombreCompleto,
                FechaUltimoAcceso = user.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm") ?? "Nunca",
                
                TotalMaterias = materias.Count(),
                TotalFlashcards = flashcards.Count(),
                TotalQuizzes = quizzes.Count(),
                FlashcardsParaRevisar = flashcards.Count(f => f.ProximaRevision <= DateTime.Now),
                QuizzesRecientes = quizzes.OrderByDescending(q => q.FechaCreacion).Take(5).ToList(),
                MateriasRecientes = materias.OrderByDescending(m => m.FechaCreacion).Take(3).ToList(),
                ActividadesRecientes = actividadesRecientes,
                ProgresoPorcentaje = progreso.Porcentaje,
                ProgresoDescripcion = progreso.Descripcion
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
                MateriasRecientes = new List<Core.Entities.Materia>(),
                ActividadesRecientes = new List<ActividadReciente>(),
                ProgresoPorcentaje = 0.0,
                ProgresoDescripcion = "Sin datos"
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
    public async Task<IActionResult> Statistics()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener todas las estadísticas del usuario
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(user.Id);
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(user.Id);
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(user.Id);

            var model = new StatisticsViewModel
            {
                NombreUsuario = user.NombreCompleto,
                TotalMaterias = materias.Count(),
                TotalFlashcards = flashcards.Count(),
                TotalQuizzes = quizzes.Count(),
                Materias = materias.ToList(),
                QuizzesRecientes = quizzes.OrderByDescending(q => q.FechaCreacion).Take(10).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar las estadísticas del usuario");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// FUNC_MostrarPaginaError - Página de error personalizada
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    /// <summary>
    /// FUNC_CalcularProgreso - Calcula el progreso general del usuario
    /// </summary>
    private (double Porcentaje, string Descripcion) CalcularProgreso(
        List<Core.Entities.Flashcard> flashcards, 
        List<Core.Entities.Quiz> quizzes, 
        List<Core.Entities.ResultadoQuiz> resultados)
    {
        if (!flashcards.Any() && !quizzes.Any())
        {
            return (0.0, "Sin actividad de estudio");
        }

        double progresoTotal = 0.0;
        var factores = new List<double>();

        // Factor 1: Progreso de flashcards (40% del total)
        if (flashcards.Any())
        {
            var flashcardsRevisadas = flashcards.Count(f => f.UltimaRevision.HasValue);
            var porcentajeFlashcards = (double)flashcardsRevisadas / flashcards.Count * 100;
            factores.Add(porcentajeFlashcards * 0.4);
        }

        // Factor 2: Progreso de quizzes completados (35% del total)
        if (quizzes.Any())
        {
            var quizzesCompletados = resultados.Count(r => r.EstaCompletado);
            var porcentajeQuizzes = Math.Min((double)quizzesCompletados / quizzes.Count * 100, 100);
            factores.Add(porcentajeQuizzes * 0.35);
        }

        // Factor 3: Rendimiento en quizzes (25% del total)
        if (resultados.Any())
        {
            var promedioRendimiento = resultados.Average(r => r.PorcentajeAcierto);
            factores.Add(promedioRendimiento * 0.25);
        }

        // Calcular progreso total
        progresoTotal = factores.Sum();

        // Generar descripción
        string descripcion = progresoTotal switch
        {
            >= 80 => "Excelente progreso",
            >= 60 => "Buen progreso",
            >= 40 => "Progreso moderado",
            >= 20 => "Progreso inicial",
            _ => "Comenzando"
        };

        return (Math.Round(progresoTotal, 1), descripcion);
    }
}
