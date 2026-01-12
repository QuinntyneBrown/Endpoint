// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Identity.Core.Entities;

namespace Identity.Core.Interfaces;

/// <summary>
/// Repository interface for Role entities.
/// </summary>
public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);

    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task AssignToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    Task RemoveFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}