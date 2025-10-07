using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Servicio que implementa el algoritmo de repetición espaciada SuperMemo SM-2
/// </summary>
public class AlgoritmoRepasoService : IAlgoritmoRepasoService
{
    /// <summary>
    /// Calcula la próxima fecha de revisión basada en el algoritmo SuperMemo SM-2
    /// </summary>
    /// <param name="flashcard">Flashcard a calcular</param>
    /// <param name="esCorrecta">Si la respuesta fue correcta</param>
    /// <returns>Fecha de próxima revisión</returns>
    public DateTime CalcularProximaRevision(Flashcard flashcard, bool esCorrecta)
    {
        var nuevoIntervalo = CalcularNuevoIntervalo(flashcard.IntervaloRepeticion, flashcard.FactorFacilidad, esCorrecta);
        return DateTime.Today.AddDays(nuevoIntervalo);
    }

    /// <summary>
    /// Actualiza el factor de facilidad basado en la calidad de la respuesta
    /// </summary>
    /// <param name="factorActual">Factor actual</param>
    /// <param name="calidad">Calidad de la respuesta (0-5, donde 3+ es correcto)</param>
    /// <returns>Nuevo factor de facilidad</returns>
    public double ActualizarFactorFacilidad(double factorActual, int calidad)
    {
        // Algoritmo SuperMemo SM-2
        var nuevoFactor = factorActual + (0.1 - (5 - calidad) * (0.08 + (5 - calidad) * 0.02));
        
        // El factor mínimo es 1.3
        return Math.Max(1.3, nuevoFactor);
    }

    /// <summary>
    /// Calcula el nuevo intervalo de repetición
    /// </summary>
    /// <param name="intervaloActual">Intervalo actual en días</param>
    /// <param name="factorFacilidad">Factor de facilidad</param>
    /// <param name="esCorrecta">Si la respuesta fue correcta</param>
    /// <returns>Nuevo intervalo en días</returns>
    public int CalcularNuevoIntervalo(int intervaloActual, double factorFacilidad, bool esCorrecta)
    {
        if (!esCorrecta)
        {
            // Si es incorrecta, reiniciar a 1 día
            return 1;
        }

        return intervaloActual switch
        {
            1 => 6,  // Primer repaso correcto: 6 días
            6 => (int)Math.Round(6 * factorFacilidad), // Segundo repaso: 6 * factor
            _ => (int)Math.Round(intervaloActual * factorFacilidad) // Subsecuentes: intervalo * factor
        };
    }

    /// <summary>
    /// Obtiene las flashcards que necesitan ser repasadas hoy, priorizadas por algoritmo
    /// </summary>
    /// <param name="flashcards">Colección de flashcards</param>
    /// <param name="limite">Número máximo de flashcards a devolver</param>
    /// <returns>Flashcards ordenadas por prioridad de repaso</returns>
    public IEnumerable<Flashcard> ObtenerFlashcardsParaRepaso(IEnumerable<Flashcard> flashcards, int limite)
    {
        var hoy = DateTime.Today;
        
        return flashcards
            .Where(f => f.ProximaRevision == null || f.ProximaRevision <= hoy)
            .OrderBy(f => CalcularPrioridadRepaso(f, hoy))
            .Take(limite);
    }

    /// <summary>
    /// Calcula la prioridad de repaso para una flashcard
    /// Valores menores = mayor prioridad
    /// </summary>
    private static double CalcularPrioridadRepaso(Flashcard flashcard, DateTime hoy)
    {
        // Flashcards nunca repasadas tienen máxima prioridad
        if (flashcard.UltimaRevision == null)
            return 0;

        // Flashcards vencidas tienen alta prioridad basada en días de retraso
        if (flashcard.ProximaRevision.HasValue && flashcard.ProximaRevision < hoy)
        {
            var diasRetraso = (hoy - flashcard.ProximaRevision.Value).Days;
            return 1 + diasRetraso; // Mayor retraso = mayor prioridad (menor número)
        }

        // Flashcards con bajo ratio de éxito tienen prioridad media
        var ratioExito = flashcard.VecesVista > 0 
            ? (double)flashcard.VecesCorrecta / flashcard.VecesVista 
            : 0;
        
        return 1000 + ratioExito * 100; // Menor ratio = mayor prioridad
    }
}