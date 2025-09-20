using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad que representa la respuesta de un usuario a una pregunta específica
/// </summary>
public class RespuestaUsuario : BaseEntity
{
    [Required]
    [StringLength(1000)]
    public string RespuestaDada { get; set; } = string.Empty;

    [Required]
    [StringLength(1)]
    public string RespuestaSeleccionada { get; set; } = string.Empty; // A, B, C, D
    
    public bool EsCorrecta { get; set; }
    public int PuntosObtenidos { get; set; }
    public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow;
    
    // Claves foráneas
    public int PreguntaId { get; set; } // Cambiado de PreguntaQuizId
    public int PreguntaQuizId { get; set; }
    public int ResultadoQuizId { get; set; }
    
    // Propiedades de navegación
    public virtual PreguntaQuiz Pregunta { get; set; } = null!; // Navegación directa a la pregunta
    public virtual PreguntaQuiz PreguntaQuiz { get; set; } = null!;
    public virtual ResultadoQuiz ResultadoQuiz { get; set; } = null!;
}