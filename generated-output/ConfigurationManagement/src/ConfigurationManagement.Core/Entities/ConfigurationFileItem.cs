// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ConfigurationManagement.Core.Entities;

public class ConfigurationFileItem
{
    public Guid ConfigurationFileItemId { get; set; }
    public Guid ConfigurationFileId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public ConfigurationValueType ValueType { get; set; } = ConfigurationValueType.String;
    public bool IsEncrypted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ConfigurationFile? ConfigurationFile { get; set; }
}

public enum ConfigurationValueType { String, Integer, Boolean, Json, Xml }