using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Representa un quiz importado por un usuario
/// </summary>
public class QuizImportado : BaseEntity
{
    
    /// <summary>
    /// ID del quiz compartido original
    /// </summary>
    public int QuizCompartidoId { get; set; }
    public QuizCompartido QuizCompartido { get; set; } = null!;
    
    /// <summary>
    /// ID del nuevo quiz creado
    /// </summary>
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    
    /// <summary>
    /// ID del usuario que importó
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
}
