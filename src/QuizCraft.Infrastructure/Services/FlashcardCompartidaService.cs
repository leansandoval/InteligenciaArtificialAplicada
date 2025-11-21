using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.Models;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using System.Security.Cryptography;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para compartir e importar flashcards
/// </summary>
public class FlashcardCompartidaService : IFlashcardCompartidaService
{
    private readonly IFlashcardCompartidaRepository _flashcardCompartidaRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IGenericRepository<FlashcardImportada> _flashcardImportadaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FlashcardCompartidaService> _logger;

    public FlashcardCompartidaService(
        IFlashcardCompartidaRepository flashcardCompartidaRepository,
        IFlashcardRepository flashcardRepository,
        IGenericRepository<FlashcardImportada> flashcardImportadaRepository,
        IUnitOfWork unitOfWork,
        ILogger<FlashcardCompartidaService> logger)
    {
        _flashcardCompartidaRepository = flashcardCompartidaRepository;
        _flashcardRepository = flashcardRepository;
        _flashcardImportadaRepository = flashcardImportadaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<string>> CompartirFlashcardAsync(
        int flashcardId, 
        string usuarioId, 
        CompartirFlashcardOptions opciones)
    {
        try
        {
            _logger.LogInformation("Iniciando compartición de flashcard {FlashcardId} por usuario {UsuarioId}", 
                flashcardId, usuarioId);
            
            // Verificar que la flashcard existe y pertenece al usuario
            var flashcard = await _flashcardRepository.GetByIdWithMateriaAsync(flashcardId);
            if (flashcard == null)
            {
                _logger.LogWarning("Flashcard {FlashcardId} no encontrada", flashcardId);
                return ServiceResult<string>.Failure("La flashcard no existe");
            }

            if (flashcard.Materia.UsuarioId != usuarioId)
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para compartir flashcard {FlashcardId}", 
                    usuarioId, flashcardId);
                return ServiceResult<string>.Failure("No tienes permisos para compartir esta flashcard");
            }

            _logger.LogInformation("Flashcard {FlashcardId} validada correctamente", flashcardId);
            
            // Generar código único
            var codigo = await GenerarCodigoUnicoAsync();
            _logger.LogInformation("Código generado: {Codigo}", codigo);

            // Crear registro de compartición
            var flashcardCompartida = new FlashcardCompartida
            {
                FlashcardId = flashcardId,
                PropietarioId = usuarioId,
                CodigoCompartido = codigo,
                FechaExpiracion = opciones.FechaExpiracion,
                MaximoUsos = opciones.MaximoUsos,
                PermiteModificaciones = opciones.PermiteModificaciones
            };

            await _flashcardCompartidaRepository.AddAsync(flashcardCompartida);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Flashcard {FlashcardId} compartida exitosamente por usuario {UsuarioId} con código {Codigo}",
                flashcardId, usuarioId, codigo);

            return ServiceResult<string>.Success(codigo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al compartir flashcard {FlashcardId}. Detalle: {Message}", 
                flashcardId, ex.Message);
            return ServiceResult<string>.Failure($"Error al generar el enlace de compartición: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> ImportarFlashcardAsync(
        string codigo, 
        string usuarioId, 
        int materiaDestinoId)
    {
        try
        {
            // Obtener flashcard compartida
            var flashcardCompartida = await _flashcardCompartidaRepository.GetByCodigoAsync(codigo);
            
            if (flashcardCompartida == null || !flashcardCompartida.EstaActivo)
            {
                return ServiceResult<int>.Failure("El enlace de compartición no es válido o ha expirado");
            }

            // Verificar expiración
            if (flashcardCompartida.FechaExpiracion.HasValue && 
                flashcardCompartida.FechaExpiracion.Value < DateTime.UtcNow)
            {
                return ServiceResult<int>.Failure("El enlace de compartición ha expirado");
            }

            // Verificar límite de usos
            if (flashcardCompartida.MaximoUsos.HasValue && 
                flashcardCompartida.VecesUsado >= flashcardCompartida.MaximoUsos.Value)
            {
                return ServiceResult<int>.Failure("El enlace ha alcanzado el límite de usos");
            }

            // Verificar si ya importó esta flashcard
            if (await _flashcardCompartidaRepository.UsuarioYaImportoAsync(flashcardCompartida.Id, usuarioId))
            {
                return ServiceResult<int>.Failure("Ya has importado esta flashcard anteriormente");
            }

            // Evitar que el propietario importe su propia flashcard
            if (flashcardCompartida.PropietarioId == usuarioId)
            {
                return ServiceResult<int>.Failure("No puedes importar tu propia flashcard");
            }

            // Verificar que la materia destino pertenece al usuario
            var materiaDestino = await _unitOfWork.MateriaRepository.GetByIdAsync(materiaDestinoId);
            if (materiaDestino == null || materiaDestino.UsuarioId != usuarioId)
            {
                return ServiceResult<int>.Failure("La materia de destino no es válida");
            }

            // Clonar la flashcard
            var flashcardOriginal = flashcardCompartida.Flashcard;
            var nuevaFlashcard = new Flashcard
            {
                Pregunta = flashcardOriginal.Pregunta,
                Respuesta = flashcardOriginal.Respuesta,
                Pista = flashcardOriginal.Pista,
                MateriaId = materiaDestinoId,
                Dificultad = flashcardOriginal.Dificultad,
                EstaActivo = true
            };

            // Guardar nueva flashcard primero para obtener su ID
            await _flashcardRepository.AddAsync(nuevaFlashcard);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Nueva flashcard creada con ID: {FlashcardId}", nuevaFlashcard.Id);

            // Ahora registrar importación con el ID real de la flashcard
            var importacion = new FlashcardImportada
            {
                FlashcardCompartidaId = flashcardCompartida.Id,
                FlashcardId = nuevaFlashcard.Id,
                UsuarioId = usuarioId
            };

            await _flashcardCompartidaRepository.AddImportacionAsync(importacion);

            // Actualizar contador de usos
            flashcardCompartida.VecesUsado++;

            // Guardar la importación y el contador
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Flashcard {FlashcardId} importada por usuario {UsuarioId} desde código {Codigo}",
                nuevaFlashcard.Id, usuarioId, codigo);

            return ServiceResult<int>.Success(nuevaFlashcard.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar flashcard con código {Codigo}", codigo);
            return ServiceResult<int>.Failure("Error al importar la flashcard");
        }
    }

