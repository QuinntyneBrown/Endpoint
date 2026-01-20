// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the JSC (Johnson Space Center) messaging project following JSC-35199 specification.
/// </summary>
public class JscProjectGenerator : ProjectGeneratorBase
{
    public JscProjectGenerator(
        ILogger<JscProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 14;

    public override bool ShouldGenerate(SharedLibraryConfig config)
        => config.Protocols.Jsc?.Enabled == true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.Jsc";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.Jsc";

        Logger.LogInformation("Generating {ProjectName}", projectName);

        var abstractionsProject = $"{context.LibraryName}.Messaging.Abstractions";

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            packageReferences: new List<string>
            {
                "<PackageReference Include=\"Microsoft.Extensions.DependencyInjection.Abstractions\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"Microsoft.Extensions.Logging.Abstractions\" Version=\"9.0.0\" />",
            },
            projectReferences: new List<string>
            {
                $"../{abstractionsProject}/{abstractionsProject}.csproj",
            },
            cancellationToken: cancellationToken);

        var jscConfig = context.Config.Protocols.Jsc!;

        // Generate BigEndianConverter
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "BigEndianConverter.cs"),
            GenerateBigEndianConverter(ns),
            cancellationToken);

        // Generate Crc32
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "Crc32.cs"),
            GenerateCrc32(ns),
            cancellationToken);

        // Generate JscMessageEnums
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "JscMessageEnums.cs"),
            GenerateJscMessageEnums(ns),
            cancellationToken);

        // Generate JscPrimaryHeader
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "JscPrimaryHeader.cs"),
            GenerateJscPrimaryHeader(ns),
            cancellationToken);

        // Generate JscSecondaryHeaders
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "JscSecondaryHeaders.cs"),
            GenerateJscSecondaryHeaders(ns),
            cancellationToken);

        // Generate JscMessage
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "JscMessage.cs"),
            GenerateJscMessage(ns, jscConfig),
            cancellationToken);

        // Generate JscMessageSerializer
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "JscMessageSerializer.cs"),
            GenerateJscMessageSerializer(ns),
            cancellationToken);

        // Generate ServiceCollectionExtensions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "ServiceCollectionExtensions.cs"),
            GenerateServiceCollectionExtensions(ns),
            cancellationToken);

        // Generate custom message types if configured
        if (jscConfig.MessageTypes.Count > 0)
        {
            var messagesDirectory = FileSystem.Path.Combine(projectDirectory, "Messages");
            FileSystem.Directory.CreateDirectory(messagesDirectory);

            foreach (var msgType in jscConfig.MessageTypes)
            {
                await WriteFileAsync(
                    FileSystem.Path.Combine(messagesDirectory, $"{msgType.Name}.cs"),
                    GenerateCustomMessageType(ns, msgType),
                    cancellationToken);
            }
        }

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var projectName = $"{context.LibraryName}.Messaging.Jsc";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/BigEndianConverter.cs");
        preview.Files.Add($"{basePath}/Crc32.cs");
        preview.Files.Add($"{basePath}/JscMessageEnums.cs");
        preview.Files.Add($"{basePath}/JscPrimaryHeader.cs");
        preview.Files.Add($"{basePath}/JscSecondaryHeaders.cs");
        preview.Files.Add($"{basePath}/JscMessage.cs");
        preview.Files.Add($"{basePath}/JscMessageSerializer.cs");
        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        var jscConfig = context.Config.Protocols.Jsc;
        if (jscConfig?.MessageTypes != null)
        {
            foreach (var msgType in jscConfig.MessageTypes)
            {
                preview.Files.Add($"{basePath}/Messages/{msgType.Name}.cs");
            }
        }

        return Task.FromResult(preview);
    }

    private string GenerateBigEndianConverter(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Provides methods for reading and writing values in big-endian (network) byte order.
/// </summary>
public static class BigEndianConverter
{{
    /// <summary>
    /// Reads a 16-bit unsigned integer from a byte array in big-endian order.
    /// </summary>
    public static ushort ReadUInt16(byte[] buffer, int offset)
    {{
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }}

    /// <summary>
    /// Reads a 32-bit unsigned integer from a byte array in big-endian order.
    /// </summary>
    public static uint ReadUInt32(byte[] buffer, int offset)
    {{
        return (uint)((buffer[offset] << 24) |
                      (buffer[offset + 1] << 16) |
                      (buffer[offset + 2] << 8) |
                      buffer[offset + 3]);
    }}

    /// <summary>
    /// Reads a 64-bit unsigned integer from a byte array in big-endian order.
    /// </summary>
    public static ulong ReadUInt64(byte[] buffer, int offset)
    {{
        return ((ulong)buffer[offset] << 56) |
               ((ulong)buffer[offset + 1] << 48) |
               ((ulong)buffer[offset + 2] << 40) |
               ((ulong)buffer[offset + 3] << 32) |
               ((ulong)buffer[offset + 4] << 24) |
               ((ulong)buffer[offset + 5] << 16) |
               ((ulong)buffer[offset + 6] << 8) |
               buffer[offset + 7];
    }}

    /// <summary>
    /// Reads a 16-bit signed integer from a byte array in big-endian order.
    /// </summary>
    public static short ReadInt16(byte[] buffer, int offset)
    {{
        return (short)ReadUInt16(buffer, offset);
    }}

    /// <summary>
    /// Reads a 32-bit signed integer from a byte array in big-endian order.
    /// </summary>
    public static int ReadInt32(byte[] buffer, int offset)
    {{
        return (int)ReadUInt32(buffer, offset);
    }}

    /// <summary>
    /// Reads a 64-bit signed integer from a byte array in big-endian order.
    /// </summary>
    public static long ReadInt64(byte[] buffer, int offset)
    {{
        return (long)ReadUInt64(buffer, offset);
    }}

    /// <summary>
    /// Writes a 16-bit unsigned integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteUInt16(byte[] buffer, int offset, ushort value)
    {{
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }}

    /// <summary>
    /// Writes a 32-bit unsigned integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteUInt32(byte[] buffer, int offset, uint value)
    {{
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }}

    /// <summary>
    /// Writes a 64-bit unsigned integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteUInt64(byte[] buffer, int offset, ulong value)
    {{
        buffer[offset] = (byte)(value >> 56);
        buffer[offset + 1] = (byte)(value >> 48);
        buffer[offset + 2] = (byte)(value >> 40);
        buffer[offset + 3] = (byte)(value >> 32);
        buffer[offset + 4] = (byte)(value >> 24);
        buffer[offset + 5] = (byte)(value >> 16);
        buffer[offset + 6] = (byte)(value >> 8);
        buffer[offset + 7] = (byte)value;
    }}

    /// <summary>
    /// Writes a 16-bit signed integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteInt16(byte[] buffer, int offset, short value)
    {{
        WriteUInt16(buffer, offset, (ushort)value);
    }}

    /// <summary>
    /// Writes a 32-bit signed integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteInt32(byte[] buffer, int offset, int value)
    {{
        WriteUInt32(buffer, offset, (uint)value);
    }}

    /// <summary>
    /// Writes a 64-bit signed integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteInt64(byte[] buffer, int offset, long value)
    {{
        WriteUInt64(buffer, offset, (ulong)value);
    }}

    /// <summary>
    /// Reads a 32-bit float from a byte array in big-endian order.
    /// </summary>
    public static float ReadFloat(byte[] buffer, int offset)
    {{
        var bytes = new byte[4];
        Array.Copy(buffer, offset, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}
        return BitConverter.ToSingle(bytes, 0);
    }}

    /// <summary>
    /// Reads a 64-bit double from a byte array in big-endian order.
    /// </summary>
    public static double ReadDouble(byte[] buffer, int offset)
    {{
        var bytes = new byte[8];
        Array.Copy(buffer, offset, bytes, 0, 8);
        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}
        return BitConverter.ToDouble(bytes, 0);
    }}

    /// <summary>
    /// Writes a 32-bit float to a byte array in big-endian order.
    /// </summary>
    public static void WriteFloat(byte[] buffer, int offset, float value)
    {{
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}
        Array.Copy(bytes, 0, buffer, offset, 4);
    }}

    /// <summary>
    /// Writes a 64-bit double to a byte array in big-endian order.
    /// </summary>
    public static void WriteDouble(byte[] buffer, int offset, double value)
    {{
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}
        Array.Copy(bytes, 0, buffer, offset, 8);
    }}
}}
";
    }

    private string GenerateCrc32(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// CRC-32 checksum calculator using IEEE 802.3 polynomial.
/// </summary>
public static class Crc32
{{
    private static readonly uint[] Table = GenerateTable();

    private static uint[] GenerateTable()
    {{
        const uint polynomial = 0xEDB88320;
        var table = new uint[256];

        for (uint i = 0; i < 256; i++)
        {{
            var crc = i;
            for (var j = 0; j < 8; j++)
            {{
                crc = (crc & 1) != 0 ? (crc >> 1) ^ polynomial : crc >> 1;
            }}
            table[i] = crc;
        }}

        return table;
    }}

    /// <summary>
    /// Calculates the CRC-32 checksum for the given data.
    /// </summary>
    public static uint Calculate(byte[] data)
    {{
        return Calculate(data, 0, data.Length);
    }}

    /// <summary>
    /// Calculates the CRC-32 checksum for a portion of the given data.
    /// </summary>
    public static uint Calculate(byte[] data, int offset, int length)
    {{
        var crc = 0xFFFFFFFF;

        for (var i = offset; i < offset + length; i++)
        {{
            crc = (crc >> 8) ^ Table[(crc ^ data[i]) & 0xFF];
        }}

        return crc ^ 0xFFFFFFFF;
    }}

    /// <summary>
    /// Updates a running CRC-32 checksum with additional data.
    /// </summary>
    public static uint Update(uint crc, byte[] data, int offset, int length)
    {{
        crc ^= 0xFFFFFFFF;

        for (var i = offset; i < offset + length; i++)
        {{
            crc = (crc >> 8) ^ Table[(crc ^ data[i]) & 0xFF];
        }}

        return crc ^ 0xFFFFFFFF;
    }}

    /// <summary>
    /// Verifies that the checksum matches the expected value.
    /// </summary>
    public static bool Verify(byte[] data, uint expectedChecksum)
    {{
        return Calculate(data) == expectedChecksum;
    }}

    /// <summary>
    /// Verifies that the checksum matches the expected value.
    /// </summary>
    public static bool Verify(byte[] data, int offset, int length, uint expectedChecksum)
    {{
        return Calculate(data, offset, length) == expectedChecksum;
    }}
}}
";
    }

    private string GenerateJscMessageEnums(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// JSC message types as defined in JSC-35199.
/// </summary>
public enum JscMessageType : byte
{{
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
}}

/// <summary>
/// JSC message flags.
/// </summary>
[Flags]
public enum JscMessageFlags : byte
{{
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
}}

/// <summary>
/// JSC secondary header types.
/// </summary>
public enum JscSecondaryHeaderType : byte
{{
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
}}

/// <summary>
/// MCC identifiers for JSC protocol.
/// </summary>
public static class MccIdentifiers
{{
    /// <summary>MCC-Houston identifier.</summary>
    public const ushort MccHouston = 0x0001;

    /// <summary>Partner MCC identifier.</summary>
    public const ushort Partner = 0x0002;

    /// <summary>Broadcast identifier (all MCCs).</summary>
    public const ushort Broadcast = 0xFFFF;
}}
";
    }

    private string GenerateJscPrimaryHeader(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// JSC Primary Header (16 bytes) as defined in JSC-35199.
/// </summary>
public class JscPrimaryHeader
{{
    /// <summary>Primary header size in bytes.</summary>
    public const int Size = 16;

    /// <summary>Protocol version (1 byte).</summary>
    public byte Version {{ get; set; }} = 1;

    /// <summary>Message type (1 byte).</summary>
    public JscMessageType MessageType {{ get; set; }}

    /// <summary>Unique message identifier (4 bytes).</summary>
    public uint MessageId {{ get; set; }}

    /// <summary>Source MCC identifier (2 bytes).</summary>
    public ushort SourceMccId {{ get; set; }}

    /// <summary>Destination MCC identifier (2 bytes).</summary>
    public ushort DestinationMccId {{ get; set; }}

    /// <summary>Message priority 0-255 (1 byte).</summary>
    public byte Priority {{ get; set; }}

    /// <summary>Message flags (1 byte).</summary>
    public JscMessageFlags Flags {{ get; set; }}

    /// <summary>Secondary header length in bytes (2 bytes).</summary>
    public ushort SecondaryHeaderLength {{ get; set; }}

    /// <summary>User data length in bytes (2 bytes).</summary>
    public ushort UserDataLength {{ get; set; }}

    /// <summary>
    /// Serializes the primary header to a byte array.
    /// </summary>
    public byte[] Serialize()
    {{
        var buffer = new byte[Size];
        Serialize(buffer, 0);
        return buffer;
    }}

    /// <summary>
    /// Serializes the primary header to a byte array at the specified offset.
    /// </summary>
    public void Serialize(byte[] buffer, int offset)
    {{
        buffer[offset] = Version;
        buffer[offset + 1] = (byte)MessageType;
        BigEndianConverter.WriteUInt32(buffer, offset + 2, MessageId);
        BigEndianConverter.WriteUInt16(buffer, offset + 6, SourceMccId);
        BigEndianConverter.WriteUInt16(buffer, offset + 8, DestinationMccId);
        buffer[offset + 10] = Priority;
        buffer[offset + 11] = (byte)Flags;
        BigEndianConverter.WriteUInt16(buffer, offset + 12, SecondaryHeaderLength);
        BigEndianConverter.WriteUInt16(buffer, offset + 14, UserDataLength);
    }}

    /// <summary>
    /// Deserializes a primary header from a byte array.
    /// </summary>
    public static JscPrimaryHeader Deserialize(byte[] buffer, int offset = 0)
    {{
        if (buffer.Length < offset + Size)
        {{
            throw new ArgumentException($""Buffer too small. Expected at least {{Size}} bytes."");
        }}

        return new JscPrimaryHeader
        {{
            Version = buffer[offset],
            MessageType = (JscMessageType)buffer[offset + 1],
            MessageId = BigEndianConverter.ReadUInt32(buffer, offset + 2),
            SourceMccId = BigEndianConverter.ReadUInt16(buffer, offset + 6),
            DestinationMccId = BigEndianConverter.ReadUInt16(buffer, offset + 8),
            Priority = buffer[offset + 10],
            Flags = (JscMessageFlags)buffer[offset + 11],
            SecondaryHeaderLength = BigEndianConverter.ReadUInt16(buffer, offset + 12),
            UserDataLength = BigEndianConverter.ReadUInt16(buffer, offset + 14)
        }};
    }}
}}
";
    }

    private string GenerateJscSecondaryHeaders(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Base class for JSC secondary headers.
/// </summary>
public abstract class JscSecondaryHeaderBase
{{
    /// <summary>Gets the secondary header type.</summary>
    public abstract JscSecondaryHeaderType HeaderType {{ get; }}

    /// <summary>Gets the size in bytes.</summary>
    public abstract int Size {{ get; }}

    /// <summary>Serializes the secondary header.</summary>
    public abstract byte[] Serialize();

    /// <summary>Serializes the secondary header to a buffer at the specified offset.</summary>
    public abstract void Serialize(byte[] buffer, int offset);
}}

/// <summary>
/// Common secondary header (8 bytes).
/// </summary>
public class JscCommonSecondaryHeader : JscSecondaryHeaderBase
{{
    public const int HeaderSize = 8;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Common;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp {{ get; set; }}

    public override byte[] Serialize()
    {{
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }}

    public override void Serialize(byte[] buffer, int offset)
    {{
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
    }}

    public static JscCommonSecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {{
        return new JscCommonSecondaryHeader
        {{
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset)
        }};
    }}
}}

