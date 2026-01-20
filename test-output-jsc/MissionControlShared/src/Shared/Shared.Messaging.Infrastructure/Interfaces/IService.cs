// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure.Interfaces;

/// <summary>
/// Base service interface for CRUD operations.
/// </summary>
public interface IService<TEntity, TDto, TId>
    where TEntity : class
    where TDto : class
{
    /// <summary>Gets an entity by ID.</summary>
    Task<TDto?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities.</summary>
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a new entity.</summary>
    Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity.</summary>
    Task<TDto> UpdateAsync(TId id, TDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity.</summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Checks if an entity exists.</summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Paged result for paginated queries.
/// </summary>
public class PagedResult<T>
{
    /// <summary>The items in this page.</summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber { get; set; }

    /// <summary>Page size.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether there is a next page.</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>Whether there is a previous page.</summary>
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Request for paged data.
/// </summary>
public class PagedRequest
{
    /// <summary>Page number (1-based).</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Page size.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Sort field name.</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort direction.</summary>
    public bool SortDescending { get; set; }

    /// <summary>Search/filter term.</summary>
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Service interface with paging support.
/// </summary>
public interface IPagedService<TDto>
    where TDto : class
{
    /// <summary>Gets a paged result.</summary>
    Task<PagedResult<TDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
