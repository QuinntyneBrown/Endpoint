// Auto-generated code
namespace ECommerce.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for CustomerId.
/// </summary>
public sealed class CustomerId : StronglyTypedId<CustomerId>
{
    public CustomerId(Guid value) : base(value)
    {
    }

    public static CustomerId New() => new(Guid.NewGuid());

    public static CustomerId From(Guid value) => new(value);

    public static CustomerId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out CustomerId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new CustomerId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
