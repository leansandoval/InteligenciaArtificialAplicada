using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Application.ViewModels;

/// <summary>
/// ViewModel para crear un nuevo repaso programado
/// </summary>
public class CrearRepasoProgramadoViewModel
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
    [Display(Name = "Título del repaso")]
    public string Titulo { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "La fecha programada es obligatoria")]
    [Display(Name = "Fecha y hora programada")]
    [DataType(DataType.DateTime)]
    public DateTime FechaProgramada { get; set; } = DateTime.Now.AddDays(1);
    
    [Display(Name = "Tipo de repaso")]
    public TipoRepaso TipoRepaso { get; set; } = TipoRepaso.Manual;
    
    [Display(Name = "Frecuencia de repetición")]
    public FrecuenciaRepaso? FrecuenciaRepeticion { get; set; }
    
    [StringLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
    [Display(Name = "Notas adicionales")]
    public string? Notas { get; set; }
    
    [Display(Name = "Materia")]
    public int? MateriaId { get; set; }
    
    [Display(Name = "Quiz específico")]
    public int? QuizId { get; set; }
    
    [Display(Name = "Flashcard específica")]
    public int? FlashcardId { get; set; }
    
    // Para los dropdowns
    public List<SelectItemViewModel> MateriasDisponibles { get; set; } = new();
    public List<SelectItemViewModel> QuizzesDisponibles { get; set; } = new();
    public List<SelectItemViewModel> FlashcardsDisponibles { get; set; } = new();
}

/// <summary>
/// ViewModel para editar un repaso programado existente
/// </summary>
public class EditarRepasoProgramadoViewModel : CrearRepasoProgramadoViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Completado")]
    public bool Completado { get; set; }
    
    [Display(Name = "Fecha de completado")]
    [DataType(DataType.DateTime)]
    public DateTime? FechaCompletado { get; set; }
    
    [Display(Name = "Último puntaje")]
    [Range(0, 100, ErrorMessage = "El puntaje debe estar entre 0 y 100")]
    public double? UltimoPuntaje { get; set; }
}

/// <summary>
/// ViewModel para mostrar la lista de repasos programados
/// </summary>
public class RepasoProgramadoListViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaProgramada { get; set; }
    public bool Completado { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public TipoRepaso TipoRepaso { get; set; }
    public FrecuenciaRepaso? FrecuenciaRepeticion { get; set; }
    public DateTime? ProximaFecha { get; set; }
    public double? UltimoPuntaje { get; set; }
    public string? MateriaNombre { get; set; }
    public string? QuizTitulo { get; set; }
    public string? FlashcardPregunta { get; set; }
    public bool NotificacionesHabilitadas { get; set; }
    
    // IDs para redirección al contenido específico
    public int? QuizId { get; set; }
    public int? FlashcardId { get; set; }
    public int? MateriaId { get; set; }
    
    /// <summary>
    /// Indica si el repaso está vencido (fecha programada pasó y no está completado)
    /// </summary>
    public bool EstaVencido => !Completado && FechaProgramada < DateTime.Now;
    
    /// <summary>
    /// Indica si el repaso está próximo (dentro de las próximas 24 horas)
    /// </summary>
    public bool EstaProximo => !Completado && FechaProgramada > DateTime.Now && FechaProgramada <= DateTime.Now.AddHours(24);
    
    /// <summary>
    /// Tiempo restante hasta el repaso
    /// </summary>
    public TimeSpan TiempoRestante => FechaProgramada - DateTime.Now;
}

/// <summary>
/// ViewModel para la página principal de repasos
/// </summary>
public class RepasosProgramadosViewModel
{
    public List<RepasoProgramadoListViewModel> RepasosPendientes { get; set; } = new();
    public List<RepasoProgramadoListViewModel> RepasosVencidos { get; set; } = new();
    public List<RepasoProgramadoListViewModel> RepasosProximos { get; set; } = new();
    public List<RepasoProgramadoListViewModel> RepasosCompletados { get; set; } = new();
    
    public int TotalRepasos => RepasosPendientes.Count + RepasosVencidos.Count + RepasosProximos.Count + RepasosCompletados.Count;
    public int RepasosPendientesCount => RepasosPendientes.Count;
    public int RepasosVencidosCount => RepasosVencidos.Count;
    public int RepasosProximosCount => RepasosProximos.Count;
    public int RepasosCompletadosCount => RepasosCompletados.Count;
}

/// <summary>
/// ViewModel para completar un repaso
/// </summary>
public class CompletarRepasoViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaProgramada { get; set; }
    public TipoRepaso TipoRepaso { get; set; }
    public string? MateriaNombre { get; set; }
    public string? QuizTitulo { get; set; }
    public string? FlashcardPregunta { get; set; }
    
    [Range(0, 100, ErrorMessage = "El puntaje debe estar entre 0 y 100")]
    [Display(Name = "Puntaje obtenido (0-100)")]
    public double? Puntaje { get; set; }
    
    [StringLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
    [Display(Name = "Notas del repaso")]
    public string? NotasRepaso { get; set; }
    
    [Display(Name = "Programar próximo repaso automáticamente")]
    public bool ProgramarProximo { get; set; } = true;
}

/// <summary>
/// ViewModel para elementos de selección
/// </summary>
public class SelectItemViewModel
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
}