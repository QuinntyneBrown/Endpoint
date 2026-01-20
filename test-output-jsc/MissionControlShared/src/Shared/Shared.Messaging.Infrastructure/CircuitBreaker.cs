// Auto-generated code
using Microsoft.Extensions.Logging;

namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Circuit breaker states.
/// </summary>
public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

/// <summary>
/// Circuit breaker configuration.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>Number of failures before opening the circuit.</summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>Duration to keep the circuit open.</summary>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Number of successful calls to close the circuit from half-open state.</summary>
    public int SuccessThreshold { get; set; } = 2;
}

/// <summary>
/// Implements the circuit breaker pattern for fault tolerance.
/// </summary>
public class CircuitBreaker
{
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly CircuitBreakerOptions _options;
    private readonly object _lock = new();

    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private int _successCount;
    private DateTime _openedAt;

    public CircuitBreaker(ILogger<CircuitBreaker> logger, CircuitBreakerOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new CircuitBreakerOptions();
    }

    /// <summary>Gets the current circuit state.</summary>
    public CircuitState State
    {
        get
        {
            lock (_lock)
            {
                if (_state == CircuitState.Open &&
                    DateTime.UtcNow - _openedAt >= _options.OpenDuration)
                {
                    _state = CircuitState.HalfOpen;
                    _successCount = 0;
                    _logger.LogInformation("Circuit breaker transitioned to HalfOpen");
                }
                return _state;
            }
        }
    }

    /// <summary>
    /// Executes an operation through the circuit breaker.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (State == CircuitState.Open)
        {
            throw new CircuitBreakerOpenException("Circuit breaker is open");
        }

        try
        {
            var result = await operation(cancellationToken);
            OnSuccess();
            return result;
        }
        catch (Exception ex) when (ex is not CircuitBreakerOpenException)
        {
            OnFailure();
            throw;
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitState.HalfOpen)
            {
                _successCount++;
                if (_successCount >= _options.SuccessThreshold)
                {
                    _state = CircuitState.Closed;
                    _failureCount = 0;
                    _logger.LogInformation("Circuit breaker closed");
                }
            }
            else
            {
                _failureCount = 0;
            }
        }
    }

    private void OnFailure()
    {
        lock (_lock)
        {
            _failureCount++;
            if (_state == CircuitState.HalfOpen || _failureCount >= _options.FailureThreshold)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                _logger.LogWarning("Circuit breaker opened after {Failures} failures", _failureCount);
            }
        }
    }

    /// <summary>
    /// Forces the circuit breaker to reset to closed state.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _state = CircuitState.Closed;
            _failureCount = 0;
            _successCount = 0;
            _logger.LogInformation("Circuit breaker manually reset");
        }
    }
}

/// <summary>
/// Exception thrown when the circuit breaker is open.
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}
