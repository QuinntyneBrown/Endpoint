// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds.Packets;

/// <summary>
/// Navigation system data
/// </summary>
public class NavigationPacket : CcsdsPacket
{
    public override int Apid => 102;

    public override int PacketType => 0;

    public uint AircraftId { get; set; }

    public bool GpsValid { get; set; }

    public bool InsValid { get; set; }

    /// <summary>
    /// Navigation mode (0=manual, 1=GPS, 2=NAV, 3=APPROACH)
    /// </summary>
    public byte NavMode { get; set; }

    /// <summary>
    /// Course deviation in 0.1 degrees
    /// Unit: degrees
    /// </summary>
    public short CourseDeviation { get; set; }

    /// <summary>
    /// Glide slope deviation in 0.01 degrees
    /// Unit: degrees
    /// </summary>
    public short GlideSlopeDeviation { get; set; }

    /// <summary>
    /// Distance to next waypoint in 0.1 nm
    /// Unit: nm
    /// </summary>
    public ushort DistanceToWaypoint { get; set; }

    /// <summary>
    /// ETA to waypoint in seconds
    /// Unit: seconds
    /// </summary>
    public ushort TimeToWaypoint { get; set; }

    public byte CurrentWaypointIndex { get; set; }

    public byte TotalWaypoints { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt32(AircraftId);
        packer.PackBool(GpsValid);
        packer.PackBool(InsValid);
        packer.PackUnsigned(NavMode, 3);
        packer.PackSpare(3); // spare
        packer.PackInt16(CourseDeviation);
        packer.PackInt16(GlideSlopeDeviation);
        packer.PackUInt16(DistanceToWaypoint);
        packer.PackUInt16(TimeToWaypoint);
        packer.PackByte(CurrentWaypointIndex);
        packer.PackByte(TotalWaypoints);
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        AircraftId = unpacker.UnpackUInt32();
        GpsValid = unpacker.UnpackBool();
        InsValid = unpacker.UnpackBool();
        NavMode = (byte)unpacker.UnpackUnsigned(3);
        unpacker.SkipBits(3); // spare
        CourseDeviation = unpacker.UnpackInt16();
        GlideSlopeDeviation = unpacker.UnpackInt16();
        DistanceToWaypoint = unpacker.UnpackUInt16();
        TimeToWaypoint = unpacker.UnpackUInt16();
        CurrentWaypointIndex = unpacker.UnpackByte();
        TotalWaypoints = unpacker.UnpackByte();
    }
}
