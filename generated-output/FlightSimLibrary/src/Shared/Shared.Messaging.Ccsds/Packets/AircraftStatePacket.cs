// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds.Packets;

/// <summary>
/// Primary aircraft state telemetry packet (high frequency)
/// </summary>
public class AircraftStatePacket : CcsdsPacket
{
    public override int Apid => 100;

    public override int PacketType => 0;

    /// <summary>
    /// Unique aircraft identifier
    /// </summary>
    public uint AircraftId { get; set; }

    /// <summary>
    /// Latitude in 10^-7 degrees
    /// Unit: degrees
    /// </summary>
    public int Latitude { get; set; }

    /// <summary>
    /// Longitude in 10^-7 degrees
    /// Unit: degrees
    /// </summary>
    public int Longitude { get; set; }

    /// <summary>
    /// Altitude MSL in feet
    /// Unit: feet
    /// </summary>
    public uint AltitudeMsl { get; set; }

    /// <summary>
    /// Altitude AGL in feet
    /// Unit: feet
    /// </summary>
    public ushort AltitudeAgl { get; set; }

    /// <summary>
    /// Heading in 0.01 degrees
    /// Unit: degrees
    /// </summary>
    public ushort Heading { get; set; }

    /// <summary>
    /// Pitch in 0.01 degrees
    /// Unit: degrees
    /// </summary>
    public short Pitch { get; set; }

    /// <summary>
    /// Roll in 0.01 degrees
    /// Unit: degrees
    /// </summary>
    public short Roll { get; set; }

    /// <summary>
    /// Ground speed in 0.1 knots
    /// Unit: knots
    /// </summary>
    public ushort GroundSpeed { get; set; }

    /// <summary>
    /// Vertical speed in feet/min
    /// Unit: fpm
    /// </summary>
    public short VerticalSpeed { get; set; }

    /// <summary>
    /// Aircraft on ground flag
    /// </summary>
    public bool OnGround { get; set; }

    /// <summary>
    /// Landing gear down flag
    /// </summary>
    public bool GearDown { get; set; }

    /// <summary>
    /// Flaps position (0-4)
    /// </summary>
    public byte FlapsPosition { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt32(AircraftId);
        packer.PackInt32(Latitude);
        packer.PackInt32(Longitude);
        packer.PackUInt32(AltitudeMsl);
        packer.PackUInt16(AltitudeAgl);
        packer.PackUInt16(Heading);
        packer.PackInt16(Pitch);
        packer.PackInt16(Roll);
        packer.PackUInt16(GroundSpeed);
        packer.PackInt16(VerticalSpeed);
        packer.PackBool(OnGround);
        packer.PackBool(GearDown);
        packer.PackUnsigned(FlapsPosition, 3);
        packer.PackSpare(3); // spare
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        AircraftId = unpacker.UnpackUInt32();
        Latitude = unpacker.UnpackInt32();
        Longitude = unpacker.UnpackInt32();
        AltitudeMsl = unpacker.UnpackUInt32();
        AltitudeAgl = unpacker.UnpackUInt16();
        Heading = unpacker.UnpackUInt16();
        Pitch = unpacker.UnpackInt16();
        Roll = unpacker.UnpackInt16();
        GroundSpeed = unpacker.UnpackUInt16();
        VerticalSpeed = unpacker.UnpackInt16();
        OnGround = unpacker.UnpackBool();
        GearDown = unpacker.UnpackBool();
        FlapsPosition = (byte)unpacker.UnpackUnsigned(3);
        unpacker.SkipBits(3); // spare
    }
}
