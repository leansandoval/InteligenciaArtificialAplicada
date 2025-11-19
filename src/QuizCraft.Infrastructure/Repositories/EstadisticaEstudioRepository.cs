using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Repositorio para estadísticas de estudio
/// </summary>
public class EstadisticaEstudioRepository : GenericRepository<EstadisticaEstudio>, IEstadisticaEstudioRepository
{
    public EstadisticaEstudioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EstadisticaEstudio>> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _context.EstadisticasEstudio
            .Where(e => e.UsuarioId == usuarioId)
            .Include(e => e.Materia)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<EstadisticaEstudio>> GetActividadRecienteAsync(string usuarioId, int dias = 7)
    {
        var fechaLimite = DateTime.Today.AddDays(-dias);
        
        return await _context.EstadisticasEstudio
            .Where(e => e.UsuarioId == usuarioId && e.Fecha >= fechaLimite)
            .Include(e => e.Materia)
            .OrderByDescending(e => e.FechaCreacion)
            .Take(10) // Limitar a las 10 más recientes
            .ToListAsync();
    }

    public async Task<EstadisticaEstudio?> GetEstadisticaHoyAsync(string usuarioId)
    {
        var hoy = DateTime.Today;
        
        return await _context.EstadisticasEstudio
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Fecha == hoy);
    }

    public async Task RegistrarActividadFlashcardAsync(string usuarioId, int materiaId, bool esCorrecta)
    {
        var estadisticaHoy = await GetEstadisticaHoyAsync(usuarioId);
        
        if (estadisticaHoy == null)
        {
            estadisticaHoy = new EstadisticaEstudio
            {
                UsuarioId = usuarioId,
                MateriaId = materiaId,
                Fecha = DateTime.Today,
                TipoActividad = TipoActividad.Flashcard
            };
            await AddAsync(estadisticaHoy);
        }
        
        estadisticaHoy.FlashcardsRevisadas++;
        if (esCorrecta)
            estadisticaHoy.FlashcardsCorrectas++;
        else
            estadisticaHoy.FlashcardsIncorrectas++;
            
        Update(estadisticaHoy);
    }

    public async Task RegistrarActividadQuizAsync(string usuarioId, int materiaId, double porcentajeAcierto)
    {
        var estadisticaHoy = await GetEstadisticaHoyAsync(usuarioId);
        
        if (estadisticaHoy == null)
        {
            estadisticaHoy = new EstadisticaEstudio
            {
                UsuarioId = usuarioId,
                MateriaId = materiaId,
                Fecha = DateTime.Today,
                TipoActividad = TipoActividad.Quiz
            };
            await AddAsync(estadisticaHoy);
        }
        
        estadisticaHoy.QuizzesRealizados++;
        // Calcular nuevo promedio
        estadisticaHoy.PromedioAcierto = ((estadisticaHoy.PromedioAcierto * (estadisticaHoy.QuizzesRealizados - 1)) + porcentajeAcierto) / estadisticaHoy.QuizzesRealizados;
        
        Update(estadisticaHoy);
    }
}