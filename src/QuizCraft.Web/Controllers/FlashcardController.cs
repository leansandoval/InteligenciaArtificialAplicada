using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Core.Interfaces;

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

    public FlashcardController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<FlashcardController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
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
}