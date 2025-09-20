using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Clase base para todas las entidades del dominio
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    public DateTime? FechaModificacion { get; set; }
    
    public bool EstaActivo { get; set; } = true;
}