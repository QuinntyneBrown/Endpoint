// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Represents trace context for distributed tracing.
/// </summary>
public class TraceContext
{
    /// <summary>Unique trace identifier spanning multiple services.</summary>
    public string TraceId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Current span identifier.</summary>
    public string SpanId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 16);

    /// <summary>Parent span identifier.</summary>
    public string? ParentSpanId { get; set; }

    /// <summary>Sampling flag.</summary>
    public bool IsSampled { get; set; } = true;

    /// <summary>Baggage items (key-value pairs propagated across services).</summary>
    public Dictionary<string, string> Baggage { get; set; } = new();

    /// <summary>
    /// Creates a child span context.
    /// </summary>
    public TraceContext CreateChild()
    {
        return new TraceContext
        {
            TraceId = TraceId,
            ParentSpanId = SpanId,
            SpanId = Guid.NewGuid().ToString("N").Substring(0, 16),
            IsSampled = IsSampled,
            Baggage = new Dictionary<string, string>(Baggage)
        };
    }

    /// <summary>
    /// Creates a new root trace context.
    /// </summary>
    public static TraceContext CreateRoot() => new();
}

/// <summary>
/// Interface for distributed tracing operations.
/// </summary>
public interface ITracer
{
    /// <summary>Gets the current trace context.</summary>
    TraceContext? Current { get; }

    /// <summary>Starts a new span.</summary>
    IDisposable StartSpan(string operationName, TraceContext? parent = null);

    /// <summary>Adds a tag to the current span.</summary>
    void AddTag(string key, string value);

    /// <summary>Logs an event in the current span.</summary>
    void LogEvent(string message);
}

/// <summary>
/// No-op tracer implementation.
/// </summary>
public class NoOpTracer : ITracer
{
    public TraceContext? Current => null;

    public IDisposable StartSpan(string operationName, TraceContext? parent = null)
        => new NoOpSpan();

    public void AddTag(string key, string value) { }

    public void LogEvent(string message) { }

    private class NoOpSpan : IDisposable
    {
        public void Dispose() { }
    }
}
