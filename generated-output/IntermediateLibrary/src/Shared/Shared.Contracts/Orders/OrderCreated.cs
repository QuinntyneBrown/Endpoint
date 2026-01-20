// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Orders;

/// <summary>
/// Fired when a new order is created
/// </summary>
[MessagePackObject]
public sealed class OrderCreated : EventBase
{
    [Key(0)]
    public Guid OrderId { get; init; }

    [Key(1)]
    public Guid CustomerId { get; init; }

    [Key(2)]
    public DateTimeOffset OrderDate { get; init; }

    [Key(3)]
    public decimal TotalAmount { get; init; }

    [Key(4)]
    public string Currency { get; init; } = "USD";

}
