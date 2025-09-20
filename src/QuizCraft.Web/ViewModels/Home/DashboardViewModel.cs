using QuizCraft.Core.Entities;

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
    
    // Estadísticas de estudio
    public int FlashcardsEstudiadasHoy { get; set; }
    public int QuizzesRealizadosHoy { get; set; }
    public int TiempoEstudioHoy { get; set; } // En minutos
    public double PromedioAcierto { get; set; }
}