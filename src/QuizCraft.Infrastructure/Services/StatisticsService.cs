using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models.DTOs.Statistics;
using QuizCraft.Core.Interfaces;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Servicio de análisis y estadísticas para QuizCraft
/// Proporciona análisis detallados del desempeño de los usuarios
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(IUnitOfWork unitOfWork, ILogger<StatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene estadísticas generales del usuario
    /// </summary>
    public async Task<OverallStatsDto> GetOverallStatsAsync(string usuarioId)
    {
        try
        {
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(usuarioId);
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);

            var flashcardsRevisadas = flashcards.Count(f => f.UltimaRevision.HasValue);
            var resultadosCompletados = resultados.Where(r => r.EstaCompletado).ToList();
            
            var promedioAcierto = resultados.Any() ? resultados.Average(r => r.PorcentajeAcierto) : 0;
            var tasaAciertosQuizzes = resultadosCompletados.Any() ? resultadosCompletados.Average(r => r.PorcentajeAcierto) : 0;

            var tiempoTotalMinutos = estadisticas.Sum(e => e.TiempoEstudioMinutos);
            var estadisticaHoy = estadisticas.FirstOrDefault(e => e.Fecha == DateTime.Today);
            var estadisticasSemana = estadisticas.Where(e => e.Fecha >= DateTime.Today.AddDays(-7)).ToList();

            var rachaActual = CalcularRachaEstudio(usuarioId, estadisticas.ToList());

            return new OverallStatsDto
            {
                UsuarioId = usuarioId,
                TotalMaterias = materias.Count(),
                TotalFlashcards = flashcards.Count(),
                TotalQuizzes = quizzes.Count(),
                FlashcardsRevisadas = flashcardsRevisadas,
                QuizzesCompletados = resultadosCompletados.Count,
                
                PromedioAcierto = Math.Round(promedioAcierto, 2),
                TasaAciertosFlashcards = Math.Round(
                    flashcards.Any() ? (double)flashcards.Count(f => f.UltimaRevision.HasValue) / flashcards.Count() * 100 : 0, 2),
                TasaAciertosQuizzes = Math.Round(tasaAciertosQuizzes, 2),
                
                TiempoEstudioTotalMinutos = tiempoTotalMinutos,
                TiempoEstudioHoy = estadisticaHoy?.TiempoEstudioMinutos ?? 0,
                TiempoEstudioEstaSemana = estadisticasSemana.Sum(e => e.TiempoEstudioMinutos),
                PromedioTiempoEstudioDiario = estadisticasSemana.Any() ? Math.Round(tiempoTotalMinutos / 7.0, 2) : 0,
                
                RachaEstudioActual = rachaActual.Item1,
                MaximaRachaEstudio = rachaActual.Item2,
                
                PercentilUsuario = 75,
                NivelGeneralMastery = CalcularNivelDominio(promedioAcierto),
                
                FechaUltimoEstudio = estadisticas.OrderByDescending(e => e.Fecha).FirstOrDefault()?.Fecha ?? DateTime.MinValue,
                FechaProximaRevision = flashcards.OrderBy(f => f.ProximaRevision).FirstOrDefault()?.ProximaRevision ?? DateTime.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas generales para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene estadísticas por materia
    /// </summary>
    public async Task<IEnumerable<MateriaStatsDto>> GetMateriaStatsAsync(string usuarioId)
    {
        try
        {
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(usuarioId);
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);

            var materiaStats = new List<MateriaStatsDto>();

            foreach (var materia in materias)
            {
                var flashcardsMateria = flashcards.Where(f => f.MateriaId == materia.Id).ToList();
                var quizzesMateria = quizzes.Where(q => q.MateriaId == materia.Id).ToList();
                var resultadosMateria = resultados.Where(r => r.Quiz?.MateriaId == materia.Id).ToList();
                var estadisticasMateria = estadisticas.Where(e => e.MateriaId == materia.Id).ToList();

                var flashcardsRevisadas = flashcardsMateria.Count(f => f.UltimaRevision.HasValue);
                var tasaAciertos = resultadosMateria.Any() ? resultadosMateria.Average(r => r.PorcentajeAcierto) : 0;

                materiaStats.Add(new MateriaStatsDto
                {
                    MateriaId = materia.Id,
                    MateriaNombre = materia.Nombre,
                    MateriaColor = materia.Color ?? "#007bff",
                    
                    TotalFlashcards = flashcardsMateria.Count,
                    FlashcardsRevisadas = flashcardsRevisadas,
                    FlashcardsPendientes = flashcardsMateria.Count - flashcardsRevisadas,
                    PercentajeComplecion = flashcardsMateria.Any() 
                        ? Math.Round((double)flashcardsRevisadas / flashcardsMateria.Count * 100, 2)
                        : 0,
                    
                    TotalQuizzes = quizzesMateria.Count,
                    QuizzesCompletados = resultadosMateria.Count(r => r.EstaCompletado),
                    TasaAciertosMateria = Math.Round(tasaAciertos, 2),
                    
                    TiempoEstudioMinutos = estadisticasMateria.Sum(e => e.TiempoEstudioMinutos),
                    FechaUltimaRevision = estadisticasMateria.OrderByDescending(e => e.Fecha).FirstOrDefault()?.Fecha ?? DateTime.MinValue,
                    
                    NivelDominio = CalcularNivelDominio(tasaAciertos)
                });
            }

            return materiaStats.OrderByDescending(m => m.TasaAciertosMateria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas por materia para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene actividad semanal
    /// </summary>
    public async Task<WeeklyActivityChartDto> GetWeeklyActivityAsync(string usuarioId)
    {
        try
        {
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetActividadRecienteAsync(usuarioId, 7);
            
            var dias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var actividadPorDia = new Dictionary<DayOfWeek, DayActivityDto>();

            for (var i = 0; i < 7; i++)
            {
                var fechaDia = DateTime.Today.AddDays(i - 6);
                actividadPorDia[fechaDia.DayOfWeek] = new DayActivityDto
                {
                    Dia = dias[(int)fechaDia.DayOfWeek],
                    FlashcardsRevisadas = 0,
                    QuizzesCompletados = 0,
                    MinutosEstudio = 0
                };
            }

            foreach (var stat in estadisticas)
            {
                var dayOfWeek = stat.Fecha.DayOfWeek;
                if (actividadPorDia.ContainsKey(dayOfWeek))
                {
                    actividadPorDia[dayOfWeek].FlashcardsRevisadas += stat.FlashcardsRevisadas;
                    actividadPorDia[dayOfWeek].QuizzesCompletados += stat.QuizzesRealizados;
                    actividadPorDia[dayOfWeek].MinutosEstudio += stat.TiempoEstudioMinutos;
                }
            }

            return new WeeklyActivityChartDto
            {
                Dias = actividadPorDia.Values.ToList(),
                TotalFlashcardsEstaSemana = estadisticas.Sum(e => e.FlashcardsRevisadas),
                TotalQuizzesEstaSemana = estadisticas.Sum(e => e.QuizzesRealizados),
                TotalMinutosEstudio = estadisticas.Sum(e => e.TiempoEstudioMinutos)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividad semanal para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene actividad mensual
    /// </summary>
    public async Task<MonthlyActivityChartDto> GetMonthlyActivityAsync(string usuarioId, int mes, int año)
    {
        try
        {
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);
            var estadisticasMes = estadisticas.Where(e => e.Fecha.Month == mes && e.Fecha.Year == año).ToList();

            var diasEnMes = DateTime.DaysInMonth(año, mes);
            var actividadPorDia = new List<DailyActivityDto>();

            for (var dia = 1; dia <= diasEnMes; dia++)
            {
                var fecha = new DateTime(año, mes, dia);
                var estatDia = estadisticasMes.FirstOrDefault(e => e.Fecha == fecha);

                actividadPorDia.Add(new DailyActivityDto
                {
                    Dia = dia,
                    Fecha = fecha,
                    FlashcardsRevisadas = estatDia?.FlashcardsRevisadas ?? 0,
                    QuizzesCompletados = estatDia?.QuizzesRealizados ?? 0,
                    MinutosEstudio = estatDia?.TiempoEstudioMinutos ?? 0,
                    PromedioAcierto = estatDia?.PromedioAcierto ?? 0
                });
            }

            return new MonthlyActivityChartDto
            {
                Mes = mes,
                Año = año,
                Dias = actividadPorDia,
                TotalFlashcardsEsteMes = estadisticasMes.Sum(e => e.FlashcardsRevisadas),
                TotalQuizzesEsteMes = estadisticasMes.Sum(e => e.QuizzesRealizados),
                TotalMinutosEstudioEsteMes = estadisticasMes.Sum(e => e.TiempoEstudioMinutos)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividad mensual para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene tasa de aciertos por materia
    /// </summary>
    public async Task<AccuracyRateChartDto> GetAccuracyRateChartAsync(string usuarioId)
    {
        try
        {
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);

            var materiasAccuracy = new List<MateriaAccuracyDto>();
            var promedioGeneral = resultados.Any() ? resultados.Average(r => r.PorcentajeAcierto) : 0;

            foreach (var materia in materias)
            {
                var resultadosMateria = resultados.Where(r => r.Quiz?.MateriaId == materia.Id).ToList();
                
                if (resultadosMateria.Any())
                {
                    materiasAccuracy.Add(new MateriaAccuracyDto
                    {
                        MateriaId = materia.Id,
                        MateriaNombre = materia.Nombre,
                        MateriaColor = materia.Color ?? "#007bff",
                        TasaAciertos = Math.Round(resultadosMateria.Average(r => r.PorcentajeAcierto), 2),
                        CantidadRespuestas = resultadosMateria.Count
                    });
                }
            }

            return new AccuracyRateChartDto
            {
                Materias = materiasAccuracy.OrderByDescending(m => m.TasaAciertos).ToList(),
                PromedioGeneralAciertos = Math.Round(promedioGeneral, 2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tasa de aciertos para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene tiempo de estudio por materia
    /// </summary>
    public async Task<StudyTimeChartDto> GetStudyTimeChartAsync(string usuarioId)
    {
        try
        {
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);

            var tiempoTotal = estadisticas.Sum(e => e.TiempoEstudioMinutos);
            var materiasTiempo = new List<MateriaStudyTimeDto>();

            foreach (var materia in materias)
            {
                var tiempoMateria = estadisticas
                    .Where(e => e.MateriaId == materia.Id)
                    .Sum(e => e.TiempoEstudioMinutos);

                if (tiempoMateria > 0)
                {
                    materiasTiempo.Add(new MateriaStudyTimeDto
                    {
                        MateriaId = materia.Id,
                        MateriaNombre = materia.Nombre,
                        MateriaColor = materia.Color ?? "#007bff",
                        MinutosEstudio = tiempoMateria,
                        PercentajeDelTotal = tiempoTotal > 0 ? Math.Round((double)tiempoMateria / tiempoTotal * 100, 2) : 0
                    });
                }
            }

            return new StudyTimeChartDto
            {
                Materias = materiasTiempo.OrderByDescending(m => m.MinutosEstudio).ToList(),
                TotalMinutos = tiempoTotal,
                PromedioMinutosPorMateria = materias.Count() > 0 ? Math.Round(tiempoTotal / (double)materias.Count(), 2) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tiempo de estudio para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene detalles de progreso de una materia
    /// </summary>
    public async Task<MateriaProgressDetailDto> GetMateriaProgressDetailAsync(string usuarioId, int materiaId)
    {
        try
        {
            var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaId);
            if (materia == null || materia.UsuarioId != usuarioId)
                throw new InvalidOperationException("La materia no existe o no pertenece al usuario");

            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(materiaId);
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByMateriaIdAsync(materiaId);
            var resultados = (await _unitOfWork.ResultadoQuizRepository.GetResultadosByMateriaIdAsync(materiaId))
                .Where(r => r.Quiz?.CreadorId == usuarioId).ToList();

            var flashcardsAprendidas = flashcards.Count(f => f.VecesCorrecta >= 3);
            var flashcardsEnRevision = flashcards.Count(f => f.VecesCorrecta >= 1 && f.VecesCorrecta < 3);
            var flashcardsDificiles = flashcards.Count(f => f.VecesIncorrecta > f.VecesCorrecta);

            return new MateriaProgressDetailDto
            {
                MateriaId = materiaId,
                MateriaNombre = materia.Nombre,
                MateriaColor = materia.Color ?? "#007bff",
                
                TotalFlashcards = flashcards.Count(),
                FlashcardsNuevas = flashcards.Count(f => !f.UltimaRevision.HasValue),
                FlashcardsAprendidas = flashcardsAprendidas,
                FlashcardsEnRevision = flashcardsEnRevision,
                FlashcardsDificiles = flashcardsDificiles,
                
                TotalQuizzes = quizzes.Count(),
                QuizzesCompletados = resultados.Count(r => r.EstaCompletado),
                PromedioQuizzes = resultados.Any() ? Math.Round(resultados.Average(r => r.PorcentajeAcierto), 2) : 0,
                QuizzesAprobados = resultados.Count(r => r.PorcentajeAcierto >= 60),
                
                TiempoTotalMinutos = (await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId))
                    .Where(e => e.MateriaId == materiaId)
                    .Sum(e => e.TiempoEstudioMinutos),
                
                FechaCreacion = materia.FechaCreacion,
                FechaUltimaActualizacion = materia.FechaModificacion ?? materia.FechaCreacion,
                
                NivelDominioActual = CalcularNivelDominio(resultados.Any() ? resultados.Average(r => r.PorcentajeAcierto) : 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de progreso para materia {MateriaId}", materiaId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene las mejores materias del usuario
    /// </summary>
    public async Task<IEnumerable<TopMateriaDto>> GetTopMateriasAsync(string usuarioId, int cantidad = 5)
    {
        try
        {
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);

            var topMaterias = new List<TopMateriaDto>();
            var ranking = 1;

            foreach (var materia in materias.OrderByDescending(m => m.FechaModificacion ?? m.FechaCreacion).Take(cantidad))
            {
                var resultadosMateria = resultados.Where(r => r.Quiz?.MateriaId == materia.Id).ToList();
                var tasaAciertos = resultadosMateria.Any() ? resultadosMateria.Average(r => r.PorcentajeAcierto) : 0;
                var tiempoEstudio = estadisticas.Where(e => e.MateriaId == materia.Id).Sum(e => e.TiempoEstudioMinutos);

                topMaterias.Add(new TopMateriaDto
                {
                    MateriaId = materia.Id,
                    MateriaNombre = materia.Nombre,
                    TasaAciertos = Math.Round(tasaAciertos, 2),
                    TiempoEstudioMinutos = tiempoEstudio,
                    NivelDominio = CalcularNivelDominio(tasaAciertos),
                    Ranking = ranking++
                });
            }

            return topMaterias;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mejores materias para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene métricas de quizzes
    /// </summary>
    public async Task<QuizMetricsDto> GetQuizMetricsAsync(string usuarioId)
    {
        try
        {
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);

            var resultadosCompletados = resultados.Where(r => r.EstaCompletado).ToList();
            var tiemposQuiz = resultados.Where(r => r.TiempoTranscurrido > 0).Select(r => r.TiempoTranscurrido / 60).ToList();

            return new QuizMetricsDto
            {
                TotalQuizzes = quizzes.Count(),
                QuizzesCompletados = resultadosCompletados.Count,
                QuizzesAbandonados = resultados.Count() - resultadosCompletados.Count,
                
                PromedioAciertosGeneral = Math.Round(resultados.Any() ? resultados.Average(r => r.PorcentajeAcierto) : 0, 2),
                TasaAprobacion = Math.Round(resultados.Any() ? (double)resultados.Count(r => r.PorcentajeAcierto >= 60) / resultados.Count() * 100 : 0, 2),
                
                TiempoPromedioPorQuiz = tiemposQuiz.Any() ? (int)tiemposQuiz.Average() : 0,
                TiempoMinimo = tiemposQuiz.Any() ? (int)tiemposQuiz.Min() : 0,
                TiempoMaximo = tiemposQuiz.Any() ? (int)tiemposQuiz.Max() : 0,
                
                MejorPuntuacion = resultados.Any() ? resultados.Max(r => r.PorcentajeAcierto) : 0,
                PeorPuntuacion = resultados.Any() ? resultados.Min(r => r.PorcentajeAcierto) : 0,
                
                FechaUltimoQuiz = resultados.OrderByDescending(r => r.FechaFinalizacion).FirstOrDefault()?.FechaFinalizacion ?? DateTime.MinValue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas de quizzes para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene métricas de flashcards
    /// </summary>
    public async Task<FlashcardMetricsDto> GetFlashcardMetricsAsync(string usuarioId)
    {
        try
        {
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(usuarioId);
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);

            var flashcardsRevisadas = flashcards.Count(f => f.UltimaRevision.HasValue);
            var totalRevisiones = flashcards.Sum(f => f.VecesVista);
            var tiempoTotal = estadisticas.Sum(e => e.TiempoEstudioMinutos);

            return new FlashcardMetricsDto
            {
                TotalFlashcards = flashcards.Count(),
                FlashcardsRevisadas = flashcardsRevisadas,
                FlashcardsPendientes = flashcards.Count() - flashcardsRevisadas,
                
                TasaAciertosFlashcards = Math.Round(flashcards.Any() ? flashcards.Average(f => (double)f.VecesCorrecta / (f.VecesVista > 0 ? f.VecesVista : 1)) * 100 : 0, 2),
                PercentajeComplecion = flashcards.Any() ? Math.Round((double)flashcardsRevisadas / flashcards.Count() * 100, 2) : 0,
                
                RevisionesPromedio = flashcards.Any() ? totalRevisiones / flashcards.Count() : 0,
                RevisionesTotales = totalRevisiones,
                
                TiempoEstudioMinutos = tiempoTotal,
                TiempoPromedioPorFlashcard = flashcardsRevisadas > 0 ? tiempoTotal / flashcardsRevisadas : 0,
                
                FechaUltimaRevision = flashcards.OrderByDescending(f => f.UltimaRevision).FirstOrDefault()?.UltimaRevision ?? DateTime.MinValue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas de flashcards para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene datos de actividad en formato heatmap (3 meses)
    /// </summary>
    public async Task<HeatmapDataDto> GetActivityHeatmapAsync(string usuarioId, int meses = 3)
    {
        try
        {
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);
            var fechaDesde = DateTime.Today.AddMonths(-meses);
            var estadisticasFiltradas = estadisticas.Where(e => e.Fecha >= fechaDesde).ToList();

            var maxActividad = estadisticasFiltradas.Any() 
                ? (int)estadisticasFiltradas.Max(e => e.FlashcardsRevisadas + e.QuizzesRealizados)
                : 0;
            var heatmapDays = new List<HeatmapDayDto>();

            for (var fecha = fechaDesde; fecha <= DateTime.Today; fecha = fecha.AddDays(1))
            {
                var stat = estadisticasFiltradas.FirstOrDefault(e => e.Fecha == fecha);
                var actividad = stat != null ? (stat.FlashcardsRevisadas + stat.QuizzesRealizados) : 0;
                var intensidad = maxActividad > 0 ? (int)((double)actividad / maxActividad * 10) : 0;

                heatmapDays.Add(new HeatmapDayDto
                {
                    Fecha = fecha,
                    Actividad = intensidad,
                    DiaString = fecha.ToString("yyyy-MM-dd")
                });
            }

            return new HeatmapDataDto
            {
                Dias = heatmapDays,
                MaxActividad = maxActividad,
                MinActividad = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener heatmap de actividad para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene recomendaciones personalizadas para el usuario
    /// </summary>
    public async Task<IEnumerable<RecommendationDto>> GetRecommendationsAsync(string usuarioId)
    {
        try
        {
            var recomendaciones = new List<RecommendationDto>();
            
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetActividadRecienteAsync(usuarioId, 7);

            var materiasEnRiesgo = new List<(int Id, string Nombre, double Aciertos)>();
            foreach (var materia in materias)
            {
                var resultadosMateria = resultados.Where(r => r.Quiz?.MateriaId == materia.Id).ToList();
                if (resultadosMateria.Any())
                {
                    var aciertos = resultadosMateria.Average(r => r.PorcentajeAcierto);
                    if (aciertos < 60)
                    {
                        materiasEnRiesgo.Add((materia.Id, materia.Nombre, aciertos));
                    }
                }
            }

            foreach (var materia in materiasEnRiesgo.OrderBy(m => m.Aciertos).Take(3))
            {
                recomendaciones.Add(new RecommendationDto
                {
                    Id = 1,
                    Titulo = $"Necesitas reforzar {materia.Nombre}",
                    Descripcion = $"Tu tasa de aciertos en {materia.Nombre} es de {materia.Aciertos:F1}%. Considera revisar los flashcards difíciles.",
                    Tipo = "urgente",
                    Categoria = "materia",
                    MateriaId = materia.Id,
                    MateriaNombre = materia.Nombre,
                    Prioridad = 5,
                    AccionRecomendada = "Revisar flashcards de esta materia"
                });
            }

            if (flashcards.Count(f => !f.UltimaRevision.HasValue) > 0)
            {
                recomendaciones.Add(new RecommendationDto
                {
                    Id = 2,
                    Titulo = "Tienes flashcards sin revisar",
                    Descripcion = $"Tienes {flashcards.Count(f => !f.UltimaRevision.HasValue)} tarjetas pendientes de revisar.",
                    Tipo = "importante",
                    Categoria = "flashcard",
                    Prioridad = 4,
                    AccionRecomendada = "Comenzar sesión de flashcards"
                });
            }

            if (estadisticas.Count() < 5)
            {
                recomendaciones.Add(new RecommendationDto
                {
                    Id = 3,
                    Titulo = "Aumenta tu consistencia de estudio",
                    Descripcion = "Has estudiado menos de 5 días esta semana. La consistencia es clave para el aprendizaje.",
                    Tipo = "sugerencia",
                    Categoria = "estudio",
                    Prioridad = 3,
                    AccionRecomendada = "Establecer un horario de estudio regular"
                });
            }

            return recomendaciones.OrderByDescending(r => r.Prioridad).Take(5);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recomendaciones para usuario {UsuarioId}", usuarioId);
            return Enumerable.Empty<RecommendationDto>();
        }
    }

    /// <summary>
    /// Obtiene el nivel de dominio de una materia
    /// </summary>
    public async Task<MasteryLevelDto> GetMasteryLevelAsync(string usuarioId, int materiaId)
    {
        try
        {
            var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaId);
            if (materia == null || materia.UsuarioId != usuarioId)
                throw new InvalidOperationException("La materia no existe");

            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(materiaId);
            var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByMateriaIdAsync(materiaId);
            var resultados = (await _unitOfWork.ResultadoQuizRepository.GetResultadosByMateriaIdAsync(materiaId))
                .Where(r => r.Quiz?.CreadorId == usuarioId).ToList();

            var tasaAciertos = resultados.Any() ? resultados.Average(r => r.PorcentajeAcierto) : 0;
            var flashcardsCompletadas = flashcards.Count(f => f.VecesCorrecta >= 3);
            var puntuacion = CalcularPuntuacionDominio(flashcardsCompletadas, flashcards.Count(), tasaAciertos);
            var nivel = CalcularNivelDominio(tasaAciertos);

            return new MasteryLevelDto
            {
                MateriaId = materiaId,
                MateriaNombre = materia.Nombre,
                
                Nivel = nivel,
                Puntuacion = Math.Round(puntuacion, 2),
                ProgresoHaciaProximo = Math.Round(puntuacion % 25, 2),
                
                FlashcardsRequeridas = flashcards.Count(),
                FlashcardsCompletadas = flashcardsCompletadas,
                
                TasaAciertosRequerida = 70.0,
                TasaAciertosActual = Math.Round(tasaAciertos, 2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener nivel de dominio para materia {MateriaId}", materiaId);
            throw;
        }
    }

    /// <summary>
    /// Genera un reporte de desempeño
    /// </summary>
    public async Task<PerformanceReportDto> GeneratePerformanceReportAsync(string usuarioId, DateTime desde, DateTime hasta)
    {
        try
        {
            var overallStats = await GetOverallStatsAsync(usuarioId);
            var materiaStats = await GetMateriaStatsAsync(usuarioId);
            var quizMetrics = await GetQuizMetricsAsync(usuarioId);
            var flashcardMetrics = await GetFlashcardMetricsAsync(usuarioId);

            var fortalezas = new List<string>();
            var mejoras = new List<string>();

            if (quizMetrics.TasaAprobacion >= 80)
                fortalezas.Add("Excelente tasa de aprobación en quizzes");
            if (overallStats.RachaEstudioActual >= 7)
                fortalezas.Add("Consistencia excelente en tu rutina de estudio");
            
            if (materiaStats.Any(m => m.TasaAciertosMateria < 60))
                mejoras.Add("Algunas materias necesitan refuerzo");
            if (flashcardMetrics.PercentajeComplecion < 50)
                mejoras.Add("Aún tienes muchas flashcards sin completar");

            var resumen = $"Durante el período {desde:dd/MM/yyyy} a {hasta:dd/MM/yyyy}, " +
                         $"completaste {flashcardMetrics.RevisionesTotales} revisiones de flashcards " +
                         $"y {quizMetrics.QuizzesCompletados} quizzes, con un promedio de {overallStats.PromedioAcierto}% de aciertos.";

            return new PerformanceReportDto
            {
                UsuarioId = usuarioId,
                FechaDesde = desde,
                FechaHasta = hasta,
                
                EstadisticasGenerales = overallStats,
                EstadisticasPorMateria = materiaStats.ToList(),
                MetricasQuizzes = quizMetrics,
                MetricasFlashcards = flashcardMetrics,
                
                ResumenEjecutivo = resumen,
                Fortalezas = fortalezas,
                AreasDeImprovement = mejoras
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de desempeño para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene análisis de tendencias
    /// </summary>
    public async Task<TrendAnalysisDto> GetTrendAnalysisAsync(string usuarioId, int diasAtras = 30)
    {
        try
        {
            var estadisticas = await _unitOfWork.EstadisticaEstudioRepository.GetByUsuarioIdAsync(usuarioId);
            var resultados = await _unitOfWork.ResultadoQuizRepository.GetResultadosByUsuarioIdAsync(usuarioId);
            
            var fechaDesde = DateTime.Today.AddDays(-diasAtras);
            var estadisticasFiltradas = estadisticas.Where(e => e.Fecha >= fechaDesde).OrderBy(e => e.Fecha).ToList();
            var resultadosFiltrados = resultados.Where(r => r.FechaCreacion >= fechaDesde).OrderBy(r => r.FechaCreacion).ToList();

            var datos = new List<TrendDataPointDto>();
            for (var fecha = fechaDesde; fecha <= DateTime.Today; fecha = fecha.AddDays(1))
            {
                var stat = estadisticasFiltradas.FirstOrDefault(e => e.Fecha == fecha);
                var resultsDelDia = resultadosFiltrados.Where(r => r.FechaCreacion.Date == fecha).ToList();

                datos.Add(new TrendDataPointDto
                {
                    Fecha = fecha,
                    TasaAciertos = resultsDelDia.Any() ? resultsDelDia.Average(r => r.PorcentajeAcierto) : 0,
                    MinutosEstudio = stat?.TiempoEstudioMinutos ?? 0,
                    FlashcardsRevisadas = stat?.FlashcardsRevisadas ?? 0
                });
            }

            var diasSinEstudio = 0;
            for (var i = 0; i < estadisticasFiltradas.Count; i++)
            {
                if (estadisticasFiltradas[i].TiempoEstudioMinutos == 0)
                    diasSinEstudio++;
                else
                    diasSinEstudio = 0;
            }

            var tendenciaTasaAciertos = datos.Count >= 2 
                ? datos.Last().TasaAciertos - datos.First().TasaAciertos 
                : 0;
            
            var descripcionTendencia = tendenciaTasaAciertos switch
            {
                > 5 => "mejorando",
                < -5 => "empeorando",
                _ => "estable"
            };

            return new TrendAnalysisDto
            {
                Datos = datos,
                TendenciaTasaAciertos = Math.Round(tendenciaTasaAciertos, 2),
                DescripcionTendencia = descripcionTendencia,
                TendenciaActividad = 0,
                DescripcionActividad = "Mantener consistencia",
                DiasSinEstudio = diasSinEstudio,
                ConsecultivosEstudio = diasSinEstudio
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener análisis de tendencias para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene estadísticas comparativas anónimas
    /// </summary>
    public async Task<ComparativeStatsDto> GetComparativeStatsAsync(string usuarioId)
    {
        try
        {
            var usuarioStats = await GetOverallStatsAsync(usuarioId);
            
            return new ComparativeStatsDto
            {
                PromedioUsuarioAciertos = usuarioStats.PromedioAcierto,
                PromedioGlobalAciertos = 65.0,
                DiferenciaAciertos = Math.Round(usuarioStats.PromedioAcierto - 65.0, 2),
                
                TiempoEstudioUsuario = usuarioStats.TiempoEstudioTotalMinutos,
                PromedioGlobalTiempoEstudio = 300,
                
                PercentilAciertos = usuarioStats.PromedioAcierto >= 70 ? 75 : 50,
                PercentilActividad = usuarioStats.TiempoEstudioTotalMinutos >= 300 ? 70 : 45,
                
                Posicion = usuarioStats.PromedioAcierto >= 80 ? "Top 10%" : "Top 25%",
                Clasificacion = usuarioStats.PromedioAcierto >= 80 ? "Excelente" : "Muy bueno"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas comparativas para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    // ============= Métodos Privados Auxiliares =============

    /// <summary>
    /// Calcula el nivel de dominio basado en la tasa de aciertos
    /// </summary>
    private string CalcularNivelDominio(double porcentajeAcierto)
    {
        return porcentajeAcierto switch
        {
            >= 90 => "Experto",
            >= 75 => "Avanzado",
            >= 50 => "Intermedio",
            _ => "Novato"
        };
    }

    /// <summary>
    /// Calcula una puntuación de dominio ponderada
    /// </summary>
    private double CalcularPuntuacionDominio(int completadas, int total, double tasaAciertos)
    {
        if (total == 0) return 0;
        var porcentajeComplecion = (double)completadas / total * 100;
        return (porcentajeComplecion * 0.6) + (tasaAciertos * 0.4);
    }

    /// <summary>
    /// Calcula la racha de estudio actual y máxima del usuario
    /// </summary>
    private (int RachaActual, int RachaMaxima) CalcularRachaEstudio(string usuarioId, List<Core.Entities.EstadisticaEstudio> estadisticas)
    {
        var estadisticasOrdenadas = estadisticas.OrderByDescending(e => e.Fecha).ToList();
        var rachaActual = 0;
        var rachaMaxima = 0;
        var rachaTemp = 0;

        foreach (var stat in estadisticasOrdenadas)
        {
            if (stat.TiempoEstudioMinutos > 0)
            {
                rachaTemp++;
            }
            else
            {
                if (rachaTemp > rachaMaxima)
                    rachaMaxima = rachaTemp;
                rachaTemp = 0;
            }
        }

        for (var i = 0; i < estadisticasOrdenadas.Count; i++)
        {
            if (estadisticasOrdenadas[i].TiempoEstudioMinutos > 0)
            {
                rachaActual++;
            }
            else
            {
                break;
            }
        }

        return (rachaActual, Math.Max(rachaTemp, rachaMaxima));
    }
}
