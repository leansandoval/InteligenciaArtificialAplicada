using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuizCraft.Application.ViewModels;
using QuizCraft.Application.Interfaces;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AIService = QuizCraft.Application.Interfaces.IAIService;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para la gestión de flashcards
/// </summary>
[Authorize]
public class FlashcardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<FlashcardController> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAlgoritmoRepasoService _algoritmoRepasoService;
    private readonly ApplicationDbContext _context;
    private readonly AIService _aiService;

    public FlashcardController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<FlashcardController> logger,
        IFileUploadService fileUploadService,
        IAlgoritmoRepasoService algoritmoRepasoService,
        ApplicationDbContext context,
        AIService aiService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _fileUploadService = fileUploadService;
        _algoritmoRepasoService = algoritmoRepasoService;
        _context = context;
        _aiService = aiService;
    }

    /// <summary>
    /// Lista todas las flashcards del usuario con filtros
    /// </summary>
    public async Task<IActionResult> Index(FlashcardFiltrosViewModel? filtros)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            filtros ??= new FlashcardFiltrosViewModel();
            
            // Obtener materias del usuario para el filtro
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            var materiasDropdown = materias.Select(m => new MateriaDropdownViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Color = m.Color ?? "#007bff",
                Icono = m.Icono ?? "fas fa-book"
            }).ToList();

            // Obtener todas las flashcards del usuario
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(usuario.Id);
            
            // Aplicar filtros
            if (filtros.MateriaId.HasValue)
            {
                flashcards = flashcards.Where(f => f.MateriaId == filtros.MateriaId.Value);
            }

            if (filtros.Dificultad.HasValue)
            {
                flashcards = flashcards.Where(f => (int)f.Dificultad == filtros.Dificultad.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtros.TextoBusqueda))
            {
                flashcards = flashcards.Where(f => 
                    f.Pregunta.Contains(filtros.TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                    f.Respuesta.Contains(filtros.TextoBusqueda, StringComparison.OrdinalIgnoreCase));
            }

            // Obtener IDs de flashcards importadas por el usuario (con manejo de error si la tabla no existe)
            HashSet<int> idsImportadas = new HashSet<int>();
            try
            {
                var flashcardsImportadas = await _unitOfWork.FlashcardCompartidaRepository
                    .GetImportadasByUsuarioAsync(usuario.Id);
                idsImportadas = new HashSet<int>(flashcardsImportadas.Select(fi => fi.FlashcardId));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener flashcards importadas (posiblemente tabla FlashcardsImportadas no existe aún)");
            }

            // Mapear a ViewModels
            var flashcardViewModels = flashcards.Select(f => new FlashcardViewModel
            {
                Id = f.Id,
                Pregunta = f.Pregunta,
                Respuesta = f.Respuesta,
                Pista = f.Pista,
                DificultadTexto = FormatearDificultad(f.Dificultad),
                MateriaNombre = f.Materia.Nombre,
                MateriaColor = f.Materia.Color ?? "#007bff",
                MateriaIcono = f.Materia.Icono ?? "fas fa-book",
                MateriaId = f.MateriaId,
                Dificultad = f.Dificultad,
                EstaActiva = true, // Por defecto todas están activas ya que la entidad no tiene esta propiedad
                FechaCreacion = f.FechaCreacion,
                FechaModificacion = f.FechaModificacion,
                VecesRepasada = f.VecesVista,
                UltimaVezRepasada = f.UltimaRevision,
                EsImportada = idsImportadas.Contains(f.Id)
            }).ToList();

            // Pasar datos a la vista
            ViewBag.Materias = materiasDropdown;
            ViewBag.MateriaSeleccionada = filtros.MateriaId;
            ViewBag.DificultadSeleccionada = filtros.Dificultad;
            ViewBag.Busqueda = filtros.TextoBusqueda;

            return View(flashcardViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener flashcards del usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al cargar las flashcards. Inténtalo de nuevo.";
            return View(new List<FlashcardViewModel>());
        }
    }

    /// <summary>
    /// Muestra el formulario para crear una nueva flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Obtener materias del usuario
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            
            var viewModel = new CreateFlashcardViewModel
            {
                MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Color = m.Color ?? "#007bff",
                    Icono = m.Icono ?? "fas fa-book"
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar formulario de creación para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al cargar el formulario. Inténtalo de nuevo.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa la creación de una nueva flashcard
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFlashcardViewModel viewModel)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            // Recargar materias si hay errores
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            viewModel.MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Color = m.Color ?? "#007bff",
                Icono = m.Icono ?? "fas fa-book"
            }).ToList();
            return View(viewModel);
        }

        try
        {
            // Verificar que la materia pertenece al usuario
            var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(viewModel.MateriaId);
            if (materia == null || materia.UsuarioId != usuario.Id)
            {
                ModelState.AddModelError("MateriaId", "La materia seleccionada no es válida.");
                return View(viewModel);
            }

            // Crear la flashcard
            var flashcard = new Flashcard
            {
                Pregunta = viewModel.Pregunta.Trim(),
                Respuesta = viewModel.Respuesta.Trim(),
                Pista = string.IsNullOrWhiteSpace(viewModel.Pista) ? null : viewModel.Pista.Trim(),
                Dificultad = viewModel.Dificultad,
                MateriaId = viewModel.MateriaId,
                VecesVista = 0,
                EstaActivo = true // Garantizar que la flashcard esté activa
            };

            await _unitOfWork.FlashcardRepository.AddAsync(flashcard);
            await _unitOfWork.SaveChangesAsync();

            // Si es una petición AJAX, devolver JSON con el ID
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, flashcardId = flashcard.Id, message = "¡Flashcard creada exitosamente!" });
            }

            TempData["Success"] = "¡Flashcard creada exitosamente!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear flashcard para usuario {UserId}", usuario.Id);
            ModelState.AddModelError("", "Error al crear la flashcard. Inténtalo de nuevo.");
            
            // Recargar materias
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            viewModel.MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Color = m.Color ?? "#007bff",
                Icono = m.Icono ?? "fas fa-book"
            }).ToList();
            return View(viewModel);
        }
    }

    /// <summary>
    /// Muestra los detalles de una flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(id);
            
            if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
            {
                TempData["Error"] = "La flashcard no existe o no tienes permisos para verla.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener archivos adjuntos de la flashcard
            var archivosAdjuntos = await _fileUploadService.ObtenerArchivosPorFlashcardAsync(id);

            var viewModel = new FlashcardViewModel
            {
                Id = flashcard.Id,
                Pregunta = flashcard.Pregunta,
                Respuesta = flashcard.Respuesta,
                Pista = flashcard.Pista,
                DificultadTexto = FormatearDificultad(flashcard.Dificultad),
                MateriaNombre = flashcard.Materia.Nombre,
                MateriaColor = flashcard.Materia.Color ?? "#007bff",
                MateriaIcono = flashcard.Materia.Icono ?? "fas fa-book",
                MateriaId = flashcard.MateriaId,
                Dificultad = flashcard.Dificultad,
                EstaActiva = true,
                FechaCreacion = flashcard.FechaCreacion,
                FechaModificacion = flashcard.FechaModificacion,
                VecesRepasada = flashcard.VecesVista,
                UltimaVezRepasada = flashcard.UltimaRevision,
                ArchivosAdjuntos = archivosAdjuntos.Select(a => new ArchivoAdjuntoViewModel
                {
                    Id = a.Id,
                    NombreOriginal = a.NombreOriginal,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    TipoMime = a.TipoMime,
                    TamanoBytes = a.TamanoBytes,
                    Descripcion = a.Descripcion,
                    FechaCreacion = a.FechaCreacion
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener flashcard {Id} para usuario {UserId}", id, usuario.Id);
            TempData["Error"] = "Error al cargar la flashcard.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Muestra el formulario para editar una flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(id);
            
            if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
            {
                TempData["Error"] = "La flashcard no existe o no tienes permisos para editarla.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener materias del usuario
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            
            // Obtener archivos adjuntos de la flashcard
            var archivosAdjuntos = await _fileUploadService.ObtenerArchivosPorFlashcardAsync(id);

            var viewModel = new EditFlashcardViewModel
            {
                Id = flashcard.Id,
                Pregunta = flashcard.Pregunta,
                Respuesta = flashcard.Respuesta,
                Pista = flashcard.Pista,
                MateriaId = flashcard.MateriaId,
                Dificultad = flashcard.Dificultad,
                EstaActiva = true, // Por defecto
                FechaCreacion = flashcard.FechaCreacion,
                VecesRepasada = flashcard.VecesVista,
                MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Color = m.Color ?? "#007bff",
                    Icono = m.Icono ?? "fas fa-book"
                }).ToList(),
                ArchivosAdjuntos = archivosAdjuntos.Select(a => new ArchivoAdjuntoViewModel
                {
                    Id = a.Id,
                    NombreOriginal = a.NombreOriginal,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    TipoMime = a.TipoMime,
                    TamanoBytes = a.TamanoBytes,
                    Descripcion = a.Descripcion,
                    FechaCreacion = a.FechaCreacion
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar flashcard {Id} para edición para usuario {UserId}", id, usuario.Id);
            TempData["Error"] = "Error al cargar la flashcard para edición.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa la edición de una flashcard
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditFlashcardViewModel viewModel)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            // Recargar materias si hay errores
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            viewModel.MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Color = m.Color ?? "#007bff",
                Icono = m.Icono ?? "fas fa-book"
            }).ToList();
            return View(viewModel);
        }

        try
        {
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(viewModel.Id);
            
            if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
            {
                TempData["Error"] = "La flashcard no existe o no tienes permisos para editarla.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que la nueva materia pertenece al usuario
            var nuevaMateria = await _unitOfWork.MateriaRepository.GetByIdAsync(viewModel.MateriaId);
            if (nuevaMateria == null || nuevaMateria.UsuarioId != usuario.Id)
            {
                ModelState.AddModelError("MateriaId", "La materia seleccionada no es válida.");
                return View(viewModel);
            }

            // Actualizar la flashcard
            flashcard.Pregunta = viewModel.Pregunta.Trim();
            flashcard.Respuesta = viewModel.Respuesta.Trim();
            flashcard.Pista = string.IsNullOrWhiteSpace(viewModel.Pista) ? null : viewModel.Pista.Trim();
            flashcard.Dificultad = viewModel.Dificultad;
            flashcard.MateriaId = viewModel.MateriaId;
            flashcard.FechaModificacion = DateTime.UtcNow;

            _unitOfWork.FlashcardRepository.Update(flashcard);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "¡Flashcard actualizada exitosamente!";
            return RedirectToAction(nameof(Details), new { id = flashcard.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar flashcard {Id} para usuario {UserId}", viewModel.Id, usuario.Id);
            ModelState.AddModelError("", "Error al actualizar la flashcard. Inténtalo de nuevo.");
            
            // Recargar materias
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            viewModel.MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Color = m.Color ?? "#007bff",
                Icono = m.Icono ?? "fas fa-book"
            }).ToList();
            return View(viewModel);
        }
    }

    /// <summary>
    /// Muestra la confirmación para eliminar una flashcard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(id);
            
            if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
            {
                TempData["Error"] = "La flashcard no existe o no tienes permisos para eliminarla.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FlashcardViewModel
            {
                Id = flashcard.Id,
                Pregunta = flashcard.Pregunta,
                Respuesta = flashcard.Respuesta,
                Pista = flashcard.Pista,
                DificultadTexto = FormatearDificultad(flashcard.Dificultad),
                MateriaNombre = flashcard.Materia.Nombre,
                MateriaColor = flashcard.Materia.Color ?? "#007bff",
                MateriaIcono = flashcard.Materia.Icono ?? "fas fa-book",
                MateriaId = flashcard.MateriaId,
                Dificultad = flashcard.Dificultad,
                EstaActiva = true,
                FechaCreacion = flashcard.FechaCreacion,
                FechaModificacion = flashcard.FechaModificacion,
                VecesRepasada = flashcard.VecesVista,
                UltimaVezRepasada = flashcard.UltimaRevision
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar flashcard {Id} para eliminación para usuario {UserId}", id, usuario.Id);
            TempData["Error"] = "Error al cargar la flashcard.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa la eliminación de una flashcard
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, IFormCollection form)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(id);
            
            if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
            {
                TempData["Error"] = "La flashcard no existe o no tienes permisos para eliminarla.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.FlashcardRepository.Remove(flashcard);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "¡Flashcard eliminada exitosamente!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar flashcard {Id} para usuario {UserId}", id, usuario.Id);
            TempData["Error"] = "Error al eliminar la flashcard. Inténtalo de nuevo.";
            return RedirectToAction(nameof(Index));
        }
    }

    #region Métodos de Repaso

    /// <summary>
    /// Muestra la página de configuración para iniciar una sesión de repaso
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ConfigurarRepaso(int? materiaId = null, int? flashcardId = null)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Obtener materias del usuario
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            
            // Obtener cantidad de flashcards disponibles para repaso por materia
            var flashcardsPorMateria = new Dictionary<int, int>();
            var totalFlashcards = 0;

            foreach (var materia in materias)
            {
                var cantidad = await _unitOfWork.FlashcardRepository.GetCantidadFlashcardsParaRepasoAsync(usuario.Id, materia.Id);
                flashcardsPorMateria[materia.Id] = cantidad;
                totalFlashcards += cantidad;
            }

            var viewModel = new ConfigurarRepasoViewModel
            {
                MateriasDisponibles = materias.Select(m => new MateriaDropdownViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Color = m.Color ?? "#007bff",
                    Icono = m.Icono ?? "fas fa-book",
                    CantidadFlashcards = flashcardsPorMateria.GetValueOrDefault(m.Id, 0)
                }).ToList(),
                FlashcardsPorMateria = flashcardsPorMateria,
                TotalFlashcardsDisponibles = totalFlashcards,
                MaximoFlashcards = Math.Min(20, totalFlashcards)
            };

            // Si viene de un repaso programado, pre-configurar con los parámetros recibidos
            if (materiaId.HasValue)
            {
                viewModel.MateriaId = materiaId.Value;
                if (flashcardId.HasValue)
                {
                    // Si viene con una flashcard específica, configurar para una sola flashcard
                    viewModel.MaximoFlashcards = 1;
                }
                else
                {
                    // Si viene con una materia, configurar para todas las flashcards de esa materia
                    var cantidadMateria = flashcardsPorMateria.GetValueOrDefault(materiaId.Value, 0);
                    viewModel.MaximoFlashcards = Math.Min(cantidadMateria, 20);
                }
                
                // Si hay TempData de repaso, indicar que viene de repaso programado
                if (TempData.ContainsKey("RepasoId"))
                {
                    ViewBag.EsRepasoProgamado = true;
                    TempData.Keep("RepasoId");
                    TempData.Keep("RepasoTitulo");
                }
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al configurar repaso para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al cargar la configuración de repaso. Inténtalo de nuevo.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Inicia una sesión de repaso con la configuración especificada
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarRepaso(ConfigurarRepasoViewModel configuracion)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            // Recargar datos necesarios para la vista
            return await ConfigurarRepaso();
        }

        try
        {
            // Obtener flashcards para repaso
            // En lugar de usar GetFlashcardsParaRepasoAsync que filtra por fecha,
            // obtener todas las flashcards y dejar que el usuario elija cuántas quiere repasar
            IEnumerable<Flashcard> flashcardsParaRepaso;
            
            if (configuracion.MateriaId.HasValue)
            {
                flashcardsParaRepaso = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(configuracion.MateriaId.Value);
            }
            else
            {
                var todasMaterias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
                var materiaIds = todasMaterias.Select(m => m.Id).ToList();
                
                var flashcardsListas = await Task.WhenAll(
                    materiaIds.Select(id => _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(id))
                );
                flashcardsParaRepaso = flashcardsListas.SelectMany(f => f);
            }

            // Aplicar filtros adicionales
            if (configuracion.DificultadFiltro.HasValue)
            {
                flashcardsParaRepaso = flashcardsParaRepaso.Where(f => f.Dificultad == configuracion.DificultadFiltro.Value);
            }

            // Usar algoritmo de repaso para priorizar las que necesitan repaso
            // pero luego completar con el resto si es necesario
            var flashcardsPriorizadas = _algoritmoRepasoService.ObtenerFlashcardsParaRepaso(
                flashcardsParaRepaso, configuracion.MaximoFlashcards);

            var flashcardsLista = flashcardsPriorizadas.ToList();
            
            // Si no hay suficientes flashcards priorizadas, agregar más de todas las disponibles
            if (flashcardsLista.Count < configuracion.MaximoFlashcards)
            {
                var idsPriorizadas = flashcardsLista.Select(f => f.Id).ToHashSet();
                var flashcardsRestantes = flashcardsParaRepaso
                    .Where(f => !idsPriorizadas.Contains(f.Id))
                    .Take(configuracion.MaximoFlashcards - flashcardsLista.Count);
                flashcardsLista.AddRange(flashcardsRestantes);
            }

            if (!flashcardsLista.Any())
            {
                TempData["Info"] = "No hay flashcards disponibles para repaso en este momento.";
                return RedirectToAction(nameof(ConfigurarRepaso));
            }

            // Mezclar si está habilitado
            if (configuracion.MezclarOrden)
            {
                flashcardsLista = flashcardsLista.OrderBy(x => Guid.NewGuid()).ToList();
            }

            // Crear sesión de repaso
            var materia = configuracion.MateriaId.HasValue 
                ? await _unitOfWork.MateriaRepository.GetByIdAsync(configuracion.MateriaId.Value)
                : null;

            var flashcardActual = flashcardsLista.First();
            var repasoViewModel = new RepasoFlashcardViewModel
            {
                FlashcardActual = new FlashcardViewModel
                {
                    Id = flashcardActual.Id,
                    Pregunta = flashcardActual.Pregunta,
                    Respuesta = flashcardActual.Respuesta,
                    Pista = configuracion.IncluirPistas ? flashcardActual.Pista : null,
                    DificultadTexto = FormatearDificultad(flashcardActual.Dificultad),
                    MateriaNombre = flashcardActual.Materia.Nombre,
                    MateriaColor = flashcardActual.Materia.Color ?? "#007bff",
                    MateriaIcono = flashcardActual.Materia.Icono ?? "fas fa-book",
                    MateriaId = flashcardActual.MateriaId,
                    Dificultad = flashcardActual.Dificultad
                },
                IndiceActual = 0,
                TotalFlashcards = flashcardsLista.Count,
                MateriaId = configuracion.MateriaId ?? 0,
                MateriaNombre = materia?.Nombre ?? "Todas las materias",
                MateriaColor = materia?.Color ?? "#007bff",
                MateriaIcono = materia?.Icono ?? "fas fa-book",
                FlashcardIds = flashcardsLista.Select(f => f.Id).ToList(),
                InicioSesion = DateTime.UtcNow
            };

            // Guardar configuración en TempData para mantener estado
            TempData["RepasoConfig"] = System.Text.Json.JsonSerializer.Serialize(configuracion);
            
            // Mantener información del repaso programado si existe
            if (TempData.ContainsKey("RepasoId"))
            {
                TempData.Keep("RepasoId");
                TempData.Keep("RepasoTitulo");
            }

            return View("Repaso", repasoViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar repaso para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al iniciar el repaso. Inténtalo de nuevo.";
            return RedirectToAction(nameof(ConfigurarRepaso));
        }
    }

    /// <summary>
    /// Inicia un repaso directo para repasos programados (sin configuración)
    /// </summary>
    public async Task<IActionResult> IniciarRepasoDirecto(int? materiaId = null, int? flashcardId = null, int? repasoId = null)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Guardar información del repaso programado
            if (repasoId.HasValue)
            {
                TempData["RepasoId"] = repasoId.Value;
                // Obtener título del repaso si es necesario
                var repasoInfo = await _context.RepasosProgramados.FindAsync(repasoId.Value);
                if (repasoInfo != null)
                {
                    TempData["RepasoTitulo"] = repasoInfo.Titulo;
                }
            }

            IEnumerable<Flashcard> flashcardsParaRepaso;

            if (flashcardId.HasValue)
            {
                // Repaso de una flashcard específica
                var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(flashcardId.Value);
                if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
                {
                    TempData["Error"] = "Flashcard no encontrada.";
                    return RedirectToAction("Index", "Repaso");
                }
                flashcardsParaRepaso = new[] { flashcard };
            }
            else if (materiaId.HasValue)
            {
                // Para repasos programados, obtener todas las flashcards de la materia del usuario
                var todasFlashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(materiaId.Value);
                // Filtrar por usuario para seguridad
                todasFlashcards = todasFlashcards.Where(f => f.Materia.UsuarioId == usuario.Id);
                
                if (!todasFlashcards.Any())
                {
                    TempData["Info"] = "No hay flashcards en esta materia para repasar.";
                    return RedirectToAction("Index", "Repaso");
                }
                
                // Para repasos programados, incluir TODAS las flashcards sin filtrar por fecha
                // Priorizar las que necesitan repaso pero incluir todas
                var flashcardsPriorizadas = _algoritmoRepasoService.ObtenerFlashcardsParaRepaso(
                    todasFlashcards, todasFlashcards.Count());
                
                // Si hay flashcards priorizadas por el algoritmo, usarlas primero y luego agregar el resto
                if (flashcardsPriorizadas.Any())
                {
                    var idsPriorizadas = flashcardsPriorizadas.Select(f => f.Id).ToHashSet();
                    var flashcardsRestantes = todasFlashcards.Where(f => !idsPriorizadas.Contains(f.Id));
                    flashcardsParaRepaso = flashcardsPriorizadas.Concat(flashcardsRestantes).Take(20);
                }
                else
                {
                    // Si el algoritmo no devuelve ninguna (todas tienen fecha futura), tomar todas
                    flashcardsParaRepaso = todasFlashcards.Take(20);
                }
            }
            else
            {
                TempData["Error"] = "No se especificó contenido para el repaso.";
                return RedirectToAction("Index", "Repaso");
            }

            var flashcardsLista = flashcardsParaRepaso.ToList();

            if (!flashcardsLista.Any())
            {
                TempData["Info"] = "No hay flashcards disponibles para repaso en este momento.";
                return RedirectToAction("Index", "Repaso");
            }

            // Crear sesión de repaso
            var materia = materiaId.HasValue 
                ? await _unitOfWork.MateriaRepository.GetByIdAsync(materiaId.Value)
                : flashcardsLista.First().Materia;

            var flashcardActual = flashcardsLista.First();

            var repasoViewModel = new RepasoFlashcardViewModel
            {
                FlashcardActual = new FlashcardViewModel
                {
                    Id = flashcardActual.Id,
                    Pregunta = flashcardActual.Pregunta,
                    Respuesta = flashcardActual.Respuesta,
                    Pista = flashcardActual.Pista, // Incluir pistas por defecto
                    DificultadTexto = FormatearDificultad(flashcardActual.Dificultad),
                    MateriaNombre = flashcardActual.Materia.Nombre,
                    MateriaColor = flashcardActual.Materia.Color ?? "#007bff",
                    MateriaIcono = flashcardActual.Materia.Icono ?? "fas fa-book",
                    MateriaId = flashcardActual.MateriaId,
                    Dificultad = flashcardActual.Dificultad
                },
                IndiceActual = 0,
                TotalFlashcards = flashcardsLista.Count,
                MateriaId = materia?.Id ?? 0,
                MateriaNombre = materia?.Nombre ?? "Todas las materias",
                MateriaColor = materia?.Color ?? "#007bff",
                MateriaIcono = materia?.Icono ?? "fas fa-book",
                FlashcardsCorrectas = 0,
                FlashcardsIncorrectas = 0,
                FlashcardIds = flashcardsLista.Select(f => f.Id).ToList(),
                InicioSesion = DateTime.UtcNow
            };

            // Configuración por defecto para repaso programado
            var configuracionDefecto = new ConfigurarRepasoViewModel
            {
                MateriaId = materiaId,
                MaximoFlashcards = flashcardsLista.Count,
                IncluirPistas = true,
                MezclarOrden = false
            };

            // Guardar configuración en TempData
            TempData["RepasoConfig"] = System.Text.Json.JsonSerializer.Serialize(configuracionDefecto);

            return View("Repaso", repasoViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar repaso directo para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al iniciar el repaso. Inténtalo de nuevo.";
            return RedirectToAction("Index", "Repaso");
        }
    }

    /// <summary>
    /// Procesa la evaluación de una flashcard durante el repaso
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EvaluarFlashcard(EvaluacionRepasoViewModel evaluacion)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Json(new { success = false, message = "Usuario no encontrado" });
        }

        try
        {
            // Obtener la flashcard
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(evaluacion.FlashcardId);
            if (flashcard == null || flashcard.Materia.UsuarioId != usuario.Id)
            {
                return Json(new { success = false, message = "Flashcard no encontrada" });
            }

            // Actualizar estadísticas usando el algoritmo de repetición espaciada
            var nuevaProximaRevision = _algoritmoRepasoService.CalcularProximaRevision(flashcard, evaluacion.EsCorrecta);
            var nuevoFactor = _algoritmoRepasoService.ActualizarFactorFacilidad(flashcard.FactorFacilidad, evaluacion.CalidadRespuesta);
            var nuevoIntervalo = _algoritmoRepasoService.CalcularNuevoIntervalo(flashcard.IntervaloRepeticion, nuevoFactor, evaluacion.EsCorrecta);

            // Actualizar la flashcard
            flashcard.ProximaRevision = nuevaProximaRevision;
            flashcard.FactorFacilidad = nuevoFactor;
            flashcard.IntervaloRepeticion = nuevoIntervalo;

            // Actualizar estadísticas de repaso
            await _unitOfWork.FlashcardRepository.ActualizarEstadisticasRepasoAsync(
                evaluacion.FlashcardId, evaluacion.EsCorrecta, evaluacion.TiempoRespuesta);

            // Crear estadística de estudio
            await CrearEstadisticaEstudio(usuario.Id, flashcard.MateriaId, evaluacion.EsCorrecta);

            await _unitOfWork.SaveChangesAsync();

            return Json(new { 
                success = true, 
                proximaRevision = nuevaProximaRevision.ToString("dd/MM/yyyy"),
                nuevoIntervalo = nuevoIntervalo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al evaluar flashcard {FlashcardId} para usuario {UserId}", 
                evaluacion.FlashcardId, usuario.Id);
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene la siguiente flashcard en la sesión de repaso
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SiguienteFlashcard([FromBody] RepasoFlashcardViewModel repasoActual)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Json(new { success = false, message = "Usuario no encontrado" });
        }

        try
        {
            var siguienteIndice = repasoActual.IndiceActual + 1;
            
            if (siguienteIndice >= repasoActual.FlashcardIds.Count)
            {
                // Mantener información del repaso programado antes de terminar la sesión
                if (TempData.ContainsKey("RepasoId"))
                {
                    TempData.Keep("RepasoId");
                    TempData.Keep("RepasoTitulo");
                }
                
                // Sesión terminada
                return Json(new { 
                    success = true, 
                    terminada = true,
                    estadisticas = new {
                        totalRevisadas = repasoActual.TotalRevisadas,
                        porcentajeAcierto = repasoActual.PorcentajeAcierto,
                        tiempoTotal = repasoActual.TiempoTranscurrido.ToString(@"mm\:ss")
                    }
                });
            }

            // Obtener siguiente flashcard
            var flashcardId = repasoActual.FlashcardIds[siguienteIndice];
            var flashcard = await _unitOfWork.FlashcardRepository.GetByIdWithMateriaAsync(flashcardId);
            
            if (flashcard == null)
            {
                return Json(new { success = false, message = "Flashcard no encontrada" });
            }

            // Recuperar configuración de TempData
            var incluirPistas = true; // Por defecto incluir pistas
            if (TempData.ContainsKey("RepasoConfig"))
            {
                var configJson = TempData["RepasoConfig"]?.ToString();
                if (!string.IsNullOrEmpty(configJson))
                {
                    var config = System.Text.Json.JsonSerializer.Deserialize<ConfigurarRepasoViewModel>(configJson);
                    incluirPistas = config?.IncluirPistas ?? true;
                    TempData.Keep("RepasoConfig"); // Mantener para próxima llamada
                }
            }
            
            // Mantener información del repaso programado si existe
            if (TempData.ContainsKey("RepasoId"))
            {
                TempData.Keep("RepasoId");
                TempData.Keep("RepasoTitulo");
            }

            var flashcardViewModel = new FlashcardViewModel
            {
                Id = flashcard.Id,
                Pregunta = flashcard.Pregunta,
                Respuesta = flashcard.Respuesta,
                Pista = incluirPistas ? flashcard.Pista : null,
                DificultadTexto = FormatearDificultad(flashcard.Dificultad),
                MateriaNombre = flashcard.Materia.Nombre,
                MateriaColor = flashcard.Materia.Color ?? "#007bff",
                MateriaIcono = flashcard.Materia.Icono ?? "fas fa-book",
                MateriaId = flashcard.MateriaId,
                Dificultad = flashcard.Dificultad
            };

            return Json(new { 
                success = true, 
                terminada = false,
                flashcard = flashcardViewModel,
                indice = siguienteIndice,
                porcentajeProgreso = (int)Math.Ceiling((double)(siguienteIndice + 1) / repasoActual.FlashcardIds.Count * 100)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener siguiente flashcard para usuario {UserId}", usuario.Id);
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Finaliza la sesión de repaso y muestra estadísticas
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> FinalizarRepaso(EstadisticasRepasoViewModel estadisticas, string flashcardsIncorrectasIds)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Obtener el RepasoId si viene de TempData
            if (TempData.ContainsKey("RepasoId") && TempData["RepasoId"] is int repasoId)
            {
                estadisticas.RepasoProgramadoId = repasoId;
                TempData.Keep("RepasoId"); // Mantener para la siguiente acción
            }
            
            // Guardar las estadísticas y flashcards incorrectas en TempData para mostrarlas
            TempData["EstadisticasRepaso"] = System.Text.Json.JsonSerializer.Serialize(estadisticas);
            TempData["FlashcardsIncorrectasIds"] = flashcardsIncorrectasIds;
            
            return View("EstadisticasRepaso", estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al finalizar repaso para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al procesar las estadísticas del repaso.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Marca el repaso como completado y guarda las estadísticas
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarRepasoCompletado(EstadisticasRepasoViewModel estadisticas, string flashcardsIncorrectasIds)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Crear estadística general de la sesión
            var estadisticaEstudio = new EstadisticaEstudio
            {
                UsuarioId = usuario.Id,
                MateriaId = estadisticas.MateriaNombre != "Todas las materias" ? (int?)null : null,
                Fecha = DateTime.Today,
                TipoActividad = TipoActividad.RepeticionEspaciada,
                FlashcardsRevisadas = estadisticas.FlashcardsRevisadas,
                FlashcardsCorrectas = estadisticas.FlashcardsCorrectas,
                FlashcardsIncorrectas = estadisticas.FlashcardsIncorrectas,
                TiempoEstudio = estadisticas.TiempoTotal,
                TiempoEstudioMinutos = (int)estadisticas.TiempoTotal.TotalMinutes,
                PromedioAcierto = estadisticas.PorcentajeAcierto
            };

            await _context.EstadisticasEstudio.AddAsync(estadisticaEstudio);
            
            // Si viene de un repaso programado, marcarlo como completado
            if (estadisticas.RepasoProgramadoId.HasValue)
            {
                var repasoProgramado = await _context.RepasosProgramados.FindAsync(estadisticas.RepasoProgramadoId.Value);
                if (repasoProgramado != null && repasoProgramado.UsuarioId == usuario.Id)
                {
                    repasoProgramado.Completado = true;
                    repasoProgramado.FechaCompletado = DateTime.Now;
                    repasoProgramado.UltimoPuntaje = estadisticas.PorcentajeAcierto;
                    _context.RepasosProgramados.Update(repasoProgramado);
                }
            }
            
            // Si hay flashcards incorrectas, crear un repaso programado para mañana
            if (!string.IsNullOrEmpty(flashcardsIncorrectasIds) && estadisticas.FlashcardsIncorrectas > 0)
            {
                var flashcardIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(flashcardsIncorrectasIds);
                
                if (flashcardIds != null && flashcardIds.Count > 0)
                {
                    // Crear un repaso para cada flashcard incorrecta
                    foreach (var flashcardId in flashcardIds)
                    {
                        var nuevoRepaso = new RepasoProgramado
                        {
                            UsuarioId = usuario.Id,
                            Titulo = $"Repaso de flashcard incorrecta",
                            Descripcion = $"Repaso automático generado desde sesión del {DateTime.Now:dd/MM/yyyy}.",
                            FechaProgramada = DateTime.Today.AddDays(1).AddHours(9), // Mañana a las 9 AM
                            Completado = false,
                            TipoRepaso = TipoRepaso.Automatico,
                            NotificacionesHabilitadas = true,
                            MinutosNotificacionPrevia = 15,
                            FlashcardId = flashcardId
                        };

                        await _context.RepasosProgramados.AddAsync(nuevoRepaso);
                    }
                }
            }
            
            await _unitOfWork.SaveChangesAsync();

            var mensaje = $"¡Repaso completado! Revisaste {estadisticas.FlashcardsRevisadas} flashcards con {estadisticas.PorcentajeAcierto:F1}% de acierto.";
            if (estadisticas.FlashcardsIncorrectas > 0)
            {
                mensaje += $" Se creó un repaso programado para mañana con las {estadisticas.FlashcardsIncorrectas} flashcard(s) incorrecta(s).";
            }
            
            TempData["Success"] = mensaje;
            
            return RedirectToAction("Index", "Repaso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar repaso como completado para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al guardar las estadísticas del repaso.";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region Métodos Privados de Apoyo

    /// <summary>
    /// Crea una estadística de estudio individual
    /// </summary>
    private async Task CrearEstadisticaEstudio(string usuarioId, int materiaId, bool esCorrecta)
    {
        var estadisticaHoy = await _context.EstadisticasEstudio
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && 
                                       e.MateriaId == materiaId && 
                                       e.Fecha == DateTime.Today &&
                                       e.TipoActividad == TipoActividad.RepeticionEspaciada);

        if (estadisticaHoy == null)
        {
            estadisticaHoy = new EstadisticaEstudio
            {
                UsuarioId = usuarioId,
                MateriaId = materiaId,
                Fecha = DateTime.Today,
                TipoActividad = TipoActividad.RepeticionEspaciada
            };
            await _context.EstadisticasEstudio.AddAsync(estadisticaHoy);
        }

        estadisticaHoy.FlashcardsRevisadas++;
        if (esCorrecta)
            estadisticaHoy.FlashcardsCorrectas++;
        else
            estadisticaHoy.FlashcardsIncorrectas++;

        estadisticaHoy.PromedioAcierto = estadisticaHoy.FlashcardsRevisadas > 0 
            ? (double)estadisticaHoy.FlashcardsCorrectas / estadisticaHoy.FlashcardsRevisadas * 100 
            : 0;
    }

    #endregion

    #region Generación con IA

    /// <summary>
    /// Muestra el formulario para generar flashcards con IA
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GenerateWithAI(int? materiaId)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Challenge();
        }

        var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
        
        var viewModel = new GenerateFlashcardsWithAIViewModel
        {
            MateriaId = materiaId,
            Materias = materias.ToList(),
            CantidadFlashcards = 5,
            NivelDificultad = NivelDificultad.Intermedio
        };

        return View(viewModel);
    }

    /// <summary>
    /// Procesa la generación de flashcards con IA
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateWithAI(GenerateFlashcardsWithAIViewModel model)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            model.Materias = materias.ToList();
            return View(model);
        }

        try
        {
            var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(model.MateriaId ?? 0);
            if (materia == null || materia.UsuarioId != usuario.Id)
            {
                ModelState.AddModelError("", "Materia no encontrada");
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
                model.Materias = materias.ToList();
                return View(model);
            }

            string contenido = model.Contenido ?? string.Empty;

            if (model.ArchivoPDF != null && model.ArchivoPDF.Length > 0)
            {
                _logger.LogWarning("Carga de PDF no implementada aún, usando contenido de texto");
            }

            if (string.IsNullOrWhiteSpace(contenido))
            {
                ModelState.AddModelError("", "Debe proporcionar contenido o subir un archivo");
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
                model.Materias = materias.ToList();
                return View(model);
            }

            var settings = new QuizCraft.Application.Interfaces.AIGenerationSettings
            {
                MaxCardsPerDocument = model.CantidadFlashcards ?? 5,
                Difficulty = model.NivelDificultad.ToString(),
                IncludeExplanations = true
            };

            var response = await _aiService.GenerateFlashcardsFromTextAsync(contenido, settings);

            if (!response.Success)
            {
                var errorMessage = response.ErrorMessage ?? "Servicio IA no disponible";
                _logger.LogWarning("AI service returned error: {Error}", errorMessage);
                
                TempData["Error"] = $"Error al generar flashcards con IA:\n\n{errorMessage}";
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
                model.Materias = materias.ToList();
                return View(model);
            }

            // Parsear el JSON de respuesta para extraer flashcards
            int flashcardsCreadas = 0;
            try
            {
                // El contenido puede venir en formato JSON
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    // Intentar parsear como array de flashcards
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    
                    // Intentar diferentes formatos de respuesta
                    JsonDocument doc = JsonDocument.Parse(response.Content);
                    JsonElement root = doc.RootElement;
                    
                    JsonElement flashcardsElement;
                    
                    // Verificar si tiene una propiedad "flashcards" o es directamente un array
                    if (root.TryGetProperty("flashcards", out flashcardsElement) || 
                        root.TryGetProperty("Flashcards", out flashcardsElement))
                    {
                        // Caso: { "flashcards": [...] }
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        // Caso: [...] directamente
                        flashcardsElement = root;
                    }
                    else
                    {
                        throw new InvalidOperationException("Formato de respuesta JSON no reconocido");
                    }

                    foreach (JsonElement item in flashcardsElement.EnumerateArray())
                    {
                        // Intentar extraer pregunta y respuesta con diferentes nombres de propiedad
                        string pregunta = GetJsonStringProperty(item, "pregunta", "question", "Pregunta", "Question") ?? "";
                        string respuesta = GetJsonStringProperty(item, "respuesta", "answer", "Respuesta", "Answer") ?? "";

                        if (!string.IsNullOrWhiteSpace(pregunta) && !string.IsNullOrWhiteSpace(respuesta))
                        {
                            var flashcard = new Flashcard
                            {
                                Pregunta = pregunta,
                                Respuesta = respuesta,
                                Dificultad = model.NivelDificultad,
                                MateriaId = materia.Id,
                                FechaCreacion = DateTime.UtcNow,
                                EstaActivo = true
                            };

                            await _unitOfWork.FlashcardRepository.AddAsync(flashcard);
                            flashcardsCreadas++;
                        }
                    }

                    if (flashcardsCreadas > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }

                if (flashcardsCreadas == 0)
                {
                    // Si no se pudo parsear JSON, crear flashcards de ejemplo basadas en el contenido
                    _logger.LogWarning("No se pudieron parsear flashcards del JSON, usando modo fallback");
                    flashcardsCreadas = await CrearFlashcardsDeEjemplo(contenido, materia, usuario, model.CantidadFlashcards ?? 5);
                }

                TempData["SuccessMessage"] = $"Se generaron {flashcardsCreadas} flashcard(s) con IA exitosamente";
                return RedirectToAction("Details", "Materia", new { id = materia.Id });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear JSON de respuesta de IA");
                
                // Fallback: crear flashcards de ejemplo
                flashcardsCreadas = await CrearFlashcardsDeEjemplo(contenido, materia, usuario, model.CantidadFlashcards ?? 5);
                
                TempData["SuccessMessage"] = $"Se generaron {flashcardsCreadas} flashcard(s) (modo básico)";
                TempData["InfoMessage"] = "La IA generó contenido pero en formato no estructurado";
                return RedirectToAction("Details", "Materia", new { id = materia.Id });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar flashcards con IA");
            ModelState.AddModelError("", "Ocurrió un error al generar las flashcards");
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);
            model.Materias = materias.ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Método auxiliar para obtener una propiedad string de un JsonElement con múltiples nombres posibles
    /// </summary>
    private string? GetJsonStringProperty(JsonElement element, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (element.TryGetProperty(name, out JsonElement property))
            {
                return property.GetString();
            }
        }
        return null;
    }

    /// <summary>
    /// Crea flashcards de ejemplo cuando no se puede parsear la respuesta de IA
    /// </summary>
    private async Task<int> CrearFlashcardsDeEjemplo(string contenido, Materia materia, ApplicationUser usuario, int cantidad)
    {
        int creadas = 0;
        var lineas = contenido.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(l => l.Length > 10)
                             .Take(cantidad * 2)
                             .ToList();

        for (int i = 0; i < Math.Min(cantidad, lineas.Count / 2); i++)
        {
            var flashcard = new Flashcard
            {
                Pregunta = $"Pregunta sobre: {lineas[i * 2].Substring(0, Math.Min(50, lineas[i * 2].Length))}...",
                Respuesta = lineas[i * 2 + 1].Length > 200 ? lineas[i * 2 + 1].Substring(0, 200) + "..." : lineas[i * 2 + 1],
                Dificultad = NivelDificultad.Intermedio,
                MateriaId = materia.Id,
                FechaCreacion = DateTime.UtcNow,
                EstaActivo = true
            };

            await _unitOfWork.FlashcardRepository.AddAsync(flashcard);
            creadas++;
        }

        if (creadas > 0)
        {
            await _context.SaveChangesAsync();
        }

        return creadas;
    }

    #endregion

    #region Generación de PDF

    /// <summary>
    /// Genera un PDF con las flashcards filtradas
    /// </summary>
    public async Task<IActionResult> DownloadPdf(int? materiaId, bool incluirRespuestas = true)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Obtener flashcards del usuario
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByUsuarioIdAsync(usuario.Id);
            
            // Filtrar por materia si se especifica
            if (materiaId.HasValue)
            {
                flashcards = flashcards.Where(f => f.MateriaId == materiaId.Value);
            }

            var flashcardsList = flashcards.OrderBy(f => f.Materia.Nombre).ThenBy(f => f.FechaCreacion).ToList();

            if (!flashcardsList.Any())
            {
                TempData["Error"] = "No hay flashcards para generar el PDF.";
                return RedirectToAction(nameof(Index), new { materiaId });
            }

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Mis Flashcards")
                                .FontSize(20).Bold().FontColor("#2c3e50");
                            
                            if (materiaId.HasValue && flashcardsList.Any())
                            {
                                column.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Materia: ").Bold();
                                    text.Span(flashcardsList.First().Materia?.Nombre ?? "Sin materia");
                                });
                            }
                            else
                            {
                                column.Item().PaddingTop(5).Text("Todas las materias").Bold();
                            }
                            
                            column.Item().PaddingTop(5).Text(text =>
                            {
                                text.Span("Total de flashcards: ").Bold();
                                text.Span($"{flashcardsList.Count}");
                            });
                            
                            column.Item().PaddingTop(10).LineHorizontal(1).LineColor("#bdc3c7");
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            string? materiaActual = null;
                            
                            for (int i = 0; i < flashcardsList.Count; i++)
                            {
                                var flashcard = flashcardsList[i];
                                
                                // Encabezado de materia si cambia
                                if (!materiaId.HasValue && flashcard.Materia?.Nombre != materiaActual)
                                {
                                    materiaActual = flashcard.Materia?.Nombre;
                                    
                                    if (i > 0)
                                    {
                                        column.Item().PaddingTop(15);
                                    }
                                    
                                    column.Item().Background("#3498db").Padding(8).Text(materiaActual ?? "Sin materia")
                                        .FontSize(14).Bold().FontColor("#ffffff");
                                    
                                    column.Item().PaddingBottom(10);
                                }
                                
                                column.Item().PaddingTop(10).Column(flashcardColumn =>
                                {
                                    // Número y pregunta
                                    flashcardColumn.Item().Background("#ecf0f1").Padding(10).Column(preguntaBox =>
                                    {
                                        preguntaBox.Item().Text($"Flashcard {i + 1}")
                                            .FontSize(12).Bold().FontColor("#2c3e50");
                                        
                                        preguntaBox.Item().PaddingTop(5).Text(flashcard.Pregunta)
                                            .FontSize(11).FontColor("#34495e");
                                        
                                        // Pista si existe
                                        if (!string.IsNullOrEmpty(flashcard.Pista))
                                        {
                                            preguntaBox.Item().PaddingTop(5).Text(text =>
                                            {
                                                text.Span("💡 Pista: ").Bold().FontColor("#f39c12");
                                                text.Span(flashcard.Pista).Italic().FontColor("#7f8c8d");
                                            });
                                        }
                                    });

                                    // Respuesta (solo si incluirRespuestas es true)
                                    if (incluirRespuestas)
                                    {
                                        flashcardColumn.Item().PaddingTop(5).Background("#d5f4e6")
                                            .Padding(10).Column(respuestaBox =>
                                            {
                                                respuestaBox.Item().Text("Respuesta:")
                                                    .FontSize(11).Bold().FontColor("#27ae60");
                                                
                                                respuestaBox.Item().PaddingTop(3).Text(flashcard.Respuesta)
                                                    .FontSize(11).FontColor("#2c3e50");
                                            });
                                    }
                                    
                                    // Información adicional
                                    flashcardColumn.Item().PaddingTop(3).Text(text =>
                                    {
                                        text.Span("Dificultad: ").FontSize(9).FontColor("#7f8c8d");
                                        text.Span(FormatearDificultad(flashcard.Dificultad)).FontSize(9).FontColor("#7f8c8d");
                                    });
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();
            var materiaNombre = materiaId.HasValue && flashcardsList.Any() 
                ? flashcardsList.First().Materia?.Nombre?.Replace(" ", "_") 
                : "Todas";
            var sufijo = incluirRespuestas ? "_con_respuestas" : "_solo_preguntas";
            var fileName = $"Flashcards_{materiaNombre}{sufijo}_{DateTime.Now:yyyyMMdd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF de flashcards para usuario {UserId}", usuario.Id);
            TempData["Error"] = "Error al generar el PDF. Por favor, intenta nuevamente.";
            return RedirectToAction(nameof(Index), new { materiaId });
        }
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Formatea el texto de la dificultad para mostrar con espacios y tildes
    /// </summary>
    private string FormatearDificultad(NivelDificultad dificultad)
    {
        return dificultad switch
        {
            NivelDificultad.MuyFacil => "Muy Fácil",
            NivelDificultad.Facil => "Fácil",
            NivelDificultad.Intermedio => "Intermedio",
            NivelDificultad.Dificil => "Difícil",
            NivelDificultad.MuyDificil => "Muy Difícil",
            _ => dificultad.ToString()
        };
    }

    #endregion
}
