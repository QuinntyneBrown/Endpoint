// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Identity.Core.Entities;

/// <summary>
/// Permission entity representing a system permission.
/// </summary>
public class Permission : IAggregateRoot
{
    public Guid PermissionId { get; set; }

    public required string Name { get; set; }

    public required string Resource { get; set; }

    public required string Action { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}