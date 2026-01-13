// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ConfigurationManagement.Core.DTOs;

public class ConfigurationFileItemDto
{
    public Guid ConfigurationFileItemId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}