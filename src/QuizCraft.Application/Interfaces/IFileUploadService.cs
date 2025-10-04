using QuizCraft.Core.Entities;

namespace QuizCraft.Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de manejo de archivos adjuntos
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Sube un archivo y lo asocia a una flashcard
    /// </summary>
    /// <param name="file">Archivo a subir</param>
    /// <param name="flashcardId">ID de la flashcard</param>
    /// <param name="descripcion">Descripción opcional del archivo</param>
    /// <returns>El archivo adjunto creado</returns>
    Task<ArchivoAdjunto> SubirArchivoAsync(Microsoft.AspNetCore.Http.IFormFile file, int flashcardId, string? descripcion = null);

    /// <summary>
    /// Elimina un archivo adjunto físicamente y de la base de datos
    /// </summary>
    /// <param name="archivoId">ID del archivo a eliminar</param>
    Task EliminarArchivoAsync(int archivoId);

    /// <summary>
    /// Obtiene la ruta física de un archivo
    /// </summary>
    /// <param name="nombreArchivo">Nombre del archivo</param>
    /// <returns>Ruta completa del archivo</returns>
    string ObtenerRutaArchivo(string nombreArchivo);

    /// <summary>
    /// Valida si el tipo de archivo es permitido
    /// </summary>
    /// <param name="file">Archivo a validar</param>
    /// <returns>True si es válido, False si no</returns>
    bool EsTipoArchivoPermitido(Microsoft.AspNetCore.Http.IFormFile file);

    /// <summary>
    /// Obtiene el tipo MIME basado en la extensión del archivo
    /// </summary>
    /// <param name="nombreArchivo">Nombre del archivo</param>
    /// <returns>Tipo MIME</returns>
    string ObtenerTipoMime(string nombreArchivo);

    /// <summary>
    /// Obtiene todos los archivos adjuntos de una flashcard
    /// </summary>
    /// <param name="flashcardId">ID de la flashcard</param>
    /// <returns>Lista de archivos adjuntos</returns>
    Task<IEnumerable<ArchivoAdjunto>> ObtenerArchivosPorFlashcardAsync(int flashcardId);
}