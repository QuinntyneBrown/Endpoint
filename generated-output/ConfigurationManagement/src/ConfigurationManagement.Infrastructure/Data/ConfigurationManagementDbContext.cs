// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationManagement.Infrastructure.Data;

public class ConfigurationManagementDbContext : DbContext
{
    public ConfigurationManagementDbContext(DbContextOptions<ConfigurationManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationFile> ConfigurationFiles => Set<ConfigurationFile>();
    public DbSet<ConfigurationFileItem> ConfigurationFileItems => Set<ConfigurationFileItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationManagementDbContext).Assembly);
    }
}