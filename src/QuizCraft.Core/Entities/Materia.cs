using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad que representa una materia de estudio
/// </summary>
public class Materia : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Descripcion { get; set; }
    
    [StringLength(50)]
    public string? Color { get; set; } = "#007bff"; // Color por defecto Bootstrap primary
    
    [StringLength(50)]
    public string? Icono { get; set; }
    
    public string UsuarioId { get; set; } = string.Empty;
    
    // Propiedades de navegación
    public virtual ApplicationUser Usuario { get; set; } = null!;
    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public virtual ICollection<EstadisticaEstudio> EstadisticasEstudio { get; set; } = new List<EstadisticaEstudio>();
}