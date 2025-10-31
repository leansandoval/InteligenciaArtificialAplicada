using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Repositorio para gestionar quizzes compartidos
/// </summary>
public interface IQuizCompartidoRepository : IGenericRepository<QuizCompartido>
{
    /// <summary>
    /// Obtiene un quiz compartido por su código
    /// </summary>
    Task<QuizCompartido?> GetByCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene todos los quizzes compartidos por un usuario
    /// </summary>
    Task<IEnumerable<QuizCompartido>> GetByPropietarioAsync(string propietarioId);
    
    /// <summary>
    /// Obtiene quizzes compartidos de un quiz específico
    /// </summary>
    Task<IEnumerable<QuizCompartido>> GetByQuizIdAsync(int quizId);
    
    /// <summary>
    /// Verifica si un código está disponible
    /// </summary>
    Task<bool> ExisteCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene los quizzes importados por un usuario
    /// </summary>
    Task<IEnumerable<QuizImportado>> GetImportadosByUsuarioAsync(string usuarioId);
    
    /// <summary>
    /// Verifica si un usuario ya importó un quiz compartido
    /// </summary>
    Task<bool> UsuarioYaImportoAsync(int quizCompartidoId, string usuarioId);
    
    /// <summary>
    /// Agrega un registro de importación
    /// </summary>
    Task AddImportacionAsync(QuizImportado importacion);
}
