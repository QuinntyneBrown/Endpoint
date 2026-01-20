// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure.Interfaces;

/// <summary>
/// Unit of work interface for coordinating transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Saves all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Begins a new transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current transaction.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
