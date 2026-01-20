// Auto-generated code
using System.Text.Json;

namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Standard message headers.
/// </summary>
public class MessageHeaders
{
    /// <summary>Unique message identifier.</summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Correlation ID for request/response tracking.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Causation ID linking to the causing message.</summary>
    public string? CausationId { get; set; }

    /// <summary>Message timestamp.</summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Source service/component.</summary>
    public string? Source { get; set; }

    /// <summary>Message type name.</summary>
    public string? MessageType { get; set; }

    /// <summary>Content type (e.g., application/json).</summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>Schema version.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Custom headers.</summary>
    public Dictionary<string, string> Custom { get; set; } = new();
}

/// <summary>
/// Message envelope wrapping a typed payload.
/// </summary>
public class MessageEnvelope<TPayload>
{
    /// <summary>Message headers.</summary>
    public MessageHeaders Headers { get; set; } = new();

    /// <summary>Message payload.</summary>
    public TPayload? Payload { get; set; }

    /// <summary>
    /// Creates a new envelope for the payload.
    /// </summary>
    public static MessageEnvelope<TPayload> Create(TPayload payload, string? source = null)
    {
        return new MessageEnvelope<TPayload>
        {
            Headers = new MessageHeaders
            {
                Source = source,
                MessageType = typeof(TPayload).Name
            },
            Payload = payload
        };
    }

    /// <summary>
    /// Creates a reply envelope linked to this message.
    /// </summary>
    public MessageEnvelope<TReply> CreateReply<TReply>(TReply payload, string? source = null)
    {
        return new MessageEnvelope<TReply>
        {
            Headers = new MessageHeaders
            {
                CorrelationId = Headers.CorrelationId ?? Headers.MessageId,
                CausationId = Headers.MessageId,
                Source = source,
                MessageType = typeof(TReply).Name
            },
            Payload = payload
        };
    }

    /// <summary>Serializes the envelope to JSON.</summary>
    public string ToJson() => JsonSerializationHelper.Serialize(this);

    /// <summary>Deserializes an envelope from JSON.</summary>
    public static MessageEnvelope<TPayload>? FromJson(string json)
        => JsonSerializationHelper.Deserialize<MessageEnvelope<TPayload>>(json);
}

/// <summary>
/// Non-generic message envelope for dynamic payloads.
/// </summary>
public class MessageEnvelope
{
    /// <summary>Message headers.</summary>
    public MessageHeaders Headers { get; set; } = new();

    /// <summary>Message payload as JsonElement.</summary>
    public JsonElement Payload { get; set; }

    /// <summary>
    /// Gets the payload as a typed object.
    /// </summary>
    public T? GetPayload<T>()
    {
        return JsonSerializer.Deserialize<T>(Payload.GetRawText());
    }
}
