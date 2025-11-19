using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;

namespace QuizCraft.Web.ViewModels.Home;

/// <summary>
/// ViewModel para el dashboard del usuario
/// </summary>
public class DashboardViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string FechaUltimoAcceso { get; set; } = string.Empty;
    
    // Estadísticas principales
    public int TotalMaterias { get; set; }
    public int TotalFlashcards { get; set; }
    public int FlashcardsParaRevisar { get; set; }
    public int TotalQuizzes { get; set; }
    
    // Actividad reciente
    public List<Quiz> QuizzesRecientes { get; set; } = new();
    public List<Materia> MateriasRecientes { get; set; } = new();
    public List<ActividadReciente> ActividadesRecientes { get; set; } = new();
    
    // Estadísticas de estudio
    public int FlashcardsEstudiadasHoy { get; set; }
    public int QuizzesRealizadosHoy { get; set; }
    public int TiempoEstudioHoy { get; set; } // En minutos
    public double PromedioAcierto { get; set; }
    
    // Progreso general
    public double ProgresoPorcentaje { get; set; }
    public string ProgresoDescripcion { get; set; } = string.Empty;
}

/// <summary>
/// Clase para representar actividad reciente del usuario
/// </summary>
public class ActividadReciente
{
    public TipoActividad TipoActividad { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaActividad { get; set; }
    public string? MateriaNombre { get; set; }
    public string? MateriaColor { get; set; }
    public double? Puntuacion { get; set; } // Para quizzes
    public int? FlashcardsRevisadas { get; set; } // Para flashcards
    public string IconoActividad => TipoActividad switch
    {
        TipoActividad.Quiz => "fas fa-question-circle",
        TipoActividad.Flashcard => "fas fa-layer-group",
        TipoActividad.EstudioLibre => "fas fa-book-open",
        _ => "fas fa-clock"
    };
    public string ColorActividad => TipoActividad switch
    {
        TipoActividad.Quiz => "text-warning",
        TipoActividad.Flashcard => "text-success",
        TipoActividad.EstudioLibre => "text-info",
        _ => "text-muted"
    };
}