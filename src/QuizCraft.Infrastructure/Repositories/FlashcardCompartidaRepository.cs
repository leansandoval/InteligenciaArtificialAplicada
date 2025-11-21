using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para flashcards compartidas
/// </summary>
public class FlashcardCompartidaRepository : GenericRepository<FlashcardCompartida>, IFlashcardCompartidaRepository
{
    public FlashcardCompartidaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<FlashcardCompartida?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(fc => fc.Flashcard)
                .ThenInclude(f => f.Materia)
            .Include(fc => fc.Propietario)
            .Include(fc => fc.Importaciones)
            .FirstOrDefaultAsync(fc => fc.CodigoCompartido == codigo);
    }

    public async Task<IEnumerable<FlashcardCompartida>> GetByPropietarioAsync(string propietarioId)
    {
        return await _dbSet
            .Include(fc => fc.Flashcard)
                .ThenInclude(f => f.Materia)
            .Where(fc => fc.PropietarioId == propietarioId)
            .OrderByDescending(fc => fc.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<FlashcardCompartida>> GetByFlashcardIdAsync(int flashcardId)
    {
        return await _dbSet
            .Include(fc => fc.Propietario)
            .Where(fc => fc.FlashcardId == flashcardId)
            .OrderByDescending(fc => fc.FechaCreacion)
            .ToListAsync();
    }

    public async Task<bool> ExisteCodigoAsync(string codigo)
    {
        return await _dbSet.AnyAsync(fc => fc.CodigoCompartido == codigo);
    }

    public async Task<IEnumerable<FlashcardImportada>> GetImportadasByUsuarioAsync(string usuarioId)
    {
        return await _context.Set<FlashcardImportada>()
            .Include(fi => fi.Flashcard)
                .ThenInclude(f => f.Materia)
            .Include(fi => fi.FlashcardCompartida)
                .ThenInclude(fc => fc.Flashcard)
                    .ThenInclude(f => f.Materia)
            .Include(fi => fi.FlashcardCompartida)
                .ThenInclude(fc => fc.Propietario)
            .Where(fi => fi.UsuarioId == usuarioId)
            .OrderByDescending(fi => fi.FechaImportacion)
            .ToListAsync();
    }

    public async Task<bool> UsuarioYaImportoAsync(int flashcardCompartidaId, string usuarioId)
    {
        return await _context.Set<FlashcardImportada>()
            .AnyAsync(fi => fi.FlashcardCompartidaId == flashcardCompartidaId && fi.UsuarioId == usuarioId);
    }

    public async Task AddImportacionAsync(FlashcardImportada importacion)
    {
        await _context.Set<FlashcardImportada>().AddAsync(importacion);
    }

    public async Task<FlashcardImportada?> GetImportacionByFlashcardIdAsync(int flashcardId)
    {
        return await _context.Set<FlashcardImportada>()
            .Include(fi => fi.FlashcardCompartida)
            .FirstOrDefaultAsync(fi => fi.FlashcardId == flashcardId);
    }
}
