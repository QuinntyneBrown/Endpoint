// Auto-generated code
namespace FlightSim.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for PilotId.
/// </summary>
public sealed class PilotId : StronglyTypedId<PilotId>
{
    public PilotId(Guid value) : base(value)
    {
    }

    public static PilotId New() => new(Guid.NewGuid());

    public static PilotId From(Guid value) => new(value);

    public static PilotId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out PilotId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new PilotId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
