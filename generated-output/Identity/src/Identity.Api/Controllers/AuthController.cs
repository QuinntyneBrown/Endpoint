// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Identity.Core.DTOs;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepository;
    private readonly ITokenService tokenService;
    private readonly IPasswordHasher passwordHasher;
    private readonly ILogger<AuthController> logger;

    public AuthController(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ILogger<AuthController> logger)
    {
        this.userRepository = userRepository;
        this.tokenService = tokenService;
        this.passwordHasher = passwordHasher;
        this.logger = logger;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsAsync(request.Username, request.Email, cancellationToken))
        {
            return BadRequest(new { error = "Username or email already exists" });
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };

        var createdUser = await userRepository.AddAsync(user, cancellationToken);

        logger.LogInformation("User {Username} registered successfully", user.Username);

        var response = new RegisterResponse
        {
            UserId = createdUser.UserId,
            Username = createdUser.Username,
            Email = createdUser.Email
        };

        return CreatedAtAction(nameof(Register), new { id = createdUser.UserId }, response);
    }

    /// <summary>
    /// Authenticate a user and return tokens.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for user {Username}", request.Username);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { error = "User account is deactivated" });
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.LastLoginAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user, cancellationToken);

        logger.LogInformation("User {Username} logged in successfully", user.Username);

        return Ok(new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 86400 // 24 hours in seconds
        });
    }

    /// <summary>
    /// Logout a user (invalidate refresh token).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        // In a full implementation, you would invalidate the refresh token here
        return NoContent();
    }
}