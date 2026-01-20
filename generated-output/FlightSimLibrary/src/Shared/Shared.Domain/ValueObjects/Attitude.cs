// Auto-generated code
namespace FlightSim.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for Attitude.
/// </summary>
public sealed class Attitude : ValueObject
{
    public double Pitch { get; }
    public double Roll { get; }
    public double Heading { get; }

    public Attitude(double pitch, double roll, double heading)
    {
        Pitch = pitch;
        Roll = roll;
        Heading = heading;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Pitch,
            Roll,
            Heading
        };
    }
}
