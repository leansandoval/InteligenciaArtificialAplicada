using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            
            // Obtener flashcards filtradas por dificultad si se especifica
            IEnumerable<Flashcard> flashcards;
            
            if (model.Dificultad != 0) // Si se seleccionó una dificultad específica
            {
                flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByDificultadAsync(model.MateriaId, model.Dificultad);
                
                // Si no hay suficientes flashcards de la dificultad seleccionada, completar con flashcards de todas las dificultades
                if (flashcards.Count() < model.NumeroPreguntas)
                {
                    var todasLasFlashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(model.MateriaId);
                    
                    // Priorizar las de la dificultad seleccionada y completar con otras
                    var flashcardsOrdenadas = todasLasFlashcards
                        .OrderBy(f => f.Dificultad == model.Dificultad ? 0 : 1) // Prioridad a la dificultad seleccionada
                        .ThenBy(f => Math.Abs((int)f.Dificultad - (int)model.Dificultad)) // Luego por proximidad de dificultad
                        .ThenBy(x => Guid.NewGuid()); // Aleatorio dentro de cada grupo
                        
                    flashcards = flashcardsOrdenadas.Take(Math.Max(model.NumeroPreguntas, todasLasFlashcards.Count()));
                    
                    _logger.LogInformation($"Se encontraron solo {flashcards.Count()} flashcards de dificultad {model.Dificultad}. Completando con flashcards de otras dificultades.");
                }
            }
            else
            {
                // Si no se especifica dificultad, obtener todas las flashcards de la materia
                flashcards = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(model.MateriaId);
            }
            
            if (!flashcards.Any())
            {
                ModelState.AddModelError("", $"No hay flashcards disponibles en la materia seleccionada{(model.Dificultad != 0 ? $" para el nivel de dificultad {model.Dificultad}" : "")}.");
                
                // Recargar datos para la vista
                var usuarioIdForReload = _userManager.GetUserId(User);
                if (usuarioIdForReload == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                var materiasForReload = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioIdForReload);
                
                var materiasConFlashcardsForReload = new List<MateriaSelectViewModel>();
                
                foreach (var materia in materiasForReload)
                {
                    var flashcardsCount = await _unitOfWork.FlashcardRepository.GetFlashcardsByMateriaIdAsync(materia.Id);
                    
                    materiasConFlashcardsForReload.Add(new MateriaSelectViewModel
                    {
                        Id = materia.Id,
                        Nombre = materia.Nombre,
                        TotalFlashcards = flashcardsCount.Count()
                    });
                }
                
                model.MateriasDisponibles = materiasConFlashcardsForReload;
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

            // Seleccionar flashcards usando algoritmo inteligente
            var flashcardsSeleccionadas = SeleccionarFlashcardsInteligente(flashcards, model.NumeroPreguntas, model.Dificultad);

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
        public async Task<IActionResult> SubmitQuiz(int QuizId, string? RespuestasUsuario, int TiempoTranscurrido)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener el quiz con preguntas
                var quiz = await _unitOfWork.QuizRepository.GetQuizConPreguntasAsync(QuizId);
                if (quiz == null)
                {
                    TempData["Error"] = "Quiz no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Procesar las respuestas del usuario
                var respuestas = ProcesarRespuestas(RespuestasUsuario);
                var preguntas = quiz.Preguntas.OrderBy(p => p.Orden).ToList();
                
                int respuestasCorrectas = 0;
                int puntajeTotal = 0;
                var respuestasDetalle = new List<RespuestaUsuario>();

                // Crear el resultado del quiz
                var resultado = new ResultadoQuiz
                {
                    QuizId = QuizId,
                    UsuarioId = usuarioId,
                    FechaInicio = DateTime.UtcNow.AddSeconds(-TiempoTranscurrido),
                    FechaFinalizacion = DateTime.UtcNow,
                    FechaRealizacion = DateTime.UtcNow,
                    TiempoTranscurrido = TiempoTranscurrido,
                    TiempoTotal = TimeSpan.FromSeconds(TiempoTranscurrido),
                    EstaCompletado = true
                };

                // Procesar las respuestas usando el formato Dictionary para mapear por ID de pregunta
                var respuestasDict = new Dictionary<string, string>();
                try
                {
                    if (!string.IsNullOrEmpty(RespuestasUsuario) && RespuestasUsuario.StartsWith("{"))
                    {
                        respuestasDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(RespuestasUsuario) ?? new Dictionary<string, string>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al parsear respuestas como diccionario");
                }

                // Procesar cada pregunta y buscar su respuesta correspondiente
                foreach (var pregunta in preguntas)
                {
                    var respuestaUsuario = "";
                    
                    // Buscar la respuesta por ID de pregunta
                    if (respuestasDict.ContainsKey(pregunta.Id.ToString()))
                    {
                        respuestaUsuario = respuestasDict[pregunta.Id.ToString()];
                    }
                    
                    bool esCorrecta = !string.IsNullOrEmpty(respuestaUsuario) && 
                                    respuestaUsuario.Equals(pregunta.RespuestaCorrecta, StringComparison.OrdinalIgnoreCase);
                    
                    if (esCorrecta)
                    {
                        respuestasCorrectas++;
                        puntajeTotal += pregunta.Puntos;
                    }

                    var respuestaDetalle = new RespuestaUsuario
                    {
                        PreguntaQuizId = pregunta.Id,
                        PreguntaId = pregunta.Id, // Mismo valor para que funcione con la FK
                        RespuestaSeleccionada = respuestaUsuario,
                        RespuestaDada = respuestaUsuario,
                        EsCorrecta = esCorrecta,
                        PuntosObtenidos = esCorrecta ? pregunta.Puntos : 0,
                        FechaRespuesta = DateTime.UtcNow
                    };
                    
                    respuestasDetalle.Add(respuestaDetalle);
                }

                // Calcular estadísticas
                var puntajeMaximo = preguntas.Sum(p => p.Puntos);
                var porcentajeAcierto = preguntas.Count > 0 ? (double)respuestasCorrectas / preguntas.Count * 100 : 0;

                resultado.PuntajeObtenido = puntajeTotal;
                resultado.PuntajeMaximo = puntajeMaximo;
                resultado.Puntuacion = puntajeMaximo > 0 ? (decimal)puntajeTotal / puntajeMaximo : 0;
                resultado.PorcentajeAcierto = porcentajeAcierto;

                // Guardar el resultado usando el contexto directamente (temporal hasta crear repositorio específico)
                // TODO: Crear un IResultadoQuizRepository en el futuro
                var context = (QuizCraft.Infrastructure.Data.ApplicationDbContext)_unitOfWork.GetType()
                    .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_unitOfWork)!;
                
                // Primero guardar el resultado para obtener el ID
                await context.Set<ResultadoQuiz>().AddAsync(resultado);
                await _unitOfWork.SaveChangesAsync();

                // Ahora asignar el ResultadoQuizId a las respuestas y guardarlas
                foreach (var respuesta in respuestasDetalle)
                {
                    respuesta.ResultadoQuizId = resultado.Id;
                    await context.Set<RespuestaUsuario>().AddAsync(respuesta);
                }
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Quiz {QuizId} completado por usuario {usuarioId}. Puntuación: {porcentajeAcierto:F1}%, Tiempo: {TiempoTranscurrido}s");

                // Redirigir a la página de resultados
                return RedirectToAction(nameof(Results), new { id = resultado.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar envío de quiz {QuizId}", QuizId);
                TempData["Error"] = "Ocurrió un error al procesar el quiz. Por favor, inténtalo de nuevo.";
                return RedirectToAction(nameof(Details), new { id = QuizId });
            }
        }

        /// <summary>
        /// Procesa las respuestas del usuario desde el formato JSON/string
        /// </summary>
        private List<string> ProcesarRespuestas(string? respuestasUsuario)
        {
            if (string.IsNullOrEmpty(respuestasUsuario))
                return new List<string>();

            try
            {
                // Intentar parsear como JSON object (formato esperado: {"1":"C","2":"B",...})
                if (respuestasUsuario.StartsWith("{") && respuestasUsuario.EndsWith("}"))
                {
                    var respuestasDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(respuestasUsuario);
                    if (respuestasDict != null)
                    {
                        // Ordenar por clave numérica y extraer solo los valores
                        return respuestasDict
                            .OrderBy(kvp => int.TryParse(kvp.Key, out int key) ? key : int.MaxValue)
                            .Select(kvp => kvp.Value)
                            .ToList();
                    }
                }
                
                // Intentar parsear como JSON array
                if (respuestasUsuario.StartsWith("[") && respuestasUsuario.EndsWith("]"))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<string>>(respuestasUsuario) ?? new List<string>();
                }
                
                // Si es una cadena simple separada por comas
                return respuestasUsuario.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al procesar respuestas del usuario: {RespuestasUsuario}", respuestasUsuario);
                return new List<string>();
            }
        }

        /// <summary>
        /// Algoritmo inteligente para seleccionar flashcards basado en dificultad y otros criterios
        /// </summary>
        private List<Flashcard> SeleccionarFlashcardsInteligente(IEnumerable<Flashcard> flashcards, int cantidadRequerida, NivelDificultad dificultadPreferida)
        {
            var flashcardsList = flashcards.ToList();
            
            if (flashcardsList.Count <= cantidadRequerida)
            {
                return flashcardsList;
            }

            // Algoritmo de selección inteligente
            var flashcardsSeleccionadas = new List<Flashcard>();
            
            // 1. Priorizar flashcards de la dificultad preferida (70%)
            var flashcardsDificultadPreferida = flashcardsList
                .Where(f => f.Dificultad == dificultadPreferida)
                .OrderBy(x => Guid.NewGuid())
                .ToList();
            
            int cantidadPreferida = Math.Min((int)(cantidadRequerida * 0.7), flashcardsDificultadPreferida.Count);
            flashcardsSeleccionadas.AddRange(flashcardsDificultadPreferida.Take(cantidadPreferida));

            // 2. Completar con flashcards de dificultades adyacentes (30%)
            var flashcardsRestantes = flashcardsList
                .Except(flashcardsSeleccionadas)
                .OrderBy(f => Math.Abs((int)f.Dificultad - (int)dificultadPreferida)) // Ordenar por proximidad de dificultad
                .ThenBy(x => Guid.NewGuid()) // Aleatorio dentro de cada nivel de proximidad
                .ToList();

            int cantidadFaltante = cantidadRequerida - flashcardsSeleccionadas.Count;
            flashcardsSeleccionadas.AddRange(flashcardsRestantes.Take(cantidadFaltante));

            // 3. Mezclar el orden final si se configuró para mezclar preguntas
            return flashcardsSeleccionadas.OrderBy(x => Guid.NewGuid()).ToList();
        }

        /// <summary>
        /// Endpoint AJAX para obtener estadísticas de flashcards por dificultad de una materia
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEstadisticasDificultad(int materiaId)
        {
            try
            {
                var estadisticas = await _unitOfWork.FlashcardRepository.GetEstadisticasDificultadAsync(materiaId);
                
                var resultado = new
                {
                    success = true,
                    estadisticas = estadisticas.Select(e => new
                    {
                        dificultad = e.Key.ToString(),
                        dificultadValue = (int)e.Key,
                        cantidad = e.Value,
                        descripcion = GetDescripcionDificultad(e.Key)
                    }).OrderBy(e => e.dificultadValue)
                };
                
                return Json(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de dificultad para materia {MateriaId}", materiaId);
                return Json(new { success = false, message = "Error al obtener estadísticas" });
            }
        }

        /// <summary>
        /// Obtiene la descripción amigable de un nivel de dificultad
        /// </summary>
        private string GetDescripcionDificultad(NivelDificultad dificultad)
        {
            return dificultad switch
            {
                NivelDificultad.MuyFacil => "Muy Fácil - Preguntas básicas",
                NivelDificultad.Facil => "Fácil - Conocimiento básico",
                NivelDificultad.Intermedio => "Intermedio - Comprensión y análisis",
                NivelDificultad.Dificil => "Difícil - Conocimiento avanzado",
                NivelDificultad.MuyDificil => "Muy Difícil - Conocimiento experto",
                _ => "Desconocido"
            };
        }

        // GET: Quiz/Results/5
        public async Task<IActionResult> Results(int id)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener el resultado con sus relaciones usando el contexto directamente
                var context = (QuizCraft.Infrastructure.Data.ApplicationDbContext)_unitOfWork.GetType()
                    .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_unitOfWork)!;

                var resultado = await context.Set<ResultadoQuiz>()
                    .Include(r => r.Quiz)
                        .ThenInclude(q => q.Materia)
                    .Include(r => r.Quiz)
                        .ThenInclude(q => q.Preguntas)
                    .Include(r => r.RespuestasUsuario)
                        .ThenInclude(ru => ru.PreguntaQuiz)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);

                if (resultado == null)
                {
                    TempData["Error"] = "Resultado no encontrado o no tienes permisos para verlo.";
                    return RedirectToAction(nameof(Index));
                }

                // Crear el ViewModel para los resultados
                var viewModel = new QuizResultsViewModel
                {
                    QuizId = resultado.QuizId,
                    Titulo = resultado.Quiz.Titulo,
                    Descripcion = resultado.Quiz.Descripcion,
                    MateriaNombre = resultado.Quiz.Materia?.Nombre ?? "Sin materia",
                    TotalPreguntas = resultado.Quiz.Preguntas.Count,
                    RespuestasCorrectas = resultado.RespuestasUsuario.Count(r => r.EsCorrecta),
                    PuntuacionTotal = resultado.PuntajeObtenido,
                    PorcentajeAcierto = Math.Round(resultado.PorcentajeAcierto, 1),
                    TiempoTotal = resultado.TiempoTotal,
                    FechaRealizacion = resultado.FechaRealizacion,
                    MostrarRespuestasDetalle = true
                };

                // Determinar mensaje y color basado en el porcentaje
                if (viewModel.PorcentajeAcierto >= 90)
                {
                    viewModel.MensajeResultado = "¡Excelente trabajo!";
                    viewModel.ColorResultado = "success";
                }
                else if (viewModel.PorcentajeAcierto >= 70)
                {
                    viewModel.MensajeResultado = "¡Buen trabajo!";
                    viewModel.ColorResultado = "primary";
                }
                else if (viewModel.PorcentajeAcierto >= 50)
                {
                    viewModel.MensajeResultado = "Puedes mejorar";
                    viewModel.ColorResultado = "warning";
                }
                else
                {
                    viewModel.MensajeResultado = "Necesitas estudiar más";
                    viewModel.ColorResultado = "danger";
                }

                // Crear detalle de respuestas
                var preguntasOrdenadas = resultado.Quiz.Preguntas.OrderBy(p => p.Orden).ToList();
                foreach (var pregunta in preguntasOrdenadas)
                {
                    var respuestaUsuario = resultado.RespuestasUsuario.FirstOrDefault(r => r.PreguntaQuizId == pregunta.Id);
                    
                    var detalleRespuesta = new RespuestaDetalleViewModel
                    {
                        Orden = pregunta.Orden,
                        Pregunta = pregunta.TextoPregunta,
                        RespuestaUsuario = respuestaUsuario?.RespuestaSeleccionada ?? "Sin respuesta",
                        RespuestaCorrecta = pregunta.RespuestaCorrecta,
                        EsCorrecta = respuestaUsuario?.EsCorrecta ?? false,
                        Explicacion = pregunta.Explicacion,
                        TodasLasOpciones = new List<OpcionRespuestaViewModel>
                        {
                            new OpcionRespuestaViewModel { Texto = pregunta.OpcionA ?? "", Valor = "A", EsCorrecta = pregunta.RespuestaCorrecta == "A" },
                            new OpcionRespuestaViewModel { Texto = pregunta.OpcionB ?? "", Valor = "B", EsCorrecta = pregunta.RespuestaCorrecta == "B" },
                            new OpcionRespuestaViewModel { Texto = pregunta.OpcionC ?? "", Valor = "C", EsCorrecta = pregunta.RespuestaCorrecta == "C" },
                            new OpcionRespuestaViewModel { Texto = pregunta.OpcionD ?? "", Valor = "D", EsCorrecta = pregunta.RespuestaCorrecta == "D" }
                        }.Where(o => !string.IsNullOrEmpty(o.Texto)).ToList()
                    };
                    
                    viewModel.DetalleRespuestas.Add(detalleRespuesta);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar resultados del quiz {Id} para usuario {UsuarioId}", id, _userManager.GetUserId(User));
                TempData["Error"] = "Ocurrió un error al cargar los resultados.";
                return RedirectToAction(nameof(Index));
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