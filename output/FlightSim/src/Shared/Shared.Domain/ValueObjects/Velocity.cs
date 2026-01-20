// Auto-generated code
namespace FlightSim.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for Velocity.
/// </summary>
public sealed class Velocity : ValueObject
{
    public double Speed { get; }
    public double Heading { get; }
    public double VerticalSpeed { get; }

    public Velocity(double speed, double heading, double verticalSpeed)
    {
        Speed = speed;
        Heading = heading;
        VerticalSpeed = verticalSpeed;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Speed,
            Heading,
            VerticalSpeed
        };
    }
}
