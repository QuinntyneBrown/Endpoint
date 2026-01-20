// Auto-generated code
namespace FlightSim.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for GeoCoordinate.
/// </summary>
public sealed class GeoCoordinate : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    public GeoCoordinate(double latitude, double longitude)
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
