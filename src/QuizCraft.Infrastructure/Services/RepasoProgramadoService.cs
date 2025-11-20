using Microsoft.EntityFrameworkCore;
using QuizCraft.Application.Interfaces;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Services;

/// <summary>
/// Servicio para la gestión de repasos programados
/// </summary>
public class RepasoProgramadoService : IRepasoProgramadoService
{
    private readonly ApplicationDbContext _context;

    public RepasoProgramadoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RepasosProgramadosViewModel> ObtenerRepasosPorUsuarioAsync(string usuarioId)
    {
        var repasos = await _context.RepasosProgramados
            .Include(r => r.Materia)
            .Include(r => r.Quiz)
            .Include(r => r.Flashcard)
            .Where(r => r.UsuarioId == usuarioId && r.EstaActivo)
            .OrderBy(r => r.FechaProgramada)
            .Select(r => new RepasoProgramadoListViewModel
            {
                Id = r.Id,
                Titulo = r.Titulo,
                Descripcion = r.Descripcion,
                FechaProgramada = r.FechaProgramada,
                Completado = r.Completado,
                FechaCompletado = r.FechaCompletado,
                TipoRepaso = r.TipoRepaso,
                FrecuenciaRepeticion = r.FrecuenciaRepeticion,
                ProximaFecha = r.ProximaFecha,
                UltimoPuntaje = r.UltimoPuntaje,
                MateriaNombre = r.Materia != null ? r.Materia.Nombre : null,
                QuizTitulo = r.Quiz != null ? r.Quiz.Titulo : null,
                FlashcardPregunta = r.Flashcard != null ? r.Flashcard.Pregunta : null,
                NotificacionesHabilitadas = r.NotificacionesHabilitadas,
                // IDs para redirección
                QuizId = r.QuizId,
                FlashcardId = r.FlashcardId,
                MateriaId = r.MateriaId
            })
            .ToListAsync();

        var ahora = DateTime.Now;
        var en24Horas = ahora.AddHours(24);

        return new RepasosProgramadosViewModel
        {
            RepasosVencidos = repasos.Where(r => !r.Completado && r.FechaProgramada < ahora).ToList(),
            RepasosProximos = repasos.Where(r => !r.Completado && r.FechaProgramada >= ahora && r.FechaProgramada <= en24Horas).ToList(),
            RepasosPendientes = repasos.Where(r => !r.Completado && r.FechaProgramada > en24Horas).ToList(),
            RepasosCompletados = repasos.Where(r => r.Completado).OrderByDescending(r => r.FechaCompletado).ToList()
        };
    }

    public async Task<RepasoProgramado?> ObtenerRepasoPorIdAsync(int repasoId, string usuarioId)
    {
        return await _context.RepasosProgramados
            .Include(r => r.Materia)
            .Include(r => r.Quiz)
            .Include(r => r.Flashcard)
            .FirstOrDefaultAsync(r => r.Id == repasoId && r.UsuarioId == usuarioId && r.EstaActivo);
    }

