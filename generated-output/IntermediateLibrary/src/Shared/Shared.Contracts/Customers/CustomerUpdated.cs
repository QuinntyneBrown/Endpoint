// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Customers;

/// <summary>
/// Fired when customer info is updated
/// </summary>
[MessagePackObject]
public sealed class CustomerUpdated : EventBase
{
    [Key(0)]
    public Guid CustomerId { get; init; }

    [Key(1)]
    public List<string> UpdatedFields { get; init; } = new();

}
