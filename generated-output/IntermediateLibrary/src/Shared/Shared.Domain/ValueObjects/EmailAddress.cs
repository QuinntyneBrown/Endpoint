// Auto-generated code
namespace ECommerce.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for EmailAddress.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Value
        };
    }
}
