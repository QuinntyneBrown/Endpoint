// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds.Packets;

/// <summary>
/// Navigation command packet
/// </summary>
public class NavigationCommandPacket : CcsdsPacket
{
    public override int Apid => 200;

    public override int PacketType => 1;

    /// <summary>
    /// Command identifier
    /// </summary>
    public ushort CommandId { get; set; }

    public uint TargetAircraftId { get; set; }

    /// <summary>
    /// Type of navigation command
    /// </summary>
    public byte CommandType { get; set; }

    public int TargetLatitude { get; set; }

    public int TargetLongitude { get; set; }

    public uint TargetAltitude { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt16(CommandId);
        packer.PackUInt32(TargetAircraftId);
        packer.PackByte(CommandType);
        packer.PackInt32(TargetLatitude);
        packer.PackInt32(TargetLongitude);
        packer.PackUInt32(TargetAltitude);
        packer.PackSpare(8); // spare
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        CommandId = unpacker.UnpackUInt16();
        TargetAircraftId = unpacker.UnpackUInt32();
        CommandType = unpacker.UnpackByte();
        TargetLatitude = unpacker.UnpackInt32();
        TargetLongitude = unpacker.UnpackInt32();
        TargetAltitude = unpacker.UnpackUInt32();
        unpacker.SkipBits(8); // spare
    }
}
