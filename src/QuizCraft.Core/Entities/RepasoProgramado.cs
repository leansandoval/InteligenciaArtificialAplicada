using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad para manejar repasos programados por el usuario
/// </summary>
public class RepasoProgramado : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Fecha y hora programada para el repaso
    /// </summary>
    public DateTime FechaProgramada { get; set; }
    
    /// <summary>
    /// Indica si el repaso ya fue completado
    /// </summary>
    public bool Completado { get; set; } = false;
    
    /// <summary>
    /// Fecha en que se completó el repaso
    /// </summary>
    public DateTime? FechaCompletado { get; set; }
    
    /// <summary>
    /// Tipo de repaso (manual o automático)
    /// </summary>
    public TipoRepaso TipoRepaso { get; set; } = TipoRepaso.Manual;
    
    /// <summary>
    /// Frecuencia de repetición para repasos automáticos
    /// </summary>
    public FrecuenciaRepaso? FrecuenciaRepeticion { get; set; }
    
    /// <summary>
    /// Próxima fecha programada (para repasos recurrentes)
    /// </summary>
    public DateTime? ProximaFecha { get; set; }
    
    /// <summary>
    /// Indica si el usuario debe recibir notificaciones
    /// </summary>
    public bool NotificacionesHabilitadas { get; set; } = true;
    
    /// <summary>
    /// Minutos antes del repaso para enviar notificación
    /// </summary>
    public int MinutosNotificacionPrevia { get; set; } = 15;
    
    /// <summary>
    /// Resultado del último repaso (puntuación)
    /// </summary>
    public double? UltimoPuntaje { get; set; }
    
    /// <summary>
    /// Notas adicionales del usuario sobre el repaso
    /// </summary>
    [StringLength(2000)]
    public string? Notas { get; set; }
    
    // Claves foráneas
    [Required]
    public string UsuarioId { get; set; } = string.Empty;
    
    public int? MateriaId { get; set; }
    
    public int? QuizId { get; set; }
    
    public int? FlashcardId { get; set; }
    
    // Propiedades de navegación
    public virtual ApplicationUser Usuario { get; set; } = null!;
    public virtual Materia? Materia { get; set; }
    public virtual Quiz? Quiz { get; set; }
    public virtual Flashcard? Flashcard { get; set; }
}