using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de estadísticas de estudio
/// </summary>
public interface IEstadisticaEstudioRepository : IGenericRepository<EstadisticaEstudio>
{
    /// <summary>
    /// Obtiene las estadísticas de estudio de un usuario por ID
    /// </summary>
    Task<IEnumerable<EstadisticaEstudio>> GetByUsuarioIdAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene las estadísticas de estudio recientes de un usuario (últimos 7 días)
    /// </summary>
    Task<IEnumerable<EstadisticaEstudio>> GetActividadRecienteAsync(string usuarioId, int dias = 7);
    
    /// <summary>
    /// Obtiene las estadísticas de hoy para un usuario
    /// </summary>
    Task<EstadisticaEstudio?> GetEstadisticaHoyAsync(string usuarioId);
    
    /// <summary>
    /// Registra actividad de flashcard
    /// </summary>
    Task RegistrarActividadFlashcardAsync(string usuarioId, int materiaId, bool esCorrecta);
    
    /// <summary>
    /// Registra actividad de quiz
    /// </summary>
    Task RegistrarActividadQuizAsync(string usuarioId, int materiaId, double porcentajeAcierto);
}