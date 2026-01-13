// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConfigurationManagement.Infrastructure.Data.Configurations;

public class ConfigurationFileConfiguration : IEntityTypeConfiguration<ConfigurationFile>
{
    public void Configure(EntityTypeBuilder<ConfigurationFile> builder)
    {
        builder.HasKey(x => x.ConfigurationFileId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Path).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.HasMany(x => x.Items)
            .WithOne(x => x.ConfigurationFile)
            .HasForeignKey(x => x.ConfigurationFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}