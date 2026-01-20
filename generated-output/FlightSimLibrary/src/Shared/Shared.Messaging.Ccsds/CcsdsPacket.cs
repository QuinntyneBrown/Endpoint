// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// Base class for CCSDS space packets.
/// </summary>
public abstract class CcsdsPacket
{
    /// <summary>
    /// Gets the Application Process Identifier (APID) for this packet type.
    /// </summary>
    public abstract int Apid { get; }

    /// <summary>
    /// Gets the packet type (0 = TM, 1 = TC).
    /// </summary>
    public abstract int PacketType { get; }

    /// <summary>
    /// Gets the sequence count for this packet instance.
    /// </summary>
    public int SequenceCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp (if secondary header is present).
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Packs the packet data fields into the bit packer.
    /// </summary>
    public abstract void PackData(BitPacker packer);

    /// <summary>
    /// Unpacks the packet data fields from the bit unpacker.
    /// </summary>
    public abstract void UnpackData(BitUnpacker unpacker);
}
