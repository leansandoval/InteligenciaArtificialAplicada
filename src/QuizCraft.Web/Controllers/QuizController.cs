using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Application.ViewModels;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using QuizCraft.Web.ViewModels;
using QuizCraft.Core.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AIService = QuizCraft.Application.Interfaces.IAIService;

namespace QuizCraft.Web.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<QuizController> _logger;
        private readonly IQuizGenerationService _quizGenerationService;
        private readonly AIService _aiService;
        private readonly IDocumentTextExtractor _textExtractor;
        private readonly IRepasoProgramadoService _repasoProgramadoService;

        private class QuizSessionData
        {
            public int QuizId { get; set; }
            public List<QuizPreguntaSession> Preguntas { get; set; } = new();
        }

        private class QuizPreguntaSession
        {
            public int PreguntaId { get; set; }
            public string RespuestaCorrecta { get; set; } = string.Empty;
            public List<OpcionRespuestaViewModel> Opciones { get; set; } = new();
        }

        public QuizController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<QuizController> logger,
            IQuizGenerationService quizGenerationService,
            AIService aiService,
            IDocumentTextExtractor textExtractor,
            IRepasoProgramadoService repasoProgramadoService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _quizGenerationService = quizGenerationService;
            _aiService = aiService;
            _textExtractor = textExtractor;
            _repasoProgramadoService = repasoProgramadoService;
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
            var todosLosQuizzes = await _unitOfWork.QuizRepository.GetQuizzesByCreadorIdAsync(usuarioId);
            var quizzesPublicos = await _unitOfWork.QuizRepository.GetQuizzesPublicosAsync();
            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);

            // Aplicar filtros a mis quizzes
            var misQuizzesFiltrados = todosLosQuizzes
                .Where(q => !materiaId.HasValue || q.MateriaId == materiaId)
                .Where(q => !dificultad.HasValue || q.NivelDificultad == (int)dificultad);

            // Debug: Log para verificar los datos
            _logger.LogInformation($"Usuario: {usuarioId}, Mis Quizzes: {todosLosQuizzes.Count()}, Quizzes Públicos: {quizzesPublicos.Count()}, Materias: {materias.Count()}");

            string ObtenerNombreUsuario(ApplicationUser? user) =>
                !string.IsNullOrWhiteSpace(user?.NombreCompleto) ? user!.NombreCompleto :
                !string.IsNullOrWhiteSpace(user?.Nombre) || !string.IsNullOrWhiteSpace(user?.Apellido)
                    ? $"{user?.Nombre} {user?.Apellido}".Trim()
                    : user?.UserName ?? "Usuario";

            int ObtenerTotalPreguntas(Quiz q) =>
                (q.Preguntas != null && q.Preguntas.Any())
                    ? q.Preguntas.Count
                    : (q.NumeroPreguntas > 0 ? q.NumeroPreguntas : 0);

            var viewModel = new QuizIndexViewModel
            {
                MisQuizzes = misQuizzesFiltrados.Select(q => new QuizItemViewModel
                {
                    Id = q.Id,
                    Titulo = q.Titulo ?? string.Empty,
                    Descripcion = q.Descripcion,
                    NumeroPreguntas = ObtenerTotalPreguntas(q),
                    MateriaNombre = q.Materia?.Nombre ?? "Sin materia",
                    CreadorNombre = ObtenerNombreUsuario(q.Creador),
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

                QuizzesCreados = misQuizzesFiltrados.Select(q => new QuizListItemViewModel
                {
                    Id = q.Id,
                    Titulo = q.Titulo ?? string.Empty,
                    Descripcion = q.Descripcion,
                    NumeroPreguntas = ObtenerTotalPreguntas(q),
                    MateriaNombre = q.Materia?.Nombre ?? "Sin materia",
                    CreadorNombre = ObtenerNombreUsuario(q.Creador),
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
                        NumeroPreguntas = ObtenerTotalPreguntas(q),
                        MateriaNombre = q.Materia?.Nombre ?? "Sin materia",
                        CreadorNombre = ObtenerNombreUsuario(q.Creador),
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
                TotalQuizzes = misQuizzesFiltrados.Count(),
                MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre ?? string.Empty,
                    TotalQuizzes = todosLosQuizzes.Count(q => q.MateriaId == m.Id)
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

            viewModel.MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre
            }).ToList();

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
                var usuarioIdForReload = _userManager.GetUserId(User);
                if (usuarioIdForReload != null)
                {
                    var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioIdForReload);
                    model.MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
                    {
                        Id = m.Id,
                        Nombre = m.Nombre
                    }).ToList();
                }
                return View(model);
            }

            var usuarioId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Validar que haya al menos una pregunta
            if (model.Preguntas == null || !model.Preguntas.Any())
            {
                ModelState.AddModelError("", "Debes agregar al menos una pregunta al quiz.");
                
                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
                model.MateriasDisponibles = materias.Select(m => new MateriaSelectViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre
                }).ToList();
                
                return View(model);
            }

            // Crear el quiz
            var quiz = new Quiz
            {
                Titulo = model.Titulo,
                Descripcion = model.Descripcion,
                NumeroPreguntas = model.Preguntas.Count,
                TiempoLimite = model.TiempoLimite,
                TiempoPorPregunta = model.TiempoPorPreguntaSegundos ?? 30,
                NivelDificultad = (int)model.Dificultad,
                EsPublico = model.EsPublico,
                MostrarRespuestasInmediato = model.MostrarRespuestasInmediato,
                PermitirReintento = model.PermitirReintento,
                MateriaId = model.MateriaId,
                CreadorId = usuarioId,
                FechaCreacion = DateTime.UtcNow
            };

            // Crear preguntas del quiz
            var preguntas = new List<PreguntaQuiz>();
            for (int i = 0; i < model.Preguntas.Count; i++)
            {
                var preguntaData = model.Preguntas[i];
                
                // Crear array con todas las opciones
                var opciones = new List<(string letra, string texto)>
                {
                    ("A", preguntaData.OpcionA),
                    ("B", preguntaData.OpcionB),
                    ("C", preguntaData.OpcionC),
                    ("D", preguntaData.OpcionD)
                };

                // Obtener el texto de la respuesta correcta
                var respuestaCorrectaTexto = preguntaData.RespuestaCorrecta.ToUpper() switch
                {
                    "A" => preguntaData.OpcionA,
                    "B" => preguntaData.OpcionB,
                    "C" => preguntaData.OpcionC,
                    "D" => preguntaData.OpcionD,
                    _ => preguntaData.OpcionA
                };

                // Si MezclarOpciones está activado, mezclar las opciones
                if (model.MezclarOpciones)
                {
                    opciones = opciones.OrderBy(x => Guid.NewGuid()).ToList();
                }

                var pregunta = new PreguntaQuiz
                {
                    TextoPregunta = preguntaData.Pregunta,
                    RespuestaCorrecta = respuestaCorrectaTexto,
                    OpcionA = opciones[0].texto,
                    OpcionB = opciones[1].texto,
                    OpcionC = opciones[2].texto,
                    OpcionD = opciones[3].texto,
                    Explicacion = preguntaData.Explicacion ?? "",
                    Orden = i + 1,
                    Puntos = 1,
                    TipoPregunta = 1,
                    FlashcardId = null // No está asociado a ninguna flashcard
                };

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

            // Crear ViewModels de las preguntas y aleatorizar sus opciones
            var preguntasViewModel = new List<PreguntaQuizViewModel>();
            var random = new Random(Guid.NewGuid().GetHashCode()); // Mejor semilla aleatoria

            foreach (var p in preguntasActivas)
            {
                // Crear lista de opciones con sus valores originales
                var opciones = new List<OpcionRespuestaViewModel>
                {
                    new OpcionRespuestaViewModel { Texto = p.OpcionA ?? "", Valor = "A", EsCorrecta = p.RespuestaCorrecta?.Trim().ToUpper() == "A" },
                    new OpcionRespuestaViewModel { Texto = p.OpcionB ?? "", Valor = "B", EsCorrecta = p.RespuestaCorrecta?.Trim().ToUpper() == "B" },
                    new OpcionRespuestaViewModel { Texto = p.OpcionC ?? "", Valor = "C", EsCorrecta = p.RespuestaCorrecta?.Trim().ToUpper() == "C" },
                    new OpcionRespuestaViewModel { Texto = p.OpcionD ?? "", Valor = "D", EsCorrecta = p.RespuestaCorrecta?.Trim().ToUpper() == "D" }
                }.Where(o => !string.IsNullOrEmpty(o.Texto)).ToList();

                // Aleatorizar las opciones usando Fisher-Yates shuffle para mejor distribución
                for (int i = opciones.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    var temp = opciones[i];
                    opciones[i] = opciones[j];
                    opciones[j] = temp;
                }

                // Reasignar letras A, B, C, D según el nuevo orden
                for (int i = 0; i < opciones.Count; i++)
                {
                    opciones[i].Valor = new[] { "A", "B", "C", "D" }[i];
                }

                // Determinar cuál es la nueva respuesta correcta
                var opcionCorrecta = opciones.FirstOrDefault(o => o.EsCorrecta);
                string nuevaRespuestaCorrecta = opcionCorrecta?.Valor ?? "A";

                var preguntaViewModel = new PreguntaQuizViewModel
                {
                    Id = p.Id,
                    TextoPregunta = p.TextoPregunta,
                    OpcionA = opciones.Count > 0 ? opciones[0].Texto : "",
                    OpcionB = opciones.Count > 1 ? opciones[1].Texto : "",
                    OpcionC = opciones.Count > 2 ? opciones[2].Texto : "",
                    OpcionD = opciones.Count > 3 ? opciones[3].Texto : "",
                    Orden = p.Orden,
                    Puntos = p.Puntos,
                    RespuestaCorrecta = nuevaRespuestaCorrecta,
                    Explicacion = p.Explicacion,
                    Opciones = opciones,
                    TodasLasOpciones = opciones
                };

                preguntasViewModel.Add(preguntaViewModel);
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
                Preguntas = preguntasViewModel
            };

            // Guardar en sesi�n el orden aleatorizado y la respuesta correcta seg�n se muestra al usuario
            try
            {
                var sessionData = new QuizSessionData
                {
                    QuizId = quiz.Id,
                    Preguntas = preguntasViewModel.Select(p => new QuizPreguntaSession
                    {
                        PreguntaId = p.Id,
                        RespuestaCorrecta = p.RespuestaCorrecta ?? string.Empty,
                        Opciones = p.Opciones
                    }).ToList()
                };

                var sessionKey = ObtenerClaveSesionQuiz(quiz.Id, usuarioId);
                HttpContext.Session.SetString(sessionKey, JsonSerializer.Serialize(sessionData));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo almacenar el orden de opciones en sesi�n para el quiz {QuizId}", quiz.Id);
            }

            // Log para debugging
            _logger.LogInformation($"Quiz {id}: TotalPreguntas={viewModel.TotalPreguntas}, PreguntasCargadas={viewModel.Preguntas.Count}");

            return View(viewModel);
        }

        // GET: Quiz/TomarQuiz - Nueva acción para el flujo pregunta por pregunta
        public async Task<IActionResult> TomarQuiz(int id)
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

            // Aleatorizar preguntas y guardar en sesión
            var random = new Random(Guid.NewGuid().GetHashCode());
            var preguntasAleatorias = preguntasActivas.OrderBy(x => random.Next()).ToList();
            
            // Capturar RepasoId si existe (viene desde repasos programados)
            var repasoId = TempData["RepasoId"] as int?;
            
            // Guardar información en TempData
            TempData["QuizId"] = id;
            TempData["PreguntaActual"] = 0;
            TempData["TotalPreguntas"] = preguntasAleatorias.Count;
            TempData["FechaInicio"] = DateTime.Now.ToString("o");
            TempData["PreguntasIds"] = JsonSerializer.Serialize(preguntasAleatorias.Select(p => p.Id).ToList());
            
            // Mantener RepasoId si existe
            if (repasoId.HasValue)
            {
                TempData["RepasoId"] = repasoId.Value;
            }
            
            TempData.Keep();

            return RedirectToAction(nameof(MostrarPregunta));
        }

        // GET: Quiz/MostrarPregunta
        public async Task<IActionResult> MostrarPregunta()
        {
            var usuarioId = _userManager.GetUserId(User);
            
            // Recuperar datos de TempData
            if (TempData["QuizId"] == null || TempData["PreguntaActual"] == null)
            {
                TempData["Error"] = "Sesión de quiz expirada.";
                return RedirectToAction(nameof(Index));
            }

            var quizId = (int)TempData["QuizId"]!;
            var preguntaActual = (int)TempData["PreguntaActual"]!;
            var totalPreguntas = (int)TempData["TotalPreguntas"]!;
            var fechaInicioStr = TempData["FechaInicio"]!.ToString();
            var preguntasIdsJson = TempData["PreguntasIds"]!.ToString();
            
            TempData.Keep();

            // Si ya se respondieron todas las preguntas
            if (preguntaActual >= totalPreguntas)
            {
                return RedirectToAction(nameof(FinalizarQuiz));
            }

            var preguntasIds = JsonSerializer.Deserialize<List<int>>(preguntasIdsJson!);
            var preguntaId = preguntasIds![preguntaActual];

            var quiz = await _unitOfWork.QuizRepository.GetQuizConPreguntasAsync(quizId);
            
            if (quiz == null)
            {
                TempData["Error"] = "Error al cargar el quiz.";
                return RedirectToAction(nameof(Index));
            }

            var pregunta = quiz.Preguntas.FirstOrDefault(p => p.Id == preguntaId);

            if (pregunta == null)
            {
                TempData["Error"] = "Error al cargar la pregunta.";
                return RedirectToAction(nameof(Index));
            }

            // Crear lista de opciones y aleatorizarlas
            var random = new Random(Guid.NewGuid().GetHashCode());
            var opciones = new List<OpcionQuizViewModel>
            {
                new OpcionQuizViewModel { Id = 1, Texto = pregunta.OpcionA ?? "", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "A", LetraOriginal = "A" },
                new OpcionQuizViewModel { Id = 2, Texto = pregunta.OpcionB ?? "", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "B", LetraOriginal = "B" },
                new OpcionQuizViewModel { Id = 3, Texto = pregunta.OpcionC ?? "", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "C", LetraOriginal = "C" },
                new OpcionQuizViewModel { Id = 4, Texto = pregunta.OpcionD ?? "", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "D", LetraOriginal = "D" }
            }.Where(o => !string.IsNullOrEmpty(o.Texto)).ToList();

            // Aleatorizar opciones
            opciones = opciones.OrderBy(x => random.Next()).ToList();
            
            // Reasignar IDs secuenciales después de aleatorizar
            for (int i = 0; i < opciones.Count; i++)
            {
                opciones[i].Id = i + 1;
            }
            
            // Guardar el mapeo de IDs a letras originales para esta pregunta
            var opcionesMapeo = opciones.ToDictionary(o => o.Id, o => o.LetraOriginal);
            TempData[$"OpcionesMapeo_{preguntaId}"] = JsonSerializer.Serialize(opcionesMapeo);
            TempData.Keep();

            var viewModel = new TomarQuizViewModel
            {
                QuizId = quizId,
                Titulo = quiz.Titulo,
                Descripcion = quiz.Descripcion,
                MateriaNombre = quiz.Materia?.Nombre ?? "Sin materia",
                TotalPreguntas = totalPreguntas,
                PreguntaActual = preguntaActual + 1,
                FechaInicio = DateTime.Parse(fechaInicioStr!),
                PreguntaId = preguntaId,
                PreguntaTexto = pregunta.TextoPregunta,
                Opciones = opciones,
                Explicacion = pregunta.Explicacion
            };

            return View(viewModel);
        }

        // POST: Quiz/EvaluarRespuesta
        [HttpPost]
        public async Task<IActionResult> EvaluarRespuesta(int preguntaId, int opcionSeleccionadaId)
        {
            // Obtener el quizId de TempData
            if (TempData["QuizId"] == null)
            {
                return Json(new { success = false, message = "Sesión expirada" });
            }

            var quizId = (int)TempData["QuizId"]!;
            TempData.Keep();

            var quiz = await _unitOfWork.QuizRepository.GetQuizConPreguntasAsync(quizId);
            
            if (quiz == null)
            {
                return Json(new { success = false, message = "Quiz no encontrado" });
            }

            var pregunta = quiz.Preguntas.FirstOrDefault(p => p.Id == preguntaId);
            
            if (pregunta == null)
            {
                return Json(new { success = false, message = "Pregunta no encontrada" });
            }

            // Recuperar el mapeo de opciones guardado en MostrarPregunta
            var mapeoJson = TempData[$"OpcionesMapeo_{preguntaId}"]?.ToString();
            TempData.Keep();
            
            if (string.IsNullOrEmpty(mapeoJson))
            {
                return Json(new { success = false, message = "Sesión expirada. El mapeo de opciones no está disponible." });
            }
            
            var opcionesMapeo = JsonSerializer.Deserialize<Dictionary<int, string>>(mapeoJson);
            if (opcionesMapeo == null || !opcionesMapeo.ContainsKey(opcionSeleccionadaId))
            {
                return Json(new { success = false, message = "Opción seleccionada no válida" });
            }
            
            // Obtener la letra original de la opción seleccionada
            var letraSeleccionada = opcionesMapeo[opcionSeleccionadaId];
            var esCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == letraSeleccionada;

            // Guardar la respuesta en TempData
            var respuestasJson = TempData["Respuestas"]?.ToString() ?? "[]";
            var respuestas = JsonSerializer.Deserialize<List<RespuestaQuizTemp>>(respuestasJson) ?? new List<RespuestaQuizTemp>();
            
            respuestas.Add(new RespuestaQuizTemp
            {
                PreguntaId = preguntaId,
                OpcionSeleccionadaId = opcionSeleccionadaId,
                LetraSeleccionada = letraSeleccionada,
                EsCorrecta = esCorrecta
            });

            TempData["Respuestas"] = JsonSerializer.Serialize(respuestas);
            TempData.Keep();

            return Json(new
            {
                success = true,
                esCorrecta = esCorrecta,
                explicacion = pregunta.Explicacion
            });
        }

        // POST: Quiz/SiguientePregunta
        [HttpPost]
        public IActionResult SiguientePregunta()
        {
            var preguntaActual = (int)TempData["PreguntaActual"]!;
            TempData["PreguntaActual"] = preguntaActual + 1;
            TempData.Keep();

            return RedirectToAction(nameof(MostrarPregunta));
        }

        // GET: Quiz/FinalizarQuiz
        public async Task<IActionResult> FinalizarQuiz()
        {
            // Recuperar datos de TempData
            if (TempData["QuizId"] == null)
            {
                TempData["Error"] = "Sesión de quiz expirada.";
                return RedirectToAction(nameof(Index));
            }

            var quizId = (int)TempData["QuizId"]!;
            var totalPreguntas = (int)TempData["TotalPreguntas"]!;
            var fechaInicioStr = TempData["FechaInicio"]!.ToString();
            var respuestasJson = TempData["Respuestas"]?.ToString() ?? "[]";
            var repasoId = TempData["RepasoId"] as int?;
            
            var quiz = await _unitOfWork.QuizRepository.GetQuizCompletoAsync(quizId);
            
            if (quiz == null)
            {
                TempData["Error"] = "Quiz no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var respuestas = JsonSerializer.Deserialize<List<RespuestaQuizTemp>>(respuestasJson) ?? new List<RespuestaQuizTemp>();
            var respuestasCorrectas = respuestas.Count(r => r.EsCorrecta);
            var respuestasIncorrectas = respuestas.Count(r => !r.EsCorrecta);
            var fechaInicio = DateTime.Parse(fechaInicioStr!);
            var fechaFin = DateTime.Now;

            // Obtener IDs de preguntas incorrectas
            var preguntasIncorrectasIds = respuestas
                .Where(r => !r.EsCorrecta)
                .Select(r => r.PreguntaId)
                .ToList();

            var viewModel = new EstadisticasQuizViewModel
            {
                QuizProgramadoId = repasoId,
                QuizId = quizId,
                QuizTitulo = quiz.Titulo,
                MateriaNombre = quiz.Materia?.Nombre ?? "Sin materia",
                TotalPreguntas = totalPreguntas,
                RespuestasCorrectas = respuestasCorrectas,
                RespuestasIncorrectas = respuestasIncorrectas,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TiempoTotal = fechaFin - fechaInicio,
                PreguntasIncorrectasIds = preguntasIncorrectasIds
            };

            // Guardar en TempData para MarcarQuizCompletado
            TempData["EstadisticasQuiz"] = JsonSerializer.Serialize(viewModel);
            TempData.Keep();

            // Limpiar datos de sesión del quiz
            TempData.Remove("QuizId");
            TempData.Remove("PreguntaActual");
            TempData.Remove("TotalPreguntas");
            TempData.Remove("FechaInicio");
            TempData.Remove("PreguntasIds");
            TempData.Remove("Respuestas");
            TempData.Remove("RepasoId");

            return RedirectToAction(nameof(EstadisticasQuiz));
        }

        [HttpGet]
        public IActionResult EstadisticasQuiz()
        {
            var estadisticasJson = TempData["EstadisticasQuiz"]?.ToString();
            
            if (string.IsNullOrEmpty(estadisticasJson))
            {
                TempData["Error"] = "No se encontraron estadísticas del quiz.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = JsonSerializer.Deserialize<EstadisticasQuizViewModel>(estadisticasJson);
            TempData.Keep("EstadisticasQuiz");

            return View(viewModel);
        }

        // POST: Quiz/MarcarQuizCompletado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarQuizCompletado(int? repasoId)
        {
            var usuarioId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Recuperar estadísticas de TempData
            var estadisticasJson = TempData["EstadisticasQuiz"]?.ToString();
            
            if (string.IsNullOrEmpty(estadisticasJson))
            {
                TempData["Error"] = "No se encontraron las estadísticas del quiz.";
                return RedirectToAction(nameof(Index));
            }

            var estadisticas = JsonSerializer.Deserialize<EstadisticasQuizViewModel>(estadisticasJson);

            if (estadisticas == null)
            {
                TempData["Error"] = "Error al procesar las estadísticas.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Guardar el resultado del quiz
                var quiz = await _unitOfWork.QuizRepository.GetQuizConPreguntasAsync(estadisticas.QuizId);
                
                if (quiz == null)
                {
                    TempData["Error"] = "Quiz no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var porcentajeAcierto = estadisticas.TotalPreguntas > 0 
                    ? (double)estadisticas.RespuestasCorrectas / estadisticas.TotalPreguntas * 100 
                    : 0;

                var puntajeMaximo = quiz.Preguntas.Sum(p => p.Puntos);
                var puntajeObtenido = (int)(puntajeMaximo * (porcentajeAcierto / 100));

                var resultado = new ResultadoQuiz
                {
                    QuizId = estadisticas.QuizId,
                    UsuarioId = usuarioId,
                    FechaInicio = estadisticas.FechaInicio,
                    FechaFinalizacion = estadisticas.FechaFin,
                    FechaRealizacion = DateTime.Now,
                    TiempoTranscurrido = (int)estadisticas.TiempoTotal.TotalSeconds,
                    TiempoTotal = estadisticas.TiempoTotal,
                    PorcentajeAcierto = porcentajeAcierto,
                    PuntajeObtenido = puntajeObtenido,
                    PuntajeMaximo = puntajeMaximo,
                    Puntuacion = puntajeMaximo > 0 ? (decimal)puntajeObtenido / puntajeMaximo : 0,
                    EstaCompletado = true
                };

                // Guardar usando el contexto directamente
                var context = (QuizCraft.Infrastructure.Data.ApplicationDbContext)_unitOfWork.GetType()
                    .GetProperty("Context")!
                    .GetValue(_unitOfWork)!;

                context.ResultadosQuiz.Add(resultado);
                await context.SaveChangesAsync();

                // Si viene de un repaso programado, completarlo y crear uno nuevo con las incorrectas
                if (repasoId.HasValue)
                {
                    var repaso = await _repasoProgramadoService.ObtenerRepasoPorIdAsync(repasoId.Value, usuarioId);
                    
                    if (repaso != null && !repaso.Completado)
                    {
                        // Completar el repaso actual
                        var completarViewModel = new CompletarRepasoViewModel
                        {
                            Id = repasoId.Value,
                            NotasRepaso = $"Completado con {estadisticas.RespuestasCorrectas} de {estadisticas.TotalPreguntas} correctas ({porcentajeAcierto:F1}%)",
                            Puntaje = porcentajeAcierto,
                            ProgramarProximo = false
                        };
                        
                        await _repasoProgramadoService.CompletarRepasoAsync(completarViewModel, usuarioId);
                        
                        // Crear repaso programado para mañana con las preguntas incorrectas si hay alguna
                        if (estadisticas.PreguntasIncorrectasIds.Any())
                        {
                            var nuevoRepaso = new CrearRepasoProgramadoViewModel
                            {
                                Titulo = $"Repaso: {quiz.Titulo} - Preguntas Incorrectas",
                                Descripcion = $"Repaso automático creado con {estadisticas.PreguntasIncorrectasIds.Count} pregunta(s) incorrecta(s) del quiz anterior.",
                                FechaProgramada = DateTime.Now.AddDays(1),
                                QuizId = estadisticas.QuizId,
                                TipoRepaso = TipoRepaso.Automatico,
                                FrecuenciaRepeticion = FrecuenciaRepaso.Unica
                            };
                            
                            await _repasoProgramadoService.CrearRepasoProgramadoAsync(nuevoRepaso, usuarioId);
                            
                            TempData["Success"] = $"¡Repaso completado! Obtuviste {estadisticas.RespuestasCorrectas} de {estadisticas.TotalPreguntas} correctas ({porcentajeAcierto:F1}%). Se ha programado un repaso completo del quiz para mañana con todas sus preguntas para reforzar tu aprendizaje.";
                        }
                        else
                        {
                            TempData["Success"] = $"¡Perfecto! Completaste el quiz con todas las respuestas correctas ({porcentajeAcierto:F1}%).";
                        }
                    }
                }
                else
                {
                    TempData["Success"] = $"Quiz completado. Obtuviste {estadisticas.RespuestasCorrectas} de {estadisticas.TotalPreguntas} respuestas correctas ({porcentajeAcierto:F1}%).";
                }
                
                // Limpiar TempData
                TempData.Remove("EstadisticasQuiz");

                return RedirectToAction("Index", "Repaso", new { tab = "completados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar quiz como completado. RepasoId: {RepasoId}", repasoId);
                TempData["Error"] = "Ocurrió un error al guardar el resultado del quiz.";
                return RedirectToAction(nameof(Index));
            }
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
                CreadorNombre = quiz.Creador?.NombreCompleto ?? quiz.Creador?.UserName ?? "Usuario",
                FechaCreacion = quiz.FechaCreacion,
                EsPublico = quiz.EsPublico,
                NumeroPreguntas = quiz.NumeroPreguntas,
                CantidadPreguntas = quiz.Preguntas?.Count ?? quiz.NumeroPreguntas, // Corregir contador de preguntas
                TiempoLimite = quiz.TiempoLimite,
                TiempoPorPregunta = quiz.TiempoPorPregunta,
                NivelDificultad = (NivelDificultad)quiz.NivelDificultad,
                PuedeEditar = quiz.CreadorId == usuarioId,
                PuedeEliminar = quiz.CreadorId == usuarioId,
                PuedeRealizarQuiz = quiz.EsPublico || quiz.CreadorId == usuarioId,
                MostrarPreguntas = quiz.CreadorId == usuarioId, // Solo el creador puede ver las preguntas
                Preguntas = quiz.Preguntas?.Select(p => new PreguntaQuizViewModel
                {
                    Id = p.Id,
                    TextoPregunta = p.TextoPregunta,
                    OpcionA = p.OpcionA ?? "",
                    OpcionB = p.OpcionB ?? "",
                    OpcionC = p.OpcionC ?? "",
                    OpcionD = p.OpcionD ?? "",
                    RespuestaCorrecta = p.RespuestaCorrecta,
                    Explicacion = p.Explicacion,
                    Puntos = p.Puntos,
                    Orden = p.Orden,
                    Opciones = new List<OpcionRespuestaViewModel>
                    {
                        new OpcionRespuestaViewModel { Texto = p.OpcionA ?? "", Valor = "A", EsCorrecta = p.RespuestaCorrecta == "A" },
                        new OpcionRespuestaViewModel { Texto = p.OpcionB ?? "", Valor = "B", EsCorrecta = p.RespuestaCorrecta == "B" },
                        new OpcionRespuestaViewModel { Texto = p.OpcionC ?? "", Valor = "C", EsCorrecta = p.RespuestaCorrecta == "C" },
                        new OpcionRespuestaViewModel { Texto = p.OpcionD ?? "", Valor = "D", EsCorrecta = p.RespuestaCorrecta == "D" }
                    }
                }).ToList() ?? new List<PreguntaQuizViewModel>(),
                MensajeNoDisponible = quiz.EsPublico || quiz.CreadorId == usuarioId
                    ? "" : "Este quiz no está disponible públicamente."
            };

            return View(viewModel);
        }

        // GET: Quiz/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id, bool incluirRespuestas = true)
        {
            var usuarioId = _userManager.GetUserId(User);
            var quiz = await _unitOfWork.QuizRepository.GetQuizCompletoAsync(id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Solo el creador puede descargar el PDF
            if (quiz.CreadorId != usuarioId)
            {
                return Forbid();
            }

            try
            {
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
                                column.Item().Text(quiz.Titulo)
                                    .FontSize(20).Bold().FontColor("#2c3e50");
                                
                                column.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Materia: ").Bold();
                                    text.Span(quiz.Materia?.Nombre ?? "Sin materia");
                                });
                                
                                if (!string.IsNullOrEmpty(quiz.Descripcion))
                                {
                                    column.Item().PaddingTop(5).Text(text =>
                                    {
                                        text.Span("Descripción: ").Bold();
                                        text.Span(quiz.Descripcion);
                                    });
                                }
                                
                                column.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Total de preguntas: ").Bold();
                                    text.Span($"{quiz.Preguntas?.Count ?? 0}");
                                });
                                
                                column.Item().PaddingTop(10).LineHorizontal(1).LineColor("#bdc3c7");
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                if (quiz.Preguntas != null && quiz.Preguntas.Any())
                                {
                                    for (int i = 0; i < quiz.Preguntas.Count; i++)
                                    {
                                        var pregunta = quiz.Preguntas.ElementAt(i);
                                        
                                        column.Item().PaddingTop(15).Column(preguntaColumn =>
                                        {
                                            // Número y texto de la pregunta
                                            preguntaColumn.Item().Text($"Pregunta {i + 1}")
                                                .FontSize(14).Bold().FontColor("#34495e");
                                            
                                            preguntaColumn.Item().PaddingTop(5).Text(pregunta.TextoPregunta)
                                                .FontSize(12);

                                            // Opciones
                                            preguntaColumn.Item().PaddingTop(10).Column(opcionesColumn =>
                                            {
                                                var opciones = new[] {
                                                    ("A", pregunta.OpcionA),
                                                    ("B", pregunta.OpcionB),
                                                    ("C", pregunta.OpcionC),
                                                    ("D", pregunta.OpcionD)
                                                };

                                                foreach (var (letra, texto) in opciones)
                                                {
                                                    if (!string.IsNullOrEmpty(texto))
                                                    {
                                                        bool esCorrecta = incluirRespuestas && pregunta.RespuestaCorrecta == letra;
                                                        
                                                        opcionesColumn.Item().PaddingTop(3).Text(text =>
                                                        {
                                                            text.Span($"{letra}) ").Bold();
                                                            text.Span(texto);
                                                            
                                                            if (esCorrecta)
                                                            {
                                                                text.Span(" ✓").Bold().FontColor("#27ae60");
                                                            }
                                                        });
                                                    }
                                                }
                                            });

                                            // Respuesta correcta destacada (solo si incluirRespuestas es true)
                                            if (incluirRespuestas)
                                            {
                                                preguntaColumn.Item().PaddingTop(10).Background("#d5f4e6")
                                                    .Padding(8).Text(text =>
                                                    {
                                                        text.Span("Respuesta correcta: ").Bold().FontColor("#27ae60");
                                                        text.Span($"{pregunta.RespuestaCorrecta}) ");
                                                        
                                                        var textoRespuesta = pregunta.RespuestaCorrecta switch
                                                        {
                                                            "A" => pregunta.OpcionA,
                                                            "B" => pregunta.OpcionB,
                                                            "C" => pregunta.OpcionC,
                                                            "D" => pregunta.OpcionD,
                                                            _ => ""
                                                        };
                                                        text.Span(textoRespuesta ?? "");
                                                    });
                                            }

                                            // Explicación si existe (solo si incluirRespuestas es true)
                                            if (incluirRespuestas && !string.IsNullOrEmpty(pregunta.Explicacion))
                                            {
                                                preguntaColumn.Item().PaddingTop(5).Background("#fff9e6")
                                                    .Padding(8).Text(text =>
                                                    {
                                                        text.Span("Explicación: ").Bold().FontColor("#f39c12");
                                                        text.Span(pregunta.Explicacion);
                                                    });
                                            }
                                        });
                                    }
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
                var sufijo = incluirRespuestas ? "_con_respuestas" : "";
                var fileName = $"{quiz.Titulo.Replace(" ", "_")}{sufijo}_{DateTime.Now:yyyyMMdd}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del quiz {QuizId}", id);
                TempData["Error"] = "Error al generar el PDF. Por favor, intenta nuevamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
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

            // Verificar si el quiz es importado
            var importacion = await _unitOfWork.QuizCompartidoRepository.GetImportacionByQuizIdAsync(id);
            
            var viewModel = new QuizDetailsViewModel
            {
                Id = quiz.Id,
                Titulo = quiz.Titulo,
                Descripcion = quiz.Descripcion,
                MateriaNombre = quiz.Materia?.Nombre ?? "Sin materia",
                CreadorNombre = quiz.Creador?.NombreCompleto ?? quiz.Creador?.UserName ?? "Usuario",
                FechaCreacion = quiz.FechaCreacion,
                EsPublico = quiz.EsPublico,
                NumeroPreguntas = quiz.NumeroPreguntas,
                CantidadPreguntas = quiz.Preguntas?.Count ?? quiz.NumeroPreguntas,
                CantidadIntentos = quiz.Resultados?.Count ?? 0,
                MisResultados = quiz.Resultados?
                    .Where(r => r.UsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaRealizacion)
                    .Select(r => new ResultadoQuizResumenViewModel
                    {
                        Id = r.Id,
                        PorcentajeAcierto = r.PorcentajeAcierto,
                        FechaRealizacion = r.FechaRealizacion,
                        TiempoTotal = r.TiempoTotal,
                        PuntajeObtenido = r.PuntajeObtenido,
                        PuntajeMaximo = r.PuntajeMaximo
                    }).ToList() ?? new List<ResultadoQuizResumenViewModel>(),
                PromedioCalificacion = quiz.Resultados?.Any(r => r.UsuarioId == usuarioId) == true
                    ? quiz.Resultados.Where(r => r.UsuarioId == usuarioId).Average(r => r.PorcentajeAcierto)
                    : null,
                // Información de importación
                EsImportado = importacion != null,
                CreadorOriginalNombre = importacion?.QuizCompartido?.Quiz?.Creador?.NombreCompleto 
                    ?? importacion?.QuizCompartido?.Quiz?.Creador?.UserName,
                FechaImportacion = importacion?.FechaCreacion
            };

            return View(viewModel);
        }

        // POST: Quiz/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                
                // Obtener quiz completo con todas sus relaciones
                var quiz = await _unitOfWork.QuizRepository.GetQuizCompletoAsync(id);

                if (quiz == null)
                {
                    TempData["Error"] = "Quiz no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Solo el creador puede eliminar
                if (quiz.CreadorId != usuarioId)
                {
                    TempData["Error"] = "No tienes permiso para eliminar este quiz.";
                    return Forbid();
                }

                var tituloQuiz = quiz.Titulo;
                var cantidadPreguntas = quiz.Preguntas?.Count ?? 0;
                var cantidadResultados = quiz.Resultados?.Count ?? 0;

                _logger.LogInformation("Eliminando quiz {QuizId} - '{Titulo}' con {Preguntas} preguntas y {Resultados} resultados",
                    quiz.Id, tituloQuiz, cantidadPreguntas, cantidadResultados);

                // Obtener contexto para trabajar directamente con las entidades
                var context = ((QuizCraft.Infrastructure.Repositories.UnitOfWork)_unitOfWork).GetContext();

                // 0. Si es un quiz importado, eliminar primero el registro de QuizImportado
                var importacion = await _unitOfWork.QuizCompartidoRepository.GetImportacionByQuizIdAsync(id);
                if (importacion != null)
                {
                    _logger.LogInformation("Quiz importado - Eliminando registro de importación {ImportacionId} antes de eliminar quiz", importacion.Id);
                    // Eliminar directamente del contexto
                    var importacionEnContexto = await context.QuizzesImportados.FindAsync(importacion.Id);
                    if (importacionEnContexto != null)
                    {
                        context.QuizzesImportados.Remove(importacionEnContexto);
                        _logger.LogInformation("Registro de importación marcado para eliminación");
                    }
                }

                // 1. Eliminar manualmente los resultados (tienen DeleteBehavior.Restrict)
                if (quiz.Resultados != null && quiz.Resultados.Any())
                {
                    foreach (var resultado in quiz.Resultados.ToList())
                    {
                        context.ResultadosQuiz.Remove(resultado);
                    }
                    _logger.LogInformation("Marcados {Count} resultados para eliminación", cantidadResultados);
                }

                // 2. Eliminar el quiz (las PreguntasQuiz se eliminarán en cascada)
                _unitOfWork.QuizRepository.Remove(quiz);
                
                // 3. Guardar todos los cambios en una sola transacción
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Quiz {QuizId} - '{Titulo}' eliminado exitosamente", id, tituloQuiz);
                TempData["Success"] = $"Quiz '{tituloQuiz}' eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar quiz {QuizId}", id);
                TempData["Error"] = "No se puede eliminar el quiz. Puede haber datos relacionados que lo impiden.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar quiz {QuizId}", id);
                TempData["Error"] = "Ocurrió un error al eliminar el quiz. Por favor, intenta nuevamente.";
                return RedirectToAction(nameof(Delete), new { id });
            }
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

                // Recuperar el orden y respuestas correctas que vio el usuario
                QuizSessionData? sessionData = null;
                try
                {
                    var sessionKey = ObtenerClaveSesionQuiz(QuizId, usuarioId);
                    var sessionJson = HttpContext.Session.GetString(sessionKey);
                    if (!string.IsNullOrEmpty(sessionJson))
                    {
                        sessionData = JsonSerializer.Deserialize<QuizSessionData>(sessionJson);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo recuperar la sesi�n del quiz {QuizId}", QuizId);
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

                    var sessionPregunta = sessionData?.Preguntas.FirstOrDefault(p => p.PreguntaId == pregunta.Id);
                    var opcionesPregunta = sessionPregunta?.Opciones ?? ConstruirOpcionesDesdePregunta(pregunta);

                    var respuestaCorrecta = sessionPregunta?.RespuestaCorrecta?.Trim().ToUpper()
                        ?? pregunta.RespuestaCorrecta?.Trim().ToUpper()
                        ?? string.Empty;

                    // Asegurar que la letra correcta coincide con las opciones mostradas al usuario
                    var respuestaCorrectaDesdeOpciones = opcionesPregunta.FirstOrDefault(o => o.EsCorrecta)?.Valor;
                    if (!string.IsNullOrEmpty(respuestaCorrectaDesdeOpciones))
                    {
                        respuestaCorrecta = respuestaCorrectaDesdeOpciones;
                    }

                    var textoSeleccionado = opcionesPregunta
                        .FirstOrDefault(o => o.Valor.Equals(respuestaUsuario, StringComparison.OrdinalIgnoreCase))
                        ?.Texto ?? respuestaUsuario;

                    var textoCorrecto = opcionesPregunta.FirstOrDefault(o => o.EsCorrecta)?.Texto ?? respuestaCorrecta;

                    bool esCorrecta = !string.IsNullOrEmpty(respuestaUsuario) &&
                                      !string.IsNullOrEmpty(respuestaCorrecta) &&
                                      respuestaUsuario.Equals(respuestaCorrecta, StringComparison.OrdinalIgnoreCase);

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
                        RespuestaDada = textoSeleccionado,
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

        private static List<OpcionRespuestaViewModel> ConstruirOpcionesDesdePregunta(PreguntaQuiz pregunta)
        {
            return new List<OpcionRespuestaViewModel>
            {
                new() { Texto = pregunta.OpcionA ?? "", Valor = "A", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "A" },
                new() { Texto = pregunta.OpcionB ?? "", Valor = "B", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "B" },
                new() { Texto = pregunta.OpcionC ?? "", Valor = "C", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "C" },
                new() { Texto = pregunta.OpcionD ?? "", Valor = "D", EsCorrecta = pregunta.RespuestaCorrecta?.Trim().ToUpper() == "D" }
            }.Where(o => !string.IsNullOrWhiteSpace(o.Texto)).ToList();
        }

        private string ObtenerClaveSesionQuiz(int quizId, string? usuarioId)
        {
            return $"QUIZ_SESSION_{quizId}_{usuarioId}";
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

                QuizSessionData? sessionData = null;
                try
                {
                    var sessionKey = ObtenerClaveSesionQuiz(resultado.QuizId, usuarioId);
                    var sessionJson = HttpContext.Session.GetString(sessionKey);
                    if (!string.IsNullOrEmpty(sessionJson))
                    {
                        sessionData = JsonSerializer.Deserialize<QuizSessionData>(sessionJson);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo recuperar la sesión de opciones para los resultados del quiz {QuizId}", resultado.QuizId);
                }

                foreach (var pregunta in preguntasOrdenadas)
                {
                    var respuestaUsuario = resultado.RespuestasUsuario.FirstOrDefault(r => r.PreguntaQuizId == pregunta.Id);

                    var sessionPregunta = sessionData?.Preguntas.FirstOrDefault(p => p.PreguntaId == pregunta.Id);
                    var opciones = sessionPregunta?.Opciones ?? ConstruirOpcionesDesdePregunta(pregunta);

                    // Asegurar que todas las opciones tengan letra asignada
                    for (int i = 0; i < opciones.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(opciones[i].Valor))
                        {
                            opciones[i].Valor = new[] { "A", "B", "C", "D" }[i];
                        }
                    }

                    var opcionCorrecta = opciones.FirstOrDefault(o => o.EsCorrecta);
                    string respuestaCorrectaValor = sessionPregunta?.RespuestaCorrecta
                        ?? opcionCorrecta?.Valor
                        ?? pregunta.RespuestaCorrecta
                        ?? "A";

                    var respuestaCorrectaTexto = opcionCorrecta?.Texto
                        ?? opciones.FirstOrDefault(o => o.Valor.Equals(respuestaCorrectaValor, StringComparison.OrdinalIgnoreCase))?.Texto
                        ?? respuestaCorrectaValor;

                    var detalleRespuesta = new RespuestaDetalleViewModel
                    {
                        Orden = pregunta.Orden,
                        Pregunta = pregunta.TextoPregunta,
                        RespuestaUsuario = respuestaUsuario?.RespuestaDada ?? "Sin respuesta",
                        RespuestaUsuarioValor = respuestaUsuario?.RespuestaSeleccionada ?? string.Empty,
                        RespuestaCorrecta = respuestaCorrectaTexto,
                        RespuestaCorrectaValor = respuestaCorrectaValor,
                        EsCorrecta = respuestaUsuario?.EsCorrecta ?? false,
                        Explicacion = pregunta.Explicacion,
                        TodasLasOpciones = opciones
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

        /// <summary>
        /// Acción para cuando se completa un quiz desde un repaso programado
        /// </summary>
        public async Task<IActionResult> CompletarDesdeRepaso(int resultadoId)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Obtener el resultado con sus relaciones
                var context = (QuizCraft.Infrastructure.Data.ApplicationDbContext)_unitOfWork.GetType()
                    .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_unitOfWork)!;

                var resultado = await context.Set<ResultadoQuiz>()
                    .Include(r => r.Quiz)
                    .FirstOrDefaultAsync(r => r.Id == resultadoId && r.UsuarioId == usuarioId);

                if (resultado == null)
                {
                    TempData["Error"] = "Resultado no encontrado.";
                    return RedirectToAction("Index", "Repaso");
                }

                // Obtener información del repaso de TempData
                var repasoId = TempData["RepasoId"] as int?;
                var repasoTitulo = TempData["RepasoTitulo"] as string;

                // Redirigir al completar repaso con los resultados
                return RedirectToAction("CompletarConResultados", "Repaso", new
                {
                    repasoId = repasoId,
                    puntaje = Math.Round(resultado.PorcentajeAcierto, 1),
                    resultado = $"Quiz '{resultado.Quiz.Titulo}' completado - {resultado.RespuestasUsuario.Count(r => r.EsCorrecta)}/{resultado.Quiz.Preguntas.Count} respuestas correctas"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar quiz desde repaso {ResultadoId}", resultadoId);
                TempData["Error"] = "Error al procesar la finalización del repaso.";
                return RedirectToAction("Index", "Repaso");
            }
        }

        #region Generación de Quizzes con IA

        // GET: Quiz/ConfigureAI
        public async Task<IActionResult> ConfigureAI(int? materiaId)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Account");

            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);

            var viewModel = new QuizGenerationConfigViewModel
            {
                MateriaId = materiaId,
                MateriaNombre = materiaId.HasValue ? materias.FirstOrDefault(m => m.Id == materiaId)?.Nombre : null,
                NumberOfQuestions = 10,
                DifficultyLevel = NivelDificultad.Intermedio,
                IncludeExplanations = true,
                VariedComplexity = false,
                SelectedQuestionTypes = new List<QuestionType> { QuestionType.MultipleChoice, QuestionType.TrueFalse }
            };

            ViewBag.Materias = materias.Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Nombre,
                Selected = m.Id == materiaId
            }).ToList();

            return View(viewModel);
        }

        // POST: Quiz/GenerateFromText
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateFromText(QuizGenerationConfigViewModel model, IFormFile? ArchivoAdjunto)
        {
            try
            {
                _logger.LogInformation("=== Iniciando GenerateFromText ===");
                _logger.LogInformation("Model.MateriaId: {MateriaId}", model.MateriaId);
                _logger.LogInformation("Model.TextContent: {TextContent}", model.TextContent?.Length);
                _logger.LogInformation("ArchivoAdjunto: {FileName}", ArchivoAdjunto?.FileName ?? "ninguno");
                _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);

                var usuarioId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(usuarioId))
                    return RedirectToAction("Login", "Account");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState no es válido");
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning("Error en {Key}: {Errors}", error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }

                    var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
                    ViewBag.Materias = materias.Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Nombre,
                        Selected = m.Id == model.MateriaId
                    }).ToList();
                    return View("ConfigureAI", model);
                }

                // Validar que se haya adjuntado un archivo
                if (ArchivoAdjunto == null || ArchivoAdjunto.Length == 0)
                {
                    _logger.LogWarning("No se adjuntó archivo");
                    TempData["Error"] = "Debes adjuntar un archivo (TXT, PDF, DOCX o PPTX) para generar el quiz.";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                _logger.LogInformation("Procesando archivo: {FileName}, Tamaño: {Size} bytes", ArchivoAdjunto.FileName, ArchivoAdjunto.Length);
                
                var extension = Path.GetExtension(ArchivoAdjunto.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".txt", ".pdf", ".docx", ".pptx" };
                
                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Formato no permitido: {Extension}", extension);
                    TempData["Error"] = $"Solo se permiten archivos TXT, PDF, DOCX o PPTX. Formato recibido: {extension}";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                string contenido;
                try
                {
                    // Extraer texto del documento usando el servicio de extracción
                    _logger.LogInformation("Extrayendo texto del archivo {FileName}...", ArchivoAdjunto.FileName);
                    var documentContent = await _textExtractor.ExtractTextAsync(ArchivoAdjunto.OpenReadStream(), ArchivoAdjunto.FileName);
                    contenido = documentContent?.RawText ?? string.Empty;
                    
                    _logger.LogInformation("Contenido extraído: {Length} caracteres", contenido.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al extraer texto del archivo {FileName}", ArchivoAdjunto.FileName);
                    TempData["Error"] = $"Error al procesar el archivo. Verifique que el archivo sea válido y contenga texto extraíble.";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                // Validar que el archivo tenga contenido
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    _logger.LogWarning("El archivo no contiene texto extraíble");
                    TempData["Error"] = "El archivo está vacío o no contiene texto extraíble. Asegúrese de que el documento contenga texto válido.";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                // Limitar el texto si es muy largo (para evitar costos excesivos y límites de la API)
                var longitudOriginal = contenido.Length;
                if (contenido.Length > 10000)
                {
                    contenido = contenido.Substring(0, 10000);
                    _logger.LogInformation("Texto truncado de {Original} a 10,000 caracteres para gestionar costos", longitudOriginal);
                    TempData["Warning"] = $"El documento es muy extenso ({longitudOriginal:N0} caracteres). Se procesarán los primeros 10,000 caracteres.";
                }

                _logger.LogInformation("Procesando {Length} caracteres para generación de quiz", contenido.Length);

                // Convertir ViewModel a configuración de generación
                var settings = model.ToQuizGenerationSettings();
                _logger.LogInformation("Settings creadas - NumberOfQuestions: {Count}, DifficultyLevel: {Difficulty}",
                    settings.NumberOfQuestions, settings.DifficultyLevel);

                // Generar quiz usando IA
                _logger.LogInformation("Llamando a GenerateFromTextAsync con {Length} caracteres...", contenido.Length);
                var result = await _quizGenerationService.GenerateFromTextAsync(contenido, settings);
                _logger.LogInformation("Resultado de IA - Success: {Success}, Questions Count: {Count}, Error: {Error}",
                    result.Success, result.Questions?.Count ?? 0, result.ErrorMessage);

                if (!result.Success)
                {
                    _logger.LogError("Error en generación de IA: {Error}", result.ErrorMessage);
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                // Verificar si se generaron preguntas
                if (result.Questions == null || result.Questions.Count == 0)
                {
                    _logger.LogWarning("No se generaron preguntas. Success: {Success}, Error: {Error}", result.Success, result.ErrorMessage);
                    TempData["Error"] = "No se pudieron generar preguntas. Intenta con un texto más largo o diferente.";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                // Validar que MateriaId no sea nulo
                if (!model.MateriaId.HasValue)
                {
                    TempData["Error"] = "La materia no fue especificada correctamente.";
                    return RedirectToAction("Index", "Materia");
                }

                // Redirigir a la vista de revisión y nombrar quiz
                _logger.LogInformation("Redirigiendo a ReviewAndName con {Count} preguntas", result.Questions.Count);

                // Crear el ViewModel para la vista de revisión
                var reviewModel = new ReviewAndNameQuizViewModel
                {
                    MateriaId = model.MateriaId.Value,
                    QuestionCount = result.Questions.Count,
                    PreviewQuestions = result.Questions.ToList(), // Mostrar TODAS las preguntas
                    OriginalSettings = settings,
                    GeneratedQuestions = System.Text.Json.JsonSerializer.Serialize(result.Questions),
                    QuizTitle = $"Quiz IA - {DateTime.Now:dd/MM/yyyy HH:mm}"
                };

                // Obtener nombre de la materia para mostrar en la vista
                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(model.MateriaId.Value);
                ViewBag.MateriaNombre = materia?.Nombre ?? "Materia";

                return View("ReviewAndName", reviewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz from text for user {UserId}", _userManager.GetUserId(User));
                TempData["Error"] = "Ocurrió un error al generar el quiz. Intente nuevamente.";
                return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
            }
        }

        // POST: Quiz/SaveGeneratedQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGeneratedQuiz(ReviewAndNameQuizViewModel model)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                _logger.LogInformation("Guardando quiz nombrado por usuario {UserId}", usuarioId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState no es válido para SaveGeneratedQuiz");
                    var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(model.MateriaId);
                    ViewBag.MateriaNombre = materia?.Nombre ?? "Materia";
                    return View("ReviewAndName", model);
                }

                // Deserializar las preguntas
                List<GeneratedQuizQuestion> questions;
                try
                {
                    questions = System.Text.Json.JsonSerializer.Deserialize<List<GeneratedQuizQuestion>>(model.GeneratedQuestions) ?? new();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializando preguntas generadas");
                    TempData["Error"] = "Error al procesar las preguntas generadas.";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                if (questions.Count == 0)
                {
                    _logger.LogWarning("No hay preguntas para guardar");
                    TempData["Error"] = "No hay preguntas para guardar.";
                    return RedirectToAction("ConfigureAI", new { materiaId = model.MateriaId });
                }

                // Crear el resultado para usar el método existente de guardado
                var result = new QuizGenerationResult
                {
                    Success = true,
                    Questions = questions,
                    ErrorMessage = string.Empty
                };

                // Crear settings básicas
                var settings = new QuizGenerationSettings
                {
                    NumberOfQuestions = questions.Count,
                    DifficultyLevel = NivelDificultad.Intermedio
                };

                // Guardar usando el método personalizado
                var quiz = await SaveQuizWithCustomTitle(result, model.MateriaId, settings, model.QuizTitle, model.QuizDescription, model.IsPublic);

                if (quiz != null)
                {
                    _logger.LogInformation("Quiz nombrado guardado exitosamente con ID: {QuizId}, Título: {Titulo}",
                        quiz.Id, quiz.Titulo);
                    TempData["Success"] = $"Quiz '{quiz.Titulo}' creado exitosamente con {quiz.Preguntas.Count} preguntas usando IA.";
                    return RedirectToAction("Details", new { id = quiz.Id });
                }
                else
                {
                    _logger.LogError("Error al guardar el quiz nombrado");
                    TempData["Error"] = "Error al guardar el quiz.";
                    var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(model.MateriaId);
                    ViewBag.MateriaNombre = materia?.Nombre ?? "Materia";
                    return View("ReviewAndName", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving named generated quiz for user {UserId}", _userManager.GetUserId(User));
                TempData["Error"] = "Ocurrió un error al guardar el quiz. Intente nuevamente.";
                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(model.MateriaId);
                ViewBag.MateriaNombre = materia?.Nombre ?? "Materia";
                return View("ReviewAndName", model);
            }
        }

        private QuestionType ParseToViewModelQuestionType(string questionType)
        {
            return questionType?.ToLower() switch
            {
                "multiplechoice" => QuestionType.MultipleChoice,
                "truefalse" => QuestionType.TrueFalse,
                "fillintheblank" => QuestionType.FillInTheBlank,
                "shortanswer" => QuestionType.ShortAnswer,
                _ => QuestionType.MultipleChoice
            };
        }

        // POST: Quiz/SaveGenerated
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGenerated(ReviewGeneratedQuizViewModel model)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                var materiaId = TempData["MateriaId"] as int?;

                if (!materiaId.HasValue)
                {
                    TempData["Error"] = "Sesión expirada. Intente generar el quiz nuevamente.";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener la materia
                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaId.Value);
                if (materia == null || materia.UsuarioId != usuarioId)
                {
                    TempData["Error"] = "Materia no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Crear el quiz
                var quiz = new Quiz
                {
                    Titulo = model.QuizTitle,
                    Descripcion = model.QuizDescription ?? $"Quiz generado automáticamente con IA el {DateTime.Now:dd/MM/yyyy}",
                    MateriaId = materiaId.Value,
                    FechaCreacion = DateTime.UtcNow,
                    EsPublico = false,
                    NivelDificultad = (int)NivelDificultad.Intermedio,
                    TiempoLimite = model.TimeLimit ?? 30,
                    Preguntas = new List<PreguntaQuiz>()
                };

                // Convertir preguntas aprobadas a entidades
                var approvedQuestions = model.Questions.Where(q => q.IsApproved).ToList();
                foreach (var question in approvedQuestions)
                {
                    var pregunta = new PreguntaQuiz
                    {
                        TextoPregunta = question.QuestionText,
                        TipoPregunta = (int)Core.Enums.TipoActividad.Quiz,
                        Puntos = question.Points,
                        Orden = quiz.Preguntas.Count + 1,
                        Explicacion = question.Explanation,
                        // Note: OpcionesRespuesta might need different handling depending on your entity structure
                    };

                    quiz.Preguntas.Add(pregunta);
                }

                // Guardar en base de datos
                await _unitOfWork.QuizRepository.AddAsync(quiz);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"Quiz '{quiz.Titulo}' creado exitosamente con {quiz.Preguntas.Count} preguntas.";
                return RedirectToAction("Details", new { id = quiz.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving generated quiz for user {UserId}", _userManager.GetUserId(User));
                TempData["Error"] = "Ocurrió un error al guardar el quiz. Intente nuevamente.";
                return View("ReviewGenerated", model);
            }
        }

        private async Task<Quiz?> SaveQuizWithCustomTitle(QuizGenerationResult result, int materiaId, QuizGenerationSettings settings, string title, string? description, bool isPublic)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                _logger.LogInformation("Guardando quiz con título personalizado para usuario {UserId}, materia {MateriaId}, preguntas {Count}",
                    usuarioId, materiaId, result.Questions?.Count ?? 0);

                // Validar que hay preguntas
                if (result.Questions == null || result.Questions.Count == 0)
                {
                    _logger.LogWarning("No se puede guardar quiz: no hay preguntas generadas");
                    return null;
                }

                // Obtener la materia
                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaId);
                if (materia == null || materia.UsuarioId != usuarioId)
                {
                    _logger.LogWarning("No se puede guardar quiz: materia no encontrada o sin permisos. MateriaId: {MateriaId}", materiaId);
                    return null;
                }

                // Crear el quiz con título personalizado
                var quiz = new Quiz
                {
                    Titulo = title,
                    Descripcion = description ?? $"Quiz generado automáticamente con IA el {DateTime.Now:dd/MM/yyyy}",
                    MateriaId = materiaId,
                    CreadorId = usuarioId,
                    FechaCreacion = DateTime.UtcNow,
                    EsPublico = isPublic,
                    NumeroPreguntas = result.Questions.Count,
                    Preguntas = new List<PreguntaQuiz>()
                };

                // Agregar preguntas generadas
                _logger.LogInformation("Agregando {Count} preguntas al quiz personalizado", result.Questions.Count);
                int orden = 1;
                foreach (var generatedQuestion in result.Questions)
                {
                    _logger.LogInformation("Procesando pregunta {Orden}: {Texto}", orden, generatedQuestion.QuestionText);

                    var pregunta = new PreguntaQuiz
                    {
                        TextoPregunta = generatedQuestion.QuestionText,
                        TipoPregunta = (int)Core.Enums.TipoActividad.Quiz,
                        Puntos = generatedQuestion.Points,
                        Explicacion = generatedQuestion.Explanation,
                        Orden = orden,
                        RespuestaCorrecta = "" // Lo asignaremos como letra (A, B, C, D)
                    };

                    // Mapear opciones de respuesta
                    if (generatedQuestion.AnswerOptions != null && generatedQuestion.AnswerOptions.Any())
                    {
                        var opciones = generatedQuestion.AnswerOptions.Take(4).ToList();

                        pregunta.OpcionA = opciones.Count > 0 ? opciones[0].Text : "";
                        pregunta.OpcionB = opciones.Count > 1 ? opciones[1].Text : "";
                        pregunta.OpcionC = opciones.Count > 2 ? opciones[2].Text : "";
                        pregunta.OpcionD = opciones.Count > 3 ? opciones[3].Text : "";

                        // Encontrar la respuesta correcta
                        var correctIndex = -1;
                        for (int i = 0; i < opciones.Count; i++)
                        {
                            if (opciones[i].IsCorrect)
                            {
                                correctIndex = i;
                                break;
                            }
                        }

                        if (correctIndex == -1)
                        {
                            _logger.LogWarning("No se encontró opción marcada como correcta, usando la primera");
                            correctIndex = 0;
                        }

                        // Asignar la letra correspondiente
                        char[] letras = { 'A', 'B', 'C', 'D' };
                        pregunta.RespuestaCorrecta = letras[correctIndex].ToString();

                        _logger.LogInformation("Respuesta correcta asignada: {RespuestaCorrecta}", pregunta.RespuestaCorrecta);
                    }
                    else
                    {
                        pregunta.OpcionA = "";
                        pregunta.OpcionB = "";
                        pregunta.OpcionC = "";
                        pregunta.OpcionD = "";
                        pregunta.RespuestaCorrecta = "A";
                    }

                    quiz.Preguntas.Add(pregunta);
                    orden++;
                }

                // Guardar en base de datos
                _logger.LogInformation("Agregando quiz personalizado a la base de datos: {QuizTitulo}", quiz.Titulo);
                await _unitOfWork.QuizRepository.AddAsync(quiz);

                _logger.LogInformation("Guardando cambios en la base de datos...");
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Quiz personalizado guardado exitosamente con ID: {QuizId}", quiz.Id);
                return quiz;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving generated quiz with custom title");
                return null;
            }
        }

        private async Task<Quiz?> SaveQuizDirectly(QuizGenerationResult result, int materiaId, QuizGenerationSettings settings)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                _logger.LogInformation("Guardando quiz directamente para usuario {UserId}, materia {MateriaId}, preguntas {Count}",
                    usuarioId, materiaId, result.Questions?.Count ?? 0);

                // Validar que hay preguntas
                if (result.Questions == null || result.Questions.Count == 0)
                {
                    _logger.LogWarning("No se puede guardar quiz: no hay preguntas generadas");
                    return null;
                }

                // Obtener la materia
                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaId);
                if (materia == null || materia.UsuarioId != usuarioId)
                {
                    _logger.LogWarning("No se puede guardar quiz: materia no encontrada o sin permisos. MateriaId: {MateriaId}", materiaId);
                    return null;
                }

                // Crear el quiz
                var quiz = new Quiz
                {
                    Titulo = $"Quiz IA - {DateTime.Now:dd/MM/yyyy HH:mm}",
                    Descripcion = $"Quiz generado automáticamente con IA el {DateTime.Now:dd/MM/yyyy}",
                    MateriaId = materiaId,
                    CreadorId = usuarioId,
                    FechaCreacion = DateTime.UtcNow,
                    EsPublico = false,
                    NumeroPreguntas = result.Questions.Count,
                    Preguntas = new List<PreguntaQuiz>()
                };

                // Agregar preguntas generadas
                _logger.LogInformation("Agregando {Count} preguntas al quiz", result.Questions.Count);
                int orden = 1;
                foreach (var generatedQuestion in result.Questions)
                {
                    _logger.LogInformation("Procesando pregunta {Orden}: {Texto}", orden, generatedQuestion.QuestionText);
                    _logger.LogInformation("AnswerOptions count: {Count}", generatedQuestion.AnswerOptions?.Count ?? 0);

                    var pregunta = new PreguntaQuiz
                    {
                        TextoPregunta = generatedQuestion.QuestionText,
                        TipoPregunta = (int)Core.Enums.TipoActividad.Quiz,
                        Puntos = generatedQuestion.Points,
                        Explicacion = generatedQuestion.Explanation,
                        Orden = orden,
                        RespuestaCorrecta = "" // Lo asignaremos como letra (A, B, C, D)
                    };

                    // Mapear opciones de respuesta a las columnas de la base de datos
                    if (generatedQuestion.AnswerOptions != null && generatedQuestion.AnswerOptions.Any())
                    {
                        _logger.LogInformation("Tiene opciones de respuesta, mapeando...");
                        var opciones = generatedQuestion.AnswerOptions.Take(4).ToList();
                        _logger.LogInformation("Opciones disponibles: {Count}", opciones.Count);

                        pregunta.OpcionA = opciones.Count > 0 ? opciones[0].Text : "";
                        pregunta.OpcionB = opciones.Count > 1 ? opciones[1].Text : "";
                        pregunta.OpcionC = opciones.Count > 2 ? opciones[2].Text : "";
                        pregunta.OpcionD = opciones.Count > 3 ? opciones[3].Text : "";

                        _logger.LogInformation("Opciones asignadas - A: {A}, B: {B}, C: {C}, D: {D}",
                            pregunta.OpcionA, pregunta.OpcionB, pregunta.OpcionC, pregunta.OpcionD);

                        // Encontrar qué opción es la correcta y asignar la letra correspondiente
                        var correctIndex = -1;
                        for (int i = 0; i < opciones.Count; i++)
                        {
                            if (opciones[i].IsCorrect)
                            {
                                correctIndex = i;
                                break;
                            }
                        }

                        // Si no se encontró por IsCorrect, usar la primera opción por defecto
                        if (correctIndex == -1)
                        {
                            _logger.LogWarning("No se encontró opción marcada como correcta, usando la primera");
                            correctIndex = 0;
                        }

                        // Asignar la letra correspondiente (A, B, C, D)
                        char[] letras = { 'A', 'B', 'C', 'D' };
                        pregunta.RespuestaCorrecta = letras[correctIndex].ToString();

                        _logger.LogInformation("Respuesta correcta asignada: {RespuestaCorrecta} (opción {Index}: {Texto})",
                            pregunta.RespuestaCorrecta, correctIndex + 1, opciones[correctIndex].Text);
                    }
                    else
                    {
                        _logger.LogWarning("No hay opciones de respuesta, asignando valores vacíos");
                        // Si no hay opciones, asignar valores vacíos para evitar NULL
                        pregunta.OpcionA = "";
                        pregunta.OpcionB = "";
                        pregunta.OpcionC = "";
                        pregunta.OpcionD = "";
                    }

                    quiz.Preguntas.Add(pregunta);
                    _logger.LogDebug("Agregada pregunta {Orden}: {Texto} con opciones: A={OpcionA}, B={OpcionB}, C={OpcionC}, D={OpcionD}",
                        orden, generatedQuestion.QuestionText, pregunta.OpcionA, pregunta.OpcionB, pregunta.OpcionC, pregunta.OpcionD);
                    orden++;
                }

                // Guardar en base de datos
                _logger.LogInformation("Agregando quiz a la base de datos: {QuizTitulo}", quiz.Titulo);
                await _unitOfWork.QuizRepository.AddAsync(quiz);

                _logger.LogInformation("Guardando cambios en la base de datos...");
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Quiz guardado exitosamente con ID: {QuizId}", quiz.Id);
                return quiz;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving generated quiz directly");
                return null;
            }
        }

        // GET: Quiz/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User);
                var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(id);

                if (quiz == null)
                {
                    TempData["Error"] = "Quiz no encontrado.";
                    return RedirectToAction("Index");
                }

                // Verificar que el usuario sea el creador del quiz
                if (quiz.CreadorId != usuarioId)
                {
                    TempData["Error"] = "No tienes permisos para editar este quiz.";
                    return RedirectToAction("Details", new { id = id });
                }

                var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);

                var model = new QuizEditViewModel
                {
                    Id = quiz.Id,
                    Titulo = quiz.Titulo,
                    Descripcion = quiz.Descripcion,
                    MateriaId = quiz.MateriaId,
                    TiempoLimite = quiz.TiempoLimite,
                    NivelDificultad = (NivelDificultad)quiz.NivelDificultad,
                    EsPublico = quiz.EsPublico,
                    MostrarRespuestasInmediato = quiz.MostrarRespuestasInmediato,
                    PermitirReintento = quiz.PermitirReintento
                };

                ViewBag.Materias = materias.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nombre,
                    Selected = m.Id == quiz.MateriaId
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quiz for edit. QuizId: {QuizId}", id);
                TempData["Error"] = "Error al cargar el quiz para edición.";
                return RedirectToAction("Index");
            }
        }

        // POST: Quiz/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuizEditViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["Error"] = "Datos inconsistentes.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    var usuarioId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(usuarioId))
                        return RedirectToAction("Login", "Account");

                    var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuarioId);
                    ViewBag.Materias = materias.Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Nombre,
                        Selected = m.Id == model.MateriaId
                    }).ToList();
                    return View(model);
                }

                var usuarioActual = _userManager.GetUserId(User);
                var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(id);

                if (quiz == null)
                {
                    TempData["Error"] = "Quiz no encontrado.";
                    return RedirectToAction("Index");
                }

                if (quiz.CreadorId != usuarioActual)
                {
                    TempData["Error"] = "No tienes permisos para editar este quiz.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Actualizar propiedades
                quiz.Titulo = model.Titulo;
                quiz.Descripcion = model.Descripcion;
                quiz.MateriaId = model.MateriaId;
                quiz.TiempoLimite = model.TiempoLimite;
                quiz.NivelDificultad = (int)model.NivelDificultad;
                quiz.EsPublico = model.EsPublico;
                quiz.MostrarRespuestasInmediato = model.MostrarRespuestasInmediato;
                quiz.PermitirReintento = model.PermitirReintento;
                quiz.FechaModificacion = DateTime.UtcNow;

                // El repositorio genérico actualizará automáticamente al hacer SaveChanges
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"Quiz '{quiz.Titulo}' actualizado exitosamente.";
                return RedirectToAction("Details", new { id = quiz.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz. QuizId: {QuizId}", id);
                TempData["Error"] = "Error al actualizar el quiz.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        #endregion
        #region Generación con IA

        // GET: Quiz/GenerateWithAI
        [HttpGet]
        public async Task<IActionResult> GenerateWithAI(int? materiaId)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToAction("Login", "Account");

            var materias = await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id);

            var viewModel = new GenerateQuizWithAIViewModel
            {
                MateriaId = materiaId,
                Materias = materias.ToList()
            };

            return View(viewModel);
        }

        // POST: Quiz/GenerateWithAI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateWithAI(GenerateQuizWithAIViewModel model)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.Materias = (await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id)).ToList();
                return View(model);
            }

            try
            {
                // Verificar que la materia existe y pertenece al usuario
                var materia = await _unitOfWork.MateriaRepository.GetByIdAsync(model.MateriaId!.Value);
                if (materia == null || materia.UsuarioId != usuario.Id)
                {
                    TempData["Error"] = "Materia no encontrada o no tiene permiso para acceder.";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener contenido (texto o PDF)
                string contenido = model.Contenido ?? string.Empty;

                // TODO: Si hay archivo PDF, extraer texto del PDF
                if (model.ArchivoPDF != null && model.ArchivoPDF.Length > 0)
                {
                    _logger.LogWarning("PDF upload not implemented yet. Using text content instead.");
                    // Por ahora, usar el contenido de texto si existe
                }

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    TempData["Error"] = "Debe proporcionar contenido de texto o un archivo PDF.";
                    model.Materias = (await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id)).ToList();
                    return View(model);
                }

                // Configurar parámetros de generación
                var settings = new QuizGenerationSettings
                {
                    NumberOfQuestions = model.CantidadPreguntas ?? 5,
                    DifficultyLevel = model.NivelDificultad,
                    IncludeExplanations = true
                };

                // Llamar al servicio de IA
                var response = await _aiService.GenerateQuizFromTextAsync(contenido, settings);

                if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
                {
                    var errorMessage = response.ErrorMessage ?? "Servicio IA no disponible";
                    _logger.LogWarning("AI service returned error: {Error}", errorMessage);
                    
                    // Mostrar el error al usuario en lugar de crear quiz de ejemplo
                    TempData["Error"] = $"Error al generar quiz con IA:\n\n{errorMessage}";
                    model.Materias = (await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id)).ToList();
                    return View(model);
                }

                // Parsear respuesta JSON
                var quiz = new Quiz
                {
                    Titulo = model.Titulo ?? $"Quiz generado por IA - {materia.Nombre}",
                    Descripcion = "Generado automáticamente con IA",
                    MateriaId = materia.Id,
                    CreadorId = usuario.Id,
                    NivelDificultad = (int)model.NivelDificultad,
                    EsPublico = false,
                    FechaCreacion = DateTime.UtcNow,
                    EstaActivo = true,
                    TiempoLimite = (model.CantidadPreguntas ?? 5) * 2, // 2 minutos por pregunta
                    NumeroPreguntas = model.CantidadPreguntas ?? 5
                };

                try
                {
                    // Intentar parsear como JSON
                    using var doc = JsonDocument.Parse(response.Content);
                    var root = doc.RootElement;

                    // Intentar obtener array de preguntas
                    JsonElement preguntasElement;
                    if (root.TryGetProperty("preguntas", out preguntasElement) ||
                        root.TryGetProperty("questions", out preguntasElement) ||
                        root.TryGetProperty("quiz", out preguntasElement))
                    {
                        // Tiene property wrapper
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        preguntasElement = root;
                    }
                    else
                    {
                        throw new JsonException("No se encontró array de preguntas en la respuesta");
                    }

                    var preguntas = new List<PreguntaQuiz>();

                    foreach (var item in preguntasElement.EnumerateArray())
                    {
                        var pregunta = GetJsonStringProperty(item, "pregunta", "question", "Pregunta", "Question");
                        var opcionA = GetJsonStringProperty(item, "opcionA", "optionA", "opciona", "a", "A");
                        var opcionB = GetJsonStringProperty(item, "opcionB", "optionB", "opcionb", "b", "B");
                        var opcionC = GetJsonStringProperty(item, "opcionC", "optionC", "opcionc", "c", "C");
                        var opcionD = GetJsonStringProperty(item, "opcionD", "optionD", "opciond", "d", "D");
                        var respuestaCorrecta = GetJsonStringProperty(item, "respuestaCorrecta", "correctAnswer", "respuesta", "answer");
                        var explicacion = GetJsonStringProperty(item, "explicacion", "explanation", "Explicacion");

                        if (!string.IsNullOrWhiteSpace(pregunta) && !string.IsNullOrWhiteSpace(respuestaCorrecta))
                        {
                            var nuevaPregunta = new PreguntaQuiz
                            {
                                TextoPregunta = pregunta,
                                OpcionA = opcionA ?? "Opción A",
                                OpcionB = opcionB ?? "Opción B",
                                OpcionC = opcionC ?? "Opción C",
                                OpcionD = opcionD ?? "Opción D",
                                RespuestaCorrecta = respuestaCorrecta,
                                Explicacion = explicacion,
                                Puntos = 1,
                                Orden = preguntas.Count + 1,
                                EstaActivo = true
                            };

                            // Aleatorizar las opciones para que la respuesta correcta no siempre esté en la posición A
                            AleatorizarOpciones(nuevaPregunta);

                            preguntas.Add(nuevaPregunta);
                        }
                    }

                    quiz.Preguntas = preguntas;

                    if (preguntas.Count == 0)
                    {
                        throw new InvalidOperationException("No se generaron preguntas válidas");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse AI response as JSON. Creating example quiz.");
                    var quizEjemplo = await CrearQuizDeEjemplo(contenido, materia, model, usuario.Id);
                    TempData["Warning"] = $"Se creó un quiz básico con {quizEjemplo.Item2} preguntas (formato IA no válido).";
                    return RedirectToAction("Details", new { id = quizEjemplo.Item1.Id });
                }

                // Guardar quiz
                await _unitOfWork.QuizRepository.AddAsync(quiz);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"Quiz generado exitosamente con {quiz.Preguntas.Count} preguntas.";
                return RedirectToAction("Details", new { id = quiz.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz with AI");
                TempData["Error"] = "Ocurrió un error al generar el quiz.";
                model.Materias = (await _unitOfWork.MateriaRepository.GetMateriasByUsuarioIdAsync(usuario.Id)).ToList();
                return View(model);
            }
        }

        private string? GetJsonStringProperty(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                {
                    return prop.GetString();
                }
            }
            return null;
        }

        /// <summary>
        /// Aleatoriza las opciones de una pregunta para que la respuesta correcta no siempre esté en la posición A
        /// </summary>
        private void AleatorizarOpciones(PreguntaQuiz pregunta)
        {
            // Crear lista de opciones con sus valores originales
            var opciones = new List<(string letra, string texto)>
            {
                ("A", pregunta.OpcionA ?? "Opción A"),
                ("B", pregunta.OpcionB ?? "Opción B"),
                ("C", pregunta.OpcionC ?? "Opción C"),
                ("D", pregunta.OpcionD ?? "Opción D")
            };

            // Encontrar cuál es la respuesta correcta original
            string respuestaCorrectaOriginal = pregunta.RespuestaCorrecta.Trim().ToUpper();
            
            // Si la respuesta correcta es una letra (A, B, C, D), obtener el texto correspondiente
            string textoRespuestaCorrecta = respuestaCorrectaOriginal;
            if (respuestaCorrectaOriginal.Length == 1 && "ABCD".Contains(respuestaCorrectaOriginal))
            {
                textoRespuestaCorrecta = opciones.FirstOrDefault(o => o.letra == respuestaCorrectaOriginal).texto;
            }
            else
            {
                // La respuesta correcta es el texto completo, buscar en qué opción está
                var opcionCorrecta = opciones.FirstOrDefault(o => 
                    o.texto.Trim().Equals(respuestaCorrectaOriginal, StringComparison.OrdinalIgnoreCase));
                if (opcionCorrecta != default)
                {
                    textoRespuestaCorrecta = opcionCorrecta.texto;
                }
            }

            // Mezclar las opciones aleatoriamente usando Random
            var random = new Random();
            var opcionesAleatorias = opciones.OrderBy(x => random.Next()).ToList();

            // Reasignar las opciones en el nuevo orden
            pregunta.OpcionA = opcionesAleatorias[0].texto;
            pregunta.OpcionB = opcionesAleatorias[1].texto;
            pregunta.OpcionC = opcionesAleatorias[2].texto;
            pregunta.OpcionD = opcionesAleatorias[3].texto;

            // Encontrar en qué posición quedó la respuesta correcta y actualizar
            for (int i = 0; i < opcionesAleatorias.Count; i++)
            {
                if (opcionesAleatorias[i].texto.Trim().Equals(textoRespuestaCorrecta.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // Actualizar la respuesta correcta a la nueva letra
                    pregunta.RespuestaCorrecta = new[] { "A", "B", "C", "D" }[i];
                    break;
                }
            }
        }

        private async Task<(Quiz, int)> CrearQuizDeEjemplo(string contenido, Materia materia, GenerateQuizWithAIViewModel model, string usuarioId)
        {
            var lineas = contenido.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(l => !string.IsNullOrWhiteSpace(l))
                                 .Take(model.CantidadPreguntas ?? 5)
                                 .ToList();

            var quiz = new Quiz
            {
                Titulo = model.Titulo ?? $"Quiz - {materia.Nombre}",
                Descripcion = "Quiz generado desde texto",
                MateriaId = materia.Id,
                CreadorId = usuarioId,
                NivelDificultad = (int)model.NivelDificultad,
                EsPublico = false,
                FechaCreacion = DateTime.UtcNow,
                EstaActivo = true,
                TiempoLimite = lineas.Count * 2,
                NumeroPreguntas = lineas.Count,
                Preguntas = new List<PreguntaQuiz>()
            };

            for (int i = 0; i < lineas.Count; i++)
            {
                var pregunta = new PreguntaQuiz
                {
                    TextoPregunta = $"¿Qué se menciona sobre: {lineas[i].Substring(0, Math.Min(50, lineas[i].Length))}...?",
                    OpcionA = lineas[i].Length > 20 ? lineas[i].Substring(0, 20) : lineas[i],
                    OpcionB = "Respuesta incorrecta B",
                    OpcionC = "Respuesta incorrecta C",
                    OpcionD = "Respuesta incorrecta D",
                    RespuestaCorrecta = lineas[i].Length > 20 ? lineas[i].Substring(0, 20) : lineas[i],
                    Puntos = 1,
                    Orden = i + 1,
                    EstaActivo = true
                };
                quiz.Preguntas.Add(pregunta);
            }

            await _unitOfWork.QuizRepository.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return (quiz, quiz.Preguntas.Count);
        }

        #endregion



        // Método auxiliar mejorado para generar opciones incorrectas más convincentes usando IA
        private async Task<string> GenerarOpcionIncorrectaConIA(string pregunta, string respuestaCorrecta, string materiaNombre)
        {
            try
            {
                // Intentar usar Gemini para generar un distractor convincente
                var prompt = $@"Genera UNA ÚNICA opción incorrecta pero plausible para esta pregunta de {materiaNombre}:

Pregunta: {pregunta}
Respuesta correcta: {respuestaCorrecta}

IMPORTANTE - La opción incorrecta debe:
- Ser ESPECÍFICA del tema de {materiaNombre}
- Ser relacionada con el concepto pero incorrecta
- Ser convincente y parecer correcta a primera vista
- NO usar frases genéricas como 'Depende del contexto', 'Ninguna de las anteriores', 'Es relativo', etc.
- NO usar frases como 'Opción incorrecta' o 'Respuesta falsa'
- Ser similar en longitud y complejidad a la respuesta correcta
- Debe ser un concepto, término, definición o dato específico pero incorrecto

Ejemplos de BUENAS opciones incorrectas:
- Si la pregunta es sobre fechas: usar fechas cercanas pero incorrectas
- Si es sobre definiciones: usar definiciones de conceptos similares
- Si es sobre personas: usar nombres de otras personas del mismo campo
- Si es sobre lugares: usar nombres de otros lugares relacionados

Responde SOLO con la opción incorrecta específica, sin explicaciones ni formato adicional.";

                var settings = new AISettings
                {
                    MaxTokens = 150,
                    Temperature = 0.8f
                };

                var response = await _aiService.GenerateTextAsync(prompt, settings);

                if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
                {
                    // Limpiar la respuesta de posibles caracteres extra
                    var distractor = response.Content.Trim()
                        .Replace("\"", "")
                        .Replace("```", "")
                        .Replace("json", "")
                        .Trim();
                    
                    // Validar que no sea una respuesta genérica
                    var respuestasProhibidas = new[] 
                    { 
                        "depende del contexto", 
                        "ninguna de las anteriores", 
                        "todas las anteriores",
                        "es un concepto relativo",
                        "no se puede determinar",
                        "respuesta alternativa",
                        "requiere información",
                        "no aplica"
                    };
                    
                    var esGenerica = respuestasProhibidas.Any(prohibida => 
                        distractor.ToLower().Contains(prohibida));
                    
                    if (!esGenerica)
                    {
                        return distractor;
                    }
                    
                    _logger.LogWarning("IA generó una respuesta genérica, reintentando con prompt más estricto");
                }
                
                // Reintentar con un prompt más estricto
                var promptEstricto = $@"Genera UNA opción incorrecta ESPECÍFICA sobre {materiaNombre} para:
Pregunta: {pregunta}
Respuesta correcta: {respuestaCorrecta}

DEBE ser un término, concepto o dato CONCRETO relacionado con {materiaNombre}.
NO uses frases genéricas.
Solo responde con el dato incorrecto específico.";
                
                var retryResponse = await _aiService.GenerateTextAsync(promptEstricto, new AISettings { MaxTokens = 100, Temperature = 0.9f });
                
                if (retryResponse.Success && !string.IsNullOrWhiteSpace(retryResponse.Content))
                {
                    var distractor = retryResponse.Content.Trim().Replace("\"", "").Replace("```", "").Trim();
                    
                    // Si aún es genérica, usar una variación de la respuesta correcta
                    var respuestasProhibidas = new[] { "depende", "ninguna", "todas", "relativo", "determinar", "alternativa", "requiere", "aplica" };
                    var esGenerica = respuestasProhibidas.Any(prohibida => distractor.ToLower().Contains(prohibida));
                    
                    if (!esGenerica)
                    {
                        return distractor;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generando distractor con IA");
            }

            // Último recurso: modificar la respuesta correcta para crear un distractor
            if (!string.IsNullOrEmpty(respuestaCorrecta))
            {
                // Intentar crear una variación de la respuesta correcta
                if (respuestaCorrecta.Contains(" "))
                {
                    var palabras = respuestaCorrecta.Split(' ');
                    if (palabras.Length > 2)
                    {
                        // Mezclar las palabras para crear algo incorrecto pero similar
                        return string.Join(" ", palabras.Take(palabras.Length - 1)) + " (variante incorrecta)";
                    }
                }
                return $"Variante incorrecta de {respuestaCorrecta.Split(' ').First()}";
            }
            
            return $"Concepto incorrecto relacionado con {materiaNombre}";
        }

        // Método para generar distractores inteligentes usando respuestas de otras flashcards
        private async Task<string> GenerarDistractorInteligente(Flashcard flashcardActual, List<Flashcard> todasFlashcards, string materiaNombre)
        {
            var respuestaCorrecta = flashcardActual.Respuesta;
            var longitudReferencia = respuestaCorrecta?.Length ?? 100;
            
            // Lista de respuestas ya usadas para evitar duplicados
            var respuestasDisponibles = todasFlashcards
                .Where(f => f.Id != flashcardActual.Id && !string.IsNullOrEmpty(f.Respuesta))
                .ToList();
            
            if (respuestasDisponibles.Any())
            {
                // Intentar primero con respuestas de longitud similar (±50%)
                var longitudMin = (int)(longitudReferencia * 0.5);
                var longitudMax = (int)(longitudReferencia * 1.5);
                
                var respuestasSimilares = respuestasDisponibles
                    .Where(f => f.Respuesta.Length >= longitudMin && f.Respuesta.Length <= longitudMax)
                    .Select(f => f.Respuesta)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault();
                
                if (respuestasSimilares != null)
                {
                    return respuestasSimilares;
                }
                
                // Si no hay respuestas similares, intentar con cualquier respuesta ajustando la longitud
                var cualquierRespuesta = respuestasDisponibles
                    .Select(f => f.Respuesta)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault();
                
                if (cualquierRespuesta != null)
                {
                    // Ajustar longitud si es necesario
                    if (cualquierRespuesta.Length > longitudMax)
                    {
                        // Truncar manteniendo palabras completas
                        var palabras = cualquierRespuesta.Split(' ');
                        var resultado = "";
                        foreach (var palabra in palabras)
                        {
                            if ((resultado + " " + palabra).Length <= longitudMax - 3)
                            {
                                resultado += (resultado.Length > 0 ? " " : "") + palabra;
                            }
                            else
                            {
                                break;
                            }
                        }
                        return resultado + "...";
                    }
                    
                    return cualquierRespuesta;
                }
            }
            
            // Si no hay otras flashcards disponibles, SIEMPRE usar IA para generar un distractor específico
            _logger.LogInformation("No hay otras flashcards disponibles, generando distractor con IA para: {Pregunta}", flashcardActual.Pregunta);
            return await GenerarOpcionIncorrectaConIA(flashcardActual.Pregunta, flashcardActual.Respuesta, materiaNombre);
        }
    }

    // Clase helper para respuestas temporales de quiz
    public class RespuestaQuizTemp
    {
        public int PreguntaId { get; set; }
        public int OpcionSeleccionadaId { get; set; }
        public string LetraSeleccionada { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; }
    }
}





