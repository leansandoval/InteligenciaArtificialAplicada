using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Repositories;

namespace QuizCraft.Web.Controllers
{
    [Authorize]
    public class MateriaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MateriaController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: Materia
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener materias con sus estadísticas de una sola vez
                var estadisticasGenerales = await _unitOfWork.MateriaRepository.GetEstadisticasGeneralesByUsuarioAsync(user.Id);
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(user.Id);
                
                var materiasViewModel = new List<MateriaViewModel>();
                
                foreach (var m in materias)
                {
                    // Obtener estadísticas de quizzes por materia
                    var quizzes = await _unitOfWork.QuizRepository.GetQuizzesByMateriaIdAsync(m.Id);
                    
                    materiasViewModel.Add(new MateriaViewModel
                    {
                        Id = m.Id,
                        Nombre = m.Nombre ?? string.Empty,
                        Descripcion = m.Descripcion ?? string.Empty,
                        Color = m.Color ?? string.Empty,
                        Icono = m.Icono ?? string.Empty,
                        FechaCreacion = m.FechaCreacion,
                        EstaActivo = m.EstaActivo,
                        TotalFlashcards = estadisticasGenerales.ContainsKey(m.Id) ? estadisticasGenerales[m.Id] : 0,
                        TotalQuizzes = quizzes.Count()
                    });
                }

                return View(materiasViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar las materias: " + ex.Message;
                return View(new List<MateriaViewModel>());
            }
        }

        // GET: Materia/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var materia = await _unitOfWork.MateriaRepository.GetMateriaCompletaAsync(id);
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    TempData["ErrorMessage"] = "Materia no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var materiaViewModel = new MateriaViewModel
                {
                    Id = materia.Id,
                    Nombre = materia.Nombre ?? string.Empty,
                    Descripcion = materia.Descripcion ?? string.Empty,
                    Color = materia.Color ?? string.Empty,
                    Icono = materia.Icono ?? string.Empty,
                    FechaCreacion = materia.FechaCreacion,
                    FechaModificacion = materia.FechaModificacion,
                    EstaActivo = materia.EstaActivo,
                    TotalFlashcards = materia.Flashcards?.Count ?? 0,
                    TotalQuizzes = materia.Quizzes?.Count ?? 0
                };

                return View(materiaViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la materia: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Materia/Create
        public IActionResult Create()
        {
            var model = new CreateMateriaViewModel();
            return View(model);
        }

        // POST: Materia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMateriaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var materia = new Materia
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Color = model.Color,
                    Icono = model.Icono,
                    UsuarioId = user.Id,
                    FechaCreacion = DateTime.Now,
                    EstaActivo = true
                };

                await _unitOfWork.MateriaRepository.AddAsync(materia);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Materia creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la materia: " + ex.Message;
                return View(model);
            }
        }

        // GET: Materia/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(id);
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    TempData["ErrorMessage"] = "Materia no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new EditMateriaViewModel
                {
                    Id = materia.Id,
                    Nombre = materia.Nombre ?? string.Empty,
                    Descripcion = materia.Descripcion ?? string.Empty,
                    Color = materia.Color ?? string.Empty,
                    Icono = materia.Icono ?? string.Empty,
                    EstaActivo = materia.EstaActivo
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la materia: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Materia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditMateriaViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "ID de materia inválido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(id);
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    TempData["ErrorMessage"] = "Materia no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                materia.Nombre = model.Nombre;
                materia.Descripcion = model.Descripcion;
                materia.Color = model.Color;
                materia.Icono = model.Icono;
                materia.EstaActivo = model.EstaActivo;
                materia.FechaModificacion = DateTime.Now;

                _unitOfWork.MateriaRepository.Update(materia);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Materia actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar la materia: " + ex.Message;
                return View(model);
            }
        }

        // GET: Materia/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(id);
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    TempData["ErrorMessage"] = "Materia no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var materiaViewModel = new MateriaViewModel
                {
                    Id = materia.Id,
                    Nombre = materia.Nombre ?? string.Empty,
                    Descripcion = materia.Descripcion ?? string.Empty,
                    Color = materia.Color ?? string.Empty,
                    Icono = materia.Icono ?? string.Empty,
                    FechaCreacion = materia.FechaCreacion,
                    EstaActivo = materia.EstaActivo,
                    TotalFlashcards = materia.Flashcards?.Count ?? 0,
                    TotalQuizzes = materia.Quizzes?.Count ?? 0
                };

                return View(materiaViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la materia: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Materia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(id);
                if (materia == null || materia.UsuarioId != user.Id)
                {
                    TempData["ErrorMessage"] = "Materia no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si tiene flashcards o quizzes asociados
                if (materia.Flashcards?.Any() == true || materia.Quizzes?.Any() == true)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar la materia porque tiene flashcards o quizzes asociados.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _unitOfWork.MateriaRepository.Remove(materia);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Materia eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la materia: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}