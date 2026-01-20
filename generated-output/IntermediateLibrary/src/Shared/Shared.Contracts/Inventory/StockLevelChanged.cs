// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Inventory;

/// <summary>
/// Fired when stock level changes
/// </summary>
[MessagePackObject]
public sealed class StockLevelChanged : EventBase
{
    [Key(0)]
    public Guid ProductId { get; init; }

    [Key(1)]
    public int PreviousQuantity { get; init; }

    [Key(2)]
    public int NewQuantity { get; init; }

    [Key(3)]
    public string Reason { get; init; } = string.Empty;

}
