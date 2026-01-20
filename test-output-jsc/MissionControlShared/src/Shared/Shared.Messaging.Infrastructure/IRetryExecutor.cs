// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Executes operations with retry logic.
/// </summary>
public interface IRetryExecutor
{
    /// <summary>
    /// Executes an async operation with retry logic.
    /// </summary>
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an async operation with retry logic (no return value).
    /// </summary>
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default);
}
