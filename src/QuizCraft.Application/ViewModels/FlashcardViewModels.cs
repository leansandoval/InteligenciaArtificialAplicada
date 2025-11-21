using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Application.ViewModels;

/// <summary>
/// ViewModel para mostrar información de una flashcard
/// </summary>
public class FlashcardViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Pregunta")]
    public string Pregunta { get; set; } = string.Empty;
    
    [Display(Name = "Respuesta")]
    public string Respuesta { get; set; } = string.Empty;
    
    [Display(Name = "Pista")]
    public string? Pista { get; set; }
    
    [Display(Name = "Dificultad")]
    public string DificultadTexto { get; set; } = string.Empty;
    
    [Display(Name = "Materia")]
    public string MateriaNombre { get; set; } = string.Empty;
    
    [Display(Name = "Color")]
    public string MateriaColor { get; set; } = string.Empty;
    
    [Display(Name = "Icono")]
    public string MateriaIcono { get; set; } = string.Empty;
    
    public int MateriaId { get; set; }
    public NivelDificultad Dificultad { get; set; }
    public bool EstaActiva { get; set; }
    
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }
    
    [Display(Name = "Última Modificación")]
    public DateTime? FechaModificacion { get; set; }
    
    [Display(Name = "Veces Repasada")]
    public int VecesRepasada { get; set; }
    
    [Display(Name = "Última Vez Repasada")]
    public DateTime? UltimaVezRepasada { get; set; }
    
    [Display(Name = "Es Importada")]
    public bool EsImportada { get; set; }
    
    // Archivos adjuntos
    public List<ArchivoAdjuntoViewModel> ArchivosAdjuntos { get; set; } = new();
}

/// <summary>
/// ViewModel para crear una nueva flashcard
/// </summary>
public class CreateFlashcardViewModel
{
    [Required(ErrorMessage = "La pregunta es obligatoria")]
    [StringLength(1000, ErrorMessage = "La pregunta no puede exceder los 1000 caracteres")]
    [Display(Name = "Pregunta")]
    public string Pregunta { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La respuesta es obligatoria")]
    [StringLength(2000, ErrorMessage = "La respuesta no puede exceder los 2000 caracteres")]
    [Display(Name = "Respuesta")]
    public string Respuesta { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La pista no puede exceder los 500 caracteres")]
    [Display(Name = "Pista (opcional)")]
    public string? Pista { get; set; }
    
    [Required(ErrorMessage = "Debe seleccionar una materia")]
    [Display(Name = "Materia")]
    public int MateriaId { get; set; }
    
    [Required]
    [Display(Name = "Dificultad")]
    public NivelDificultad Dificultad { get; set; } = NivelDificultad.Intermedio;
    
    [Display(Name = "Etiquetas (separadas por comas)")]
    public string? Etiquetas { get; set; }
    
    // Lista de materias disponibles para el dropdown
    public List<MateriaDropdownViewModel> MateriasDisponibles { get; set; } = new();
    
    // Lista de archivos que se subirán con la flashcard (inicialmente vacía)
    public List<ArchivoAdjuntoViewModel> ArchivosAdjuntos { get; set; } = new();
}

/// <summary>
/// ViewModel para editar una flashcard existente
/// </summary>
public class EditFlashcardViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "La pregunta es obligatoria")]
    [StringLength(1000, ErrorMessage = "La pregunta no puede exceder los 1000 caracteres")]
    [Display(Name = "Pregunta")]
    public string Pregunta { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La respuesta es obligatoria")]
    [StringLength(2000, ErrorMessage = "La respuesta no puede exceder los 2000 caracteres")]
    [Display(Name = "Respuesta")]
    public string Respuesta { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La pista no puede exceder los 500 caracteres")]
    [Display(Name = "Pista (opcional)")]
    public string? Pista { get; set; }
    
    [Required(ErrorMessage = "Debe seleccionar una materia")]
    [Display(Name = "Materia")]
    public int MateriaId { get; set; }
    
    [Required]
    [Display(Name = "Dificultad")]
    public NivelDificultad Dificultad { get; set; }
    
    [Display(Name = "Etiquetas (separadas por comas)")]
    public string? Etiquetas { get; set; }
    
    [Display(Name = "Activa")]
    public bool EstaActiva { get; set; } = true;
    
    // Lista de materias disponibles para el dropdown
    public List<MateriaDropdownViewModel> MateriasDisponibles { get; set; } = new();
    
    // Información adicional para mostrar en la vista
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }
    
    [Display(Name = "Veces Repasada")]
    public int VecesRepasada { get; set; }
    
    // Archivos adjuntos existentes
    public List<ArchivoAdjuntoViewModel> ArchivosAdjuntos { get; set; } = new();
}

/// <summary>
/// ViewModel para el sistema de repaso de flashcards
/// </summary>
public class RepasoFlashcardViewModel
{
    public FlashcardViewModel? FlashcardActual { get; set; }
    public int IndiceActual { get; set; }
    public int TotalFlashcards { get; set; }
    public int MateriaId { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public string MateriaColor { get; set; } = string.Empty;
    public string MateriaIcono { get; set; } = string.Empty;
    public bool MostrarRespuesta { get; set; } = false;
    public List<int> FlashcardIds { get; set; } = new();
    
    // Propiedades calculadas
    public bool EsPrimera => IndiceActual <= 0;
    public bool EsUltima => IndiceActual >= TotalFlashcards - 1;
    public int PorcentajeProgreso => TotalFlashcards > 0 ? (int)Math.Ceiling((double)(IndiceActual + 1) / TotalFlashcards * 100) : 0;
    
    // Estadísticas de la sesión
    public DateTime InicioSesion { get; set; } = DateTime.UtcNow;
    public int FlashcardsCorrectas { get; set; } = 0;
    public int FlashcardsIncorrectas { get; set; } = 0;
    public int FlashcardsRevisadas { get; set; } = 0;
    
    // Propiedades calculadas de estadísticas
    public int TotalRevisadas => FlashcardsCorrectas + FlashcardsIncorrectas;
    public double PorcentajeAcierto => TotalRevisadas > 0 ? (double)FlashcardsCorrectas / TotalRevisadas * 100 : 0;
    public TimeSpan TiempoTranscurrido => DateTime.UtcNow - InicioSesion;
}

/// <summary>
/// ViewModel para configurar una sesión de repaso
/// </summary>
public class ConfigurarRepasoViewModel
{
    [Display(Name = "Materia")]
    public int? MateriaId { get; set; }
    
