// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// JSC protocol message following JSC-35199 specification.
/// </summary>
public class JscMessage
{
    private static uint _nextMessageId = 1;
    private static readonly object _idLock = new();

    /// <summary>Checksum size in bytes (CRC-32).</summary>
    public const int ChecksumSize = 4;

    /// <summary>Gets or sets the primary header.</summary>
    public JscPrimaryHeader PrimaryHeader { get; set; } = new();

    /// <summary>Gets or sets the secondary header (optional).</summary>
    public JscSecondaryHeaderBase? SecondaryHeader { get; set; }

    /// <summary>Gets or sets the user data payload.</summary>
    public byte[] UserData { get; set; } = Array.Empty<byte>();

    /// <summary>Gets or sets the CRC-32 checksum.</summary>
    public uint Checksum { get; set; }

    /// <summary>
    /// Gets the total message size in bytes.
    /// </summary>
    public int TotalSize => JscPrimaryHeader.Size +
                           (SecondaryHeader?.Size ?? 0) +
                           UserData.Length +
                           ChecksumSize;

    /// <summary>
    /// Creates a new message with a unique ID.
    /// </summary>
    public static JscMessage Create(
        JscMessageType messageType,
        byte[] userData,
        JscSecondaryHeaderBase? secondaryHeader = null,
        byte priority = 128,
        JscMessageFlags flags = JscMessageFlags.None)
    {
        uint messageId;
        lock (_idLock)
        {
            messageId = _nextMessageId++;
        }

        var message = new JscMessage
        {
            PrimaryHeader = new JscPrimaryHeader
            {
                Version = 1,
                MessageType = messageType,
                MessageId = messageId,
                SourceMccId = 1,
                DestinationMccId = 2,
                Priority = priority,
                Flags = flags,
                SecondaryHeaderLength = (ushort)(secondaryHeader?.Size ?? 0),
                UserDataLength = (ushort)userData.Length
            },
            SecondaryHeader = secondaryHeader,
            UserData = userData
        };

        return message;
    }

    /// <summary>
    /// Serializes the message to a byte array.
    /// </summary>
    public byte[] Serialize()
    {
        var buffer = new byte[TotalSize];
        var offset = 0;

        // Primary header
        PrimaryHeader.Serialize(buffer, offset);
        offset += JscPrimaryHeader.Size;

        // Secondary header (if present)
        if (SecondaryHeader != null)
        {
            SecondaryHeader.Serialize(buffer, offset);
            offset += SecondaryHeader.Size;
        }

        // User data
        if (UserData.Length > 0)
        {
            Array.Copy(UserData, 0, buffer, offset, UserData.Length);
            offset += UserData.Length;
        }

        // Calculate and write checksum
        Checksum = Crc32.Calculate(buffer, 0, offset);
        BigEndianConverter.WriteUInt32(buffer, offset, Checksum);

        return buffer;
    }

    /// <summary>
    /// Deserializes a message from a byte array.
    /// </summary>
    public static JscMessage Deserialize(byte[] buffer)
    {
        if (buffer.Length < JscPrimaryHeader.Size + ChecksumSize)
        {
            throw new ArgumentException("Buffer too small for JSC message.");
        }

        var message = new JscMessage();
        var offset = 0;

        // Primary header
        message.PrimaryHeader = JscPrimaryHeader.Deserialize(buffer, offset);
        offset += JscPrimaryHeader.Size;

        // Secondary header (if present)
        if (message.PrimaryHeader.SecondaryHeaderLength > 0)
        {
            message.SecondaryHeader = DeserializeSecondaryHeader(
                buffer, offset, message.PrimaryHeader.MessageType);
            offset += message.PrimaryHeader.SecondaryHeaderLength;
        }

        // User data
        if (message.PrimaryHeader.UserDataLength > 0)
        {
            message.UserData = new byte[message.PrimaryHeader.UserDataLength];
            Array.Copy(buffer, offset, message.UserData, 0, message.UserData.Length);
            offset += message.UserData.Length;
        }

        // Checksum
        message.Checksum = BigEndianConverter.ReadUInt32(buffer, offset);

        // Verify checksum
        var calculatedChecksum = Crc32.Calculate(buffer, 0, offset);
        if (calculatedChecksum != message.Checksum)
        {
            throw new InvalidOperationException(
                $"Checksum mismatch. Expected {message.Checksum:X8}, calculated {calculatedChecksum:X8}");
        }

        return message;
    }

    private static JscSecondaryHeaderBase? DeserializeSecondaryHeader(
        byte[] buffer, int offset, JscMessageType messageType)
    {
        return messageType switch
        {
            JscMessageType.Command or JscMessageType.CommandAck
                => JscCommandSecondaryHeader.Deserialize(buffer, offset),
            JscMessageType.Telemetry
                => JscTelemetrySecondaryHeader.Deserialize(buffer, offset),
            JscMessageType.Heartbeat
                => JscHeartbeatSecondaryHeader.Deserialize(buffer, offset),
            _ => JscCommonSecondaryHeader.Deserialize(buffer, offset)
        };
    }
}
