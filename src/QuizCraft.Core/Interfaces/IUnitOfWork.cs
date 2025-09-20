namespace QuizCraft.Core.Interfaces;

/// <summary>
/// Interfaz para el patrón Unit of Work que coordina operaciones de repositorio
/// y garantiza la consistencia transaccional
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IMateriaRepository MateriaRepository { get; }
    IFlashcardRepository FlashcardRepository { get; }
    IQuizRepository QuizRepository { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}