// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// CCSDS Space Packet Primary Header (6 bytes / 48 bits).
/// </summary>
public class CcsdsPrimaryHeader
{
    /// <summary>
    /// Packet version number (3 bits). Always 0 for CCSDS.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Packet type (1 bit). 0 = Telemetry, 1 = Telecommand.
    /// </summary>
    public int PacketType { get; set; }

    /// <summary>
    /// Secondary header flag (1 bit). 1 = present, 0 = absent.
    /// </summary>
    public bool SecondaryHeaderFlag { get; set; }

    /// <summary>
    /// Application Process Identifier (11 bits). Range 0-2047.
    /// </summary>
    public int Apid { get; set; }

    /// <summary>
    /// Sequence flags (2 bits). 3 = unsegmented.
    /// </summary>
    public int SequenceFlags { get; set; } = 3;

    /// <summary>
    /// Packet sequence count (14 bits). Range 0-16383.
    /// </summary>
    public int SequenceCount { get; set; }

    /// <summary>
    /// Packet data length (16 bits). Length of data field - 1.
    /// </summary>
    public int PacketDataLength { get; set; }

    /// <summary>
    /// Packs the primary header into the bit packer.
    /// </summary>
    public void Pack(BitPacker packer)
    {
        packer.PackUnsigned((ulong)Version, 3);
        packer.PackUnsigned((ulong)PacketType, 1);
        packer.PackBool(SecondaryHeaderFlag);
        packer.PackUnsigned((ulong)Apid, 11);
        packer.PackUnsigned((ulong)SequenceFlags, 2);
        packer.PackUnsigned((ulong)SequenceCount, 14);
        packer.PackUnsigned((ulong)PacketDataLength, 16);
    }

    /// <summary>
    /// Unpacks the primary header from the bit unpacker.
    /// </summary>
    public static CcsdsPrimaryHeader Unpack(BitUnpacker unpacker)
    {
        return new CcsdsPrimaryHeader
        {
            Version = (int)unpacker.UnpackUnsigned(3),
            PacketType = (int)unpacker.UnpackUnsigned(1),
            SecondaryHeaderFlag = unpacker.UnpackBool(),
            Apid = (int)unpacker.UnpackUnsigned(11),
            SequenceFlags = (int)unpacker.UnpackUnsigned(2),
            SequenceCount = (int)unpacker.UnpackUnsigned(14),
            PacketDataLength = (int)unpacker.UnpackUnsigned(16),
        };
    }
}
