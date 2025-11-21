namespace QuizCraft.Application.Models.DTOs.Statistics;

/// <summary>
/// DTO para estadísticas generales del usuario
/// </summary>
public class OverallStatsDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    
    // Contadores básicos
    public int TotalMaterias { get; set; }
    public int TotalFlashcards { get; set; }
    public int TotalQuizzes { get; set; }
    public int FlashcardsRevisadas { get; set; }
    public int QuizzesCompletados { get; set; }
    
    // Métricas clave
    public double PromedioAcierto { get; set; }
    public double TasaAciertosFlashcards { get; set; }
    public double TasaAciertosQuizzes { get; set; }
    
    // Tiempo de estudio
    public int TiempoEstudioTotalMinutos { get; set; }
    public int TiempoEstudioHoy { get; set; }
    public int TiempoEstudioEstaSemana { get; set; }
    public double PromedioTiempoEstudioDiario { get; set; }
    
    // Rachas
    public int RachaEstudioActual { get; set; }
    public int MaximaRachaEstudio { get; set; }
    
    // Ranking
    public int PercentilUsuario { get; set; } // 0-100
    public string NivelGeneralMastery { get; set; } = "Novato"; // Novato, Intermedio, Avanzado, Experto
    
    // Fechas
    public DateTime FechaUltimoEstudio { get; set; }
    public DateTime FechaProximaRevision { get; set; }
}

/// <summary>
/// DTO para estadísticas de una materia específica
/// </summary>
public class MateriaStatsDto
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public string MateriaColor { get; set; } = string.Empty;
    
    public int TotalFlashcards { get; set; }
    public int FlashcardsRevisadas { get; set; }
    public int FlashcardsPendientes { get; set; }
    public double PercentajeComplecion { get; set; }
    
    public int TotalQuizzes { get; set; }
    public int QuizzesCompletados { get; set; }
    public double TasaAciertosMateria { get; set; }
    
    public int TiempoEstudioMinutos { get; set; }
    public DateTime FechaUltimaRevision { get; set; }
    
    public string NivelDominio { get; set; } = "Novato"; // Novato, Intermedio, Avanzado, Experto
}

/// <summary>
/// DTO para datos del gráfico de actividad semanal
/// </summary>
public class WeeklyActivityChartDto
{
    public List<DayActivityDto> Dias { get; set; } = new();
    public int TotalFlashcardsEstaSemana { get; set; }
    public int TotalQuizzesEstaSemana { get; set; }
    public int TotalMinutosEstudio { get; set; }
}

public class DayActivityDto
{
    public string Dia { get; set; } = string.Empty; // Lunes, Martes, etc.
    public int FlashcardsRevisadas { get; set; }
    public int QuizzesCompletados { get; set; }
    public int MinutosEstudio { get; set; }
}

/// <summary>
/// DTO para datos del gráfico de actividad mensual
/// </summary>
public class MonthlyActivityChartDto
{
    public int Mes { get; set; }
    public int Año { get; set; }
    public List<DailyActivityDto> Dias { get; set; } = new();
    public int TotalFlashcardsEsteMes { get; set; }
    public int TotalQuizzesEsteMes { get; set; }
    public int TotalMinutosEstudioEsteMes { get; set; }
}

public class DailyActivityDto
{
    public int Dia { get; set; }
    public DateTime Fecha { get; set; }
    public int FlashcardsRevisadas { get; set; }
    public int QuizzesCompletados { get; set; }
    public int MinutosEstudio { get; set; }
    public double PromedioAcierto { get; set; }
}

/// <summary>
/// DTO para gráfico de tasa de aciertos por materia
/// </summary>
public class AccuracyRateChartDto
{
    public List<MateriaAccuracyDto> Materias { get; set; } = new();
    public double PromedioGeneralAciertos { get; set; }
}

public class MateriaAccuracyDto
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public string MateriaColor { get; set; } = string.Empty;
    public double TasaAciertos { get; set; }
    public int CantidadRespuestas { get; set; }
}

/// <summary>
/// DTO para gráfico de tiempo de estudio
/// </summary>
public class StudyTimeChartDto
{
    public List<MateriaStudyTimeDto> Materias { get; set; } = new();
    public int TotalMinutos { get; set; }
    public double PromedioMinutosPorMateria { get; set; }
}

public class MateriaStudyTimeDto
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public string MateriaColor { get; set; } = string.Empty;
    public int MinutosEstudio { get; set; }
    public double PercentajeDelTotal { get; set; }
}

/// <summary>
/// DTO para detalles del progreso de una materia
/// </summary>
public class MateriaProgressDetailDto
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public string MateriaColor { get; set; } = string.Empty;
    
    // Flashcards
    public int TotalFlashcards { get; set; }
    public int FlashcardsNuevas { get; set; }
    public int FlashcardsAprendidas { get; set; }
    public int FlashcardsEnRevision { get; set; }
    public int FlashcardsDificiles { get; set; }
    
    // Quizzes
    public int TotalQuizzes { get; set; }
    public int QuizzesCompletados { get; set; }
    public double PromedioQuizzes { get; set; }
    public int QuizzesAprobados { get; set; }
    
    // Tiempo
    public int TiempoTotalMinutos { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaUltimaActualizacion { get; set; }
    
    // Predicciones
    public DateTime FechaEstimadaDominio { get; set; }
    public string NivelDominioActual { get; set; } = "Novato";
}

