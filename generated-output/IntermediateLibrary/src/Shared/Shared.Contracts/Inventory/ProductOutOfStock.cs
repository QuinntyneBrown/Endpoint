// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Inventory;

/// <summary>
/// Fired when a product goes out of stock
/// </summary>
[MessagePackObject]
public sealed class ProductOutOfStock : EventBase
{
    [Key(0)]
    public Guid ProductId { get; init; }

    [Key(1)]
    public DateTimeOffset LastAvailableAt { get; init; }

}