/// <summary>
/// Command secondary header (24 bytes).
/// </summary>
public class JscCommandSecondaryHeader : JscSecondaryHeaderBase
{{
    public const int HeaderSize = 24;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Command;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp {{ get; set; }}

    /// <summary>Command code.</summary>
    public uint CommandCode {{ get; set; }}

    /// <summary>Command sequence number.</summary>
    public ushort SequenceNumber {{ get; set; }}

    /// <summary>Scheduled execution time.</summary>
    public uint ExecutionTime {{ get; set; }}

    /// <summary>Command timeout in seconds.</summary>
    public ushort TimeoutSeconds {{ get; set; }}

    /// <summary>Retry count.</summary>
    public ushort RetryCount {{ get; set; }}

    public override byte[] Serialize()
    {{
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }}

    public override void Serialize(byte[] buffer, int offset)
    {{
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
        BigEndianConverter.WriteUInt32(buffer, offset + 8, CommandCode);
        BigEndianConverter.WriteUInt16(buffer, offset + 12, SequenceNumber);
        BigEndianConverter.WriteUInt32(buffer, offset + 14, ExecutionTime);
        BigEndianConverter.WriteUInt16(buffer, offset + 18, TimeoutSeconds);
        BigEndianConverter.WriteUInt16(buffer, offset + 20, RetryCount);
        // 2 bytes padding
    }}

    public static JscCommandSecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {{
        return new JscCommandSecondaryHeader
        {{
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset),
            CommandCode = BigEndianConverter.ReadUInt32(buffer, offset + 8),
            SequenceNumber = BigEndianConverter.ReadUInt16(buffer, offset + 12),
            ExecutionTime = BigEndianConverter.ReadUInt32(buffer, offset + 14),
            TimeoutSeconds = BigEndianConverter.ReadUInt16(buffer, offset + 18),
            RetryCount = BigEndianConverter.ReadUInt16(buffer, offset + 20)
        }};
    }}
}}

