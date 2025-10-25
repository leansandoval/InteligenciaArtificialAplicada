using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Usuario de la aplicación extendiendo IdentityUser con propiedades adicionales
/// </summary>
public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? Nombre { get; set; }
    
    [StringLength(100)]
    public string? Apellido { get; set; }

    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;
    
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    
    public DateTime? UltimoAcceso { get; set; }
    
    public bool EstaActivo { get; set; } = true;
    
    // Configuración de usuario
    public bool NotificacionesEmail { get; set; } = true;
    public bool NotificacionesWeb { get; set; } = true;
    public bool NotificacionesHabilitadas { get; set; } = true;
    public string? TemaPreferido { get; set; } = "light";

    [StringLength(10)]
    public string PreferenciaIdioma { get; set; } = "es";
    
    // Propiedades de navegación
    public virtual ICollection<Materia> Materias { get; set; } = new List<Materia>();
    public virtual ICollection<Quiz> QuizzesCreados { get; set; } = new List<Quiz>();
    public virtual ICollection<ResultadoQuiz> ResultadosQuiz { get; set; } = new List<ResultadoQuiz>();
    public virtual ICollection<EstadisticaEstudio> EstadisticasEstudio { get; set; } = new List<EstadisticaEstudio>();
    public virtual ICollection<RepasoProgramado> RepasosProgramados { get; set; } = new List<RepasoProgramado>();
}