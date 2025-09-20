using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad que representa una flashcard o tarjeta de estudio
/// </summary>
public class Flashcard : BaseEntity
{
    [Required]
    [StringLength(1000)]
    public string Pregunta { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Respuesta { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Pista { get; set; }
    
    [StringLength(500)]
    public string? RutaImagen { get; set; }
    
    [StringLength(500)]
    public string? RutaAudio { get; set; }
    
    public NivelDificultad Dificultad { get; set; } = NivelDificultad.Facil;
    
    public int VecesVista { get; set; } = 0;
    public int VecesCorrecta { get; set; } = 0;
    public int VecesIncorrecta { get; set; } = 0;
    
    public DateTime? UltimaRevision { get; set; }
    public DateTime? ProximaRevision { get; set; }
    
    // Intervalo de repetición espaciada (en días)
    public int IntervaloRepeticion { get; set; } = 1;
    public double FactorFacilidad { get; set; } = 2.5;
    
    // Claves foráneas
    public int MateriaId { get; set; }
    
    // Propiedades de navegación
    public virtual Materia Materia { get; set; } = null!;
    public virtual ICollection<PreguntaQuiz> PreguntasQuiz { get; set; } = new List<PreguntaQuiz>();
    public virtual ICollection<ArchivoAdjunto> ArchivosAdjuntos { get; set; } = new List<ArchivoAdjunto>();
}