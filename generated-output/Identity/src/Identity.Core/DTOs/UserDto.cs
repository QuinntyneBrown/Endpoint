// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Identity.Core.DTOs;

/// <summary>
/// Data transfer object for User.
/// </summary>
public sealed class UserDto
{
    public Guid UserId { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? LastLoginAt { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}