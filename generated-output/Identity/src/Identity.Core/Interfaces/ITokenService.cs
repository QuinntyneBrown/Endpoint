// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Identity.Core.Entities;

namespace Identity.Core.Interfaces;

/// <summary>
/// Service interface for JWT token operations.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);

    string GenerateRefreshToken();

    bool ValidateToken(string token);
}