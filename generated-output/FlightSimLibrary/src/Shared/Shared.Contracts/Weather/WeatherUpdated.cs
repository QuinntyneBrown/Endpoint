// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.Weather;

/// <summary>
/// Fired when weather data is updated
/// </summary>
[MessagePackObject]
public sealed class WeatherUpdated : EventBase
{
    [Key(0)]
    public string StationId { get; init; } = string.Empty;

    [Key(1)]
    public double Latitude { get; init; }

    [Key(2)]
    public double Longitude { get; init; }

    [Key(3)]
    public double Temperature { get; init; }

    [Key(4)]
    public double DewPoint { get; init; }

    [Key(5)]
    public int WindDirection { get; init; }

    [Key(6)]
    public int WindSpeed { get; init; }

    [Key(7)]
    public double Visibility { get; init; }

    [Key(8)]
    public int CloudCeiling { get; init; }

    [Key(9)]
    public double Altimeter { get; init; }

    [Key(10)]
    public DateTimeOffset ObservationTime { get; init; }

}
