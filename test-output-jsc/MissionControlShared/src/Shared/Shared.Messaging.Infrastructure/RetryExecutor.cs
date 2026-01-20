// Auto-generated code
using Microsoft.Extensions.Logging;

namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Default implementation of retry executor.
/// </summary>
public class RetryExecutor : IRetryExecutor
{
    private readonly ILogger<RetryExecutor> _logger;

    public RetryExecutor(ILogger<RetryExecutor> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        policy ??= RetryPolicy.Default;
        var attempt = 0;
        var exceptions = new List<Exception>();

        while (true)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                exceptions.Add(ex);

                if (!policy.ShouldRetry(ex, attempt))
                {
                    _logger.LogError(ex, "Operation failed after {Attempts} attempts", attempt + 1);
                    throw new AggregateException("All retry attempts failed", exceptions);
                }

                var delay = policy.CalculateDelay(attempt);
                _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying in {Delay}ms",
                    attempt + 1, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
                attempt++;
            }
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async ct =>
        {
            await operation(ct);
            return true;
        }, policy, cancellationToken);
    }
}
