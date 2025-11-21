using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Repositorio para resultados de quiz
/// </summary>
public class ResultadoQuizRepository : GenericRepository<ResultadoQuiz>, IResultadoQuizRepository
{
    public ResultadoQuizRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ResultadoQuiz>> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _context.ResultadosQuiz
            .Where(r => r.UsuarioId == usuarioId)
            .Include(r => r.Quiz)
            .ThenInclude(q => q.Materia)
            .OrderByDescending(r => r.FechaRealizacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResultadoQuiz>> GetResultadosRecientesAsync(string usuarioId, int cantidad = 10)
    {
        return await _context.ResultadosQuiz
            .Where(r => r.UsuarioId == usuarioId && r.EstaCompletado)
            .Include(r => r.Quiz)
            .ThenInclude(q => q.Materia)
            .OrderByDescending(r => r.FechaFinalizacion)
            .Take(cantidad)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResultadoQuiz>> GetByQuizIdAsync(int quizId, string usuarioId)
    {
        return await _context.ResultadosQuiz
            .Where(r => r.QuizId == quizId && r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRealizacion)
            .ToListAsync();
    }

    public async Task<ResultadoQuiz?> GetMejorResultadoAsync(int quizId, string usuarioId)
    {
        return await _context.ResultadosQuiz
            .Where(r => r.QuizId == quizId && r.UsuarioId == usuarioId && r.EstaCompletado)
            .OrderByDescending(r => r.PorcentajeAcierto)
            .ThenByDescending(r => r.FechaRealizacion)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ResultadoQuiz>> GetResultadosByUsuarioIdAsync(string usuarioId)
    {
        return await _context.ResultadosQuiz
            .Where(r => r.UsuarioId == usuarioId)
            .Include(r => r.Quiz)
            .ThenInclude(q => q.Materia)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResultadoQuiz>> GetResultadosByMateriaIdAsync(int materiaId)
    {
        return await _context.ResultadosQuiz
            .Where(r => r.Quiz != null && r.Quiz.MateriaId == materiaId)
            .Include(r => r.Quiz)
            .ThenInclude(q => q.Materia)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }
}