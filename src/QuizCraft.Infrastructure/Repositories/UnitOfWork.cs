using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;
using QuizCraft.Infrastructure.Repositories;

namespace QuizCraft.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del patrón Unit of Work para coordinar las operaciones de repositorio
    /// y garantizar la consistencia transaccional
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        // Repositorios lazy-loaded
        private IMateriaRepository? _materiaRepository;
        private IFlashcardRepository? _flashcardRepository;
        private IQuizRepository? _quizRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IMateriaRepository MateriaRepository
        {
            get
            {
                _materiaRepository ??= new MateriaRepository(_context);
                return _materiaRepository;
            }
        }

        public IFlashcardRepository FlashcardRepository
        {
            get
            {
                _flashcardRepository ??= new FlashcardRepository(_context);
                return _flashcardRepository;
            }
        }

        public IQuizRepository QuizRepository
        {
            get
            {
                _quizRepository ??= new QuizRepository(_context);
                return _quizRepository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log del error (implementar logging más adelante)
                throw new Exception($"Error al guardar cambios en la base de datos: {ex.Message}", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction == null)
            {
                await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await _context.Database.CurrentTransaction.CommitAsync();
                await _context.Database.CurrentTransaction.DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await _context.Database.CurrentTransaction.RollbackAsync();
                await _context.Database.CurrentTransaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _context.Dispose();
                }
                catch
                {
                    // Ignorar errores al disponer contexto
                }

                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}