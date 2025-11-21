using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Repositorio para gestionar flashcards compartidas
/// </summary>
public interface IFlashcardCompartidaRepository : IGenericRepository<FlashcardCompartida>
{
    /// <summary>
    /// Obtiene una flashcard compartida por su código
    /// </summary>
    Task<FlashcardCompartida?> GetByCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene todas las flashcards compartidas por un usuario
    /// </summary>
    Task<IEnumerable<FlashcardCompartida>> GetByPropietarioAsync(string propietarioId);
    
    /// <summary>
    /// Obtiene flashcards compartidas de una flashcard específica
    /// </summary>
    Task<IEnumerable<FlashcardCompartida>> GetByFlashcardIdAsync(int flashcardId);
    
    /// <summary>
    /// Verifica si un código está disponible
    /// </summary>
    Task<bool> ExisteCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene las flashcards importadas por un usuario
    /// </summary>
    Task<IEnumerable<FlashcardImportada>> GetImportadasByUsuarioAsync(string usuarioId);
    
    /// <summary>
    /// Verifica si un usuario ya importó una flashcard compartida
    /// </summary>
    Task<bool> UsuarioYaImportoAsync(int flashcardCompartidaId, string usuarioId);
    
    /// <summary>
    /// Agrega un registro de importación
    /// </summary>
    Task AddImportacionAsync(FlashcardImportada importacion);
    
    /// <summary>
    /// Obtiene la información de importación de una flashcard específica
    /// </summary>
    Task<FlashcardImportada?> GetImportacionByFlashcardIdAsync(int flashcardId);
}
