// Auto-generated code
namespace ECommerce.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for Address.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Street,
            City,
            State,
            ZipCode,
            Country
        };
    }
}
