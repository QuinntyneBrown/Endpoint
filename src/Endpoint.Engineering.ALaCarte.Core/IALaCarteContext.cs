// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Endpoint.Engineering.ALaCarte.Core;

/// <summary>
/// Database context interface for ALaCarte operations.
/// </summary>
public interface IALaCarteContext
{
    /// <summary>
    /// Gets or sets the repository configurations.
    /// </summary>
    DbSet<RepositoryConfiguration> RepositoryConfigurations { get; set; }

    /// <summary>
    /// Gets or sets the ALaCarte requests.
    /// </summary>
    DbSet<ALaCarteRequest> ALaCarteRequests { get; set; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