    public async Task<ServiceResult<FlashcardCompartidaInfo>> ObtenerInfoFlashcardCompartidaAsync(string codigo)
    {
        try
        {
            _logger.LogInformation("Buscando flashcard compartida con código: {Codigo}", codigo);
            var flashcardCompartida = await _flashcardCompartidaRepository.GetByCodigoAsync(codigo);
            
            if (flashcardCompartida == null)
            {
                _logger.LogWarning("No se encontró flashcard compartida con código: {Codigo}", codigo);
                return ServiceResult<FlashcardCompartidaInfo>.Failure("El código de compartición no existe");
            }
            
            _logger.LogInformation("Flashcard compartida encontrada - Id: {Id}, EstaActivo: {EstaActivo}", 
                flashcardCompartida.Id, flashcardCompartida.EstaActivo);

            if (flashcardCompartida.Flashcard == null)
            {
                _logger.LogError("La Flashcard del código {Codigo} es NULL", codigo);
                return ServiceResult<FlashcardCompartidaInfo>.Failure("Error: La flashcard no se cargó correctamente");
            }

            if (flashcardCompartida.Flashcard.Materia == null)
            {
                _logger.LogError("La Materia de la flashcard del código {Codigo} es NULL", codigo);
                return ServiceResult<FlashcardCompartidaInfo>.Failure("Error: La materia de la flashcard no se cargó correctamente");
            }

            if (flashcardCompartida.Propietario == null)
            {
                _logger.LogError("El Propietario del código {Codigo} es NULL", codigo);
                return ServiceResult<FlashcardCompartidaInfo>.Failure("Error: El propietario no se cargó correctamente");
            }

            var disponible = flashcardCompartida.EstaActivo &&
                           (!flashcardCompartida.FechaExpiracion.HasValue || 
                            flashcardCompartida.FechaExpiracion.Value >= DateTime.UtcNow) &&
                           (!flashcardCompartida.MaximoUsos.HasValue || 
                            flashcardCompartida.VecesUsado < flashcardCompartida.MaximoUsos.Value);

            int? usosRestantes = null;
            if (flashcardCompartida.MaximoUsos.HasValue)
            {
                usosRestantes = flashcardCompartida.MaximoUsos.Value - flashcardCompartida.VecesUsado;
            }

            _logger.LogInformation("Info calculada - Disponible: {Disponible}, UsosRestantes: {UsosRestantes}", 
                disponible, usosRestantes);

            var info = new FlashcardCompartidaInfo
            {
                Pregunta = flashcardCompartida.Flashcard.Pregunta,
                Respuesta = flashcardCompartida.Flashcard.Respuesta,
                Pista = flashcardCompartida.Flashcard.Pista,
                NombreMateria = flashcardCompartida.Flashcard.Materia.Nombre,
                NombrePropietario = flashcardCompartida.Propietario.UserName ?? "Usuario",
                Dificultad = flashcardCompartida.Flashcard.Dificultad.ToString(),
                FechaExpiracion = flashcardCompartida.FechaExpiracion,
                UsosRestantes = usosRestantes,
                PermiteModificaciones = flashcardCompartida.PermiteModificaciones,
                EstaDisponible = disponible,
                MensajeError = disponible ? null : "Este enlace no está disponible"
            };

            return ServiceResult<FlashcardCompartidaInfo>.Success(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de flashcard compartida {Codigo}", codigo);
            return ServiceResult<FlashcardCompartidaInfo>.Failure("Error al obtener la información");
        }
    }

    public async Task<ServiceResult> RevocarComparticionAsync(int flashcardCompartidaId, string usuarioId)
    {
        try
        {
            var flashcardCompartida = await _flashcardCompartidaRepository.GetByIdAsync(flashcardCompartidaId);
            
            if (flashcardCompartida == null)
            {
                return ServiceResult.Failure("La compartición no existe");
            }

            if (flashcardCompartida.PropietarioId != usuarioId)
            {
                return ServiceResult.Failure("No tienes permisos para revocar esta compartición");
            }

            // ELIMINACIÓN COMPLETA EN CASCADA MANUAL:
            // 1. Obtener todos los FlashcardImportada relacionados
            var importaciones = await _flashcardImportadaRepository
                .FindAsync(fi => fi.FlashcardCompartidaId == flashcardCompartidaId);
            var importacionesList = importaciones.ToList();
            
            _logger.LogInformation(
                "Revocando compartición {Id}: Encontradas {Count} importaciones para eliminar",
                flashcardCompartidaId, importacionesList.Count);
            
            // 2. Eliminar las Flashcards importadas (la Flashcard de cada estudiante)
            foreach (var importacion in importacionesList)
            {
                var flashcardImportada = await _flashcardRepository.GetByIdAsync(importacion.FlashcardId);
                if (flashcardImportada != null)
                {
                    _flashcardRepository.Remove(flashcardImportada);
                    _logger.LogInformation(
                        "Eliminando Flashcard importada {FlashcardId} del usuario {UsuarioId}",
                        flashcardImportada.Id, importacion.UsuarioId);
                }
            }
            
            // 3. Eliminar el FlashcardCompartida (que eliminará los FlashcardImportada por cascada)
            _flashcardCompartidaRepository.Remove(flashcardCompartida);
            
            // 4. Guardar todos los cambios
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Compartición {Id} ELIMINADA COMPLETAMENTE por usuario {UsuarioId} - {Count} flashcards importadas eliminadas",
                flashcardCompartidaId, usuarioId, importacionesList.Count);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar compartición {Id}", flashcardCompartidaId);
            return ServiceResult.Failure("Error al eliminar la compartición");
        }
    }

