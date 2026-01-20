// Auto-generated code
namespace ECommerce.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for Money.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Amount,
            Currency
        };
    }
}