    [Display(Name = "Número máximo de flashcards")]
    [Range(1, 100, ErrorMessage = "El número debe estar entre 1 y 100")]
    public int MaximoFlashcards { get; set; } = 20;
    
    [Display(Name = "Incluir solo flashcards vencidas")]
    public bool SoloVencidas { get; set; } = true;
    
    [Display(Name = "Mezclar orden")]
    public bool MezclarOrden { get; set; } = true;
    
    [Display(Name = "Incluir pistas")]
    public bool IncluirPistas { get; set; } = true;
    
    [Display(Name = "Dificultad")]
    public NivelDificultad? DificultadFiltro { get; set; }
    
    // Datos para la vista
    public List<MateriaDropdownViewModel> MateriasDisponibles { get; set; } = new();
    public Dictionary<int, int> FlashcardsPorMateria { get; set; } = new();
    public int TotalFlashcardsDisponibles { get; set; }
}

/// <summary>
/// ViewModel para el resultado de una evaluación de flashcard durante repaso
/// </summary>
public class EvaluacionRepasoViewModel
{
    public int FlashcardId { get; set; }
    public bool EsCorrecta { get; set; }
    public int CalidadRespuesta { get; set; } // 0-5 para algoritmo SuperMemo
    public TimeSpan TiempoRespuesta { get; set; }
    public DateTime ProximaRevision { get; set; }
    public string? Comentario { get; set; }
}

/// <summary>
/// ViewModel para mostrar estadísticas de repaso
/// </summary>
public class EstadisticasRepasoViewModel
{
    public int TotalFlashcards { get; set; }
    public int FlashcardsRevisadas { get; set; }
    public int FlashcardsCorrectas { get; set; }
    public int FlashcardsIncorrectas { get; set; }
    public double PorcentajeAcierto { get; set; }
    public TimeSpan TiempoTotal { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string MateriaNombre { get; set; } = string.Empty;
    public string MateriaColor { get; set; } = string.Empty;
    
    // Estadísticas adicionales
    public double TiempoPromedioRespuesta { get; set; }
    public int FlashcardsParaManana { get; set; }
    public int FlashcardsParaSiguienteSemana { get; set; }
    
    // ID del repaso programado asociado (si aplica)
    public int? RepasoProgramadoId { get; set; }
}

/// <summary>
/// ViewModel para el dropdown de materias
/// </summary>
public class MateriaDropdownViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public int CantidadFlashcards { get; set; }
}

/// <summary>
/// ViewModel para filtros de flashcards
/// </summary>
public class FlashcardFiltrosViewModel
{
    [Display(Name = "Materia")]
    public int? MateriaId { get; set; }
    
    [Display(Name = "Dificultad")]
    public int? Dificultad { get; set; }
    
    [Display(Name = "Buscar")]
    public string? TextoBusqueda { get; set; }
    
    [Display(Name = "Solo activas")]
    public bool SoloActivas { get; set; } = true;
    
    [Display(Name = "Ordenar por")]
    public string OrdenarPor { get; set; } = "FechaCreacion";
    
    // Listas para los dropdowns
    public List<MateriaDropdownViewModel> MateriasDisponibles { get; set; } = new();
}

/// <summary>
/// ViewModel para mostrar información de archivos adjuntos
/// </summary>
public class ArchivoAdjuntoViewModel
{
    public int Id { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    // Propiedades calculadas para la vista
    public string TamanoFormateado => FormatearTamano(TamanoBytes);
    public string TipoIcono => ObtenerIconoTipo(TipoMime);
    public bool EsImagen => TipoMime.StartsWith("image/");
    public bool EsVideo => TipoMime.StartsWith("video/");
    public bool EsAudio => TipoMime.StartsWith("audio/");
    public bool EsDocumento => TipoMime.Contains("pdf") || TipoMime.Contains("doc") || TipoMime.Contains("text");
    
    private static string FormatearTamano(long bytes)
    {
        string[] unidades = { "B", "KB", "MB", "GB" };
        double tamano = bytes;
        int unidad = 0;
        
        while (tamano >= 1024 && unidad < unidades.Length - 1)
        {
            tamano /= 1024;
            unidad++;
        }
        
        return $"{tamano:F1} {unidades[unidad]}";
    }
    
    private static string ObtenerIconoTipo(string tipoMime)
    {
        return tipoMime switch
        {
            var mime when mime.StartsWith("image/") => "fas fa-image",
            var mime when mime.StartsWith("video/") => "fas fa-video",
            var mime when mime.StartsWith("audio/") => "fas fa-music",
            var mime when mime.Contains("pdf") => "fas fa-file-pdf",
            var mime when mime.Contains("doc") => "fas fa-file-word",
            var mime when mime.Contains("text") => "fas fa-file-alt",
            _ => "fas fa-file"
        };
    }
}