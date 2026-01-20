// Auto-generated code
namespace FlightSim.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for FlightPlanId.
/// </summary>
public sealed class FlightPlanId : StronglyTypedId<FlightPlanId>
{
    public FlightPlanId(Guid value) : base(value)
    {
    }

    public static FlightPlanId New() => new(Guid.NewGuid());

    public static FlightPlanId From(Guid value) => new(value);

    public static FlightPlanId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out FlightPlanId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new FlightPlanId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
