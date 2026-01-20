// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds.Packets;

/// <summary>
/// Flight control command (telecommand)
/// </summary>
public class FlightControlCommandPacket : CcsdsPacket
{
    public override int Apid => 200;

    public override int PacketType => 1;

    /// <summary>
    /// Unique command identifier
    /// </summary>
    public ushort CommandId { get; set; }

    public uint TargetAircraftId { get; set; }

    /// <summary>
    /// Command type (0=heading, 1=altitude, 2=speed, 3=direct-to)
    /// </summary>
    public byte CommandType { get; set; }

    /// <summary>
    /// Target value based on command type
    /// </summary>
    public int TargetValue { get; set; }

    /// <summary>
    /// Rate of change (degrees/sec, fpm, or knots/sec)
    /// </summary>
    public ushort TransitionRate { get; set; }

    /// <summary>
    /// Execute immediately flag
    /// </summary>
    public bool Immediate { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt16(CommandId);
        packer.PackUInt32(TargetAircraftId);
        packer.PackByte(CommandType);
        packer.PackInt32(TargetValue);
        packer.PackUInt16(TransitionRate);
        packer.PackBool(Immediate);
        packer.PackSpare(7); // spare
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        CommandId = unpacker.UnpackUInt16();
        TargetAircraftId = unpacker.UnpackUInt32();
        CommandType = unpacker.UnpackByte();
        TargetValue = unpacker.UnpackInt32();
        TransitionRate = unpacker.UnpackUInt16();
        Immediate = unpacker.UnpackBool();
        unpacker.SkipBits(7); // spare
    }
}
