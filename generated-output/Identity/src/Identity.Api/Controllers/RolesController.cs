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
/// Controller for role management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository roleRepository;
    private readonly ILogger<RolesController> logger;

    public RolesController(
        IRoleRepository roleRepository,
        ILogger<RolesController> logger)
    {
        this.roleRepository = roleRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Get all roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);

        var roleDtos = roles.Select(role => new RoleDto
        {
            RoleId = role.RoleId,
            Name = role.Name,
            Description = role.Description
        });

        return Ok(roleDtos);
    }

    /// <summary>
    /// Create a new role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDto>> Create(
        [FromBody] RoleDto request,
        CancellationToken cancellationToken)
    {
        var existingRole = await roleRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingRole != null)
        {
            return BadRequest(new { error = "Role with this name already exists" });
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        var createdRole = await roleRepository.AddAsync(role, cancellationToken);
        logger.LogInformation("Role {RoleName} created", role.Name);

        var response = new RoleDto
        {
            RoleId = createdRole.RoleId,
            Name = createdRole.Name,
            Description = createdRole.Description
        };

        return CreatedAtAction(nameof(GetAll), response);
    }

    /// <summary>
    /// Delete a role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken);

        if (role == null)
        {
            return NotFound();
        }

        await roleRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Role {RoleId} deleted", id);

        return NoContent();
    }

    /// <summary>
    /// Assign roles to a user.
    /// </summary>
    [HttpPut("/api/users/{userId:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRolesToUser(
        Guid userId,
        [FromBody] List<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        foreach (var roleId in roleIds)
        {
            await roleRepository.AssignToUserAsync(userId, roleId, cancellationToken);
        }

        logger.LogInformation("Roles assigned to user {UserId}", userId);
        return Ok();
    }
}