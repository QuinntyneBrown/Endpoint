// Auto-generated code
namespace MCC.Shared.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for SpacecraftId.
/// </summary>
public sealed class SpacecraftId : StronglyTypedId<SpacecraftId>
{
    public SpacecraftId(Guid value) : base(value)
    {
    }

    public static SpacecraftId New() => new(Guid.NewGuid());

    public static SpacecraftId From(Guid value) => new(value);

    public static SpacecraftId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out SpacecraftId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new SpacecraftId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
