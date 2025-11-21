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
    IQuizCompartidoRepository QuizCompartidoRepository { get; }
    IFlashcardCompartidaRepository FlashcardCompartidaRepository { get; }
    IEstadisticaEstudioRepository EstadisticaEstudioRepository { get; }
    IResultadoQuizRepository ResultadoQuizRepository { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
}