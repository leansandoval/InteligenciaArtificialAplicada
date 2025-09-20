using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad que representa un quiz o cuestionario
/// </summary>
public class Quiz : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Descripcion { get; set; }
    
    public int NumeroPreguntas { get; set; }
    public int TiempoLimite { get; set; } = 0; // 0 = sin límite, en minutos
    public int TiempoPorPregunta { get; set; } = 30; // tiempo en segundos por pregunta
    public int NivelDificultad { get; set; } = 1; // 1=Fácil, 2=Medio, 3=Difícil
    
    public bool EsPublico { get; set; } = false;
    public bool EsActivo { get; set; } = true;
    public bool MostrarRespuestasInmediato { get; set; } = true;
    public bool PermitirReintento { get; set; } = true;
    
    // Claves foráneas
    public int MateriaId { get; set; }
    public string CreadorId { get; set; } = string.Empty;
    
    // Propiedades de navegación
    public virtual Materia Materia { get; set; } = null!;
    public virtual ApplicationUser Creador { get; set; } = null!;
    public virtual ICollection<PreguntaQuiz> Preguntas { get; set; } = new List<PreguntaQuiz>();
    public virtual ICollection<ResultadoQuiz> Resultados { get; set; } = new List<ResultadoQuiz>();
}