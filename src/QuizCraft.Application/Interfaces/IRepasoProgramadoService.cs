using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;

namespace QuizCraft.Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de gestión de repasos programados
/// </summary>
public interface IRepasoProgramadoService
{
    /// <summary>
    /// Obtiene todos los repasos programados de un usuario
    /// </summary>
    Task<RepasosProgramadosViewModel> ObtenerRepasosPorUsuarioAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene un repaso programado por su ID
    /// </summary>
    Task<RepasoProgramado?> ObtenerRepasoPorIdAsync(int repasoId, string usuarioId);
    
    /// <summary>
    /// Crea un nuevo repaso programado
    /// </summary>
    Task<bool> CrearRepasoProgramadoAsync(CrearRepasoProgramadoViewModel modelo, string usuarioId);
    
    /// <summary>
    /// Actualiza un repaso programado existente
    /// </summary>
    Task<bool> ActualizarRepasoProgramadoAsync(EditarRepasoProgramadoViewModel modelo, string usuarioId);
    
    /// <summary>
    /// Elimina un repaso programado
    /// </summary>
    Task<bool> EliminarRepasoProgramadoAsync(int repasoId, string usuarioId);
    
    /// <summary>
    /// Marca un repaso como completado
    /// </summary>
    Task<bool> CompletarRepasoAsync(CompletarRepasoViewModel modelo, string usuarioId);
    
    /// <summary>
    /// Obtiene los repasos que deben notificarse próximamente
    /// </summary>
    Task<List<RepasoProgramado>> ObtenerRepasosParaNotificarAsync();
    
    /// <summary>
    /// Obtiene los repasos vencidos de un usuario
    /// </summary>
    Task<List<RepasoProgramadoListViewModel>> ObtenerRepasosVencidosAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene los repasos próximos de un usuario (siguientes 24 horas)
    /// </summary>
    Task<List<RepasoProgramadoListViewModel>> ObtenerRepasosProximosAsync(string usuarioId);
    
    /// <summary>
    /// Programa el próximo repaso automático basado en la frecuencia configurada
    /// </summary>
    Task<bool> ProgramarProximoRepasoAutomaticoAsync(int repasoId);
    
    /// <summary>
    /// Obtiene las materias disponibles para un usuario
    /// </summary>
    Task<List<SelectItemViewModel>> ObtenerMateriasDisponiblesAsync(string usuarioId);
    
    /// <summary>
    /// Obtiene los quizzes disponibles para una materia específica
    /// </summary>
    Task<List<SelectItemViewModel>> ObtenerQuizzesDisponiblesAsync(string usuarioId, int? materiaId = null);
    
    /// <summary>
    /// Obtiene las flashcards disponibles para una materia específica
    /// </summary>
    Task<List<SelectItemViewModel>> ObtenerFlashcardsDisponiblesAsync(string usuarioId, int? materiaId = null);
    
    /// <summary>
    /// Genera repasos automáticos basados en el rendimiento del usuario
    /// </summary>
    Task<List<RepasoProgramado>> GenerarRepasosAutomaticosAsync(string usuarioId);
}