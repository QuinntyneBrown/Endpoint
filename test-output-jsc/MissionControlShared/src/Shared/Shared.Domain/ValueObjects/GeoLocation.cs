// Auto-generated code
namespace MCC.Shared.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for GeoLocation.
/// </summary>
public sealed class GeoLocation : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }
    public double Altitude { get; }

    public GeoLocation(double latitude, double longitude, double altitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Latitude,
            Longitude,
            Altitude
        };
    }
}
