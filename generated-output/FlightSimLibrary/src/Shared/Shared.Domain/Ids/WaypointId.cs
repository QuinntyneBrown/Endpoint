// Auto-generated code
namespace FlightSim.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for WaypointId.
/// </summary>
public sealed class WaypointId : StronglyTypedId<WaypointId>
{
    public WaypointId(Guid value) : base(value)
    {
    }

    public static WaypointId New() => new(Guid.NewGuid());

    public static WaypointId From(Guid value) => new(value);

    public static WaypointId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out WaypointId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new WaypointId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