    public async Task<List<FlashcardCompartidaResumen>> ObtenerFlashcardsCompartidasAsync(string usuarioId)
    {
        var compartidas = await _flashcardCompartidaRepository.GetByPropietarioAsync(usuarioId);
        
        return compartidas.Select(fc => new FlashcardCompartidaResumen
        {
            Id = fc.Id,
            FlashcardId = fc.FlashcardId,
            Codigo = fc.CodigoCompartido,
            Pregunta = fc.Flashcard?.Pregunta ?? "Sin pregunta",
            NombreMateria = fc.Flashcard?.Materia?.Nombre ?? "Sin materia",
            Dificultad = fc.Flashcard?.Dificultad.ToString() ?? "Desconocida",
            FechaCreacion = fc.FechaCreacion,
            FechaExpiracion = fc.FechaExpiracion,
            VecesUsado = fc.VecesUsado,
            MaximoUsos = fc.MaximoUsos,
            EstaExpirado = fc.FechaExpiracion.HasValue && fc.FechaExpiracion.Value < DateTime.UtcNow,
            EstaAgotado = fc.MaximoUsos.HasValue && fc.VecesUsado >= fc.MaximoUsos.Value,
            EstaActivo = fc.EstaActivo
        }).ToList();
    }

    public async Task<List<FlashcardImportadaResumen>> ObtenerFlashcardsImportadasAsync(string usuarioId)
    {
        var importadas = await _flashcardCompartidaRepository.GetImportadasByUsuarioAsync(usuarioId);
        
        return importadas.Select(fi => new FlashcardImportadaResumen
        {
            FlashcardId = fi.FlashcardId,
            Pregunta = fi.Flashcard?.Pregunta ?? "Sin pregunta",
            NombreMateria = fi.Flashcard?.Materia?.Nombre ?? "Sin materia",
            Dificultad = fi.Flashcard?.Dificultad.ToString() ?? "Desconocida",
            NombrePropietarioOriginal = fi.FlashcardCompartida?.Propietario?.UserName ?? "Usuario desconocido",
            FechaImportacion = fi.FechaImportacion,
            PermiteModificaciones = fi.FlashcardCompartida?.PermiteModificaciones ?? true
        }).ToList();
    }

    private async Task<string> GenerarCodigoUnicoAsync()
    {
        string codigo;
        do
        {
            codigo = GenerarCodigoAleatorio();
        } 
        while (await _flashcardCompartidaRepository.ExisteCodigoAsync(codigo));
        
        return codigo;
    }

    private string GenerarCodigoAleatorio()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
