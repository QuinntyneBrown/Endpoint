// Auto-generated code
using MessagePack;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Contracts.Customers;

/// <summary>
/// Fired when a new customer registers
/// </summary>
[MessagePackObject]
public sealed class CustomerRegistered : EventBase
{
    [Key(0)]
    public Guid CustomerId { get; init; }

    [Key(1)]
    public string Email { get; init; } = string.Empty;

    [Key(2)]
    public string Name { get; init; } = string.Empty;

    [Key(3)]
    public DateTimeOffset RegisteredAt { get; init; }

}
