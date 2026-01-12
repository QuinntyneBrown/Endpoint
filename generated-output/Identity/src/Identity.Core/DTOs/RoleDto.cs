// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Identity.Core.DTOs;

/// <summary>
/// Data transfer object for Role.
/// </summary>
public sealed class RoleDto
{
    public Guid RoleId { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }
}