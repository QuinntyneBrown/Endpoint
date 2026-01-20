// Auto-generated code
using System.Linq.Expressions;

namespace MCC.Shared.Shared.Messaging.Infrastructure.Interfaces;

/// <summary>
/// Read-only repository interface.
/// </summary>
public interface IReadOnlyRepository<TEntity, TId>
    where TEntity : class
{
    /// <summary>Gets an entity by ID.</summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities.</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Finds entities matching a predicate.</summary>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Gets the first entity matching a predicate or null.</summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Checks if any entity matches the predicate.</summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>Counts entities matching the predicate.</summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Full repository interface with write operations.
/// </summary>
public interface IRepository<TEntity, TId> : IReadOnlyRepository<TEntity, TId>
    where TEntity : class
{
    /// <summary>Adds an entity.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Adds multiple entities.</summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Updates an entity.</summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Updates multiple entities.</summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity by ID.</summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity.</summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes multiple entities.</summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience repository interface with Guid ID.
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : class
{
}
