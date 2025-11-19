using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de resultados de quiz
/// </summary>
public interface IResultadoQuizRepository : IGenericRepository<ResultadoQuiz>
{
    /// <summary>
    /// Obtiene los resultados de quiz de un usuario por ID
    /// </summary>
    Task<IEnumerable<ResultadoQuiz>> GetByUsuarioIdAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene los resultados recientes de un usuario (últimos N resultados)
    /// </summary>
    Task<IEnumerable<ResultadoQuiz>> GetResultadosRecientesAsync(string usuarioId, int cantidad = 10);
    
    /// <summary>
    /// Obtiene los resultados de un quiz específico para un usuario
    /// </summary>
    Task<IEnumerable<ResultadoQuiz>> GetByQuizIdAsync(int quizId, string usuarioId);
    
    /// <summary>
    /// Obtiene el mejor resultado de un usuario para un quiz específico
    /// </summary>
    Task<ResultadoQuiz?> GetMejorResultadoAsync(int quizId, string usuarioId);
}