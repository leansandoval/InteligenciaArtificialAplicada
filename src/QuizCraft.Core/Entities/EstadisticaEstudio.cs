using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad para almacenar estadísticas de estudio por usuario
/// </summary>
public class EstadisticaEstudio : BaseEntity
{
    public DateTime Fecha { get; set; } = DateTime.Today;

    public TipoActividad TipoActividad { get; set; } = TipoActividad.EstudioLibre;
    
    public int FlashcardsRevisadas { get; set; } = 0;
    public int FlashcardsCorrectas { get; set; } = 0;
    public int FlashcardsIncorrectas { get; set; } = 0;
    
    public int QuizzesRealizados { get; set; } = 0;
    public double PromedioAcierto { get; set; } = 0;
    
    public int TiempoEstudioMinutos { get; set; } = 0;
    public TimeSpan TiempoEstudio { get; set; } = TimeSpan.Zero; // Tiempo como TimeSpan
    
    // Claves foráneas
    public string UsuarioId { get; set; } = string.Empty;
    public int? MateriaId { get; set; }
    
    // Propiedades de navegación
    public virtual ApplicationUser Usuario { get; set; } = null!;
    public virtual Materia? Materia { get; set; }
}