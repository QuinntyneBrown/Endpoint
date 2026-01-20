// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Configuration for messaging protocols.
/// </summary>
public class ProtocolsConfig
{
    /// <summary>
    /// Gets or sets the Azure Service Bus configuration.
    /// </summary>
    public AzureServiceBusConfig? AzureServiceBus { get; set; }

    /// <summary>
    /// Gets or sets the Redis Pub/Sub configuration.
    /// </summary>
    public RedisConfig? Redis { get; set; }

    /// <summary>
    /// Gets or sets the UDP Multicast configuration.
    /// </summary>
    public UdpMulticastConfig? UdpMulticast { get; set; }

    /// <summary>
    /// Gets or sets the CCSDS configuration.
    /// </summary>
    public CcsdsProtocolConfig? Ccsds { get; set; }

    /// <summary>
    /// Gets or sets the JSC (Johnson Space Center) protocol configuration.
    /// </summary>
    public JscProtocolConfig? Jsc { get; set; }
}

/// <summary>
/// Azure Service Bus protocol configuration.
/// </summary>
public class AzureServiceBusConfig
{
    /// <summary>
    /// Gets or sets whether Azure Service Bus is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the default topic name.
    /// </summary>
    public string DefaultTopic { get; set; } = "events";

    /// <summary>
    /// Gets or sets whether to use sessions.
    /// </summary>
    public bool UseSessions { get; set; }
}

/// <summary>
/// Redis Pub/Sub protocol configuration.
/// </summary>
public class RedisConfig
{
    /// <summary>
    /// Gets or sets whether Redis is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the default channel prefix.
    /// </summary>
    public string ChannelPrefix { get; set; } = "events";
}

/// <summary>
/// UDP Multicast protocol configuration.
/// </summary>
public class UdpMulticastConfig
{
    /// <summary>
    /// Gets or sets whether UDP Multicast is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the default multicast group.
    /// </summary>
    public string DefaultMulticastGroup { get; set; } = "239.0.0.1";

    /// <summary>
    /// Gets or sets the default port.
    /// </summary>
    public int DefaultPort { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the default TTL.
    /// </summary>
    public int DefaultTtl { get; set; } = 32;
}

/// <summary>
/// CCSDS protocol configuration.
/// </summary>
public class CcsdsProtocolConfig
{
    /// <summary>
    /// Gets or sets whether CCSDS is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets whether to include secondary header.
    /// </summary>
    public bool IncludeSecondaryHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets the secondary header format (CUC or CDS).
    /// </summary>
    public string SecondaryHeaderFormat { get; set; } = "CUC";

    /// <summary>
    /// Gets or sets the spacecraft ID.
    /// </summary>
    public int SpacecraftId { get; set; }
}

/// <summary>
/// JSC (Johnson Space Center) protocol configuration following JSC-35199 specification.
/// </summary>
public class JscProtocolConfig
{
    /// <summary>
    /// Gets or sets whether JSC protocol is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the source MCC identifier (2 bytes).
    /// </summary>
    public int SourceMccId { get; set; } = 1;

    /// <summary>
    /// Gets or sets the default destination MCC identifier (2 bytes).
    /// </summary>
    public int DefaultDestinationMccId { get; set; } = 2;

    /// <summary>
    /// Gets or sets the protocol version (default 1).
    /// </summary>
    public int ProtocolVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to include CRC-32 checksum.
    /// </summary>
    public bool IncludeCrc32 { get; set; } = true;

    /// <summary>
    /// Gets or sets the message types to generate.
    /// </summary>
    public List<JscMessageTypeConfig> MessageTypes { get; set; } = new();
}

/// <summary>
/// Configuration for a JSC message type.
/// </summary>
public class JscMessageTypeConfig
{
    /// <summary>
    /// Gets or sets the message type name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message type code (hex value 0x01-0xFF).
    /// </summary>
    public int TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the secondary header type (Common, Command, Telemetry, FileTransfer, Heartbeat).
    /// </summary>
    public string SecondaryHeaderType { get; set; } = "Common";

    /// <summary>
    /// Gets or sets the user data fields.
    /// </summary>
    public List<JscFieldConfig> UserDataFields { get; set; } = new();
}

/// <summary>
/// Configuration for a JSC message field.
/// </summary>
public class JscFieldConfig
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field type (byte, ushort, uint, ulong, string, bytes).
    /// </summary>
    public string Type { get; set; } = "byte";

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the fixed length for string/bytes types.
    /// </summary>
    public int? Length { get; set; }
}
