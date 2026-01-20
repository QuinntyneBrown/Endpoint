// Auto-generated code
namespace ECommerce.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for OrderId.
/// </summary>
public sealed class OrderId : StronglyTypedId<OrderId>
{
    public OrderId(Guid value) : base(value)
    {
    }

    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId From(Guid value) => new(value);

    public static OrderId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out OrderId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new OrderId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
