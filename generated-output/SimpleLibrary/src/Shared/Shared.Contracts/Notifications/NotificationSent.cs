// Auto-generated code
using MessagePack;
using Simple.Shared.Messaging.Abstractions;

namespace Simple.Shared.Contracts.Notifications;

/// <summary>
/// Fired when a notification is sent
/// </summary>
[MessagePackObject]
public sealed class NotificationSent : EventBase
{
    [Key(0)]
    public Guid NotificationId { get; init; }

    [Key(1)]
    public string Message { get; init; } = string.Empty;

    [Key(2)]
    public string Recipient { get; init; } = string.Empty;

    [Key(3)]
    public DateTimeOffset SentAt { get; init; }

}
