using QuizCraft.Core.Entities;

namespace QuizCraft.Web.ViewModels.Home;

/// <summary>
/// ViewModel para la página principal de QuizCraft
/// </summary>
public class HomeIndexViewModel
{
    public bool EsUsuarioAutenticado { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public int TotalMaterias { get; set; }
    public int TotalFlashcards { get; set; }
    public int TotalQuizzes { get; set; }
    public List<Quiz> QuizzesRecientes { get; set; } = new();
    
    // Estadísticas para usuarios no autenticados
    public int TotalUsuariosRegistrados { get; set; }
    public int TotalQuizzesPublicos { get; set; }
}