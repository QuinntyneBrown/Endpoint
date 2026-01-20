// Auto-generated code
namespace MCC.Shared.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for MissionId.
/// </summary>
public sealed class MissionId : StronglyTypedId<MissionId>
{
    public MissionId(Guid value) : base(value)
    {
    }

    public static MissionId New() => new(Guid.NewGuid());

    public static MissionId From(Guid value) => new(value);

    public static MissionId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out MissionId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new MissionId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
