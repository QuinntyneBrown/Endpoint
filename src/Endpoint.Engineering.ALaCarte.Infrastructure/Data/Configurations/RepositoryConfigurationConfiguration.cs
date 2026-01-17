// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Endpoint.Engineering.ALaCarte.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for RepositoryConfiguration.
/// </summary>
public class RepositoryConfigurationConfiguration : IEntityTypeConfiguration<RepositoryConfiguration>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<RepositoryConfiguration> builder)
    {
        builder.HasKey(x => x.RepositoryConfigurationId);

        builder.Property(x => x.Url)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Branch)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue("main");

        builder.Property(x => x.LocalDirectory)
            .HasMaxLength(500)
            .IsRequired(false);

        // Ignore Folders collection as it's not stored directly - it's configuration data
        builder.Ignore(x => x.Folders);
    }
}