/// <summary>
/// Telemetry secondary header (16 bytes).
/// </summary>
public class JscTelemetrySecondaryHeader : JscSecondaryHeaderBase
{{
    public const int HeaderSize = 16;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Telemetry;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp {{ get; set; }}

    /// <summary>Source identifier.</summary>
    public uint SourceId {{ get; set; }}

    /// <summary>Telemetry type identifier.</summary>
    public ushort TelemetryType {{ get; set; }}

    /// <summary>Sample rate in Hz.</summary>
    public byte SampleRate {{ get; set; }}

    /// <summary>Quality indicator (0-100).</summary>
    public byte QualityIndicator {{ get; set; }}

    public override byte[] Serialize()
    {{
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }}

    public override void Serialize(byte[] buffer, int offset)
    {{
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
        BigEndianConverter.WriteUInt32(buffer, offset + 8, SourceId);
        BigEndianConverter.WriteUInt16(buffer, offset + 12, TelemetryType);
        buffer[offset + 14] = SampleRate;
        buffer[offset + 15] = QualityIndicator;
    }}

    public static JscTelemetrySecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {{
        return new JscTelemetrySecondaryHeader
        {{
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset),
            SourceId = BigEndianConverter.ReadUInt32(buffer, offset + 8),
            TelemetryType = BigEndianConverter.ReadUInt16(buffer, offset + 12),
            SampleRate = buffer[offset + 14],
            QualityIndicator = buffer[offset + 15]
        }};
    }}
}}

