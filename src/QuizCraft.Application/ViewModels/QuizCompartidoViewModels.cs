using QuizCraft.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Application.ViewModels;

/// <summary>
/// ViewModel para compartir un quiz
/// </summary>
public class CompartirQuizViewModel
{
    public int QuizId { get; set; }
    public string TituloQuiz { get; set; } = string.Empty;
    
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
/// ViewModel para importar un quiz
/// </summary>
public class ImportarQuizViewModel
{
    [Required(ErrorMessage = "El código de compartición es requerido")]
    [Display(Name = "Código de compartición")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El código debe tener 8 caracteres")]
    public string CodigoCompartido { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Selecciona una materia de destino")]
    [Display(Name = "Materia de destino")]
    public int MateriaId { get; set; }
    
    // Información del quiz (se llena después de validar el código)
    public QuizCompartidoInfo? InfoQuiz { get; set; }
}

/// <summary>
/// ViewModel para listar quizzes compartidos
/// </summary>
public class QuizzesCompartidosViewModel
{
    public List<QuizCompartidoListItem> QuizzesCompartidos { get; set; } = new();
    public List<QuizImportadoListItem> QuizzesImportados { get; set; } = new();
}

public class QuizCompartidoListItem
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string TituloQuiz { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int NumeroPreguntas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int VecesUsado { get; set; }
    public int? MaximoUsos { get; set; }
    public bool EstaExpirado { get; set; }
    public bool EstaAgotado { get; set; }
    public bool EstaActivo { get; set; }
}

public class QuizImportadoListItem
{
    public int QuizId { get; set; }
    public string TituloQuiz { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int NumeroPreguntas { get; set; }
    public string NombrePropietarioOriginal { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; }
}