/// <summary>
/// DTO para mejores materias del usuario
/// </summary>
public class TopMateriaDto
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public double TasaAciertos { get; set; }
    public int TiempoEstudioMinutos { get; set; }
    public string NivelDominio { get; set; } = string.Empty;
    public int Ranking { get; set; } // Posición en el ranking
}

/// <summary>
/// DTO para métricas de quizzes
/// </summary>
public class QuizMetricsDto
{
    public int TotalQuizzes { get; set; }
    public int QuizzesCompletados { get; set; }
    public int QuizzesAbandonados { get; set; }
    
    public double PromedioAciertosGeneral { get; set; }
    public double TasaAprobacion { get; set; }
    
    public int TiempoPromedioPorQuiz { get; set; }
    public int TiempoMinimo { get; set; }
    public int TiempoMaximo { get; set; }
    
    public double MejorPuntuacion { get; set; }
    public double PeorPuntuacion { get; set; }
    
    public DateTime FechaUltimoQuiz { get; set; }
}

/// <summary>
/// DTO para métricas de flashcards
/// </summary>
public class FlashcardMetricsDto
{
    public int TotalFlashcards { get; set; }
    public int FlashcardsRevisadas { get; set; }
    public int FlashcardsPendientes { get; set; }
    
    public double TasaAciertosFlashcards { get; set; }
    public double PercentajeComplecion { get; set; }
    
    public int RevisionesPromedio { get; set; }
    public int RevisionesTotales { get; set; }
    
    public int TiempoEstudioMinutos { get; set; }
    public int TiempoPromedioPorFlashcard { get; set; }
    
    public DateTime FechaUltimaRevision { get; set; }
}

/// <summary>
/// DTO para datos del heatmap de actividad
/// </summary>
public class HeatmapDataDto
{
    public List<HeatmapDayDto> Dias { get; set; } = new();
    public int MaxActividad { get; set; }
    public int MinActividad { get; set; }
}

public class HeatmapDayDto
{
    public DateTime Fecha { get; set; }
    public int Actividad { get; set; } // 0-10, nivel de intensidad de estudio
    public string DiaString { get; set; } = string.Empty;
}

/// <summary>
/// DTO para recomendaciones personalizadas
/// </summary>
public class RecommendationDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // "urgente", "importante", "sugerencia"
    public string Categoria { get; set; } = string.Empty; // "materia", "quiz", "flashcard", "estudio"
    
    public int? MateriaId { get; set; }
    public string? MateriaNombre { get; set; }
    
    public int Prioridad { get; set; } // 1-5
    public string AccionRecomendada { get; set; } = string.Empty;
}

/// <summary>
/// DTO para nivel de dominio de una materia
/// </summary>
public class MasteryLevelDto
{
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    
    public string Nivel { get; set; } = "Novato"; // Novato, Intermedio, Avanzado, Experto
    public double Puntuacion { get; set; } // 0-100
    public double ProgresoHaciaProximo { get; set; } // 0-100
    
    public int FlashcardsRequeridas { get; set; }
    public int FlashcardsCompletadas { get; set; }
    
    public double TasaAciertosRequerida { get; set; }
    public double TasaAciertosActual { get; set; }
    
    public DateTime FechaEstimadaProximoNivel { get; set; }
}

/// <summary>
/// DTO para reporte de desempeño
/// </summary>
public class PerformanceReportDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    
    public OverallStatsDto EstadisticasGenerales { get; set; } = new();
    public List<MateriaStatsDto> EstadisticasPorMateria { get; set; } = new();
    public QuizMetricsDto MetricasQuizzes { get; set; } = new();
    public FlashcardMetricsDto MetricasFlashcards { get; set; } = new();
    
    public string ResumenEjecutivo { get; set; } = string.Empty;
    public List<string> Fortalezas { get; set; } = new();
    public List<string> AreasDeImprovement { get; set; } = new();
}

/// <summary>
/// DTO para análisis de tendencias
/// </summary>
public class TrendAnalysisDto
{
    public List<TrendDataPointDto> Datos { get; set; } = new();
    
    public double TendenciaTasaAciertos { get; set; } // +5.2, -3.1, 0.0
    public string DescripcionTendencia { get; set; } = string.Empty; // "mejorando", "empeorando", "estable"
    
    public double TendenciaActividad { get; set; }
    public string DescripcionActividad { get; set; } = string.Empty;
    
    public int DiasSinEstudio { get; set; }
    public int ConsecultivosEstudio { get; set; }
}

public class TrendDataPointDto
{
    public DateTime Fecha { get; set; }
    public double TasaAciertos { get; set; }
    public int MinutosEstudio { get; set; }
    public int FlashcardsRevisadas { get; set; }
}

/// <summary>
/// DTO para estadísticas comparativas con otros usuarios
/// </summary>
public class ComparativeStatsDto
{
    public double PromedioUsuarioAciertos { get; set; }
    public double PromedioGlobalAciertos { get; set; }
    public double DiferenciaAciertos { get; set; }
    
    public int TiempoEstudioUsuario { get; set; }
    public int PromedioGlobalTiempoEstudio { get; set; }
    
    public int PercentilAciertos { get; set; } // 0-100
    public int PercentilActividad { get; set; }
    
    public string Posicion { get; set; } = string.Empty; // "Top 10%", "Top 25%", etc.
    public string Clasificacion { get; set; } = string.Empty; // "Excelente", "Muy bueno", etc.
}
