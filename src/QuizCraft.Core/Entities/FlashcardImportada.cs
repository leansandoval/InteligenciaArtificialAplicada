using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Representa una flashcard que fue importada por un usuario
/// </summary>
public class FlashcardImportada : BaseEntity
{
    /// <summary>
    /// ID de la flashcard compartida original
    /// </summary>
    public int FlashcardCompartidaId { get; set; }
    public FlashcardCompartida FlashcardCompartida { get; set; } = null!;
    
    /// <summary>
    /// ID de la nueva flashcard creada en la colección del usuario
    /// </summary>
    public int FlashcardId { get; set; }
    public Flashcard Flashcard { get; set; } = null!;
    
    /// <summary>
    /// ID del usuario que importó
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
    
    /// <summary>
    /// Fecha en que se importó
    /// </summary>
    public DateTime FechaImportacion { get; set; } = DateTime.UtcNow;
}
