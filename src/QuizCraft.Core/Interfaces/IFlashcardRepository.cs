using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de flashcards con métodos especializados
/// </summary>
public interface IFlashcardRepository : IGenericRepository<Flashcard>
{
    Task<IEnumerable<Flashcard>> GetFlashcardsByMateriaIdAsync(int materiaId);
    Task<IEnumerable<Flashcard>> GetFlashcardsByDificultadAsync(int materiaId, NivelDificultad dificultad);
    Task<IEnumerable<Flashcard>> GetFlashcardsAleatoriosAsync(int materiaId, int cantidad);
    Task<IEnumerable<Flashcard>> BuscarFlashcardsAsync(string termino, int? materiaId = null);
    Task<int> GetCantidadByDificultadAsync(int materiaId, NivelDificultad dificultad);
    Task<Dictionary<NivelDificultad, int>> GetEstadisticasDificultadAsync(int materiaId);
    Task<IEnumerable<Flashcard>> GetFlashcardsRecientesAsync(int materiaId, int cantidad = 10);
    Task<IEnumerable<Flashcard>> GetFlashcardsModificadosAsync(int materiaId, DateTime fechaDesde);
    Task<bool> ExisteFlashcardSimilarAsync(string pregunta, int materiaId, int? excludeId = null);
    Task<IEnumerable<Flashcard>> GetFlashcardsPaginadosAsync(
        int materiaId, 
        int pagina, 
        int tamaño, 
        NivelDificultad? dificultad = null,
        string? terminoBusqueda = null);
    Task<double> GetPromedioComplejidadAsync(int materiaId);
    Task ActualizarDificultadesEnLoteAsync(int materiaId, NivelDificultad nuevaDificultad);
    Task<IEnumerable<Flashcard>> GetFlashcardsByUsuarioIdAsync(string usuarioId);
    Task<Flashcard?> GetByIdWithMateriaAsync(int id);
    
    // Métodos específicos para sistema de repaso
    Task<IEnumerable<Flashcard>> GetFlashcardsParaRepasoAsync(string usuarioId, int? materiaId = null);
    Task<IEnumerable<Flashcard>> GetFlashcardsParaRepasoByMateriaAsync(int materiaId);
    Task<int> GetCantidadFlashcardsParaRepasoAsync(string usuarioId, int? materiaId = null);
    Task ActualizarEstadisticasRepasoAsync(int flashcardId, bool esCorrecta, TimeSpan tiempoRespuesta);
}