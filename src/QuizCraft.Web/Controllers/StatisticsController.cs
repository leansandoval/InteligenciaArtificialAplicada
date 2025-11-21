using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.Interfaces;
using QuizCraft.Core.Entities;
using QuizCraft.Web.ViewModels.Statistics;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador de estadísticas y análisis de QuizCraft
/// Proporciona acceso a dashboards, gráficos y reportes detallados
/// </summary>
[Authorize]
[Route("Statistics")]
public class StatisticsController : Controller
{
    private readonly IStatisticsService _statisticsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        IStatisticsService statisticsService,
        UserManager<ApplicationUser> userManager,
        ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard principal de estadísticas
    /// </summary>
    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var overallStats = await _statisticsService.GetOverallStatsAsync(user.Id);
            var topMaterias = await _statisticsService.GetTopMateriasAsync(user.Id, 5);
            var recomendaciones = await _statisticsService.GetRecommendationsAsync(user.Id);
            var quizMetrics = await _statisticsService.GetQuizMetricsAsync(user.Id);
            var flashcardMetrics = await _statisticsService.GetFlashcardMetricsAsync(user.Id);

            var model = new DashboardStatsViewModel
            {
                NombreUsuario = user.NombreCompleto,
                OverallStats = overallStats,
                TopMaterias = topMaterias.ToList(),
                Recomendaciones = recomendaciones.ToList(),
                QuizMetrics = quizMetrics,
                FlashcardMetrics = flashcardMetrics
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar dashboard de estadísticas");
            return RedirectToAction("Dashboard", "Home");
        }
    }

    /// <summary>
    /// Página de gráficos de desempeño
    /// </summary>
    [HttpGet("Charts")]
    public async Task<IActionResult> Charts()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var accuracyRate = await _statisticsService.GetAccuracyRateChartAsync(user.Id);
            var studyTime = await _statisticsService.GetStudyTimeChartAsync(user.Id);
            var weeklyActivity = await _statisticsService.GetWeeklyActivityAsync(user.Id);
            var heatmap = await _statisticsService.GetActivityHeatmapAsync(user.Id, 3);
            var trends = await _statisticsService.GetTrendAnalysisAsync(user.Id, 30);

            var model = new PerformanceChartsViewModel
            {
                AccuracyRate = accuracyRate,
                StudyTime = studyTime,
                WeeklyActivity = weeklyActivity,
                ActivityHeatmap = heatmap,
                TrendAnalysis = trends
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar gráficos de desempeño");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Análisis por materia individual
    /// </summary>
    [HttpGet("Materia/{id}")]
    public async Task<IActionResult> MateriaAnalytics(int id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var materiaProgress = await _statisticsService.GetMateriaProgressDetailAsync(user.Id, id);
            var masteryLevel = await _statisticsService.GetMasteryLevelAsync(user.Id, id);
            var comparacion = (await _statisticsService.GetMateriaStatsAsync(user.Id)).ToList();

            var model = new MateriaAnalyticsViewModel
            {
                MateriaId = id,
                MateriaProgress = materiaProgress,
                MasteryLevel = masteryLevel,
                ComparacionMaterias = comparacion
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar análisis de materia {MateriaId}", id);
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Actividad semanal
    /// </summary>
    [HttpGet("Weekly")]
    public async Task<IActionResult> WeeklyActivity()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var weeklyData = await _statisticsService.GetWeeklyActivityAsync(user.Id);

            ViewBag.Title = "Actividad Semanal";
            return View(weeklyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar actividad semanal");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Actividad mensual
    /// </summary>
    [HttpGet("Monthly")]
    public async Task<IActionResult> MonthlyActivity(int? mes = null, int? año = null)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            mes = mes ?? DateTime.Now.Month;
            año = año ?? DateTime.Now.Year;

            var monthlyData = await _statisticsService.GetMonthlyActivityAsync(user.Id, mes.Value, año.Value);
            var diasEnMes = new Dictionary<int, string>
            {
                { 1, "Enero" }, { 2, "Febrero" }, { 3, "Marzo" },
                { 4, "Abril" }, { 5, "Mayo" }, { 6, "Junio" },
                { 7, "Julio" }, { 8, "Agosto" }, { 9, "Septiembre" },
                { 10, "Octubre" }, { 11, "Noviembre" }, { 12, "Diciembre" }
            };

            var model = new MonthlyActivityViewModel
            {
                Mes = mes.Value,
                Año = año.Value,
                ActivityData = monthlyData,
                MesNombre = diasEnMes[mes.Value]
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar actividad mensual");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Heatmap de actividad diaria
    /// </summary>
    [HttpGet("Heatmap")]
    public async Task<IActionResult> ActivityHeatmap(int meses = 3)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var heatmapData = await _statisticsService.GetActivityHeatmapAsync(user.Id, meses);

            var model = new ActivityHeatmapViewModel
            {
                HeatmapData = heatmapData,
                Meses = meses,
                PromedioActividad = heatmapData.Dias.Sum(d => d.Actividad) / heatmapData.Dias.Count,
                DiasMasActivos = heatmapData.Dias.Count(d => d.Actividad >= 7)
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar heatmap de actividad");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Recomendaciones personalizadas
    /// </summary>
    [HttpGet("Recommendations")]
    public async Task<IActionResult> Recommendations()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var recomendaciones = await _statisticsService.GetRecommendationsAsync(user.Id);

            var model = new RecommendationsViewModel
            {
                Urgentes = recomendaciones.Where(r => r.Tipo == "urgente").ToList(),
                Importantes = recomendaciones.Where(r => r.Tipo == "importante").ToList(),
                Sugerencias = recomendaciones.Where(r => r.Tipo == "sugerencia").ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar recomendaciones");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Análisis de quizzes
    /// </summary>
    [HttpGet("Quizzes")]
    public async Task<IActionResult> QuizAnalytics()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var quizMetrics = await _statisticsService.GetQuizMetricsAsync(user.Id);

            var model = new QuizAnalyticsViewModel
            {
                Metrics = quizMetrics,
                TasaCompletacion = quizMetrics.TotalQuizzes > 0 
                    ? (double)quizMetrics.QuizzesCompletados / quizMetrics.TotalQuizzes * 100
                    : 0
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar análisis de quizzes");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Análisis de flashcards
    /// </summary>
    [HttpGet("Flashcards")]
    public async Task<IActionResult> FlashcardAnalytics()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var flashcardMetrics = await _statisticsService.GetFlashcardMetricsAsync(user.Id);

            var model = new FlashcardAnalyticsViewModel
            {
                Metrics = flashcardMetrics,
                EstadoFlashcards = new Dictionary<string, int>
                {
                    { "Nuevas", (int)(flashcardMetrics.FlashcardsPendientes * 0.3) },
                    { "Aprendidas", (int)(flashcardMetrics.FlashcardsRevisadas * 0.7) },
                    { "Difíciles", flashcardMetrics.FlashcardsPendientes }
                }
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar análisis de flashcards");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Análisis de tendencias
    /// </summary>
    [HttpGet("Trends")]
    public async Task<IActionResult> TrendAnalysis(int dias = 30)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var trendData = await _statisticsService.GetTrendAnalysisAsync(user.Id, dias);

            var model = new LongTermTrendViewModel
            {
                TrendAnalysis = trendData,
                DiasAnalizados = dias,
                Interpretacion = GenerarInterpretacionTendencias(trendData),
                Predicciones = new List<string>
                {
                    $"Tu tasa de aciertos está {trendData.DescripcionTendencia}",
                    $"Has mantenido una consistencia de {trendData.ConsecultivosEstudio} días",
                    "Continúa con tu rutina actual para mantener el momentum"
                }
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar análisis de tendencias");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Comparación con otros usuarios (anónima)
    /// </summary>
    [HttpGet("Comparison")]
    public async Task<IActionResult> Comparison()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var comparativeStats = await _statisticsService.GetComparativeStatsAsync(user.Id);
            var misMaterias = (await _statisticsService.GetMateriaStatsAsync(user.Id)).ToList();

            var model = new ComparisonViewModel
            {
                ComparativeStats = comparativeStats,
                MisMaterias = misMaterias,
                Interpretacion = GenerarInterpretacionComparativa(comparativeStats)
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar comparación de desempeño");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Reporte detallado de desempeño
    /// </summary>
    [HttpGet("Report")]
    public async Task<IActionResult> PerformanceReport(DateTime? desde = null, DateTime? hasta = null)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            desde = desde ?? DateTime.Today.AddMonths(-3);
            hasta = hasta ?? DateTime.Today;

            var report = await _statisticsService.GeneratePerformanceReportAsync(user.Id, desde.Value, hasta.Value);

            var model = new PerformanceReportViewModel
            {
                Report = report,
                FechaGeneracion = DateTime.Now
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de desempeño");
            return RedirectToAction("Dashboard");
        }
    }

    /// <summary>
    /// Exportar reporte en diferentes formatos
    /// </summary>
    [HttpPost("Export")]
    public async Task<IActionResult> ExportReport(string formato = "PDF", DateTime? desde = null, DateTime? hasta = null)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            desde = desde ?? DateTime.Today.AddMonths(-3);
            hasta = hasta ?? DateTime.Today;

            var report = await _statisticsService.GeneratePerformanceReportAsync(user.Id, desde.Value, hasta.Value);

            // Aquí iría la lógica de exportación según el formato
            // Por ahora, retornamos el reporte en JSON
            return Json(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte");
            return StatusCode(500, "Error al exportar reporte");
        }
    }

    /// <summary>
    /// API: Obtener datos para gráfico de tarta (pie chart)
    /// </summary>
    [HttpGet("api/accuracy-pie")]
    public async Task<IActionResult> GetAccuracyPieData()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var accuracyData = await _statisticsService.GetAccuracyRateChartAsync(user.Id);

            var datos = new
            {
                labels = accuracyData.Materias.Select(m => m.MateriaNombre).ToList(),
                data = accuracyData.Materias.Select(m => m.TasaAciertos).ToList(),
                backgroundColor = accuracyData.Materias.Select(m => m.MateriaColor).ToList()
            };

            return Json(datos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos para gráfico de tarta");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// API: Obtener datos para gráfico de barras de tiempo de estudio
    /// </summary>
    [HttpGet("api/study-time-bar")]
    public async Task<IActionResult> GetStudyTimeBarData()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var studyTimeData = await _statisticsService.GetStudyTimeChartAsync(user.Id);

            var datos = new
            {
                labels = studyTimeData.Materias.Select(m => m.MateriaNombre).ToList(),
                data = new
                {
                    label = "Minutos de Estudio",
                    data = studyTimeData.Materias.Select(m => m.MinutosEstudio).ToList(),
                    backgroundColor = studyTimeData.Materias.Select(m => m.MateriaColor).ToList()
                }
            };

            return Json(datos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos para gráfico de barras");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// API: Obtener datos para gráfico de línea de actividad semanal
    /// </summary>
    [HttpGet("api/weekly-line")]
    public async Task<IActionResult> GetWeeklyLineData()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var weeklyData = await _statisticsService.GetWeeklyActivityAsync(user.Id);

            var datos = new
            {
                labels = weeklyData.Dias.Select(d => d.Dia).ToList(),
                datasets = new[]
                {
                    new
                    {
                        label = "Flashcards Revisadas",
                        data = weeklyData.Dias.Select(d => d.FlashcardsRevisadas).ToList(),
                        borderColor = "#007bff",
                        fill = false
                    },
                    new
                    {
                        label = "Quizzes Completados",
                        data = weeklyData.Dias.Select(d => d.QuizzesCompletados).ToList(),
                        borderColor = "#28a745",
                        fill = false
                    }
                }
            };

            return Json(datos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos para gráfico semanal");
            return StatusCode(500);
        }
    }

    // Métodos auxiliares privados

    private string GenerarInterpretacionTendencias(QuizCraft.Application.Models.DTOs.Statistics.TrendAnalysisDto trends)
    {
        var interpretacion = $"Durante los últimos {trends.Datos.Count} días, tu desempeño está {trends.DescripcionTendencia}. ";
        
        if (trends.TendenciaTasaAciertos > 0)
            interpretacion += $"Tu tasa de aciertos ha mejorado en {trends.TendenciaTasaAciertos:F1} puntos. ";
        else if (trends.TendenciaTasaAciertos < 0)
            interpretacion += $"Tu tasa de aciertos ha bajado {Math.Abs(trends.TendenciaTasaAciertos):F1} puntos. ";
        else
            interpretacion += "Tu tasa de aciertos se ha mantenido estable. ";

        if (trends.ConsecultivosEstudio >= 7)
            interpretacion += "¡Excelente consistencia en tu rutina de estudio!";
        else if (trends.ConsecultivosEstudio >= 3)
            interpretacion += "Buena consistencia en tu estudio.";
        else
            interpretacion += "Intenta mantener una rutina más consistente.";

        return interpretacion;
    }

    private string GenerarInterpretacionComparativa(QuizCraft.Application.Models.DTOs.Statistics.ComparativeStatsDto stats)
    {
        var interpretacion = $"Tu desempeño está en el {stats.PercentilAciertos}º percentil. ";
        
        if (stats.DiferenciaAciertos > 0)
            interpretacion += $"Estás {Math.Abs(stats.DiferenciaAciertos):F1} puntos por encima del promedio. ¡Excelente trabajo! ";
        else
            interpretacion += $"Estás {Math.Abs(stats.DiferenciaAciertos):F1} puntos por debajo del promedio. Hay espacio para mejora. ";

        interpretacion += $"Tu clasificación es: {stats.Clasificacion}";

        return interpretacion;
    }
}
