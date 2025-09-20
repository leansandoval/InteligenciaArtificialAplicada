using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad para almacenar archivos adjuntos a las flashcards
/// </summary>
public class ArchivoAdjunto : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string NombreOriginal { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string NombreArchivo { get; set; } = string.Empty; // Nombre del archivo en el sistema
    
    [Required]
    [StringLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string TipoMime { get; set; } = string.Empty;

    public TipoEntidad TipoEntidad { get; set; } = TipoEntidad.Flashcard;
    
    public long TamanoBytes { get; set; }
    
    [StringLength(1000)]
    public string? Descripcion { get; set; }
    
    // Claves foráneas
    public int FlashcardId { get; set; }
    
    // Propiedades de navegación
    public virtual Flashcard Flashcard { get; set; } = null!;
}