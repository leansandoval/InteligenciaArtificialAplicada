using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de quizzes con métodos especializados
/// </summary>
public interface IQuizRepository : IGenericRepository<Quiz>
{
    Task<IEnumerable<Quiz>> GetQuizzesByMateriaIdAsync(int materiaId);
    Task<IEnumerable<Quiz>> GetQuizzesByCreadorIdAsync(string creadorId);
    Task<Quiz?> GetQuizCompletoAsync(int quizId);
    Task<Quiz?> GetQuizConPreguntasAsync(int quizId);
    Task<IEnumerable<Quiz>> GetQuizzesPublicosAsync(int? materiaId = null);
    Task<IEnumerable<Quiz>> GetQuizzesActivosAsync();
    Task<IEnumerable<Quiz>> BuscarQuizzesAsync(string termino, string? usuarioId = null);
    Task<int> GetCantidadPreguntasAsync(int quizId);
    Task<int> GetCantidadResultadosAsync(int quizId);
    Task<double> GetPuntuacionPromedioAsync(int quizId);
    Task<IEnumerable<Quiz>> GetQuizzesRecientesAsync(string usuarioId, int cantidad = 10);
    Task<IEnumerable<Quiz>> GetQuizzesRecientesByUsuarioAsync(string usuarioId, int cantidad = 10);
    Task<IEnumerable<Quiz>> GetQuizzesPopularesAsync(int cantidad = 10);
    Task<bool> ExisteQuizByTituloAsync(string titulo, string creadorId, int? excludeId = null);
    Task<Dictionary<int, int>> GetEstadisticasQuizzesByMateriaAsync(string usuarioId);
    Task<IEnumerable<Quiz>> GetQuizzesPorDificultadAsync(int materiaId, int tiempoMinimo, int tiempoMaximo);
    Task<IEnumerable<Quiz>> GetQuizzesSinResultadosAsync(string usuarioId);
    Task<bool> TienePermisosAccesoAsync(int quizId, string usuarioId);
    Task ActualizarEstadoEnLoteAsync(IEnumerable<int> quizIds, bool esActivo);
}