/// <summary>
/// Heartbeat secondary header (12 bytes).
/// </summary>
public class JscHeartbeatSecondaryHeader : JscSecondaryHeaderBase
{{
    public const int HeaderSize = 12;

    public override JscSecondaryHeaderType HeaderType => JscSecondaryHeaderType.Heartbeat;
    public override int Size => HeaderSize;

    /// <summary>Timestamp in milliseconds since Unix epoch.</summary>
    public ulong Timestamp {{ get; set; }}

    /// <summary>Sequence number.</summary>
    public ushort SequenceNumber {{ get; set; }}

    /// <summary>System status flags.</summary>
    public ushort SystemStatus {{ get; set; }}

    public override byte[] Serialize()
    {{
        var buffer = new byte[HeaderSize];
        Serialize(buffer, 0);
        return buffer;
    }}

    public override void Serialize(byte[] buffer, int offset)
    {{
        BigEndianConverter.WriteUInt64(buffer, offset, Timestamp);
        BigEndianConverter.WriteUInt16(buffer, offset + 8, SequenceNumber);
        BigEndianConverter.WriteUInt16(buffer, offset + 10, SystemStatus);
    }}

    public static JscHeartbeatSecondaryHeader Deserialize(byte[] buffer, int offset = 0)
    {{
        return new JscHeartbeatSecondaryHeader
        {{
            Timestamp = BigEndianConverter.ReadUInt64(buffer, offset),
            SequenceNumber = BigEndianConverter.ReadUInt16(buffer, offset + 8),
            SystemStatus = BigEndianConverter.ReadUInt16(buffer, offset + 10)
        }};
    }}
}}
";
    }

    private string GenerateJscMessage(string ns, JscProtocolConfig config)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// JSC protocol message following JSC-35199 specification.
