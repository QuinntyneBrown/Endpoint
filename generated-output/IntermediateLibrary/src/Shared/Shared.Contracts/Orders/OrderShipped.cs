// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Orders;

/// <summary>
/// Fired when an order is shipped
/// </summary>
[MessagePackObject]
public sealed class OrderShipped : EventBase
{
    [Key(0)]
    public Guid OrderId { get; init; }

    [Key(1)]
    public DateTimeOffset ShippedAt { get; init; }

    [Key(2)]
    public string TrackingNumber { get; init; } = string.Empty;

    [Key(3)]
    public string Carrier { get; init; } = string.Empty;

}
