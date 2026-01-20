// Auto-generated code
namespace FlightSim.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for AircraftId.
/// </summary>
public sealed class AircraftId : StronglyTypedId<AircraftId>
{
    public AircraftId(Guid value) : base(value)
    {
    }

    public static AircraftId New() => new(Guid.NewGuid());

    public static AircraftId From(Guid value) => new(value);

    public static AircraftId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out AircraftId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new AircraftId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
