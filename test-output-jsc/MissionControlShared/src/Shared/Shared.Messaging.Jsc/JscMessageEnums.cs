// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// JSC message types as defined in JSC-35199.
/// </summary>
public enum JscMessageType : byte
{
    /// <summary>Command message from MCC-H to partner.</summary>
    Command = 0x01,

    /// <summary>Command acknowledgment from partner to MCC-H.</summary>
    CommandAck = 0x02,

    /// <summary>Telemetry data from partner to MCC-H.</summary>
    Telemetry = 0x03,

    /// <summary>Status message (bi-directional).</summary>
    Status = 0x04,

    /// <summary>File transfer start request.</summary>
    FileTransferStart = 0x05,

    /// <summary>File transfer data packet.</summary>
    FileTransferData = 0x06,

    /// <summary>File transfer end notification.</summary>
    FileTransferEnd = 0x07,

    /// <summary>Event notification (bi-directional).</summary>
    EventNotification = 0x08,

    /// <summary>Heartbeat message (bi-directional).</summary>
    Heartbeat = 0x09,

    /// <summary>Time synchronization from MCC-H to partner.</summary>
    TimeSync = 0x0A,

    /// <summary>Emergency message (bi-directional, highest priority).</summary>
    Emergency = 0xFF
}

/// <summary>
/// JSC message flags.
/// </summary>
[Flags]
public enum JscMessageFlags : byte
{
    /// <summary>No flags set.</summary>
    None = 0x00,

    /// <summary>Acknowledgment is required.</summary>
    AckRequired = 0x01,

    /// <summary>Message payload is encrypted.</summary>
    Encrypted = 0x02,

    /// <summary>Message payload is compressed.</summary>
    Compressed = 0x04,

    /// <summary>Message is part of a fragmented sequence.</summary>
    Fragmented = 0x08
}

/// <summary>
/// JSC secondary header types.
/// </summary>
public enum JscSecondaryHeaderType : byte
{
    /// <summary>Common secondary header (8 bytes).</summary>
    Common = 0x00,

    /// <summary>Command secondary header (24 bytes).</summary>
    Command = 0x01,

    /// <summary>Telemetry secondary header (16 bytes).</summary>
    Telemetry = 0x02,

    /// <summary>File transfer secondary header (32 bytes).</summary>
    FileTransfer = 0x03,

    /// <summary>Heartbeat secondary header (12 bytes).</summary>
    Heartbeat = 0x04
}

/// <summary>
/// MCC identifiers for JSC protocol.
/// </summary>
public static class MccIdentifiers
{
    /// <summary>MCC-Houston identifier.</summary>
    public const ushort MccHouston = 0x0001;

    /// <summary>Partner MCC identifier.</summary>
    public const ushort Partner = 0x0002;

    /// <summary>Broadcast identifier (all MCCs).</summary>
    public const ushort Broadcast = 0xFFFF;
}
