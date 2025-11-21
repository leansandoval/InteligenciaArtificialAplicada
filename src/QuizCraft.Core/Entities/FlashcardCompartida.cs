using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Representa una flashcard compartida con un enlace de acceso
/// </summary>
public class FlashcardCompartida : BaseEntity
{
    /// <summary>
    /// ID de la flashcard compartida
    /// </summary>
    public int FlashcardId { get; set; }
    public Flashcard Flashcard { get; set; } = null!;
    
    /// <summary>
    /// ID del usuario propietario que comparte
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string PropietarioId { get; set; } = string.Empty;
    public ApplicationUser Propietario { get; set; } = null!;
    
    /// <summary>
    /// Código único para compartir
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CodigoCompartido { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de expiración del enlace (opcional)
    /// </summary>
    public DateTime? FechaExpiracion { get; set; }
    
    /// <summary>
    /// Número de veces que se ha usado este enlace
    /// </summary>
    public int VecesUsado { get; set; } = 0;
    
    /// <summary>
    /// Máximo número de usos permitidos (null = ilimitado)
    /// </summary>
    public int? MaximoUsos { get; set; }
    
    /// <summary>
    /// Si permite modificaciones del contenido importado
    /// </summary>
    public bool PermiteModificaciones { get; set; } = true;
    
    /// <summary>
    /// Usuarios que han importado esta flashcard
    /// </summary>
    public ICollection<FlashcardImportada> Importaciones { get; set; } = new List<FlashcardImportada>();
}
