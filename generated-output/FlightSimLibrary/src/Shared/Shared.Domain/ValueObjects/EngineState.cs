// Auto-generated code
namespace FlightSim.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for EngineState.
/// </summary>
public sealed class EngineState : ValueObject
{
    public bool IsRunning { get; }
    public double Rpm { get; }
    public double Power { get; }

    public EngineState(bool isRunning, double rpm, double power)
    {
        IsRunning = isRunning;
        Rpm = rpm;
        Power = power;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            IsRunning,
            Rpm,
            Power
        };
    }
}
