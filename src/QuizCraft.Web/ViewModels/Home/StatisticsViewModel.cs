using QuizCraft.Core.Entities;

namespace QuizCraft.Web.ViewModels.Home;

/// <summary>
/// ViewModel para la página de estadísticas del usuario
/// </summary>
public class StatisticsViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;
    
    // Estadísticas generales
    public int TotalMaterias { get; set; }
    public int TotalFlashcards { get; set; }
    public int TotalQuizzes { get; set; }
    
    // Colecciones para análisis detallado
    public List<Materia> Materias { get; set; } = new();
    public List<Quiz> QuizzesRecientes { get; set; } = new();
}
