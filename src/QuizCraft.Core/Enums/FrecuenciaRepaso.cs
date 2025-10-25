namespace QuizCraft.Core.Enums;

/// <summary>
/// Frecuencias de repetición para repasos automáticos
/// </summary>
public enum FrecuenciaRepaso
{
    /// <summary>
    /// Una sola vez (sin repetición)
    /// </summary>
    Unica = 0,
    
    /// <summary>
    /// Cada día
    /// </summary>
    Diaria = 1,
    
    /// <summary>
    /// Cada dos días
    /// </summary>
    CadaDosDias = 2,
    
    /// <summary>
    /// Cada tres días
    /// </summary>
    CadaTresDias = 3,
    
    /// <summary>
    /// Semanal
    /// </summary>
    Semanal = 7,
    
    /// <summary>
    /// Cada dos semanas
    /// </summary>
    Quincenal = 14,
    
    /// <summary>
    /// Mensual
    /// </summary>
    Mensual = 30,
    
    /// <summary>
    /// Personalizada (definida por el usuario)
    /// </summary>
    Personalizada = -1
}