    public async Task<bool> CrearRepasoProgramadoAsync(CrearRepasoProgramadoViewModel modelo, string usuarioId)
    {
        try
        {
            var repaso = new RepasoProgramado
            {
                Titulo = modelo.Titulo,
                Descripcion = modelo.Descripcion,
                FechaProgramada = modelo.FechaProgramada,
                TipoRepaso = modelo.TipoRepaso,
                FrecuenciaRepeticion = modelo.FrecuenciaRepeticion,
                NotificacionesHabilitadas = false, // Valor por defecto
                MinutosNotificacionPrevia = 15, // Valor por defecto
                Notas = modelo.Notas,
                UsuarioId = usuarioId,
                MateriaId = modelo.MateriaId,
                QuizId = modelo.QuizId,
                FlashcardId = modelo.FlashcardId
            };

            // Si es un repaso recurrente, calcular la próxima fecha
            if (modelo.FrecuenciaRepeticion.HasValue && modelo.FrecuenciaRepeticion != FrecuenciaRepaso.Unica)
            {
                repaso.ProximaFecha = CalcularProximaFecha(modelo.FechaProgramada, modelo.FrecuenciaRepeticion.Value);
            }

            _context.RepasosProgramados.Add(repaso);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ActualizarRepasoProgramadoAsync(EditarRepasoProgramadoViewModel modelo, string usuarioId)
    {
        try
        {
            var repaso = await _context.RepasosProgramados
                .FirstOrDefaultAsync(r => r.Id == modelo.Id && r.UsuarioId == usuarioId && r.EstaActivo);

            if (repaso == null) return false;

            repaso.Titulo = modelo.Titulo;
            repaso.Descripcion = modelo.Descripcion;
            repaso.FechaProgramada = modelo.FechaProgramada;
            repaso.TipoRepaso = modelo.TipoRepaso;
            repaso.FrecuenciaRepeticion = modelo.FrecuenciaRepeticion;
            repaso.Notas = modelo.Notas;
            repaso.MateriaId = modelo.MateriaId;
            repaso.QuizId = modelo.QuizId;
            repaso.FlashcardId = modelo.FlashcardId;
            repaso.Completado = modelo.Completado;
            repaso.FechaCompletado = modelo.FechaCompletado;
            repaso.UltimoPuntaje = modelo.UltimoPuntaje;
            repaso.FechaModificacion = DateTime.Now;

            // Recalcular próxima fecha si cambió la frecuencia
            if (modelo.FrecuenciaRepeticion.HasValue && modelo.FrecuenciaRepeticion != FrecuenciaRepaso.Unica)
            {
                repaso.ProximaFecha = CalcularProximaFecha(modelo.FechaProgramada, modelo.FrecuenciaRepeticion.Value);
            }
            else
            {
                repaso.ProximaFecha = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EliminarRepasoProgramadoAsync(int repasoId, string usuarioId)
    {
        try
        {
            var repaso = await _context.RepasosProgramados
                .FirstOrDefaultAsync(r => r.Id == repasoId && r.UsuarioId == usuarioId);

            if (repaso == null) return false;

            repaso.EstaActivo = false;
            repaso.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CompletarRepasoAsync(CompletarRepasoViewModel modelo, string usuarioId)
    {
        try
        {
            var repaso = await _context.RepasosProgramados
                .FirstOrDefaultAsync(r => r.Id == modelo.Id && r.UsuarioId == usuarioId && r.EstaActivo);

            if (repaso == null) return false;

            repaso.Completado = true;
            repaso.FechaCompletado = DateTime.Now;
            repaso.UltimoPuntaje = modelo.Puntaje;
            if (!string.IsNullOrEmpty(modelo.NotasRepaso))
            {
                repaso.Notas = string.IsNullOrEmpty(repaso.Notas) 
                    ? modelo.NotasRepaso 
                    : $"{repaso.Notas}\n\n--- Notas del repaso ({DateTime.Now:dd/MM/yyyy HH:mm}) ---\n{modelo.NotasRepaso}";
            }
            repaso.FechaModificacion = DateTime.Now;

            // Si está configurado para programar el próximo automáticamente
            if (modelo.ProgramarProximo && repaso.FrecuenciaRepeticion.HasValue && repaso.FrecuenciaRepeticion != FrecuenciaRepaso.Unica)
            {
                await ProgramarProximoRepasoAutomaticoAsync(repaso.Id);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<RepasoProgramado>> ObtenerRepasosParaNotificarAsync()
    {
        var ahora = DateTime.Now;
        
        return await _context.RepasosProgramados
            .Include(r => r.Usuario)
            .Include(r => r.Materia)
            .Include(r => r.Quiz)
            .Include(r => r.Flashcard)
            .Where(r => r.EstaActivo && 
                       !r.Completado && 
                       r.NotificacionesHabilitadas &&
                       r.FechaProgramada.AddMinutes(-r.MinutosNotificacionPrevia) <= ahora &&
                       r.FechaProgramada > ahora)
            .ToListAsync();
    }

    public async Task<List<RepasoProgramadoListViewModel>> ObtenerRepasosVencidosAsync(string usuarioId)
    {
        var ahora = DateTime.Now;
        
        return await _context.RepasosProgramados
            .Include(r => r.Materia)
            .Include(r => r.Quiz)
            .Include(r => r.Flashcard)
            .Where(r => r.UsuarioId == usuarioId && 
                       r.EstaActivo && 
                       !r.Completado && 
                       r.FechaProgramada < ahora)
            .Select(r => new RepasoProgramadoListViewModel
            {
                Id = r.Id,
                Titulo = r.Titulo,
                Descripcion = r.Descripcion,
                FechaProgramada = r.FechaProgramada,
                Completado = r.Completado,
                MateriaNombre = r.Materia != null ? r.Materia.Nombre : null,
                QuizTitulo = r.Quiz != null ? r.Quiz.Titulo : null,
                FlashcardPregunta = r.Flashcard != null ? r.Flashcard.Pregunta : null,
                // IDs para redirección
                QuizId = r.QuizId,
                FlashcardId = r.FlashcardId,
                MateriaId = r.MateriaId
            })
            .OrderBy(r => r.FechaProgramada)
            .ToListAsync();
    }

    public async Task<List<RepasoProgramadoListViewModel>> ObtenerRepasosProximosAsync(string usuarioId)
    {
        var ahora = DateTime.Now;
        var en24Horas = ahora.AddHours(24);
        
        return await _context.RepasosProgramados
            .Include(r => r.Materia)
            .Include(r => r.Quiz)
            .Include(r => r.Flashcard)
            .Where(r => r.UsuarioId == usuarioId && 
                       r.EstaActivo && 
                       !r.Completado && 
                       r.FechaProgramada >= ahora && 
                       r.FechaProgramada <= en24Horas)
            .Select(r => new RepasoProgramadoListViewModel
            {
                Id = r.Id,
                Titulo = r.Titulo,
                Descripcion = r.Descripcion,
                FechaProgramada = r.FechaProgramada,
                Completado = r.Completado,
                MateriaNombre = r.Materia != null ? r.Materia.Nombre : null,
                QuizTitulo = r.Quiz != null ? r.Quiz.Titulo : null,
                FlashcardPregunta = r.Flashcard != null ? r.Flashcard.Pregunta : null,
                // IDs para redirección
                QuizId = r.QuizId,
                FlashcardId = r.FlashcardId,
                MateriaId = r.MateriaId
            })
            .OrderBy(r => r.FechaProgramada)
            .ToListAsync();
    }

    public async Task<bool> ProgramarProximoRepasoAutomaticoAsync(int repasoId)
    {
        try
        {
            var repasoOriginal = await _context.RepasosProgramados
                .FirstOrDefaultAsync(r => r.Id == repasoId);

            if (repasoOriginal == null || 
                !repasoOriginal.FrecuenciaRepeticion.HasValue || 
                repasoOriginal.FrecuenciaRepeticion == FrecuenciaRepaso.Unica)
                return false;

            var proximaFecha = CalcularProximaFecha(repasoOriginal.FechaProgramada, repasoOriginal.FrecuenciaRepeticion.Value);

            var nuevoRepaso = new RepasoProgramado
            {
                Titulo = repasoOriginal.Titulo,
                Descripcion = repasoOriginal.Descripcion,
                FechaProgramada = proximaFecha,
                TipoRepaso = TipoRepaso.Automatico,
                FrecuenciaRepeticion = repasoOriginal.FrecuenciaRepeticion,
                ProximaFecha = CalcularProximaFecha(proximaFecha, repasoOriginal.FrecuenciaRepeticion.Value),
                NotificacionesHabilitadas = repasoOriginal.NotificacionesHabilitadas,
                MinutosNotificacionPrevia = repasoOriginal.MinutosNotificacionPrevia,
                UsuarioId = repasoOriginal.UsuarioId,
                MateriaId = repasoOriginal.MateriaId,
                QuizId = repasoOriginal.QuizId,
                FlashcardId = repasoOriginal.FlashcardId
            };

            _context.RepasosProgramados.Add(nuevoRepaso);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SelectItemViewModel>> ObtenerMateriasDisponiblesAsync(string usuarioId)
    {
        return await _context.Materias
            .Where(m => m.UsuarioId == usuarioId && m.EstaActivo)
            .Select(m => new SelectItemViewModel
            {
                Value = m.Id,
                Text = m.Nombre
            })
            .OrderBy(m => m.Text)
            .ToListAsync();
    }

    public async Task<List<SelectItemViewModel>> ObtenerQuizzesDisponiblesAsync(string usuarioId, int? materiaId = null)
    {
        var query = _context.Quizzes
            .Where(q => q.CreadorId == usuarioId && q.EstaActivo);

        if (materiaId.HasValue)
        {
            query = query.Where(q => q.MateriaId == materiaId.Value);
        }

        return await query
            .Select(q => new SelectItemViewModel
            {
                Value = q.Id,
                Text = q.Titulo
            })
            .OrderBy(q => q.Text)
            .ToListAsync();
    }

    public async Task<List<SelectItemViewModel>> ObtenerFlashcardsDisponiblesAsync(string usuarioId, int? materiaId = null)
    {
        var query = _context.Flashcards
            .Include(f => f.Materia)
            .Where(f => f.Materia.UsuarioId == usuarioId && f.EstaActivo);

        if (materiaId.HasValue)
        {
            query = query.Where(f => f.MateriaId == materiaId.Value);
        }

        return await query
            .Select(f => new SelectItemViewModel
            {
                Value = f.Id,
                Text = f.Pregunta.Length > 50 ? f.Pregunta.Substring(0, 50) + "..." : f.Pregunta
            })
            .OrderBy(f => f.Text)
            .ToListAsync();
    }

    public async Task<List<RepasoProgramado>> GenerarRepasosAutomaticosAsync(string usuarioId)
    {
        // Lógica para generar repasos automáticos basados en el rendimiento
        var usuario = await _context.Users.FindAsync(usuarioId);
        if (usuario == null) return new List<RepasoProgramado>();

        var repasosGenerados = new List<RepasoProgramado>();

        // Obtener flashcards con bajo rendimiento (menos del 70% de aciertos)
        var flashcardsProblematicas = await _context.Flashcards
            .Include(f => f.Materia)
            .Where(f => f.Materia.UsuarioId == usuarioId && 
                       f.EstaActivo && 
                       f.VecesVista > 0 &&
                       (double)f.VecesCorrecta / f.VecesVista < 0.7)
            .Take(5) // Limitar a 5 flashcards
            .ToListAsync();

        foreach (var flashcard in flashcardsProblematicas)
        {
            var repaso = new RepasoProgramado
            {
                Titulo = $"Repaso automático: {flashcard.Pregunta.Substring(0, Math.Min(flashcard.Pregunta.Length, 30))}...",
                Descripcion = "Repaso generado automáticamente basado en bajo rendimiento",
                FechaProgramada = DateTime.Now.AddDays(1), // Mañana
                TipoRepaso = TipoRepaso.Sugerido,
                FrecuenciaRepeticion = FrecuenciaRepaso.CadaTresDias,
                NotificacionesHabilitadas = usuario.NotificacionesHabilitadas,
                UsuarioId = usuarioId,
                MateriaId = flashcard.MateriaId,
                FlashcardId = flashcard.Id
            };

            repasosGenerados.Add(repaso);
        }

        if (repasosGenerados.Any())
        {
            _context.RepasosProgramados.AddRange(repasosGenerados);
            await _context.SaveChangesAsync();
        }

        return repasosGenerados;
    }

    private static DateTime CalcularProximaFecha(DateTime fechaBase, FrecuenciaRepaso frecuencia)
    {
        return frecuencia switch
        {
            FrecuenciaRepaso.Diaria => fechaBase.AddDays(1),
            FrecuenciaRepaso.CadaDosDias => fechaBase.AddDays(2),
            FrecuenciaRepaso.CadaTresDias => fechaBase.AddDays(3),
            FrecuenciaRepaso.Semanal => fechaBase.AddDays(7),
            FrecuenciaRepaso.Quincenal => fechaBase.AddDays(14),
            FrecuenciaRepaso.Mensual => fechaBase.AddMonths(1),
            _ => fechaBase.AddDays(1) // Por defecto, un día
        };
    }
}