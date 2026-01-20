// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// JSC Primary Header (16 bytes) as defined in JSC-35199.
/// </summary>
public class JscPrimaryHeader
{
    /// <summary>Primary header size in bytes.</summary>
    public const int Size = 16;

    /// <summary>Protocol version (1 byte).</summary>
    public byte Version { get; set; } = 1;

    /// <summary>Message type (1 byte).</summary>
    public JscMessageType MessageType { get; set; }

    /// <summary>Unique message identifier (4 bytes).</summary>
    public uint MessageId { get; set; }

    /// <summary>Source MCC identifier (2 bytes).</summary>
    public ushort SourceMccId { get; set; }

    /// <summary>Destination MCC identifier (2 bytes).</summary>
    public ushort DestinationMccId { get; set; }

    /// <summary>Message priority 0-255 (1 byte).</summary>
    public byte Priority { get; set; }

    /// <summary>Message flags (1 byte).</summary>
    public JscMessageFlags Flags { get; set; }

    /// <summary>Secondary header length in bytes (2 bytes).</summary>
    public ushort SecondaryHeaderLength { get; set; }

    /// <summary>User data length in bytes (2 bytes).</summary>
    public ushort UserDataLength { get; set; }

    /// <summary>
    /// Serializes the primary header to a byte array.
    /// </summary>
    public byte[] Serialize()
    {
        var buffer = new byte[Size];
        Serialize(buffer, 0);
        return buffer;
    }

    /// <summary>
    /// Serializes the primary header to a byte array at the specified offset.
    /// </summary>
    public void Serialize(byte[] buffer, int offset)
    {
        buffer[offset] = Version;
        buffer[offset + 1] = (byte)MessageType;
        BigEndianConverter.WriteUInt32(buffer, offset + 2, MessageId);
        BigEndianConverter.WriteUInt16(buffer, offset + 6, SourceMccId);
        BigEndianConverter.WriteUInt16(buffer, offset + 8, DestinationMccId);
        buffer[offset + 10] = Priority;
        buffer[offset + 11] = (byte)Flags;
        BigEndianConverter.WriteUInt16(buffer, offset + 12, SecondaryHeaderLength);
        BigEndianConverter.WriteUInt16(buffer, offset + 14, UserDataLength);
    }

    /// <summary>
    /// Deserializes a primary header from a byte array.
    /// </summary>
    public static JscPrimaryHeader Deserialize(byte[] buffer, int offset = 0)
    {
        if (buffer.Length < offset + Size)
        {
            throw new ArgumentException($"Buffer too small. Expected at least {Size} bytes.");
        }

        return new JscPrimaryHeader
        {
            Version = buffer[offset],
            MessageType = (JscMessageType)buffer[offset + 1],
            MessageId = BigEndianConverter.ReadUInt32(buffer, offset + 2),
            SourceMccId = BigEndianConverter.ReadUInt16(buffer, offset + 6),
            DestinationMccId = BigEndianConverter.ReadUInt16(buffer, offset + 8),
            Priority = buffer[offset + 10],
            Flags = (JscMessageFlags)buffer[offset + 11],
            SecondaryHeaderLength = BigEndianConverter.ReadUInt16(buffer, offset + 12),
            UserDataLength = BigEndianConverter.ReadUInt16(buffer, offset + 14)
        };
    }
}
