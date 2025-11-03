using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para quizzes compartidos
/// </summary>
public class QuizCompartidoRepository : GenericRepository<QuizCompartido>, IQuizCompartidoRepository
{
    public QuizCompartidoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<QuizCompartido?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(qc => qc.Quiz)
                .ThenInclude(q => q.Preguntas)
            .Include(qc => qc.Quiz)
                .ThenInclude(q => q.Materia)
            .Include(qc => qc.Propietario)
            .FirstOrDefaultAsync(qc => qc.CodigoCompartido == codigo && qc.EstaActivo);
    }

    public async Task<IEnumerable<QuizCompartido>> GetByPropietarioAsync(string propietarioId)
    {
        return await _dbSet
            .Include(qc => qc.Quiz)
            .Include(qc => qc.Importaciones)
            .Where(qc => qc.PropietarioId == propietarioId)
            .OrderByDescending(qc => qc.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuizCompartido>> GetByQuizIdAsync(int quizId)
    {
        return await _dbSet
            .Include(qc => qc.Importaciones)
            .Where(qc => qc.QuizId == quizId)
            .OrderByDescending(qc => qc.FechaCreacion)
            .ToListAsync();
    }

    public async Task<bool> ExisteCodigoAsync(string codigo)
    {
        return await _dbSet.AnyAsync(qc => qc.CodigoCompartido == codigo);
    }

    public async Task<IEnumerable<QuizImportado>> GetImportadosByUsuarioAsync(string usuarioId)
    {
        return await _context.QuizzesImportados
            .Include(qi => qi.Quiz)
                .ThenInclude(q => q.Materia)
            .Include(qi => qi.QuizCompartido)
                .ThenInclude(qc => qc.Propietario)
            .Where(qi => qi.UsuarioId == usuarioId)
            .OrderByDescending(qi => qi.FechaCreacion)
            .ToListAsync();
    }

    public async Task<bool> UsuarioYaImportoAsync(int quizCompartidoId, string usuarioId)
    {
        return await _context.QuizzesImportados
            .AnyAsync(qi => qi.QuizCompartidoId == quizCompartidoId && qi.UsuarioId == usuarioId);
    }

    public async Task AddImportacionAsync(QuizImportado importacion)
    {
        await _context.QuizzesImportados.AddAsync(importacion);
    }

    public async Task<QuizImportado?> GetImportacionByQuizIdAsync(int quizId)
    {
        return await _context.QuizzesImportados
            .Include(qi => qi.QuizCompartido)
                .ThenInclude(qc => qc.Quiz)
                    .ThenInclude(q => q.Creador)
            .FirstOrDefaultAsync(qi => qi.QuizId == quizId);
    }
}
