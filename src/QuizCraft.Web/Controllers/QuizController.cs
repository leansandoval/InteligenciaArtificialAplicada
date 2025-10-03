using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Enums;

namespace QuizCraft.Web.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ILogger<QuizController> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Quiz
        public async Task<IActionResult> Index(int? materiaId, NivelDificultad? dificultad)
        {
            var usuarioId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Obtener datos de forma secuencial para evitar problemas de concurrencia
            var misQuizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(usuarioId);
            var quizzesPublicos = await _unitOfWork.QuizRepository.GetQuizzesPublicosAsync();
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            
            // Debug: Log para verificar los datos
            _logger.LogInformation($"Usuario: {usuarioId}, Mis Quizzes: {misQuizzes.Count()}, Quizzes Públicos: {quizzesPublicos.Count()}, Materias: {materias.Count()}");

            var viewModel = new QuizIndexViewModel
            {
                MisQuizzes = misQuizzes.Select(q => new QuizItemViewModel
                {
                    Id = q.Id,
                    Titulo = q.Titulo ?? string.Empty,
                    Descripcion = q.Descripcion,
                    NumeroPreguntas = q.NumeroPreguntas,
                    MateriaNombre = q.Materia?.Nombre ?? "Sin materia",
                    CreadorNombre = q.Creador?.UserName ?? "Usuario",
                    FechaCreacion = q.FechaCreacion,
                    EsPublico = q.EsPublico,
                    TotalResultados = q.Resultados?.Count ?? 0,
                    Dificultad = (NivelDificultad)q.NivelDificultad,
                    TiempoLimite = q.TiempoLimite,
                    YaRealizado = q.Resultados?.Any(r => r.UsuarioId == usuarioId) == true,
                    UltimoResultado = q.Resultados?
                        .Where(r => r.UsuarioId == usuarioId)
                        .OrderByDescending(r => r.FechaRealizacion)
                        .FirstOrDefault()?.PorcentajeAcierto
                }).ToList(),
                
                QuizzesCreados = misQuizzes.Select(q => new QuizListItemViewModel
                {
                    Id = q.Id,
                    Titulo = q.Titulo ?? string.Empty,
                    Descripcion = q.Descripcion,
                    NumeroPreguntas = q.NumeroPreguntas,
                    MateriaNombre = q.Materia?.Nombre ?? "Sin materia",
                    CreadorNombre = q.Creador?.UserName ?? "Usuario",
                    FechaCreacion = q.FechaCreacion,
                    EsPublico = q.EsPublico,
                    TotalResultados = q.Resultados?.Count ?? 0,
                    Dificultad = (NivelDificultad)q.NivelDificultad,
                    TiempoLimite = q.TiempoLimite,
                    YaRealizado = q.Resultados?.Any(r => r.UsuarioId == usuarioId) == true,
                    UltimoResultado = (int?)(q.Resultados?
                        .Where(r => r.UsuarioId == usuarioId)
                        .OrderByDescending(r => r.FechaRealizacion)
                        .FirstOrDefault()?.PorcentajeAcierto)
                }).ToList(),
                
                QuizzesPublicos = quizzesPublicos
                    .Where(q => q.CreadorId != usuarioId)
                    .Where(q => !materiaId.HasValue || q.MateriaId == materiaId)
                    .Where(q => !dificultad.HasValue || q.NivelDificultad == (int)dificultad)
                    .Select(q => new QuizItemViewModel
                    {
                        Id = q.Id,
                        Titulo = q.Titulo,
                        Descripcion = q.Descripcion,
                        NumeroPreguntas = q.NumeroPreguntas,
                        MateriaNombre = q.Materia?.Nombre ?? "Sin materia",
                        CreadorNombre = q.Creador?.UserName ?? "Usuario",
                        FechaCreacion = q.FechaCreacion,
                        EsPublico = q.EsPublico,
                        TotalResultados = q.Resultados?.Count ?? 0,
                        Dificultad = (NivelDificultad)q.NivelDificultad,
                        TiempoLimite = q.TiempoLimite,
                        YaRealizado = q.Resultados?.Any(r => r.UsuarioId == usuarioId) == true,
                        UltimoResultado = q.Resultados?
                            .Where(r => r.UsuarioId == usuarioId)
                            .OrderByDescending(r => r.FechaRealizacion)
                            .FirstOrDefault()?.PorcentajeAcierto
                    }).ToList(),
                
                MateriaSeleccionada = materiaId,
                DificultadSeleccionada = dificultad,
                TotalQuizzes = misQuizzes.Count(),
                MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre ?? string.Empty
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Quiz/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? materiaId = null)
        {
            var usuarioId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var viewModel = new CreateQuizViewModel();

            // Cargar materias del usuario autenticado
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
            
            var materiasConFlashcards = new List<MateriaSelectViewModel>();
            
            foreach (var materia in materias)
            {
                // Obtener el conteo real de flashcards por materia
                var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(materia.Id);
                
                materiasConFlashcards.Add(new MateriaSelectViewModel
                {
                    Id = materia.Id,
                    Nombre = materia.Nombre,
                    TotalFlashcards = flashcards.Count()
                });
            }
            
            viewModel.MateriasDisponibles = materiasConFlashcards;
            
            // Preseleccionar materia si se especifica
            if (materiaId.HasValue)
            {
                viewModel.MateriaId = materiaId.Value;
            }

            return View(viewModel);
        }

        // POST: Quiz/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar materias en caso de error
                var materias = await _unitOfWork.MateriaRepository.GetAllAsync();
                model.MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    TotalFlashcards = m.Flashcards?.Count ?? 0
                }).ToList();
                return View(model);
            }

            var usuarioId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Obtener flashcards de la materia seleccionada
            var flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(model.MateriaId);
            
            if (!flashcards.Any())
            {
                ModelState.AddModelError("", "No hay flashcards disponibles en la materia seleccionada.");
                var materias = await _unitOfWork.MateriaRepository.GetAllAsync();
                model.MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    TotalFlashcards = m.Flashcards?.Count ?? 0
                }).ToList();
                return View(model);
            }

            // Crear el quiz
            var quiz = new Quiz
            {
                Titulo = model.Titulo,
                Descripcion = model.Descripcion,
                NumeroPreguntas = model.NumeroPreguntas,
                TiempoLimite = model.TiempoLimite,
                TiempoPorPregunta = 30,
                NivelDificultad = (int)model.Dificultad,
                EsPublico = model.EsPublico,
                MostrarRespuestasInmediato = model.MostrarRespuestasInmediato,
                PermitirReintento = true,
                MateriaId = model.MateriaId,
                CreadorId = usuarioId,
                FechaCreacion = DateTime.UtcNow
            };

            // Seleccionar flashcards aleatoriamente
            var flashcardsSeleccionadas = flashcards
                .OrderBy(x => Guid.NewGuid())
                .Take(model.NumeroPreguntas)
                .ToList();

            // Crear preguntas basadas en las flashcards
            var preguntas = new List<PreguntaQuiz>();
            for (int i = 0; i < flashcardsSeleccionadas.Count; i++)
            {
                var flashcard = flashcardsSeleccionadas[i];
                var pregunta = new PreguntaQuiz
                {
                    TextoPregunta = flashcard.Pregunta,
                    RespuestaCorrecta = flashcard.Respuesta,
                    OpcionA = flashcard.Respuesta,
                    OpcionB = GenerarOpcionIncorrecta(),
                    OpcionC = GenerarOpcionIncorrecta(),
                    OpcionD = GenerarOpcionIncorrecta(),
                    Explicacion = flashcard.Pista ?? "",
                    Orden = i + 1,
                    Puntos = 1,
                    TipoPregunta = 1,
                    FlashcardId = flashcard.Id
                };
                
                // Mezclar las opciones
                var opciones = new List<string> { pregunta.OpcionA, pregunta.OpcionB, pregunta.OpcionC, pregunta.OpcionD }
                    .Where(o => !string.IsNullOrEmpty(o))
                    .OrderBy(x => Guid.NewGuid())
                    .ToList();
                
                if (opciones.Count >= 4)
                {
                    pregunta.OpcionA = opciones[0];
                    pregunta.OpcionB = opciones[1];
                    pregunta.OpcionC = opciones[2];
                    pregunta.OpcionD = opciones[3];
                    
                    // Determinar cuál opción es la correcta después del mezclado
                    var indiceCorrecta = opciones.IndexOf(flashcard.Respuesta);
                    pregunta.RespuestaCorrecta = ((char)('A' + indiceCorrecta)).ToString();
                }
                
                preguntas.Add(pregunta);
            }

            quiz.Preguntas = preguntas;

            await _unitOfWork.QuizRepository.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Quiz creado exitosamente.";
            return RedirectToAction(nameof(Details), new { id = quiz.Id });
        }

        // GET: Quiz/Take/5
        public async Task<IActionResult> Take(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            var quiz = await _unitOfWork.QuizRepository.GetQuizConPreguntasAsync(id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Verificar permisos
            if (!quiz.EsPublico && quiz.CreadorId != usuarioId)
            {
                return Forbid();
            }

            // Verificar que hay preguntas disponibles
            var preguntasActivas = quiz.Preguntas.Where(p => p.EstaActivo).OrderBy(p => p.Orden).ToList();
            
            if (!preguntasActivas.Any())
            {
                TempData["Error"] = "Este quiz no tiene preguntas disponibles.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new TakeQuizViewModel
            {
                Id = quiz.Id,
                QuizId = quiz.Id,
                Titulo = quiz.Titulo,
                Descripcion = quiz.Descripcion,
                NumeroPreguntas = quiz.NumeroPreguntas,
                TotalPreguntas = preguntasActivas.Count, // Usar preguntas reales, no el campo NumeroPreguntas
                TiempoPorPregunta = quiz.TiempoPorPregunta,
                TiempoLimite = quiz.TiempoLimite,
                MostrarRespuestasInmediato = quiz.MostrarRespuestasInmediato,
                MateriaNombre = quiz.Materia?.Nombre ?? "Sin materia",
                PreguntaActual = 1,
                FechaInicio = DateTime.UtcNow,
                Preguntas = preguntasActivas.Select(p => new PreguntaQuizViewModel
                {
                    Id = p.Id,
                    TextoPregunta = p.TextoPregunta,
                    OpcionA = p.OpcionA ?? "",
                    OpcionB = p.OpcionB ?? "",
                    OpcionC = p.OpcionC ?? "",
                    OpcionD = p.OpcionD ?? "",
                    Orden = p.Orden,
                    Puntos = p.Puntos,
                    RespuestaCorrecta = p.RespuestaCorrecta,
                    Explicacion = p.Explicacion,
                    Opciones = new List<OpcionRespuestaViewModel>
                    {
                        new OpcionRespuestaViewModel { Texto = p.OpcionA ?? "", Valor = "A", EsCorrecta = p.RespuestaCorrecta == "A" },
                        new OpcionRespuestaViewModel { Texto = p.OpcionB ?? "", Valor = "B", EsCorrecta = p.RespuestaCorrecta == "B" },
                        new OpcionRespuestaViewModel { Texto = p.OpcionC ?? "", Valor = "C", EsCorrecta = p.RespuestaCorrecta == "C" },
                        new OpcionRespuestaViewModel { Texto = p.OpcionD ?? "", Valor = "D", EsCorrecta = p.RespuestaCorrecta == "D" }
                    }.Where(o => !string.IsNullOrEmpty(o.Texto)).ToList()
                }).ToList()
            };

            // Log para debugging
            _logger.LogInformation($"Quiz {id}: TotalPreguntas={viewModel.TotalPreguntas}, PreguntasCargadas={viewModel.Preguntas.Count}");

            return View(viewModel);
        }

        // GET: Quiz/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            var quiz = await _unitOfWork.QuizRepository.GetQuizCompletoAsync(id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Verificar permisos para ver el quiz
            if (!quiz.EsPublico && quiz.CreadorId != usuarioId)
            {
                return Forbid();
            }

            var viewModel = new QuizDetailsViewModel
            {
                Id = quiz.Id,
                Titulo = quiz.Titulo,
                Descripcion = quiz.Descripcion,
                MateriaNombre = quiz.Materia?.Nombre ?? "Sin materia",
                CreadorNombre = quiz.Creador?.UserName ?? "Usuario",
                FechaCreacion = quiz.FechaCreacion,
                EsPublico = quiz.EsPublico,
                NumeroPreguntas = quiz.NumeroPreguntas,
                TiempoLimite = quiz.TiempoLimite,
                NivelDificultad = (NivelDificultad)quiz.NivelDificultad,
                PuedeEditar = quiz.CreadorId == usuarioId,
                PuedeEliminar = quiz.CreadorId == usuarioId,
                PuedeRealizarQuiz = quiz.EsPublico || quiz.CreadorId == usuarioId,
                MensajeNoDisponible = quiz.EsPublico || quiz.CreadorId == usuarioId 
                    ? "" : "Este quiz no está disponible públicamente."
            };

            return View(viewModel);
        }

        // GET: Quiz/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            var quiz = await _unitOfWork.QuizRepository.GetQuizCompletoAsync(id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Solo el creador puede eliminar
            if (quiz.CreadorId != usuarioId)
            {
                return Forbid();
            }

            var viewModel = new QuizDetailsViewModel
            {
                Id = quiz.Id,
                Titulo = quiz.Titulo,
                Descripcion = quiz.Descripcion,
                MateriaNombre = quiz.Materia?.Nombre ?? "Sin materia",
                CreadorNombre = quiz.Creador?.UserName ?? "Usuario",
                FechaCreacion = quiz.FechaCreacion,
                EsPublico = quiz.EsPublico,
                NumeroPreguntas = quiz.NumeroPreguntas
            };

            return View(viewModel);
        }

        // POST: Quiz/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Solo el creador puede eliminar
            if (quiz.CreadorId != usuarioId)
            {
                return Forbid();
            }

            _unitOfWork.QuizRepository.Remove(quiz);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Quiz eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Quiz/SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitQuiz(int QuizId, string? RespuestasUsuario, int TiempoTranscurrido)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Por ahora, solo loggeamos la información y redirigimos a Details con un parámetro de completado
                _logger.LogInformation($"Quiz {QuizId} completado por usuario {usuarioId}. Tiempo: {TiempoTranscurrido}s, Respuestas: {RespuestasUsuario}");

                // Guardar información en TempData para mostrar en la página de detalles
                TempData["QuizCompletado"] = true;
                TempData["TiempoTranscurrido"] = TiempoTranscurrido;
                TempData["Success"] = "¡Quiz completado exitosamente!";

                // Redirigir a la página de detalles del quiz
                return RedirectToAction(nameof(Details), new { id = QuizId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar envío de quiz {QuizId}", QuizId);
                TempData["Error"] = "Ocurrió un error al procesar el quiz. Por favor, inténtalo de nuevo.";
                return RedirectToAction(nameof(Details), new { id = QuizId });
            }
        }

        // Método auxiliar para generar opciones incorrectas
        private string GenerarOpcionIncorrecta()
        {
            var opciones = new[]
            {
                "Opción incorrecta A",
                "Opción incorrecta B", 
                "Opción incorrecta C",
                "Respuesta falsa",
                "Alternativa incorrecta"
            };
            
            return opciones[new Random().Next(opciones.Length)];
        }
    }
}