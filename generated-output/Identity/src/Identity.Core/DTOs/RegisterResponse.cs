// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Identity.Core.DTOs;

/// <summary>
/// Response model for successful registration.
/// </summary>
public sealed class RegisterResponse
{
    public Guid UserId { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }
}