using QuizCraft.Application.Models.DTOs.Statistics;

namespace QuizCraft.Web.ViewModels.Statistics;

/// <summary>
/// ViewModel para el dashboard de estadísticas completo
/// </summary>
public class DashboardStatsViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;
    
    // Estadísticas generales
    public OverallStatsDto OverallStats { get; set; } = new();
    
    // Top materias
    public List<TopMateriaDto> TopMaterias { get; set; } = new();
    
    // Recomendaciones
    public List<RecommendationDto> Recomendaciones { get; set; } = new();
    
    // Métricas rápidas
    public QuizMetricsDto QuizMetrics { get; set; } = new();
    public FlashcardMetricsDto FlashcardMetrics { get; set; } = new();
}

/// <summary>
/// ViewModel para la página de análisis de materias
/// </summary>
public class MateriaAnalyticsViewModel
{
    public int MateriaId { get; set; }
    public MateriaProgressDetailDto MateriaProgress { get; set; } = new();
    public MasteryLevelDto MasteryLevel { get; set; } = new();
    public List<MateriaStatsDto> ComparacionMaterias { get; set; } = new();
}

/// <summary>
/// ViewModel para gráficos de desempeño
/// </summary>
public class PerformanceChartsViewModel
{
    public AccuracyRateChartDto AccuracyRate { get; set; } = new();
    public StudyTimeChartDto StudyTime { get; set; } = new();
    public WeeklyActivityChartDto WeeklyActivity { get; set; } = new();
    public HeatmapDataDto ActivityHeatmap { get; set; } = new();
    public TrendAnalysisDto TrendAnalysis { get; set; } = new();
}

/// <summary>
/// ViewModel para reporte detallado de desempeño
/// </summary>
public class PerformanceReportViewModel
{
    public PerformanceReportDto Report { get; set; } = new();
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    public string TipoExportacion { get; set; } = "PDF"; // PDF, Excel, etc.
}

/// <summary>
/// ViewModel para comparación de desempeño
/// </summary>
public class ComparisonViewModel
{
    public ComparativeStatsDto ComparativeStats { get; set; } = new();
    public List<MateriaStatsDto> MisMaterias { get; set; } = new();
    public string Interpretacion { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel para gráfico de actividad mensual
/// </summary>
public class MonthlyActivityViewModel
{
    public int Mes { get; set; }
    public int Año { get; set; }
    public MonthlyActivityChartDto ActivityData { get; set; } = new();
    public string MesNombre { get; set; } = string.Empty;
    public List<MonthlyActivityChartDto> ÚltimosTresMeses { get; set; } = new();
}

/// <summary>
/// ViewModel para datos del heatmap (visualización de actividad diaria)
/// </summary>
public class ActivityHeatmapViewModel
{
    public HeatmapDataDto HeatmapData { get; set; } = new();
    public int Meses { get; set; } = 3;
    public string Interpretacion { get; set; } = string.Empty;
    public int DiasMasActivos { get; set; }
    public int PromedioActividad { get; set; }
}

/// <summary>
/// ViewModel para recomendaciones personalizadas
/// </summary>
public class RecommendationsViewModel
{
    public List<RecommendationDto> Urgentes { get; set; } = new();
    public List<RecommendationDto> Importantes { get; set; } = new();
    public List<RecommendationDto> Sugerencias { get; set; } = new();
    public string ResumenAcciones { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel para análisis detallado de una materia
/// </summary>
public class DetailedMateriaAnalyticsViewModel
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    
    // Progreso y dominio
    public MateriaProgressDetailDto Progress { get; set; } = new();
    public MasteryLevelDto MasteryLevel { get; set; } = new();
    
    // Gráficos específicos de la materia
    public Dictionary<string, object> GraficosDatos { get; set; } = new();
    
    // Comparación con otras materias
    public List<MateriaStatsDto> OtrasMaterias { get; set; } = new();
    
    // Flashcards dificiles
    public List<(string Pregunta, double Dominio)> FlashcardsDificiles { get; set; } = new();
}

/// <summary>
/// ViewModel para estadísticas de quizzes
/// </summary>
public class QuizAnalyticsViewModel
{
    public QuizMetricsDto Metrics { get; set; } = new();
    public List<(string Nombre, double Puntuacion, DateTime Fecha)> QuizzesRecientes { get; set; } = new();
    public Dictionary<string, int> DistribucionPuntuaciones { get; set; } = new();
    public double TasaCompletacion { get; set; }
}

/// <summary>
/// ViewModel para estadísticas de flashcards
/// </summary>
public class FlashcardAnalyticsViewModel
{
    public FlashcardMetricsDto Metrics { get; set; } = new();
    public Dictionary<string, int> EstadoFlashcards { get; set; } = new(); // Nueva, Aprendida, Dificil
    public List<string> TopMateriasPendientes { get; set; } = new();
    public Dictionary<string, double> DominioPromedioPorMateria { get; set; } = new();
}

/// <summary>
/// ViewModel para análisis de tendencias a largo plazo
/// </summary>
public class LongTermTrendViewModel
{
    public TrendAnalysisDto TrendAnalysis { get; set; } = new();
    public int DiasAnalizados { get; set; } = 30;
    public string Interpretacion { get; set; } = string.Empty;
    public List<string> Predicciones { get; set; } = new();
}

/// <summary>
/// ViewModel para exportación de reportes
/// </summary>
public class ExportReportViewModel
{
    public PerformanceReportDto Report { get; set; } = new();
    public string Formato { get; set; } = "PDF"; // PDF, Excel, JSON
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public bool IncluirGraficos { get; set; } = true;
    public bool IncluirRecomendaciones { get; set; } = true;
}
