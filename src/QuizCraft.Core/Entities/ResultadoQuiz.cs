using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad que representa el resultado de un usuario en un quiz
/// </summary>
public class ResultadoQuiz : BaseEntity
{
    public int PuntajeObtenido { get; set; }
    public int PuntajeMaximo { get; set; }
    public decimal Puntuacion { get; set; } // Puntuación decimal para mayor precisión
    public double PorcentajeAcierto { get; set; }
    
    public int TiempoTranscurrido { get; set; } // En segundos
    public TimeSpan TiempoTotal { get; set; } // Tiempo total como TimeSpan
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFinalizacion { get; set; }
    public DateTime FechaRealizacion { get; set; } // Fecha de realización
    
    public bool EstaCompletado { get; set; } = false;
    
    // Claves foráneas
    public int QuizId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    
    // Propiedades de navegación
    public virtual Quiz Quiz { get; set; } = null!;
    public virtual ApplicationUser Usuario { get; set; } = null!;
    public virtual ICollection<RespuestaUsuario> RespuestasUsuario { get; set; } = new List<RespuestaUsuario>();
}