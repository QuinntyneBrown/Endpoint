// Auto-generated code
using MessagePack;

namespace ECommerce.Shared.Contracts.Orders;

/// <summary>
/// Command to create a new order
/// </summary>
[MessagePackObject]
public sealed class CreateOrder
{
    [Key(0)]
    public Guid CustomerId { get; init; }

    [Key(1)]
    public List<Guid> Items { get; init; } = new();

    [Key(2)]
    public string ShippingAddress { get; init; } = string.Empty;

}
