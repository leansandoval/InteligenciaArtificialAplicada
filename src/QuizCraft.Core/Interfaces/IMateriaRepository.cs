using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de materias con métodos especializados
/// </summary>
public interface IMateriaRepository : IGenericRepository<Materia>
{
    Task<IEnumerable<Materia>> GetMateriasByUsuarioIdAsync(string usuarioId);
    Task<Materia?> GetMateriaWithFlashcardsAsync(int materiaId);
    Task<Materia?> GetMateriaWithQuizzesAsync(int materiaId);
    Task<Materia?> GetMateriaCompletaAsync(int materiaId);
    Task<bool> ExisteMateriaByNombreAsync(string nombre, string usuarioId, int? excludeId = null);
    Task<IEnumerable<Materia>> GetMateriasConEstadisticasAsync(string usuarioId, DateTime fechaDesde, DateTime fechaHasta);
    Task<int> GetCantidadFlashcardsByMateriaAsync(int materiaId);
    Task<int> GetCantidadQuizzesByMateriaAsync(int materiaId);
    Task<IEnumerable<Materia>> BuscarMateriasAsync(string termino, string usuarioId);
    Task<Dictionary<int, int>> GetEstadisticasGeneralesByUsuarioAsync(string usuarioId);
    Task<bool> TieneDependenciasAsync(int materiaId);
}