// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Configuration for a CCSDS packet.
/// </summary>
public class CcsdsPacketConfig
{
    /// <summary>
    /// Gets or sets the packet name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the packet description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Application Process Identifier (APID).
    /// </summary>
    public int Apid { get; set; }

    /// <summary>
    /// Gets or sets the packet type (0 = TM, 1 = TC).
    /// </summary>
    public int PacketType { get; set; }

    /// <summary>
    /// Gets or sets whether the packet has secondary header.
    /// </summary>
    public bool HasSecondaryHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets the secondary header configuration.
    /// </summary>
    public CcsdsSecondaryHeaderConfig? SecondaryHeader { get; set; }

    /// <summary>
    /// Gets or sets the data fields.
    /// </summary>
    public List<CcsdsFieldConfig> Fields { get; set; } = new();
}

/// <summary>
/// Configuration for CCSDS secondary header.
/// </summary>
public class CcsdsSecondaryHeaderConfig
{
    /// <summary>
    /// Gets or sets the time format (CUC, CDS, or Custom).
    /// </summary>
    public string TimeFormat { get; set; } = "CUC";

    /// <summary>
    /// Gets or sets the coarse time size in bytes (for CUC).
    /// </summary>
    public int CoarseTimeBytes { get; set; } = 4;

    /// <summary>
    /// Gets or sets the fine time size in bytes (for CUC).
    /// </summary>
    public int FineTimeBytes { get; set; } = 2;

    /// <summary>
    /// Gets or sets additional secondary header fields.
    /// </summary>
    public List<CcsdsFieldConfig> AdditionalFields { get; set; } = new();
}

/// <summary>
/// Configuration for a CCSDS data field with bit-level precision.
/// </summary>
public class CcsdsFieldConfig
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type (uint8, uint16, uint32, int8, int16, int32, float32, float64, bool, enum, bitfield).
    /// </summary>
    public string DataType { get; set; } = "uint8";

    /// <summary>
    /// Gets or sets the bit size for the field.
    /// </summary>
    public int BitSize { get; set; }

    /// <summary>
    /// Gets or sets the bit offset within the byte (for sub-byte fields).
    /// </summary>
    public int BitOffset { get; set; }

    /// <summary>
    /// Gets or sets the byte offset from start of data field.
    /// </summary>
    public int ByteOffset { get; set; }

    /// <summary>
    /// Gets or sets the array length (0 or 1 for scalar).
    /// </summary>
    public int ArrayLength { get; set; } = 1;

    /// <summary>
    /// Gets or sets the enumeration values (for enum type).
    /// </summary>
    public Dictionary<string, int>? EnumValues { get; set; }

    /// <summary>
    /// Gets or sets the scale factor for calibration.
    /// </summary>
    public double? Scale { get; set; }

    /// <summary>
    /// Gets or sets the offset for calibration.
    /// </summary>
    public double? Offset { get; set; }

    /// <summary>
    /// Gets or sets the engineering unit.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets whether this field is a spare/reserved.
    /// </summary>
    public bool IsSpare { get; set; }
}
