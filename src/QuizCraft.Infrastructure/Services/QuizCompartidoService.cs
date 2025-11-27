using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using System.Security.Cryptography;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para compartir e importar quizzes
/// </summary>
public class QuizCompartidoService : IQuizCompartidoService
{
    private readonly IQuizCompartidoRepository _quizCompartidoRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IGenericRepository<QuizImportado> _quizImportadoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuizCompartidoService> _logger;

    public QuizCompartidoService(
        IQuizCompartidoRepository quizCompartidoRepository,
        IQuizRepository quizRepository,
        IGenericRepository<QuizImportado> quizImportadoRepository,
        IUnitOfWork unitOfWork,
        ILogger<QuizCompartidoService> logger)
    {
        _quizCompartidoRepository = quizCompartidoRepository;
        _quizRepository = quizRepository;
        _quizImportadoRepository = quizImportadoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<string>> CompartirQuizAsync(
        int quizId, 
        string usuarioId, 
        CompartirQuizOptions opciones)
    {
        try
        {
            _logger.LogInformation("Iniciando compartición de quiz {QuizId} por usuario {UsuarioId}", quizId, usuarioId);
            
            // Verificar que el quiz existe y pertenece al usuario (cargar con preguntas)
            var quiz = await _quizRepository.GetQuizConPreguntasAsync(quizId);
            if (quiz == null)
            {
                _logger.LogWarning("Quiz {QuizId} no encontrado", quizId);
                return ServiceResult<string>.Failure("El quiz no existe");
            }

            if (quiz.CreadorId != usuarioId)
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para compartir quiz {QuizId}", usuarioId, quizId);
                return ServiceResult<string>.Failure("No tienes permisos para compartir este quiz");
            }

            if (quiz.Preguntas == null || !quiz.Preguntas.Any())
            {
                _logger.LogWarning("Quiz {QuizId} no tiene preguntas", quizId);
                return ServiceResult<string>.Failure("No puedes compartir un quiz sin preguntas");
            }

            _logger.LogInformation("Quiz {QuizId} tiene {CantidadPreguntas} preguntas", quizId, quiz.Preguntas.Count);
            
            // Generar código único
            var codigo = await GenerarCodigoUnicoAsync();
            _logger.LogInformation("Código generado: {Codigo}", codigo);

            // Crear registro de compartición
            var quizCompartido = new QuizCompartido
            {
                QuizId = quizId,
                PropietarioId = usuarioId,
                CodigoCompartido = codigo,
                FechaExpiracion = opciones.FechaExpiracion,
                MaximoUsos = opciones.MaximoUsos,
                PermiteModificaciones = opciones.PermiteModificaciones
            };

            await _quizCompartidoRepository.AddAsync(quizCompartido);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Quiz {QuizId} compartido exitosamente por usuario {UsuarioId} con código {Codigo}",
                quizId, usuarioId, codigo);

            return ServiceResult<string>.Success(codigo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al compartir quiz {QuizId}. Detalle: {Message}", quizId, ex.Message);
            return ServiceResult<string>.Failure($"Error al generar el enlace de compartición: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> ImportarQuizAsync(
        string codigo, 
        string usuarioId, 
        int materiaDestinoId)
    {
        try
        {
            // Obtener quiz compartido
            var quizCompartido = await _quizCompartidoRepository.GetByCodigoAsync(codigo);
            
            if (quizCompartido == null || !quizCompartido.EstaActivo)
            {
                return ServiceResult<int>.Failure("El enlace de compartición no es válido o ha expirado");
            }

            // Verificar expiración
            if (quizCompartido.FechaExpiracion.HasValue && 
                quizCompartido.FechaExpiracion.Value < DateTime.UtcNow)
            {
                return ServiceResult<int>.Failure("El enlace de compartición ha expirado");
            }

            // Verificar límite de usos
            if (quizCompartido.MaximoUsos.HasValue && 
                quizCompartido.VecesUsado >= quizCompartido.MaximoUsos.Value)
            {
                return ServiceResult<int>.Failure("El enlace ha alcanzado el límite de usos");
            }

            // Verificar si ya importó este quiz
            if (await _quizCompartidoRepository.UsuarioYaImportoAsync(quizCompartido.Id, usuarioId))
            {
                return ServiceResult<int>.Failure("Ya has importado este quiz anteriormente");
            }

            // Evitar que el propietario importe su propio quiz
            if (quizCompartido.PropietarioId == usuarioId)
            {
                return ServiceResult<int>.Failure("No puedes importar tu propio quiz");
            }

            // Verificar que la materia destino pertenece al usuario
            var materiaDestino = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaDestinoId);
            if (materiaDestino == null || materiaDestino.UsuarioId != usuarioId)
            {
                return ServiceResult<int>.Failure("La materia de destino no es válida");
            }

            // Clonar el quiz
            var quizOriginal = quizCompartido.Quiz;
            var nuevoQuiz = new Quiz
            {
                Titulo = $"{quizOriginal.Titulo} (Importado)",
                Descripcion = quizOriginal.Descripcion,
                MateriaId = materiaDestinoId,
                CreadorId = usuarioId,
                TiempoLimite = quizOriginal.TiempoLimite,
                TiempoPorPregunta = quizOriginal.TiempoPorPregunta,
                NivelDificultad = quizOriginal.NivelDificultad,
                EsPublico = false,
                EsActivo = true,
                MostrarRespuestasInmediato = quizOriginal.MostrarRespuestasInmediato,
                PermitirReintento = quizOriginal.PermitirReintento
            };

            // Clonar preguntas
            foreach (var preguntaOriginal in quizOriginal.Preguntas)
            {
                var nuevaPregunta = new PreguntaQuiz
                {
                    TextoPregunta = preguntaOriginal.TextoPregunta,
                    TipoPregunta = preguntaOriginal.TipoPregunta,
                    Puntos = preguntaOriginal.Puntos,
                    Orden = preguntaOriginal.Orden,
                    OpcionA = preguntaOriginal.OpcionA,
                    OpcionB = preguntaOriginal.OpcionB,
                    OpcionC = preguntaOriginal.OpcionC,
                    OpcionD = preguntaOriginal.OpcionD,
                    RespuestaCorrecta = preguntaOriginal.RespuestaCorrecta,
                    Explicacion = preguntaOriginal.Explicacion
                };
                
                nuevoQuiz.Preguntas.Add(nuevaPregunta);
            }

            // Actualizar número de preguntas
            nuevoQuiz.NumeroPreguntas = nuevoQuiz.Preguntas.Count;

            // Guardar nuevo quiz primero para obtener su ID
            await _quizRepository.AddAsync(nuevoQuiz);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Nuevo quiz creado con ID: {QuizId}", nuevoQuiz.Id);

            // Ahora registrar importación con el ID real del quiz
            var importacion = new QuizImportado
            {
                QuizCompartidoId = quizCompartido.Id,
                QuizId = nuevoQuiz.Id,
                UsuarioId = usuarioId
            };

            await _quizCompartidoRepository.AddImportacionAsync(importacion);

            // Actualizar contador de usos
            quizCompartido.VecesUsado++;

            // Guardar la importación y el contador
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Quiz {QuizId} importado por usuario {UsuarioId} desde código {Codigo}",
                nuevoQuiz.Id, usuarioId, codigo);

            return ServiceResult<int>.Success(nuevoQuiz.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar quiz con código {Codigo}", codigo);
            return ServiceResult<int>.Failure("Error al importar el quiz");
        }
    }

    public async Task<ServiceResult<QuizCompartidoInfo>> ObtenerInfoQuizCompartidoAsync(string codigo)
    {
        try
        {
            _logger.LogInformation("Buscando quiz compartido con código: {Codigo}", codigo);
            var quizCompartido = await _quizCompartidoRepository.GetByCodigoAsync(codigo);
            
            if (quizCompartido == null)
            {
                _logger.LogWarning("No se encontró quiz compartido con código: {Codigo}", codigo);
                return ServiceResult<QuizCompartidoInfo>.Failure("El código de compartición no existe");
            }
            
            _logger.LogInformation("Quiz compartido encontrado - Id: {Id}, EstaActivo: {EstaActivo}, Quiz: {QuizTitulo}", 
                quizCompartido.Id, quizCompartido.EstaActivo, quizCompartido.Quiz?.Titulo ?? "NULL");

            // Verificar que el Quiz y sus relaciones se cargaron correctamente
            if (quizCompartido.Quiz == null)
            {
                _logger.LogError("El Quiz asociado al código {Codigo} es NULL", codigo);
                return ServiceResult<QuizCompartidoInfo>.Failure("Error: El quiz no se cargó correctamente");
            }

            if (quizCompartido.Quiz.Materia == null)
            {
                _logger.LogError("La Materia del Quiz {QuizId} es NULL", quizCompartido.QuizId);
                return ServiceResult<QuizCompartidoInfo>.Failure("Error: La materia del quiz no se cargó correctamente");
            }

            if (quizCompartido.Propietario == null)
            {
                _logger.LogError("El Propietario del código {Codigo} es NULL", codigo);
                return ServiceResult<QuizCompartidoInfo>.Failure("Error: El propietario no se cargó correctamente");
            }

            var disponible = quizCompartido.EstaActivo &&
                           (!quizCompartido.FechaExpiracion.HasValue || 
                            quizCompartido.FechaExpiracion.Value >= DateTime.UtcNow) &&
                           (!quizCompartido.MaximoUsos.HasValue || 
                            quizCompartido.VecesUsado < quizCompartido.MaximoUsos.Value);

            int? usosRestantes = null;
            if (quizCompartido.MaximoUsos.HasValue)
            {
                usosRestantes = quizCompartido.MaximoUsos.Value - quizCompartido.VecesUsado;
            }

            _logger.LogInformation("Info calculada - Disponible: {Disponible}, Preguntas: {NumPreguntas}, UsosRestantes: {UsosRestantes}", 
                disponible, quizCompartido.Quiz.Preguntas?.Count ?? 0, usosRestantes);

            var info = new QuizCompartidoInfo
            {
                TituloQuiz = quizCompartido.Quiz.Titulo,
                Descripcion = quizCompartido.Quiz.Descripcion,
                NombreMateria = quizCompartido.Quiz.Materia.Nombre,
                NombrePropietario = quizCompartido.Propietario.UserName ?? "Usuario",
                NumeroPreguntas = quizCompartido.Quiz.Preguntas?.Count ?? 0,
                Dificultad = quizCompartido.Quiz.NivelDificultad.ToString(),
                FechaExpiracion = quizCompartido.FechaExpiracion,
                UsosRestantes = usosRestantes,
                PermiteModificaciones = quizCompartido.PermiteModificaciones,
                EstaDisponible = disponible,
                MensajeError = disponible ? null : "Este enlace no está disponible"
            };

            return ServiceResult<QuizCompartidoInfo>.Success(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de quiz compartido {Codigo}", codigo);
            return ServiceResult<QuizCompartidoInfo>.Failure("Error al obtener la información");
        }
    }

    public async Task<ServiceResult> RevocarComparticionAsync(int quizCompartidoId, string usuarioId)
    {
        try
        {
            var quizCompartido = await _quizCompartidoRepository.GetByIdAsync(quizCompartidoId);
            
            if (quizCompartido == null)
            {
                return ServiceResult.Failure("La compartición no existe");
            }

            if (quizCompartido.PropietarioId != usuarioId)
            {
                return ServiceResult.Failure("No tienes permisos para revocar esta compartición");
            }

            // ELIMINACIÓN COMPLETA EN CASCADA MANUAL:
            // 1. Obtener todos los QuizImportado relacionados
            var importaciones = await _quizImportadoRepository.FindAsync(qi => qi.QuizCompartidoId == quizCompartidoId);
            var importacionesList = importaciones.ToList();
            
            _logger.LogInformation(
                "Revocando compartición {Id}: Encontradas {Count} importaciones para eliminar",
                quizCompartidoId, importacionesList.Count);
            
            // 2. Eliminar los Quizzes importados (el Quiz de cada estudiante)
            foreach (var importacion in importacionesList)
            {
                var quizImportado = await _quizRepository.GetByIdAsync(importacion.QuizId);
                if (quizImportado != null)
                {
                    _quizRepository.Remove(quizImportado);
                    _logger.LogInformation(
                        "Eliminando Quiz importado {QuizId} del usuario {UsuarioId}",
                        quizImportado.Id, importacion.UsuarioId);
                }
            }
            
            // 3. Eliminar el QuizCompartido (que eliminará los QuizImportado por cascada)
            _quizCompartidoRepository.Remove(quizCompartido);
            
            // 4. Guardar todos los cambios
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Compartición {Id} ELIMINADA COMPLETAMENTE por usuario {UsuarioId} - {Count} quizzes importados eliminados",
                quizCompartidoId, usuarioId, importacionesList.Count);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar compartición {Id}", quizCompartidoId);
            return ServiceResult.Failure("Error al eliminar la compartición");
        }
    }

    public async Task<List<QuizCompartidoResumen>> ObtenerQuizzesCompartidosAsync(string usuarioId)
    {
        var compartidos = await _quizCompartidoRepository.GetByPropietarioAsync(usuarioId);
        
        return compartidos.Select(qc => new QuizCompartidoResumen
        {
            Id = qc.Id,
            QuizId = qc.QuizId,
            Codigo = qc.CodigoCompartido,
            TituloQuiz = qc.Quiz?.Titulo ?? "Sin título",
            NombreMateria = qc.Quiz?.Materia?.Nombre ?? "Sin materia",
            Dificultad = qc.Quiz?.NivelDificultad.ToString() ?? "Desconocida",
            NumeroPreguntas = qc.Quiz?.Preguntas?.Count ?? 0,
            FechaCreacion = qc.FechaCreacion,
            FechaExpiracion = qc.FechaExpiracion,
            VecesUsado = qc.VecesUsado,
            MaximoUsos = qc.MaximoUsos,
            EstaExpirado = qc.FechaExpiracion.HasValue && qc.FechaExpiracion.Value < DateTime.UtcNow,
            EstaAgotado = qc.MaximoUsos.HasValue && qc.VecesUsado >= qc.MaximoUsos.Value,
            EstaActivo = qc.EstaActivo
        }).ToList();
    }

    public async Task<List<QuizImportadoResumen>> ObtenerQuizzesImportadosAsync(string usuarioId)
    {
        var importados = await _quizCompartidoRepository.GetImportadosByUsuarioAsync(usuarioId);
        
        return importados.Select(qi => new QuizImportadoResumen
        {
            QuizId = qi.QuizId,
            TituloQuiz = qi.Quiz?.Titulo ?? "Sin título",
            NombreMateria = qi.Quiz?.Materia?.Nombre ?? "Sin materia",
            Dificultad = qi.Quiz?.NivelDificultad.ToString() ?? "Desconocida",
            NumeroPreguntas = qi.Quiz?.Preguntas?.Count ?? 0,
            NombrePropietarioOriginal = qi.QuizCompartido?.Propietario?.UserName ?? "Usuario",
            FechaImportacion = qi.FechaCreacion
        }).ToList();
    }

    private async Task<string> GenerarCodigoUnicoAsync()
    {
        string codigo;
        do
        {
            // Generar código alfanumérico de 8 caracteres usando GUID
            var guid = Guid.NewGuid().ToString("N"); // N = sin guiones
            // Tomar los primeros 8 caracteres del GUID
            codigo = guid.Substring(0, 8).ToUpper();
        }
        while (await _quizCompartidoRepository.ExisteCodigoAsync(codigo));

        return codigo;
    }
}
