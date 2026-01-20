// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Configures retry behavior for operations.
/// </summary>
public class RetryPolicy
{
    /// <summary>Maximum number of retry attempts.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Initial delay between retries.</summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Maximum delay between retries.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Multiplier for exponential backoff.</summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>Whether to add jitter to delays.</summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>Exception types that should trigger a retry.</summary>
    public List<Type> RetryableExceptions { get; set; } = new()
    {
        typeof(TimeoutException),
        typeof(TaskCanceledException)
    };

    /// <summary>Exception types that should not trigger a retry.</summary>
    public List<Type> NonRetryableExceptions { get; set; } = new()
    {
        typeof(ArgumentException),
        typeof(InvalidOperationException)
    };

    /// <summary>
    /// Calculates the delay for the given retry attempt.
    /// </summary>
    public TimeSpan CalculateDelay(int attempt)
    {
        var delay = TimeSpan.FromMilliseconds(
            InitialDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attempt));

        if (delay > MaxDelay)
        {
            delay = MaxDelay;
        }

        if (UseJitter)
        {
            var jitter = Random.Shared.NextDouble() * 0.3; // Up to 30% jitter
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));
        }

        return delay;
    }

    /// <summary>
    /// Determines whether the exception should trigger a retry.
    /// </summary>
    public bool ShouldRetry(Exception exception, int attempt)
    {
        if (attempt >= MaxRetries)
        {
            return false;
        }

        var exceptionType = exception.GetType();

        if (NonRetryableExceptions.Any(t => t.IsAssignableFrom(exceptionType)))
        {
            return false;
        }

        if (RetryableExceptions.Count == 0)
        {
            return true; // Retry all exceptions if no specific list
        }

        return RetryableExceptions.Any(t => t.IsAssignableFrom(exceptionType));
    }

    /// <summary>
    /// Creates a default retry policy.
    /// </summary>
    public static RetryPolicy Default => new();

    /// <summary>
    /// Creates a retry policy with no retries.
    /// </summary>
    public static RetryPolicy NoRetry => new() { MaxRetries = 0 };
}
