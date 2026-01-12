// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Identity.Core.DTOs;

/// <summary>
/// Response model for successful login.
/// </summary>
public sealed class LoginResponse
{
    public required string Token { get; init; }

    public required string RefreshToken { get; init; }

    public required int ExpiresIn { get; init; }
}