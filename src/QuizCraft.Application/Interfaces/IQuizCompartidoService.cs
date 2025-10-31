using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces;

/// <summary>
/// Servicio para compartir e importar quizzes
/// </summary>
public interface IQuizCompartidoService
{
    /// <summary>
    /// Genera un enlace para compartir un quiz
    /// </summary>
    Task<ServiceResult<string>> CompartirQuizAsync(int quizId, string usuarioId, CompartirQuizOptions opciones);
    
    /// <summary>
    /// Importa un quiz compartido
    /// </summary>
    Task<ServiceResult<int>> ImportarQuizAsync(string codigo, string usuarioId, int materiaDestinoId);
    
    /// <summary>
    /// Obtiene información de un quiz compartido
    /// </summary>
    Task<ServiceResult<QuizCompartidoInfo>> ObtenerInfoQuizCompartidoAsync(string codigo);
    
    /// <summary>
    /// Revoca un enlace de compartición
    /// </summary>
    Task<ServiceResult> RevocarComparticionAsync(int quizCompartidoId, string usuarioId);
    
    /// <summary>
    /// Obtiene los quizzes compartidos por un usuario
    /// </summary>
    Task<List<QuizCompartidoResumen>> ObtenerQuizzesCompartidosAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene los quizzes importados por un usuario
    /// </summary>
    Task<List<QuizImportadoResumen>> ObtenerQuizzesImportadosAsync(string usuarioId);
}

public class CompartirQuizOptions
{
    public DateTime? FechaExpiracion { get; set; }
    public int? MaximoUsos { get; set; }
    public bool PermiteModificaciones { get; set; } = true;
}

public class QuizCompartidoInfo
{
    public string TituloQuiz { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string NombreMateria { get; set; } = string.Empty;
    public string NombrePropietario { get; set; } = string.Empty;
    public int NumeroPreguntas { get; set; }
    public string Dificultad { get; set; } = string.Empty;
    public DateTime? FechaExpiracion { get; set; }
    public int? UsosRestantes { get; set; }
    public bool PermiteModificaciones { get; set; }
    public bool EstaDisponible { get; set; }
    public string? MensajeError { get; set; }
}

public class QuizCompartidoResumen
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string TituloQuiz { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int NumeroPreguntas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int VecesUsado { get; set; }
    public int? MaximoUsos { get; set; }
    public bool EstaExpirado { get; set; }
    public bool EstaAgotado { get; set; }
    public bool EstaActivo { get; set; }
}

public class QuizImportadoResumen
{
    public int QuizId { get; set; }
    public string TituloQuiz { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int NumeroPreguntas { get; set; }
    public string NombrePropietarioOriginal { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; }
}
