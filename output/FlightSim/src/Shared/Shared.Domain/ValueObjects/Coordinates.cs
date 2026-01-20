// Auto-generated code
namespace FlightSim.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for Coordinates.
/// </summary>
public sealed class Coordinates : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Latitude,
            Longitude
        };
    }
}
