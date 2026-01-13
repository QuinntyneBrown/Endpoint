// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ConfigurationManagement.Core.Entities;

public class ConfigurationFile
{
    public Guid ConfigurationFileId { get; set; }
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required string ContentType { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ConfigurationFileItem> Items { get; set; } = new List<ConfigurationFileItem>();
}