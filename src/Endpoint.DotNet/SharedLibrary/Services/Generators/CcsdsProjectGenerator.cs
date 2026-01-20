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
/// Generates the Shared.Messaging.Ccsds project with bit-level packing/unpacking.
/// </summary>
public class CcsdsProjectGenerator : ProjectGeneratorBase
{
    public CcsdsProjectGenerator(
        ILogger<CcsdsProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 13;

    public override bool ShouldGenerate(SharedLibraryConfig config)
        => config.Protocols.Ccsds?.Enabled == true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.Ccsds";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.Ccsds";

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

        // Generate BitPacker
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "BitPacker.cs"),
            GenerateBitPacker(ns),
            cancellationToken);

        // Generate BitUnpacker
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "BitUnpacker.cs"),
            GenerateBitUnpacker(ns),
            cancellationToken);

        // Generate CcsdsPacket base
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "CcsdsPacket.cs"),
            GenerateCcsdsPacketBase(ns),
            cancellationToken);

        // Generate CcsdsPrimaryHeader
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "CcsdsPrimaryHeader.cs"),
            GenerateCcsdsPrimaryHeader(ns),
            cancellationToken);

        // Generate CcsdsSerializer
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "CcsdsSerializer.cs"),
            GenerateCcsdsSerializer(ns),
            cancellationToken);

        // Generate PacketRegistry
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "PacketRegistry.cs"),
            GeneratePacketRegistry(ns),
            cancellationToken);

        // Generate ServiceCollectionExtensions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "ServiceCollectionExtensions.cs"),
            GenerateServiceCollectionExtensions(ns),
            cancellationToken);

        // Generate packet classes from config
        var packetsDirectory = FileSystem.Path.Combine(projectDirectory, "Packets");
        FileSystem.Directory.CreateDirectory(packetsDirectory);

        foreach (var packet in context.Config.CcsdsPackets)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(packetsDirectory, $"{packet.Name}.cs"),
                GeneratePacketClass(ns, packet),
                cancellationToken);
        }

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var projectName = $"{context.LibraryName}.Messaging.Ccsds";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/BitPacker.cs");
        preview.Files.Add($"{basePath}/BitUnpacker.cs");
        preview.Files.Add($"{basePath}/CcsdsPacket.cs");
        preview.Files.Add($"{basePath}/CcsdsPrimaryHeader.cs");
        preview.Files.Add($"{basePath}/CcsdsSerializer.cs");
        preview.Files.Add($"{basePath}/PacketRegistry.cs");
        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        foreach (var packet in context.Config.CcsdsPackets)
        {
            preview.Files.Add($"{basePath}/Packets/{packet.Name}.cs");
        }

        return Task.FromResult(preview);
    }

    private string GenerateBitPacker(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Packs values at bit-level precision into a byte array.
/// Follows big-endian byte order (network order) as per CCSDS standard.
/// </summary>
public class BitPacker
{{
    private readonly List<byte> _buffer = new();
    private int _currentByte;
    private int _bitPosition; // 0-7, MSB first

    /// <summary>
    /// Gets the packed data.
    /// </summary>
    public byte[] GetBytes()
    {{
        // Flush any partial byte
        if (_bitPosition > 0)
        {{
            _buffer.Add((byte)_currentByte);
        }}

        return _buffer.ToArray();
    }}

    /// <summary>
    /// Packs an unsigned value with the specified bit width.
    /// </summary>
    public void PackUnsigned(ulong value, int bitWidth)
    {{
        for (int i = bitWidth - 1; i >= 0; i--)
        {{
            int bit = (int)((value >> i) & 1);
            _currentByte = (_currentByte << 1) | bit;
            _bitPosition++;

            if (_bitPosition == 8)
            {{
                _buffer.Add((byte)_currentByte);
                _currentByte = 0;
                _bitPosition = 0;
            }}
        }}
    }}

    /// <summary>
    /// Packs a signed value with the specified bit width (two's complement).
    /// </summary>
    public void PackSigned(long value, int bitWidth)
    {{
        // Convert to unsigned representation for packing
        ulong unsigned = (ulong)value & ((1UL << bitWidth) - 1);
        PackUnsigned(unsigned, bitWidth);
    }}

    /// <summary>
    /// Packs a boolean as a single bit.
    /// </summary>
    public void PackBool(bool value) => PackUnsigned(value ? 1UL : 0UL, 1);

    /// <summary>
    /// Packs a byte (8 bits).
    /// </summary>
    public void PackByte(byte value) => PackUnsigned(value, 8);

    /// <summary>
    /// Packs a 16-bit unsigned integer.
    /// </summary>
    public void PackUInt16(ushort value) => PackUnsigned(value, 16);

    /// <summary>
    /// Packs a 32-bit unsigned integer.
    /// </summary>
    public void PackUInt32(uint value) => PackUnsigned(value, 32);

    /// <summary>
    /// Packs a 16-bit signed integer.
    /// </summary>
    public void PackInt16(short value) => PackSigned(value, 16);

    /// <summary>
    /// Packs a 32-bit signed integer.
    /// </summary>
    public void PackInt32(int value) => PackSigned(value, 32);

    /// <summary>
    /// Packs a 32-bit float.
    /// </summary>
    public void PackFloat32(float value)
    {{
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}

        foreach (var b in bytes)
        {{
            PackByte(b);
        }}
    }}

    /// <summary>
    /// Packs a 64-bit float.
    /// </summary>
    public void PackFloat64(double value)
    {{
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}

        foreach (var b in bytes)
        {{
            PackByte(b);
        }}
    }}

    /// <summary>
    /// Packs raw bytes.
    /// </summary>
    public void PackBytes(byte[] data)
    {{
        foreach (var b in data)
        {{
            PackByte(b);
        }}
    }}

    /// <summary>
    /// Packs spare/reserved bits.
    /// </summary>
    public void PackSpare(int bitWidth) => PackUnsigned(0, bitWidth);

    /// <summary>
    /// Aligns to the next byte boundary.
    /// </summary>
    public void AlignToByte()
    {{
        if (_bitPosition > 0)
        {{
            PackSpare(8 - _bitPosition);
        }}
    }}
}}
";
    }

    private string GenerateBitUnpacker(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Unpacks values at bit-level precision from a byte array.
/// Follows big-endian byte order (network order) as per CCSDS standard.
/// </summary>
public class BitUnpacker
{{
    private readonly byte[] _data;
    private int _bytePosition;
    private int _bitPosition; // 0-7, MSB first

    public BitUnpacker(byte[] data)
    {{
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }}

    /// <summary>
    /// Gets or sets the current bit position.
    /// </summary>
    public int Position
    {{
        get => _bytePosition * 8 + _bitPosition;
        set
        {{
            _bytePosition = value / 8;
            _bitPosition = value % 8;
        }}
    }}

    /// <summary>
    /// Gets the total number of bits available.
    /// </summary>
    public int TotalBits => _data.Length * 8;

    /// <summary>
    /// Unpacks an unsigned value with the specified bit width.
    /// </summary>
    public ulong UnpackUnsigned(int bitWidth)
    {{
        ulong value = 0;
        for (int i = 0; i < bitWidth; i++)
        {{
            if (_bytePosition >= _data.Length)
            {{
                throw new InvalidOperationException(""Attempted to read beyond end of data"");
            }}

            int bit = (_data[_bytePosition] >> (7 - _bitPosition)) & 1;
            value = (value << 1) | (ulong)bit;

            _bitPosition++;
            if (_bitPosition == 8)
            {{
                _bitPosition = 0;
                _bytePosition++;
            }}
        }}

        return value;
    }}

    /// <summary>
    /// Unpacks a signed value with the specified bit width (two's complement).
    /// </summary>
    public long UnpackSigned(int bitWidth)
    {{
        ulong unsigned = UnpackUnsigned(bitWidth);

        // Sign extend if necessary
        if ((unsigned & (1UL << (bitWidth - 1))) != 0)
        {{
            // Negative number - extend sign
            unsigned |= ~((1UL << bitWidth) - 1);
        }}

        return (long)unsigned;
    }}

    /// <summary>
    /// Unpacks a boolean from a single bit.
    /// </summary>
    public bool UnpackBool() => UnpackUnsigned(1) != 0;

    /// <summary>
    /// Unpacks a byte (8 bits).
    /// </summary>
    public byte UnpackByte() => (byte)UnpackUnsigned(8);

    /// <summary>
    /// Unpacks a 16-bit unsigned integer.
    /// </summary>
    public ushort UnpackUInt16() => (ushort)UnpackUnsigned(16);

    /// <summary>
    /// Unpacks a 32-bit unsigned integer.
    /// </summary>
    public uint UnpackUInt32() => (uint)UnpackUnsigned(32);

    /// <summary>
    /// Unpacks a 16-bit signed integer.
    /// </summary>
    public short UnpackInt16() => (short)UnpackSigned(16);

    /// <summary>
    /// Unpacks a 32-bit signed integer.
    /// </summary>
    public int UnpackInt32() => (int)UnpackSigned(32);

    /// <summary>
    /// Unpacks a 32-bit float.
    /// </summary>
    public float UnpackFloat32()
    {{
        var bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {{
            bytes[i] = UnpackByte();
        }}

        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}

        return BitConverter.ToSingle(bytes, 0);
    }}

    /// <summary>
    /// Unpacks a 64-bit float.
    /// </summary>
    public double UnpackFloat64()
    {{
        var bytes = new byte[8];
        for (int i = 0; i < 8; i++)
        {{
            bytes[i] = UnpackByte();
        }}

        if (BitConverter.IsLittleEndian)
        {{
            Array.Reverse(bytes);
        }}

        return BitConverter.ToDouble(bytes, 0);
    }}

    /// <summary>
    /// Unpacks raw bytes.
    /// </summary>
    public byte[] UnpackBytes(int count)
    {{
        var bytes = new byte[count];
        for (int i = 0; i < count; i++)
        {{
            bytes[i] = UnpackByte();
        }}

        return bytes;
    }}

    /// <summary>
    /// Skips spare/reserved bits.
    /// </summary>
    public void SkipBits(int bitWidth) => UnpackUnsigned(bitWidth);

    /// <summary>
    /// Aligns to the next byte boundary.
    /// </summary>
    public void AlignToByte()
    {{
        if (_bitPosition > 0)
        {{
            SkipBits(8 - _bitPosition);
        }}
    }}
}}
";
    }

    private string GenerateCcsdsPacketBase(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Base class for CCSDS space packets.
/// </summary>
public abstract class CcsdsPacket
{{
    /// <summary>
    /// Gets the Application Process Identifier (APID) for this packet type.
    /// </summary>
    public abstract int Apid {{ get; }}

    /// <summary>
    /// Gets the packet type (0 = TM, 1 = TC).
    /// </summary>
    public abstract int PacketType {{ get; }}

    /// <summary>
    /// Gets the sequence count for this packet instance.
    /// </summary>
    public int SequenceCount {{ get; set; }}

    /// <summary>
    /// Gets or sets the timestamp (if secondary header is present).
    /// </summary>
    public DateTimeOffset? Timestamp {{ get; set; }}

    /// <summary>
    /// Packs the packet data fields into the bit packer.
    /// </summary>
    public abstract void PackData(BitPacker packer);

    /// <summary>
    /// Unpacks the packet data fields from the bit unpacker.
    /// </summary>
    public abstract void UnpackData(BitUnpacker unpacker);
}}
";
    }

    private string GenerateCcsdsPrimaryHeader(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// CCSDS Space Packet Primary Header (6 bytes / 48 bits).
/// </summary>
public class CcsdsPrimaryHeader
{{
    /// <summary>
    /// Packet version number (3 bits). Always 0 for CCSDS.
    /// </summary>
    public int Version {{ get; set; }}

    /// <summary>
    /// Packet type (1 bit). 0 = Telemetry, 1 = Telecommand.
    /// </summary>
    public int PacketType {{ get; set; }}

    /// <summary>
    /// Secondary header flag (1 bit). 1 = present, 0 = absent.
    /// </summary>
    public bool SecondaryHeaderFlag {{ get; set; }}

    /// <summary>
    /// Application Process Identifier (11 bits). Range 0-2047.
    /// </summary>
    public int Apid {{ get; set; }}

    /// <summary>
    /// Sequence flags (2 bits). 3 = unsegmented.
    /// </summary>
    public int SequenceFlags {{ get; set; }} = 3;

    /// <summary>
    /// Packet sequence count (14 bits). Range 0-16383.
    /// </summary>
    public int SequenceCount {{ get; set; }}

    /// <summary>
    /// Packet data length (16 bits). Length of data field - 1.
    /// </summary>
    public int PacketDataLength {{ get; set; }}

    /// <summary>
    /// Packs the primary header into the bit packer.
    /// </summary>
    public void Pack(BitPacker packer)
    {{
        packer.PackUnsigned((ulong)Version, 3);
        packer.PackUnsigned((ulong)PacketType, 1);
        packer.PackBool(SecondaryHeaderFlag);
        packer.PackUnsigned((ulong)Apid, 11);
        packer.PackUnsigned((ulong)SequenceFlags, 2);
        packer.PackUnsigned((ulong)SequenceCount, 14);
        packer.PackUnsigned((ulong)PacketDataLength, 16);
    }}

    /// <summary>
    /// Unpacks the primary header from the bit unpacker.
    /// </summary>
    public static CcsdsPrimaryHeader Unpack(BitUnpacker unpacker)
    {{
        return new CcsdsPrimaryHeader
        {{
            Version = (int)unpacker.UnpackUnsigned(3),
            PacketType = (int)unpacker.UnpackUnsigned(1),
            SecondaryHeaderFlag = unpacker.UnpackBool(),
            Apid = (int)unpacker.UnpackUnsigned(11),
            SequenceFlags = (int)unpacker.UnpackUnsigned(2),
            SequenceCount = (int)unpacker.UnpackUnsigned(14),
            PacketDataLength = (int)unpacker.UnpackUnsigned(16),
        }};
    }}
}}
";
    }

    private string GenerateCcsdsSerializer(string ns)
    {
        var abstractionsNs = ns.Replace(".Ccsds", ".Abstractions");
        return $@"// Auto-generated code
using {abstractionsNs};

namespace {ns};

/// <summary>
/// Serializer for CCSDS space packets.
/// </summary>
public class CcsdsSerializer : IMessageSerializer
{{
    private readonly PacketRegistry _registry;

    public CcsdsSerializer(PacketRegistry registry)
    {{
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }}

    public byte[] Serialize<T>(T value)
    {{
        if (value is not CcsdsPacket packet)
        {{
            throw new ArgumentException($""Type {{typeof(T).Name}} is not a CcsdsPacket"");
        }}

        var packer = new BitPacker();

        // Pack data to get its length
        var dataPacker = new BitPacker();
        packet.PackData(dataPacker);
        var dataBytes = dataPacker.GetBytes();

        // Create and pack header
        var header = new CcsdsPrimaryHeader
        {{
            Version = 0,
            PacketType = packet.PacketType,
            SecondaryHeaderFlag = packet.Timestamp.HasValue,
            Apid = packet.Apid,
            SequenceFlags = 3, // Unsegmented
            SequenceCount = packet.SequenceCount & 0x3FFF,
            PacketDataLength = dataBytes.Length - 1 + (packet.Timestamp.HasValue ? 6 : 0),
        }};

        header.Pack(packer);

        // Pack secondary header if present (CUC time: 4 bytes coarse + 2 bytes fine)
        if (packet.Timestamp.HasValue)
        {{
            var epoch = new DateTimeOffset(1958, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var totalSeconds = (packet.Timestamp.Value - epoch).TotalSeconds;
            var coarse = (uint)totalSeconds;
            var fine = (ushort)((totalSeconds - coarse) * 65536);

            packer.PackUInt32(coarse);
            packer.PackUInt16(fine);
        }}

        // Pack data
        packer.PackBytes(dataBytes);

        return packer.GetBytes();
    }}

    public T? Deserialize<T>(byte[] data)
    {{
        var result = Deserialize(data, typeof(T));
        return result is T typed ? typed : default;
    }}

    public object? Deserialize(byte[] data, Type type)
    {{
        var unpacker = new BitUnpacker(data);

        // Unpack header
        var header = CcsdsPrimaryHeader.Unpack(unpacker);

        // Get packet type from registry
        var packetType = _registry.GetPacketType(header.Apid);
        if (packetType == null)
        {{
            throw new InvalidOperationException($""No packet type registered for APID {{header.Apid}}"");
        }}

        if (!type.IsAssignableFrom(packetType))
        {{
            throw new InvalidOperationException($""Registered type {{packetType.Name}} is not assignable to {{type.Name}}"");
        }}

        // Create packet instance
        var packet = (CcsdsPacket?)Activator.CreateInstance(packetType);
        if (packet == null)
        {{
            throw new InvalidOperationException($""Could not create instance of {{packetType.Name}}"");
        }}

        packet.SequenceCount = header.SequenceCount;

        // Unpack secondary header if present
        if (header.SecondaryHeaderFlag)
        {{
            var coarse = unpacker.UnpackUInt32();
            var fine = unpacker.UnpackUInt16();
            var epoch = new DateTimeOffset(1958, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var totalSeconds = coarse + (fine / 65536.0);
            packet.Timestamp = epoch.AddSeconds(totalSeconds);
        }}

        // Unpack data
        packet.UnpackData(unpacker);

        return packet;
    }}
}}
";
    }

    private string GeneratePacketRegistry(string ns)
    {
        return $@"// Auto-generated code
using System.Collections.Concurrent;

namespace {ns};

/// <summary>
/// Registry for mapping APIDs to packet types.
/// </summary>
public class PacketRegistry
{{
    private readonly ConcurrentDictionary<int, Type> _packetTypes = new();

    /// <summary>
    /// Registers a packet type by its APID.
    /// </summary>
    /// <typeparam name=""T"">The packet type.</typeparam>
    public void Register<T>() where T : CcsdsPacket, new()
    {{
        var instance = new T();
        _packetTypes[instance.Apid] = typeof(T);
    }}

    /// <summary>
    /// Registers a packet type by APID.
    /// </summary>
    public void Register(int apid, Type packetType)
    {{
        if (!typeof(CcsdsPacket).IsAssignableFrom(packetType))
        {{
            throw new ArgumentException($""Type {{packetType.Name}} must derive from CcsdsPacket"");
        }}

        _packetTypes[apid] = packetType;
    }}

    /// <summary>
    /// Gets the packet type for an APID.
    /// </summary>
    public Type? GetPacketType(int apid)
    {{
        return _packetTypes.TryGetValue(apid, out var type) ? type : null;
    }}

    /// <summary>
    /// Gets all registered APIDs.
    /// </summary>
    public IEnumerable<int> GetRegisteredApids() => _packetTypes.Keys;
}}
";
    }

    private string GenerateServiceCollectionExtensions(string ns)
    {
        var abstractionsNs = ns.Replace(".Ccsds", ".Abstractions");
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
    /// Adds CCSDS serialization to the service collection.
    /// </summary>
    /// <param name=""services"">The service collection.</param>
    /// <param name=""configureRegistry"">Action to configure the packet registry.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCcsdsSerializer(
        this IServiceCollection services,
        Action<PacketRegistry>? configureRegistry = null)
    {{
        var registry = new PacketRegistry();
        configureRegistry?.Invoke(registry);

        services.AddSingleton(registry);
        services.AddSingleton<IMessageSerializer, CcsdsSerializer>();

        return services;
    }}
}}
";
    }

    private string GeneratePacketClass(string ns, CcsdsPacketConfig packet)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine($"namespace {ns}.Packets;");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(packet.Description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {packet.Description}");
            sb.AppendLine("/// </summary>");
        }

        sb.AppendLine($"public class {packet.Name} : CcsdsPacket");
        sb.AppendLine("{");

        // Override Apid
        sb.AppendLine($"    public override int Apid => {packet.Apid};");
        sb.AppendLine();

        // Override PacketType
        sb.AppendLine($"    public override int PacketType => {packet.PacketType};");
        sb.AppendLine();

        // Generate properties for each field
        foreach (var field in packet.Fields.Where(f => !f.IsSpare))
        {
            if (!string.IsNullOrWhiteSpace(field.Description))
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// {field.Description}");
                if (!string.IsNullOrWhiteSpace(field.Unit))
                {
                    sb.AppendLine($"    /// Unit: {field.Unit}");
                }

                sb.AppendLine("    /// </summary>");
            }

            var propertyType = GetClrType(field);
            sb.AppendLine($"    public {propertyType} {field.Name} {{ get; set; }}");
            sb.AppendLine();
        }

        // Generate PackData method
        sb.AppendLine("    public override void PackData(BitPacker packer)");
        sb.AppendLine("    {");
        foreach (var field in packet.Fields)
        {
            if (field.IsSpare)
            {
                sb.AppendLine($"        packer.PackSpare({field.BitSize}); // spare");
            }
            else
            {
                var packCall = GetPackCall(field);
                sb.AppendLine($"        {packCall}");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate UnpackData method
        sb.AppendLine("    public override void UnpackData(BitUnpacker unpacker)");
        sb.AppendLine("    {");
        foreach (var field in packet.Fields)
        {
            if (field.IsSpare)
            {
                sb.AppendLine($"        unpacker.SkipBits({field.BitSize}); // spare");
            }
            else
            {
                var unpackCall = GetUnpackCall(field);
                sb.AppendLine($"        {unpackCall}");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GetClrType(CcsdsFieldConfig field)
    {
        return field.DataType.ToLowerInvariant() switch
        {
            "uint8" => "byte",
            "uint16" => "ushort",
            "uint32" => "uint",
            "uint64" => "ulong",
            "int8" => "sbyte",
            "int16" => "short",
            "int32" => "int",
            "int64" => "long",
            "float32" => "float",
            "float64" => "double",
            "bool" => "bool",
            "bitfield" => field.BitSize <= 8 ? "byte" :
                          field.BitSize <= 16 ? "ushort" :
                          field.BitSize <= 32 ? "uint" : "ulong",
            _ => "byte",
        };
    }

    private string GetPackCall(CcsdsFieldConfig field)
    {
        return field.DataType.ToLowerInvariant() switch
        {
            "uint8" => $"packer.PackByte({field.Name});",
            "uint16" => $"packer.PackUInt16({field.Name});",
            "uint32" => $"packer.PackUInt32({field.Name});",
            "int8" => $"packer.PackSigned({field.Name}, 8);",
            "int16" => $"packer.PackInt16({field.Name});",
            "int32" => $"packer.PackInt32({field.Name});",
            "float32" => $"packer.PackFloat32({field.Name});",
            "float64" => $"packer.PackFloat64({field.Name});",
            "bool" => $"packer.PackBool({field.Name});",
            "bitfield" => $"packer.PackUnsigned({field.Name}, {field.BitSize});",
            _ => $"packer.PackUnsigned({field.Name}, {field.BitSize});",
        };
    }

    private string GetUnpackCall(CcsdsFieldConfig field)
    {
        return field.DataType.ToLowerInvariant() switch
        {
            "uint8" => $"{field.Name} = unpacker.UnpackByte();",
            "uint16" => $"{field.Name} = unpacker.UnpackUInt16();",
            "uint32" => $"{field.Name} = unpacker.UnpackUInt32();",
            "int8" => $"{field.Name} = (sbyte)unpacker.UnpackSigned(8);",
            "int16" => $"{field.Name} = unpacker.UnpackInt16();",
            "int32" => $"{field.Name} = unpacker.UnpackInt32();",
            "float32" => $"{field.Name} = unpacker.UnpackFloat32();",
            "float64" => $"{field.Name} = unpacker.UnpackFloat64();",
            "bool" => $"{field.Name} = unpacker.UnpackBool();",
            "bitfield" => $"{field.Name} = ({GetClrType(field)})unpacker.UnpackUnsigned({field.BitSize});",
            _ => $"{field.Name} = ({GetClrType(field)})unpacker.UnpackUnsigned({field.BitSize});",
        };
    }
}
