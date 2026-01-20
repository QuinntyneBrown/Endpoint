// Auto-generated code
using MessagePack;
using Simple.Shared.Messaging.Abstractions;

namespace Simple.Shared.Contracts.Notifications;

/// <summary>
/// Fired when a notification is read
/// </summary>
[MessagePackObject]
public sealed class NotificationRead : EventBase
{
    [Key(0)]
    public Guid NotificationId { get; init; }

    [Key(1)]
    public DateTimeOffset ReadAt { get; init; }

}
