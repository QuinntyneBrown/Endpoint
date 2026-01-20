// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds.Packets;

/// <summary>
/// Primary telemetry packet for aircraft state
/// </summary>
public class AircraftTelemetryPacket : CcsdsPacket
{
    public override int Apid => 100;

    public override int PacketType => 0;

    /// <summary>
    /// Unique aircraft identifier
    /// </summary>
    public uint AircraftId { get; set; }

    /// <summary>
    /// Latitude in micro-degrees
    /// </summary>
    public int Latitude { get; set; }

    /// <summary>
    /// Longitude in micro-degrees
    /// </summary>
    public int Longitude { get; set; }

    /// <summary>
    /// Altitude in feet
    /// </summary>
    public uint Altitude { get; set; }

    /// <summary>
    /// Heading in 0.01 degrees
    /// </summary>
    public ushort Heading { get; set; }

    /// <summary>
    /// Ground speed in 0.1 knots
    /// </summary>
    public ushort Speed { get; set; }

    /// <summary>
    /// Vertical speed in feet/min
    /// </summary>
    public short VerticalSpeed { get; set; }

    /// <summary>
    /// Aircraft status bit flags
    /// </summary>
    public byte StatusFlags { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt32(AircraftId);
        packer.PackInt32(Latitude);
        packer.PackInt32(Longitude);
        packer.PackUInt32(Altitude);
        packer.PackUInt16(Heading);
        packer.PackUInt16(Speed);
        packer.PackInt16(VerticalSpeed);
        packer.PackUnsigned(StatusFlags, 8);
        packer.PackSpare(8); // spare
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        AircraftId = unpacker.UnpackUInt32();
        Latitude = unpacker.UnpackInt32();
        Longitude = unpacker.UnpackInt32();
        Altitude = unpacker.UnpackUInt32();
        Heading = unpacker.UnpackUInt16();
        Speed = unpacker.UnpackUInt16();
        VerticalSpeed = unpacker.UnpackInt16();
        StatusFlags = (byte)unpacker.UnpackUnsigned(8);
        unpacker.SkipBits(8); // spare
    }
}