/// </summary>
public class JscMessage
{{
    private static uint _nextMessageId = 1;
    private static readonly object _idLock = new();

    /// <summary>Checksum size in bytes (CRC-32).</summary>
    public const int ChecksumSize = 4;

    /// <summary>Gets or sets the primary header.</summary>
    public JscPrimaryHeader PrimaryHeader {{ get; set; }} = new();

    /// <summary>Gets or sets the secondary header (optional).</summary>
    public JscSecondaryHeaderBase? SecondaryHeader {{ get; set; }}

    /// <summary>Gets or sets the user data payload.</summary>
    public byte[] UserData {{ get; set; }} = Array.Empty<byte>();

    /// <summary>Gets or sets the CRC-32 checksum.</summary>
    public uint Checksum {{ get; set; }}

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
    {{
        uint messageId;
        lock (_idLock)
        {{
            messageId = _nextMessageId++;
        }}

        var message = new JscMessage
        {{
            PrimaryHeader = new JscPrimaryHeader
            {{
                Version = {config.ProtocolVersion},
                MessageType = messageType,
                MessageId = messageId,
                SourceMccId = {config.SourceMccId},
                DestinationMccId = {config.DefaultDestinationMccId},
                Priority = priority,
                Flags = flags,
                SecondaryHeaderLength = (ushort)(secondaryHeader?.Size ?? 0),
                UserDataLength = (ushort)userData.Length
            }},
            SecondaryHeader = secondaryHeader,
            UserData = userData
        }};

        return message;
    }}

    /// <summary>
    /// Serializes the message to a byte array.
    /// </summary>
    public byte[] Serialize()
    {{
        var buffer = new byte[TotalSize];
        var offset = 0;

        // Primary header
        PrimaryHeader.Serialize(buffer, offset);
        offset += JscPrimaryHeader.Size;

        // Secondary header (if present)
        if (SecondaryHeader != null)
        {{
            SecondaryHeader.Serialize(buffer, offset);
            offset += SecondaryHeader.Size;
        }}

        // User data
        if (UserData.Length > 0)
        {{
            Array.Copy(UserData, 0, buffer, offset, UserData.Length);
            offset += UserData.Length;
        }}

        // Calculate and write checksum
        Checksum = Crc32.Calculate(buffer, 0, offset);
        BigEndianConverter.WriteUInt32(buffer, offset, Checksum);

        return buffer;
    }}

    /// <summary>
    /// Deserializes a message from a byte array.
    /// </summary>
    public static JscMessage Deserialize(byte[] buffer)
    {{
        if (buffer.Length < JscPrimaryHeader.Size + ChecksumSize)
        {{
            throw new ArgumentException(""Buffer too small for JSC message."");
        }}

        var message = new JscMessage();
        var offset = 0;

        // Primary header
        message.PrimaryHeader = JscPrimaryHeader.Deserialize(buffer, offset);
        offset += JscPrimaryHeader.Size;

        // Secondary header (if present)
        if (message.PrimaryHeader.SecondaryHeaderLength > 0)
        {{
            message.SecondaryHeader = DeserializeSecondaryHeader(
                buffer, offset, message.PrimaryHeader.MessageType);
            offset += message.PrimaryHeader.SecondaryHeaderLength;
        }}

        // User data
        if (message.PrimaryHeader.UserDataLength > 0)
        {{
            message.UserData = new byte[message.PrimaryHeader.UserDataLength];
            Array.Copy(buffer, offset, message.UserData, 0, message.UserData.Length);
            offset += message.UserData.Length;
        }}

        // Checksum
        message.Checksum = BigEndianConverter.ReadUInt32(buffer, offset);

        // Verify checksum
        var calculatedChecksum = Crc32.Calculate(buffer, 0, offset);
        if (calculatedChecksum != message.Checksum)
        {{
            throw new InvalidOperationException(
                $""Checksum mismatch. Expected {{message.Checksum:X8}}, calculated {{calculatedChecksum:X8}}"");
        }}

        return message;
    }}

    private static JscSecondaryHeaderBase? DeserializeSecondaryHeader(
        byte[] buffer, int offset, JscMessageType messageType)
    {{
        return messageType switch
        {{
            JscMessageType.Command or JscMessageType.CommandAck
                => JscCommandSecondaryHeader.Deserialize(buffer, offset),
            JscMessageType.Telemetry
                => JscTelemetrySecondaryHeader.Deserialize(buffer, offset),
            JscMessageType.Heartbeat
                => JscHeartbeatSecondaryHeader.Deserialize(buffer, offset),
            _ => JscCommonSecondaryHeader.Deserialize(buffer, offset)
        }};
    }}
}}
";
    }

    private string GenerateJscMessageSerializer(string ns)
    {
        var abstractionsNs = ns.Replace(".Jsc", ".Abstractions");
        return $@"// Auto-generated code
using {abstractionsNs};

namespace {ns};

/// <summary>
/// JSC message serializer implementing IMessageSerializer.
/// </summary>
public class JscMessageSerializer : IMessageSerializer
{{
    /// <inheritdoc />
    public byte[] Serialize<T>(T value)
    {{
        if (value is JscMessage message)
        {{
            return message.Serialize();
        }}

        throw new ArgumentException($""Type {{typeof(T).Name}} is not a JscMessage"");
    }}

    /// <inheritdoc />
    public T? Deserialize<T>(byte[] data)
    {{
        if (typeof(T) == typeof(JscMessage))
        {{
            return (T)(object)JscMessage.Deserialize(data);
        }}

        throw new ArgumentException($""Type {{typeof(T).Name}} is not supported"");
    }}

    /// <inheritdoc />
    public object? Deserialize(byte[] data, Type type)
    {{
        if (type == typeof(JscMessage))
        {{
            return JscMessage.Deserialize(data);
        }}

        throw new ArgumentException($""Type {{type.Name}} is not supported"");
    }}
}}
";
    }

    private string GenerateServiceCollectionExtensions(string ns)
    {
        var abstractionsNs = ns.Replace(".Jsc", ".Abstractions");
        return $@"// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using {abstractionsNs};

namespace {ns};

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{{
    /// <summary>
    /// Adds JSC messaging to the service collection.
    /// </summary>
    public static IServiceCollection AddJscMessaging(this IServiceCollection services)
    {{
        services.AddSingleton<IMessageSerializer, JscMessageSerializer>();
        return services;
    }}
}}
";
    }

    private string GenerateCustomMessageType(string ns, JscMessageTypeConfig msgType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine($"namespace {ns}.Messages;");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(msgType.Description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {msgType.Description}");
            sb.AppendLine("/// </summary>");
        }

        sb.AppendLine($"public class {msgType.Name}");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>Message type code.</summary>");
        sb.AppendLine($"    public const byte TypeCode = 0x{msgType.TypeCode:X2};");
        sb.AppendLine();

        // Generate properties for user data fields
        foreach (var field in msgType.UserDataFields)
        {
            if (!string.IsNullOrWhiteSpace(field.Description))
            {
                sb.AppendLine($"    /// <summary>{field.Description}</summary>");
            }

            var clrType = GetClrType(field.Type);
            var initializer = GetDefaultInitializer(field.Type);
            sb.AppendLine($"    public {clrType} {field.Name} {{ get; set; }}{initializer}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GetClrType(string fieldType)
    {
        return fieldType.ToLowerInvariant() switch
        {
            "byte" => "byte",
            "ushort" => "ushort",
            "uint" => "uint",
            "ulong" => "ulong",
            "short" => "short",
            "int" => "int",
            "long" => "long",
            "float" => "float",
            "double" => "double",
            "string" => "string",
            "bytes" => "byte[]",
            _ => "byte[]"
        };
    }

    private static string GetDefaultInitializer(string fieldType)
    {
        return fieldType.ToLowerInvariant() switch
        {
            "string" => " = string.Empty;",
            "bytes" => " = Array.Empty<byte>();",
            _ => ""
        };
    }
}
