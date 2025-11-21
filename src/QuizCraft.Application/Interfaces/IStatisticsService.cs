using QuizCraft.Application.Models.DTOs.Statistics;
using QuizCraft.Core.Entities;

namespace QuizCraft.Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de análisis y estadísticas de QuizCraft
/// Proporciona métodos para obtener análisis detallados del rendimiento del usuario
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Obtiene un resumen general de estadísticas del usuario
    /// </summary>
    Task<OverallStatsDto> GetOverallStatsAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene estadísticas de desempeño por materia
    /// </summary>
    Task<IEnumerable<MateriaStatsDto>> GetMateriaStatsAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene datos para gráfico de actividad semanal
    /// </summary>
    Task<WeeklyActivityChartDto> GetWeeklyActivityAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene datos para gráfico de actividad mensual
    /// </summary>
    Task<MonthlyActivityChartDto> GetMonthlyActivityAsync(string usuarioId, int mes, int año);
    
    /// <summary>
    /// Obtiene datos para gráfico de tasa de aciertos por materia
    /// </summary>
    Task<AccuracyRateChartDto> GetAccuracyRateChartAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene datos para gráfico de tiempo de estudio
    /// </summary>
    Task<StudyTimeChartDto> GetStudyTimeChartAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene información detallada sobre el progreso de una materia específica
    /// </summary>
    Task<MateriaProgressDetailDto> GetMateriaProgressDetailAsync(string usuarioId, int materiaId);
    
    /// <summary>
    /// Obtiene el ranking de mejores materias del usuario
    /// </summary>
    Task<IEnumerable<TopMateriaDto>> GetTopMateriasAsync(string usuarioId, int cantidad = 5);
    
    /// <summary>
    /// Obtiene métricas de quizzes completados
    /// </summary>
    Task<QuizMetricsDto> GetQuizMetricsAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene métricas de flashcards revisadas
    /// </summary>
    Task<FlashcardMetricsDto> GetFlashcardMetricsAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene datos para gráfico de heatmap (matriz de actividad diaria)
    /// </summary>
    Task<HeatmapDataDto> GetActivityHeatmapAsync(string usuarioId, int meses = 3);
    
    /// <summary>
    /// Obtiene recomendaciones basadas en el desempeño del usuario
    /// </summary>
    Task<IEnumerable<RecommendationDto>> GetRecommendationsAsync(string usuarioId);
    
    /// <summary>
    /// Calcula el nivel de dominio de una materia (novato, intermedio, avanzado, experto)
    /// </summary>
    Task<MasteryLevelDto> GetMasteryLevelAsync(string usuarioId, int materiaId);
    
    /// <summary>
    /// Obtiene un reporte detallado de rendimiento para exportar
    /// </summary>
    Task<PerformanceReportDto> GeneratePerformanceReportAsync(string usuarioId, DateTime desde, DateTime hasta);
    
    /// <summary>
    /// Obtiene tendencias de desempeño en el tiempo
    /// </summary>
    Task<TrendAnalysisDto> GetTrendAnalysisAsync(string usuarioId, int diasAtras = 30);
    
    /// <summary>
    /// Obtiene estadísticas comparativas con otros usuarios (anónimamente)
    /// </summary>
    Task<ComparativeStatsDto> GetComparativeStatsAsync(string usuarioId);
}
