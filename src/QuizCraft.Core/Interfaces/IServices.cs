using QuizCraft.Core.Entities;

namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de integración con OpenAI
/// </summary>
public interface IOpenAIService
{
    Task<string> GenerarFlashcardsDesdeTextoAsync(string contenido, string materia);
    Task<string> GenerarFlashcardsDesdeDocumentoAsync(byte[] documento, string tipoArchivo, string materia);
    Task<string> MejorarPreguntaAsync(string pregunta, string respuesta);
    Task<string> GenerarExplicacionAsync(string pregunta, string respuesta);
}

/// <summary>
/// Interfaz para el servicio de procesamiento de documentos
/// </summary>
public interface IDocumentProcessorService
{
    Task<string> ExtraerTextoDesdeAsync(byte[] documento, string tipoArchivo);
    Task<bool> EsFormatoSoportadoAsync(string tipoArchivo);
    Task<byte[]> ConvertirAFormatoCompatibleAsync(byte[] documento, string tipoArchivo);
}

/// <summary>
/// Interfaz para el servicio de algoritmo de repetición espaciada
/// </summary>
public interface IAlgoritmoRepasoService
{
    DateTime CalcularProximaRevision(Flashcard flashcard, bool esCorrecta);
    double ActualizarFactorFacilidad(double factorActual, int calidad);
    int CalcularNuevoIntervalo(int intervaloActual, double factorFacilidad, bool esCorrecta);
    IEnumerable<Flashcard> ObtenerFlashcardsParaRepaso(IEnumerable<Flashcard> flashcards, int limite);
}