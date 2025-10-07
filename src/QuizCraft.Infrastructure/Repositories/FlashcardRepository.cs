using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio específico para la entidad Flashcard con operaciones especializadas
    /// </summary>
    public class FlashcardRepository : GenericRepository<Flashcard>, IFlashcardRepository
    {
        public FlashcardRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsByMateriaIdAsync(int materiaId)
        {
            return await _dbSet
                .Include(f => f.Materia)
                .Where(f => f.MateriaId == materiaId)
                .OrderBy(f => f.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsByDificultadAsync(int materiaId, NivelDificultad dificultad)
        {
            return await _dbSet
                .Include(f => f.Materia)
                .Where(f => f.MateriaId == materiaId && f.Dificultad == dificultad)
                .OrderBy(f => f.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsAleatoriosAsync(int materiaId, int cantidad)
        {
            // Entity Framework no soporta NEWID() de forma directa, así que obtenemos todos y ordenamos en memoria
            var flashcards = await _dbSet
                .Include(f => f.Materia)  
                .Where(f => f.MateriaId == materiaId)
                .ToListAsync();

            return flashcards
                .OrderBy(x => Guid.NewGuid())
                .Take(cantidad)
                .ToList();
        }

        public async Task<IEnumerable<Flashcard>> BuscarFlashcardsAsync(string termino, int? materiaId = null)
        {
            IQueryable<Flashcard> query = _dbSet.Include(f => f.Materia);

            if (materiaId.HasValue)
                query = query.Where(f => f.MateriaId == materiaId.Value);

            return await query
                .Where(f => f.Pregunta.Contains(termino) || f.Respuesta.Contains(termino))
                .OrderBy(f => f.Pregunta)
                .ToListAsync();
        }

        public async Task<int> GetCantidadByDificultadAsync(int materiaId, NivelDificultad dificultad)
        {
            return await _dbSet
                .CountAsync(f => f.MateriaId == materiaId && f.Dificultad == dificultad);
        }

        public async Task<Dictionary<NivelDificultad, int>> GetEstadisticasDificultadAsync(int materiaId)
        {
            var estadisticas = await _dbSet
                .Where(f => f.MateriaId == materiaId)
                .GroupBy(f => f.Dificultad)
                .Select(g => new { Dificultad = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            var resultado = new Dictionary<NivelDificultad, int>();
            
            // Inicializamos con todas las dificultades en 0
            foreach (var dificultad in Enum.GetValues<NivelDificultad>())
            {
                resultado[dificultad] = 0;
            }

            // Actualizamos con los valores reales
            foreach (var estadistica in estadisticas)
            {
                resultado[estadistica.Dificultad] = estadistica.Cantidad;
            }

            return resultado;
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsRecientesAsync(int materiaId, int cantidad = 10)
        {
            return await _dbSet
                .Include(f => f.Materia)
                .Where(f => f.MateriaId == materiaId)
                .OrderByDescending(f => f.FechaCreacion)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsModificadosAsync(int materiaId, DateTime fechaDesde)
        {
            return await _dbSet
                .Include(f => f.Materia)
                .Where(f => f.MateriaId == materiaId && f.FechaModificacion >= fechaDesde)
                .OrderByDescending(f => f.FechaModificacion)
                .ToListAsync();
        }

        public async Task<bool> ExisteFlashcardSimilarAsync(string pregunta, int materiaId, int? excludeId = null)
        {
            var query = _dbSet.Where(f => f.MateriaId == materiaId && 
                                         f.Pregunta.ToLower() == pregunta.ToLower());
            
            if (excludeId.HasValue)
                query = query.Where(f => f.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsPaginadosAsync(
            int materiaId, 
            int pagina, 
            int tamaño, 
            NivelDificultad? dificultad = null,
            string? terminoBusqueda = null)
        {
            IQueryable<Flashcard> query = _dbSet
                .Include(f => f.Materia)
                .Where(f => f.MateriaId == materiaId);

            if (dificultad.HasValue)
                query = query.Where(f => f.Dificultad == dificultad.Value);

            if (!string.IsNullOrWhiteSpace(terminoBusqueda))
                query = query.Where(f => f.Pregunta.Contains(terminoBusqueda) || 
                                        f.Respuesta.Contains(terminoBusqueda));

            return await query
                .OrderBy(f => f.Pregunta)
                .Skip((pagina - 1) * tamaño)
                .Take(tamaño)
                .ToListAsync();
        }

        public async Task<double> GetPromedioComplejidadAsync(int materiaId)
        {
            var flashcards = await _dbSet
                .Where(f => f.MateriaId == materiaId)
                .Select(f => f.Dificultad)
                .ToListAsync();

            if (!flashcards.Any())
                return 0;

            var suma = flashcards.Sum(d => (int)d);
            return suma / (double)flashcards.Count;
        }

        public async Task ActualizarDificultadesEnLoteAsync(int materiaId, NivelDificultad nuevaDificultad)
        {
            var flashcards = await _dbSet
                .Where(f => f.MateriaId == materiaId)
                .ToListAsync();

            foreach (var flashcard in flashcards)
            {
                flashcard.Dificultad = nuevaDificultad;
                flashcard.FechaModificacion = DateTime.UtcNow;
            }

            _dbSet.UpdateRange(flashcards);
        }

        public async Task<IEnumerable<Flashcard>> GetFlashcardsByUsuarioIdAsync(string usuarioId)
        {
            return await _dbSet
                .Include(f => f.Materia)
                .Where(f => f.Materia.UsuarioId == usuarioId)
                .OrderByDescending(f => f.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Flashcard?> GetByIdWithMateriaAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Materia)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        /// <summary>
        /// Obtiene flashcards que necesitan repaso para un usuario específico
        /// </summary>
        public async Task<IEnumerable<Flashcard>> GetFlashcardsParaRepasoAsync(string usuarioId, int? materiaId = null)
        {
            var hoy = DateTime.Today;
            
            IQueryable<Flashcard> query = _dbSet
                .Include(f => f.Materia)
                .Where(f => f.Materia.UsuarioId == usuarioId);

            if (materiaId.HasValue)
                query = query.Where(f => f.MateriaId == materiaId.Value);

            return await query
                .Where(f => f.ProximaRevision == null || f.ProximaRevision <= hoy)
                .OrderBy(f => f.ProximaRevision ?? DateTime.MinValue)
                .ThenBy(f => f.UltimaRevision ?? DateTime.MinValue)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene flashcards que necesitan repaso para una materia específica
        /// </summary>
        public async Task<IEnumerable<Flashcard>> GetFlashcardsParaRepasoByMateriaAsync(int materiaId)
        {
            var hoy = DateTime.Today;
            
            return await _dbSet
                .Include(f => f.Materia)
                .Where(f => f.MateriaId == materiaId && 
                           (f.ProximaRevision == null || f.ProximaRevision <= hoy))
                .OrderBy(f => f.ProximaRevision ?? DateTime.MinValue)
                .ThenBy(f => f.UltimaRevision ?? DateTime.MinValue)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la cantidad de flashcards que necesitan repaso
        /// </summary>
        public async Task<int> GetCantidadFlashcardsParaRepasoAsync(string usuarioId, int? materiaId = null)
        {
            var hoy = DateTime.Today;
            
            IQueryable<Flashcard> query = _dbSet
                .Include(f => f.Materia)
                .Where(f => f.Materia.UsuarioId == usuarioId);

            if (materiaId.HasValue)
                query = query.Where(f => f.MateriaId == materiaId.Value);

            return await query
                .CountAsync(f => f.ProximaRevision == null || f.ProximaRevision <= hoy);
        }

        /// <summary>
        /// Actualiza las estadísticas de repaso de una flashcard
        /// </summary>
        public async Task ActualizarEstadisticasRepasoAsync(int flashcardId, bool esCorrecta, TimeSpan tiempoRespuesta)
        {
            var flashcard = await _dbSet.FindAsync(flashcardId);
            if (flashcard == null) return;

            flashcard.VecesVista++;
            if (esCorrecta)
                flashcard.VecesCorrecta++;
            else
                flashcard.VecesIncorrecta++;

            flashcard.UltimaRevision = DateTime.UtcNow;
            flashcard.FechaModificacion = DateTime.UtcNow;

            _dbSet.Update(flashcard);
        }
    }
}