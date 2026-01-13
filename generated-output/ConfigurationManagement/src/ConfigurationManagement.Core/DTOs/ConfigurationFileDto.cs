// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ConfigurationManagement.Core.DTOs;

public class ConfigurationFileDto
{
    public Guid ConfigurationFileId { get; set; }
    public required string Name { get; set; }
    public required string Path { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}