using QuizCraft.Application.Models;

namespace QuizCraft.Application.Interfaces;

/// <summary>
/// Servicio para compartir e importar flashcards
/// </summary>
public interface IFlashcardCompartidaService
{
    /// <summary>
    /// Genera un enlace para compartir una flashcard
    /// </summary>
    Task<ServiceResult<string>> CompartirFlashcardAsync(int flashcardId, string usuarioId, CompartirFlashcardOptions opciones);
    
    /// <summary>
    /// Importa una flashcard compartida
    /// </summary>
    Task<ServiceResult<int>> ImportarFlashcardAsync(string codigo, string usuarioId, int materiaDestinoId);
    
    /// <summary>
    /// Obtiene información de una flashcard compartida
    /// </summary>
    Task<ServiceResult<FlashcardCompartidaInfo>> ObtenerInfoFlashcardCompartidaAsync(string codigo);
    
    /// <summary>
    /// Revoca un enlace de compartición
    /// </summary>
    Task<ServiceResult> RevocarComparticionAsync(int flashcardCompartidaId, string usuarioId);
    
    /// <summary>
    /// Obtiene las flashcards compartidas por un usuario
    /// </summary>
    Task<List<FlashcardCompartidaResumen>> ObtenerFlashcardsCompartidasAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene las flashcards importadas por un usuario
    /// </summary>
    Task<List<FlashcardImportadaResumen>> ObtenerFlashcardsImportadasAsync(string usuarioId);
}

public class CompartirFlashcardOptions
{
    public DateTime? FechaExpiracion { get; set; }
    public int? MaximoUsos { get; set; }
    public bool PermiteModificaciones { get; set; } = true;
}

public class FlashcardCompartidaInfo
{
    public string Pregunta { get; set; } = string.Empty;
    public string Respuesta { get; set; } = string.Empty;
    public string? Pista { get; set; }
    public string NombreMateria { get; set; } = string.Empty;
    public string NombrePropietario { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public DateTime? FechaExpiracion { get; set; }
    public int? UsosRestantes { get; set; }
    public bool PermiteModificaciones { get; set; }
    public bool EstaDisponible { get; set; }
    public string? MensajeError { get; set; }
}

public class FlashcardCompartidaResumen
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

public class FlashcardImportadaResumen
{
    public int FlashcardId { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public string NombrePropietarioOriginal { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; }
    public bool PermiteModificaciones { get; set; }
}
