using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.Interfaces;
using QuizCraft.Web.ViewModels;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para manejo de archivos adjuntos
/// </summary>
[Authorize]
public class ArchivosController : Controller
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArchivosController> _logger;

    public ArchivosController(IFileUploadService fileUploadService, ApplicationDbContext context, ILogger<ArchivosController> logger)
    {
        _fileUploadService = fileUploadService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sube un archivo adjunto a una flashcard
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirArchivo(int flashcardId, IFormFile file, string? descripcion = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Debe seleccionar un archivo" });
            }

            if (!_fileUploadService.EsTipoArchivoPermitido(file))
            {
                return Json(new { success = false, message = "Tipo de archivo no permitido" });
            }

            // Verificar tamaño máximo (10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return Json(new { success = false, message = "El archivo no puede superar los 10MB" });
            }

            var archivo = await _fileUploadService.SubirArchivoAsync(file, flashcardId, descripcion);

            return Json(new
            {
                success = true,
                message = "Archivo subido exitosamente",
                archivo = new
                {
                    id = archivo.Id,
                    nombreOriginal = archivo.NombreOriginal,
                    rutaArchivo = archivo.RutaArchivo,
                    tipoMime = archivo.TipoMime,
                    tamanoBytes = archivo.TamanoBytes,
                    descripcion = archivo.Descripcion,
                    fechaCreacion = archivo.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo para flashcard {FlashcardId}", flashcardId);
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un archivo adjunto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarArchivo(int archivoId)
    {
        try
        {
            await _fileUploadService.EliminarArchivoAsync(archivoId);
            return Json(new { success = true, message = "Archivo eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo {ArchivoId}", archivoId);
            return Json(new { success = false, message = "Error al eliminar el archivo" });
        }
    }

    /// <summary>
    /// Obtiene la lista de archivos de una flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerArchivos(int flashcardId)
    {
        try
        {
            var archivos = await _fileUploadService.ObtenerArchivosPorFlashcardAsync(flashcardId);
            
            var resultado = archivos.Select(a => new
            {
                id = a.Id,
                nombreOriginal = a.NombreOriginal,
                rutaArchivo = a.RutaArchivo,
                tipoMime = a.TipoMime,
                tamanoBytes = a.TamanoBytes,
                descripcion = a.Descripcion,
                fechaCreacion = a.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                esImagen = a.TipoMime.StartsWith("image/"),
                esAudio = a.TipoMime.StartsWith("audio/"),
                esVideo = a.TipoMime.StartsWith("video/")
            });

            return Json(new { success = true, archivos = resultado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivos de flashcard {FlashcardId}", flashcardId);
            return Json(new { success = false, message = "Error al cargar los archivos" });
        }
    }

    /// <summary>
    /// Descarga un archivo
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Descargar(int archivoId)
    {
        try
        {
            // Obtener el archivo directamente por ID
            var archivo = await _context.ArchivosAdjuntos.FindAsync(archivoId);
            
            if (archivo == null)
            {
                return NotFound("Archivo no encontrado");
            }

            var rutaArchivo = _fileUploadService.ObtenerRutaArchivo(archivo.NombreArchivo);
            
            if (!System.IO.File.Exists(rutaArchivo))
            {
                return NotFound("El archivo físico no existe");
            }

            var memoria = new MemoryStream();
            using (var stream = new FileStream(rutaArchivo, FileMode.Open))
            {
                await stream.CopyToAsync(memoria);
            }
            memoria.Position = 0;

            return File(memoria, archivo.TipoMime, archivo.NombreOriginal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo {ArchivoId}", archivoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}