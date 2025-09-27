using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio específico para la entidad Materia con operaciones especializadas
    /// </summary>
    public class MateriaRepository : GenericRepository<Materia>, IMateriaRepository
    {
        public MateriaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Materia>> GetMateriasByUsuarioIdAsync(string usuarioId)
        {
            return await _dbSet
                .Where(m => m.UsuarioId == usuarioId)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Materia?> GetMateriaWithFlashcardsAsync(int materiaId)
        {
            return await _dbSet
                .Include(m => m.Flashcards)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
        }

        public async Task<Materia?> GetMateriaWithQuizzesAsync(int materiaId)
        {
            return await _dbSet
                .Include(m => m.Quizzes)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
        }

        public async Task<Materia?> GetMateriaCompletaAsync(int materiaId)
        {
            return await _dbSet
                .Include(m => m.Flashcards)
                .Include(m => m.Quizzes)
                .Include(m => m.EstadisticasEstudio)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
        }

        public async Task<bool> ExisteMateriaByNombreAsync(string nombre, string usuarioId, int? excludeId = null)
        {
            var query = _dbSet.Where(m => m.Nombre.ToLower() == nombre.ToLower() && m.UsuarioId == usuarioId);
            
            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Materia>> GetMateriasConEstadisticasAsync(string usuarioId, DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _dbSet
                .Where(m => m.UsuarioId == usuarioId)
                .Include(m => m.EstadisticasEstudio.Where(e => e.Fecha >= fechaDesde && e.Fecha <= fechaHasta))
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<int> GetCantidadFlashcardsByMateriaAsync(int materiaId)
        {
            var materia = await _dbSet
                .Include(m => m.Flashcards)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
            
            return materia?.Flashcards?.Count ?? 0;
        }

        public async Task<int> GetCantidadQuizzesByMateriaAsync(int materiaId)
        {
            var materia = await _dbSet
                .Include(m => m.Quizzes)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
            
            return materia?.Quizzes?.Count ?? 0;
        }

        public async Task<IEnumerable<Materia>> BuscarMateriasAsync(string termino, string usuarioId)
        {
            return await _dbSet
                .Where(m => m.UsuarioId == usuarioId && 
                           (m.Nombre.Contains(termino) || 
                            (m.Descripcion != null && m.Descripcion.Contains(termino))))
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetEstadisticasGeneralesByUsuarioAsync(string usuarioId)
        {
            var materias = await _dbSet
                .Where(m => m.UsuarioId == usuarioId)
                .Include(m => m.Flashcards)
                .ToListAsync();

            return materias.ToDictionary(
                m => m.Id,
                m => m.Flashcards?.Count ?? 0
            );
        }

        public async Task<bool> TieneDependenciasAsync(int materiaId)
        {
            var materia = await _dbSet
                .Include(m => m.Flashcards)
                .Include(m => m.Quizzes)
                .Include(m => m.EstadisticasEstudio)
                .FirstOrDefaultAsync(m => m.Id == materiaId);

            return materia != null && 
                   ((materia.Flashcards?.Any() == true) || 
                    (materia.Quizzes?.Any() == true) || 
                    (materia.EstadisticasEstudio?.Any() == true));
        }
    }
}