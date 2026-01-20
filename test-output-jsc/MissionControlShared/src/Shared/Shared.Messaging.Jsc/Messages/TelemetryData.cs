// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc.Messages;

/// <summary>
/// Real-time telemetry from spacecraft
/// </summary>
public class TelemetryData
{
    /// <summary>Message type code.</summary>
    public const byte TypeCode = 0x10;

    /// <summary>Temperature reading</summary>
    public ushort Temperature { get; set; }

    /// <summary>Pressure reading</summary>
    public uint Pressure { get; set; }

    /// <summary>Status flags</summary>
    public byte StatusFlags { get; set; }

}
