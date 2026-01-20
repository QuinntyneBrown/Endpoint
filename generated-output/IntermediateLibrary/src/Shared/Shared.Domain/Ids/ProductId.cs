// Auto-generated code
namespace ECommerce.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for ProductId.
/// </summary>
public sealed class ProductId : StronglyTypedId<ProductId>
{
    public ProductId(Guid value) : base(value)
    {
    }

    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId From(Guid value) => new(value);

    public static ProductId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out ProductId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new ProductId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
