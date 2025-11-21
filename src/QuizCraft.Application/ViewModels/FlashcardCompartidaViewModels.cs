using QuizCraft.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Application.ViewModels;

/// <summary>
/// ViewModel para compartir una flashcard
/// </summary>
public class CompartirFlashcardViewModel
{
    public int FlashcardId { get; set; }
    public string PreguntaFlashcard { get; set; } = string.Empty;
    
    [Display(Name = "Fecha de expiración (opcional)")]
    [DataType(DataType.DateTime)]
    public DateTime? FechaExpiracion { get; set; }
    
    [Display(Name = "Máximo de usos (opcional)")]
    [Range(1, 1000, ErrorMessage = "El máximo de usos debe estar entre 1 y 1000")]
    public int? MaximoUsos { get; set; }
    
    [Display(Name = "Permitir modificaciones")]
    public bool PermiteModificaciones { get; set; } = true;
}

/// <summary>
/// ViewModel para importar una flashcard
/// </summary>
public class ImportarFlashcardViewModel
{
    [Required(ErrorMessage = "El código es requerido")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "El código debe tener 8 caracteres")]
    [Display(Name = "Código de compartición")]
    public string Codigo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Debe seleccionar una materia")]
    [Display(Name = "Materia de destino")]
    public int MateriaDestinoId { get; set; }
    
    public FlashcardCompartidaInfo? InfoFlashcard { get; set; }
    public List<MateriaDropdownViewModel> MateriasDisponibles { get; set; } = new();
}

/// <summary>
/// ViewModel para listar flashcards compartidas
/// </summary>
public class FlashcardsCompartidasViewModel
{
    public List<FlashcardCompartidaListItem> FlashcardsCompartidas { get; set; } = new();
    public List<FlashcardImportadaListItem> FlashcardsImportadas { get; set; } = new();
}

public class FlashcardCompartidaListItem
{
    public int Id { get; set; }
    public int FlashcardId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Pregunta { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int VecesUsado { get; set; }
    public int? MaximoUsos { get; set; }
    public bool EstaExpirado { get; set; }
    public bool EstaAgotado { get; set; }
    public bool EstaActivo { get; set; }
}

public class FlashcardImportadaListItem
{
    public int FlashcardId { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public string NombrePropietarioOriginal { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; }
    public bool PermiteModificaciones { get; set; }
}
