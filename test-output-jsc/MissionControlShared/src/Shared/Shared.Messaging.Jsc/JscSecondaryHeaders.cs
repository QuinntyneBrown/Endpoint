// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// Base class for JSC secondary headers.
/// </summary>
public abstract class JscSecondaryHeaderBase
{
    /// <summary>Gets the secondary header type.</summary>
    public abstract JscSecondaryHeaderType HeaderType { get; }

    /// <summary>Gets the size in bytes.</summary>
    public abstract int Size { get; }

    /// <summary>Serializes the secondary header.</summary>
    public abstract byte[] Serialize();

    /// <summary>Serializes the secondary header to a buffer at the specified offset.</summary>
    public abstract void Serialize(byte[] buffer, int offset);
}

/// <summary>
/// Common secondary header (8 bytes).
/// </summary>
public class JscCommonSecondaryHeader : JscSecondaryHeaderBase
{
    public const int HeaderSize = 8;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Common;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp { get; set; }

    public override byte[] Serialize()
    {
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }

    public override void Serialize(byte[] buffer, int offset)
    {
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
    }

    public static JscCommonSecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {
        return new JscCommonSecondaryHeader
        {
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset)
        };
    }
}

/// <summary>
/// Command secondary header (24 bytes).
/// </summary>
public class JscCommandSecondaryHeader : JscSecondaryHeaderBase
{
    public const int HeaderSize = 24;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Command;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp { get; set; }

    /// <summary>Command code.</summary>
    public uint CommandCode { get; set; }

    /// <summary>Command sequence number.</summary>
    public ushort SequenceNumber { get; set; }

    /// <summary>Scheduled execution time.</summary>
    public uint ExecutionTime { get; set; }

    /// <summary>Command timeout in seconds.</summary>
    public ushort TimeoutSeconds { get; set; }

    /// <summary>Retry count.</summary>
    public ushort RetryCount { get; set; }

    public override byte[] Serialize()
    {
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }

    public override void Serialize(byte[] buffer, int offset)
    {
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
        BigEndianConverter.WriteUInt32(buffer, offset + 8, CommandCode);
        BigEndianConverter.WriteUInt16(buffer, offset + 12, SequenceNumber);
        BigEndianConverter.WriteUInt32(buffer, offset + 14, ExecutionTime);
        BigEndianConverter.WriteUInt16(buffer, offset + 18, TimeoutSeconds);
        BigEndianConverter.WriteUInt16(buffer, offset + 20, RetryCount);
        // 2 bytes padding
    }

    public static JscCommandSecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {
        return new JscCommandSecondaryHeader
        {
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset),
            CommandCode = BigEndianConverter.ReadUInt32(buffer, offset + 8),
            SequenceNumber = BigEndianConverter.ReadUInt16(buffer, offset + 12),
            ExecutionTime = BigEndianConverter.ReadUInt32(buffer, offset + 14),
            TimeoutSeconds = BigEndianConverter.ReadUInt16(buffer, offset + 18),
            RetryCount = BigEndianConverter.ReadUInt16(buffer, offset + 20)
        };
    }
}

/// <summary>
/// Telemetry secondary header (16 bytes).
/// </summary>
public class JscTelemetrySecondaryHeader : JscSecondaryHeaderBase
{
    public const int HeaderSize = 16;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Telemetry;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp { get; set; }

    /// <summary>Source identifier.</summary>
    public uint SourceId { get; set; }

    /// <summary>Telemetry type identifier.</summary>
    public ushort TelemetryType { get; set; }

    /// <summary>Sample rate in Hz.</summary>
    public byte SampleRate { get; set; }

    /// <summary>Quality indicator (0-100).</summary>
    public byte QualityIndicator { get; set; }

    public override byte[] Serialize()
    {
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }

    public override void Serialize(byte[] buffer, int offset)
    {
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
        BigEndianConverter.WriteUInt32(buffer, offset + 8, SourceId);
        BigEndianConverter.WriteUInt16(buffer, offset + 12, TelemetryType);
        buffer[offset + 14] = SampleRate;
        buffer[offset + 15] = QualityIndicator;
    }

    public static JscTelemetrySecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {
        return new JscTelemetrySecondaryHeader
        {
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset),
            SourceId = BigEndianConverter.ReadUInt32(buffer, offset + 8),
            TelemetryType = BigEndianConverter.ReadUInt16(buffer, offset + 12),
            SampleRate = buffer[offset + 14],
            QualityIndicator = buffer[offset + 15]
        };
    }
}

/// <summary>
/// Heartbeat secondary header (12 bytes).
/// </summary>
public class JscHeartbeatSecondaryHeader : JscSecondaryHeaderBase
{
    public const int HeaderSize = 12;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Heartbeat;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp { get; set; }

    /// <summary>Sequence number.</summary>
    public ushort SequenceNumber { get; set; }

    /// <summary>System status flags.</summary>
    public ushort SystemStatus { get; set; }

    public override byte[] Serialize()
    {
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }

    public override void Serialize(byte[] buffer, int offset)
    {
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
        BigEndianConverter.WriteUInt16(buffer, offset + 8, SequenceNumber);
        BigEndianConverter.WriteUInt16(buffer, offset + 10, SystemStatus);
    }

    public static JscHeartbeatSecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {
        return new JscHeartbeatSecondaryHeader
        {
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset),
            SequenceNumber = BigEndianConverter.ReadUInt16(buffer, offset + 8),
            SystemStatus = BigEndianConverter.ReadUInt16(buffer, offset + 10)
        };
    }
}
