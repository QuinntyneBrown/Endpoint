// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Orders;

/// <summary>
/// Fired when an order is cancelled
/// </summary>
[MessagePackObject]
public sealed class OrderCancelled : EventBase
{
    [Key(0)]
    public Guid OrderId { get; init; }

    [Key(1)]
    public DateTimeOffset CancelledAt { get; init; }

    [Key(2)]
    public string Reason { get; init; } = string.Empty;

}
