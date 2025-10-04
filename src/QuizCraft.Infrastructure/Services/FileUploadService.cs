using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Servicio para manejo de archivos adjuntos
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _uploadsPath;
    
    // Tipos de archivo permitidos
    private static readonly string[] TiposPermitidos = 
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", // Imágenes
        ".pdf", ".doc", ".docx", ".txt", ".rtf", // Documentos
        ".mp3", ".wav", ".ogg", ".m4a", // Audio
        ".mp4", ".avi", ".mov", ".webm" // Video
    };

    private static readonly Dictionary<string, string> TiposMime = new()
    {
        // Imágenes
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".webp", "image/webp" },
        
        // Documentos
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".txt", "text/plain" },
        { ".rtf", "application/rtf" },
        
        // Audio
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".ogg", "audio/ogg" },
        { ".m4a", "audio/mp4" },
        
        // Video
        { ".mp4", "video/mp4" },
        { ".avi", "video/x-msvideo" },
        { ".mov", "video/quicktime" },
        { ".webm", "video/webm" }
    };

    public FileUploadService(ApplicationDbContext context, IConfiguration configuration, ILogger<FileUploadService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        
        // Configurar la carpeta de uploads
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        // Crear la carpeta si no existe
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async Task<ArchivoAdjunto> SubirArchivoAsync(IFormFile file, int flashcardId, string? descripcion = null)
    {
        try
        {
            // Validaciones
            if (file == null || file.Length == 0)
                throw new ArgumentException("El archivo no puede estar vacío");

            if (!EsTipoArchivoPermitido(file))
                throw new ArgumentException($"Tipo de archivo no permitido: {Path.GetExtension(file.FileName)}");

            // Verificar que la flashcard existe
            var flashcard = await _context.Flashcards.FindAsync(flashcardId);
            if (flashcard == null)
                throw new ArgumentException($"Flashcard con ID {flashcardId} no encontrada");

            // Generar nombre único para el archivo
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(_uploadsPath, nombreArchivo);

            // Guardar archivo físicamente
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Crear registro en base de datos
            var archivoAdjunto = new ArchivoAdjunto
            {
                NombreOriginal = file.FileName,
                NombreArchivo = nombreArchivo,
                RutaArchivo = $"/uploads/{nombreArchivo}",
                TipoMime = ObtenerTipoMime(file.FileName),
                TipoEntidad = TipoEntidad.Flashcard,
                TamanoBytes = file.Length,
                Descripcion = descripcion,
                FlashcardId = flashcardId,
                FechaCreacion = DateTime.UtcNow,
                EstaActivo = true
            };

            _context.ArchivosAdjuntos.Add(archivoAdjunto);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Archivo {file.FileName} subido exitosamente para flashcard {flashcardId}");

            return archivoAdjunto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al subir archivo {file?.FileName} para flashcard {flashcardId}");
            throw;
        }
    }

    public async Task EliminarArchivoAsync(int archivoId)
    {
        try
        {
            var archivo = await _context.ArchivosAdjuntos.FindAsync(archivoId);
            if (archivo == null)
                throw new ArgumentException($"Archivo con ID {archivoId} no encontrado");

            // Eliminar archivo físico
            var rutaCompleta = Path.Combine(_uploadsPath, archivo.NombreArchivo);
            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
            }

            // Eliminar registro de base de datos
            _context.ArchivosAdjuntos.Remove(archivo);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Archivo {archivo.NombreOriginal} eliminado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar archivo con ID {archivoId}");
            throw;
        }
    }

    public string ObtenerRutaArchivo(string nombreArchivo)
    {
        return Path.Combine(_uploadsPath, nombreArchivo);
    }

    public bool EsTipoArchivoPermitido(IFormFile file)
    {
        if (file == null) return false;
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return TiposPermitidos.Contains(extension);
    }

    public string ObtenerTipoMime(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return TiposMime.TryGetValue(extension, out var tipoMime) ? tipoMime : "application/octet-stream";
    }

    public async Task<IEnumerable<ArchivoAdjunto>> ObtenerArchivosPorFlashcardAsync(int flashcardId)
    {
        return await _context.ArchivosAdjuntos
            .Where(a => a.FlashcardId == flashcardId && a.EstaActivo)
            .OrderBy(a => a.FechaCreacion)
            .ToListAsync();
    }
}