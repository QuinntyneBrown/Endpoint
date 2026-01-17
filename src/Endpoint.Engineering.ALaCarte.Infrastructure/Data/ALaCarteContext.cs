// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Endpoint.Engineering.ALaCarte.Infrastructure.Data;

/// <summary>
/// Database context implementation for ALaCarte operations.
/// </summary>
public class ALaCarteContext : DbContext, IALaCarteContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ALaCarteContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public ALaCarteContext(DbContextOptions<ALaCarteContext> options)
        : base(options)
    {
    }

    /// <inheritdoc/>
    public DbSet<RepositoryConfiguration> RepositoryConfigurations { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ALaCarteContext).Assembly);
    }
}
