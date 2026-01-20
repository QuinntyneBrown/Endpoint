// Auto-generated code
using MessagePack;

namespace ECommerce.Shared.Contracts.Inventory;

[MessagePackObject]
public sealed class AdjustStock
{
    [Key(0)]
    public Guid ProductId { get; init; }

    [Key(1)]
    public int Adjustment { get; init; }

    [Key(2)]
    public string Reason { get; init; } = string.Empty;

}
