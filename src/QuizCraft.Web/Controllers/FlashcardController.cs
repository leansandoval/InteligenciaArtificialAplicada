using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Application.ViewModels;
using QuizCraft.Application.Interfaces;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

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

    public FlashcardController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<FlashcardController> logger,
        IFileUploadService fileUploadService,
        IAlgoritmoRepasoService algoritmoRepasoService,
        ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _fileUploadService = fileUploadService;
        _algoritmoRepasoService = algoritmoRepasoService;
        _context = context;
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

            // Mapear a ViewModels
            var flashcardViewModels = flashcards.Select(f => new FlashcardViewModel
            {
                Id = f.Id,
                Pregunta = f.Pregunta,
                Respuesta = f.Respuesta,
                Pista = f.Pista,
                DificultadTexto = f.Dificultad.ToString(),
                MateriaNombre = f.Materia.Nombre,
                MateriaColor = f.Materia.Color ?? "#007bff",
                MateriaIcono = f.Materia.Icono ?? "fas fa-book",
                MateriaId = f.MateriaId,
                Dificultad = f.Dificultad,
                EstaActiva = true, // Por defecto todas están activas ya que la entidad no tiene esta propiedad
                FechaCreacion = f.FechaCreacion,
                FechaModificacion = f.FechaModificacion,
                VecesRepasada = f.VecesVista,
                UltimaVezRepasada = f.UltimaRevision
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
                VecesVista = 0
            };

            await _unitOfWork.FlashcardRepository.AddAsync(flashcard);
            await _unitOfWork.SaveChangesAsync();

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

            var viewModel = new FlashcardViewModel
            {
                Id = flashcard.Id,
                Pregunta = flashcard.Pregunta,
                Respuesta = flashcard.Respuesta,
                Pista = flashcard.Pista,
                DificultadTexto = flashcard.Dificultad.ToString(),
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
                DificultadTexto = flashcard.Dificultad.ToString(),
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
    public async Task<IActionResult> ConfigurarRepaso()
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
            var flashcardsParaRepaso = await _unitOfWork.FlashcardRepository.GetFlashcardsParaRepasoAsync(
                usuario.Id, configuracion.MateriaId);

            // Aplicar filtros adicionales
            if (configuracion.DificultadFiltro.HasValue)
            {
                flashcardsParaRepaso = flashcardsParaRepaso.Where(f => f.Dificultad == configuracion.DificultadFiltro.Value);
            }

            // Usar algoritmo de repaso para priorizar
            var flashcardsOptimizadas = _algoritmoRepasoService.ObtenerFlashcardsParaRepaso(
                flashcardsParaRepaso, configuracion.MaximoFlashcards);

            var flashcardsLista = flashcardsOptimizadas.ToList();

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
                    DificultadTexto = flashcardActual.Dificultad.ToString(),
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

            var flashcardViewModel = new FlashcardViewModel
            {
                Id = flashcard.Id,
                Pregunta = flashcard.Pregunta,
                Respuesta = flashcard.Respuesta,
                Pista = incluirPistas ? flashcard.Pista : null,
                DificultadTexto = flashcard.Dificultad.ToString(),
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
    public async Task<IActionResult> FinalizarRepaso(EstadisticasRepasoViewModel estadisticas)
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
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"¡Sesión de repaso completada! Revisaste {estadisticas.FlashcardsRevisadas} flashcards con {estadisticas.PorcentajeAcierto:F1}% de acierto.";
            
            return View("EstadisticasRepaso", estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al finalizar repaso para usuario {UserId}", usuario.Id);
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
}