// Auto-generated code
namespace FlightSim.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for Velocity.
/// </summary>
public sealed class Velocity : ValueObject
{
    public double GroundSpeed { get; }
    public double TrueAirspeed { get; }
    public double IndicatedAirspeed { get; }
    public double VerticalSpeed { get; }

    public Velocity(double groundSpeed, double trueAirspeed, double indicatedAirspeed, double verticalSpeed)
    {
        GroundSpeed = groundSpeed;
        TrueAirspeed = trueAirspeed;
        IndicatedAirspeed = indicatedAirspeed;
        VerticalSpeed = verticalSpeed;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            GroundSpeed,
            TrueAirspeed,
            IndicatedAirspeed,
            VerticalSpeed
        };
    }
}
