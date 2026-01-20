// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Configuration for message serializers.
/// </summary>
public class SerializersConfig
{
    /// <summary>
    /// Gets or sets the MessagePack configuration.
    /// </summary>
    public MessagePackConfig? MessagePack { get; set; }

    /// <summary>
    /// Gets or sets the JSON configuration.
    /// </summary>
    public JsonSerializerConfig? Json { get; set; }

    /// <summary>
    /// Gets or sets the CCSDS binary serializer configuration.
    /// </summary>
    public CcsdsBinaryConfig? CcsdsBinary { get; set; }
}

/// <summary>
/// MessagePack serializer configuration.
/// </summary>
public class MessagePackConfig
{
    /// <summary>
    /// Gets or sets whether MessagePack is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets whether to use LZ4 compression.
    /// </summary>
    public bool UseLz4Compression { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use contract-less serialization.
    /// </summary>
    public bool UseContractless { get; set; }
}

/// <summary>
/// JSON serializer configuration.
/// </summary>
public class JsonSerializerConfig
{
    /// <summary>
    /// Gets or sets whether JSON is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets whether to use camelCase naming.
    /// </summary>
    public bool UseCamelCase { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to indent JSON output.
    /// </summary>
    public bool WriteIndented { get; set; }
}

/// <summary>
/// CCSDS binary serializer configuration.
/// </summary>
public class CcsdsBinaryConfig
{
    /// <summary>
    /// Gets or sets whether CCSDS binary is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets whether to use big endian byte order.
    /// </summary>
    public bool BigEndian { get; set; } = true;
}
