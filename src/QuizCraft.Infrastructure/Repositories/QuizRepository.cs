using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio específico para la entidad Quiz con operaciones especializadas
    /// </summary>
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        public QuizRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByMateriaIdAsync(int materiaId)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Where(q => q.MateriaId == materiaId)
                .OrderByDescending(q => q.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByCreadorIdAsync(string creadorId)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Include(q => q.Preguntas)
                .Include(q => q.Resultados)
                .Where(q => q.CreadorId == creadorId)
                .OrderByDescending(q => q.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizCompletoAsync(int quizId)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Include(q => q.Preguntas)
                .Include(q => q.Resultados)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<Quiz?> GetQuizConPreguntasAsync(int quizId)
        {
            return await _dbSet
                .Include(q => q.Preguntas)
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesPublicosAsync(int? materiaId = null)
        {
            IQueryable<Quiz> query = _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Include(q => q.Resultados)
                .Where(q => q.EsPublico);

            if (materiaId.HasValue)
                query = query.Where(q => q.MateriaId == materiaId.Value);

            return await query
                .OrderByDescending(q => q.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesActivosAsync()
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Where(q => q.EsActivo)
                .OrderByDescending(q => q.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> BuscarQuizzesAsync(string termino, string? usuarioId = null)
        {
            IQueryable<Quiz> query = _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador);

            // Si se especifica usuario, buscar solo sus quizzes, sino buscar solo públicos
            if (!string.IsNullOrEmpty(usuarioId))
                query = query.Where(q => q.CreadorId == usuarioId);
            else
                query = query.Where(q => q.EsPublico);

            return await query
                .Where(q => q.Titulo.Contains(termino) || 
                           (q.Descripcion != null && q.Descripcion.Contains(termino)))
                .OrderBy(q => q.Titulo)
                .ToListAsync();
        }

        public async Task<int> GetCantidadPreguntasAsync(int quizId)
        {
            var quiz = await _dbSet
                .Include(q => q.Preguntas)
                .FirstOrDefaultAsync(q => q.Id == quizId);
            
            return quiz?.Preguntas?.Count ?? 0;
        }

        public async Task<int> GetCantidadResultadosAsync(int quizId)
        {
            var quiz = await _dbSet
                .Include(q => q.Resultados)
                .FirstOrDefaultAsync(q => q.Id == quizId);
            
            return quiz?.Resultados?.Count ?? 0;
        }

        public async Task<double> GetPuntuacionPromedioAsync(int quizId)
        {
            var resultados = await _context.ResultadosQuiz
                .Where(r => r.QuizId == quizId)
                .Select(r => r.Puntuacion)
                .ToListAsync();

            return resultados.Any() ? (double)resultados.Average() : 0;
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesRecientesAsync(string usuarioId, int cantidad = 10)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Where(q => q.CreadorId == usuarioId)
                .OrderByDescending(q => q.FechaCreacion)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesPopularesAsync(int cantidad = 10)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Include(q => q.Resultados)
                .Where(q => q.EsPublico && q.EsActivo)
                .OrderByDescending(q => q.Resultados.Count)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<bool> ExisteQuizByTituloAsync(string titulo, string creadorId, int? excludeId = null)
        {
            var query = _dbSet.Where(q => q.Titulo.ToLower() == titulo.ToLower() && 
                                         q.CreadorId == creadorId);
            
            if (excludeId.HasValue)
                query = query.Where(q => q.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<Dictionary<int, int>> GetEstadisticasQuizzesByMateriaAsync(string usuarioId)
        {
            return await _dbSet
                .Where(q => q.CreadorId == usuarioId)
                .GroupBy(q => q.MateriaId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesPorDificultadAsync(int materiaId, int tiempoMinimo, int tiempoMaximo)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Where(q => q.MateriaId == materiaId && 
                           q.TiempoPorPregunta >= tiempoMinimo && 
                           q.TiempoPorPregunta <= tiempoMaximo)
                .OrderBy(q => q.TiempoPorPregunta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesSinResultadosAsync(string usuarioId)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Include(q => q.Resultados)
                .Where(q => q.CreadorId == usuarioId && !q.Resultados.Any())
                .OrderByDescending(q => q.FechaCreacion)
                .ToListAsync();
        }

        public async Task<bool> TienePermisosAccesoAsync(int quizId, string usuarioId)
        {
            var quiz = await _dbSet.FirstOrDefaultAsync(q => q.Id == quizId);
            return quiz != null && (quiz.EsPublico || quiz.CreadorId == usuarioId);
        }

        public async Task ActualizarEstadoEnLoteAsync(IEnumerable<int> quizIds, bool esActivo)
        {
            var quizzes = await _dbSet
                .Where(q => quizIds.Contains(q.Id))
                .ToListAsync();

            foreach (var quiz in quizzes)
            {
                quiz.EsActivo = esActivo;
                quiz.FechaModificacion = DateTime.UtcNow;
            }

            _dbSet.UpdateRange(quizzes);
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesRecientesByUsuarioAsync(string usuarioId, int cantidad = 10)
        {
            return await _dbSet
                .Include(q => q.Materia)
                .Include(q => q.Creador)
                .Where(q => q.CreadorId == usuarioId && q.EstaActivo)
                .OrderByDescending(q => q.FechaCreacion)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}