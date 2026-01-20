// Auto-generated code
using Microsoft.Extensions.Logging;

namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Represents a failed message in the dead letter queue.
/// </summary>
public class DeadLetterMessage
{
    /// <summary>Unique identifier for this dead letter entry.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Original message content.</summary>
    public byte[] MessageContent { get; set; } = Array.Empty<byte>();

    /// <summary>Message type name.</summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>Reason for failure.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Exception details if available.</summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>When the message was dead-lettered.</summary>
    public DateTimeOffset DeadLetteredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Number of processing attempts.</summary>
    public int AttemptCount { get; set; }

    /// <summary>Original correlation ID.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Additional metadata.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Interface for dead letter queue operations.
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>Adds a message to the dead letter queue.</summary>
    Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);

    /// <summary>Gets messages from the dead letter queue.</summary>
    Task<IReadOnlyList<DeadLetterMessage>> GetMessagesAsync(int maxCount = 100, CancellationToken cancellationToken = default);

    /// <summary>Removes a message from the dead letter queue.</summary>
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Attempts to reprocess a dead letter message.</summary>
    Task<bool> ReprocessAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation of dead letter queue (for development/testing).
/// </summary>
public class InMemoryDeadLetterQueue : IDeadLetterQueue
{
    private readonly ILogger<InMemoryDeadLetterQueue> _logger;
    private readonly List<DeadLetterMessage> _messages = new();
    private readonly object _lock = new();

    public InMemoryDeadLetterQueue(ILogger<InMemoryDeadLetterQueue> logger)
    {
        _logger = logger;
    }

    public Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _messages.Add(message);
            _logger.LogWarning("Message dead-lettered: {Reason}", message.Reason);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DeadLetterMessage>> GetMessagesAsync(int maxCount = 100, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<DeadLetterMessage>>(
                _messages.Take(maxCount).ToList());
        }
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _messages.RemoveAll(m => m.Id == id);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ReprocessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would resubmit to the message bus
        _logger.LogInformation("Reprocess requested for message {Id}", id);
        return Task.FromResult(false);
    }
}
