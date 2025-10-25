namespace QuizCraft.Core.Enums;

/// <summary>
/// Tipos de repaso disponibles
/// </summary>
public enum TipoRepaso
{
    /// <summary>
    /// Repaso programado manualmente por el usuario
    /// </summary>
    Manual = 1,
    
    /// <summary>
    /// Repaso automático basado en algoritmos de repetición espaciada
    /// </summary>
    Automatico = 2,
    
    /// <summary>
    /// Repaso sugerido por el sistema basado en errores previos
    /// </summary>
    Sugerido = 3
}