// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConfigurationManagement.Infrastructure.Data.Configurations;

public class ConfigurationFileItemConfiguration : IEntityTypeConfiguration<ConfigurationFileItem>
{
    public void Configure(EntityTypeBuilder<ConfigurationFileItem> builder)
    {
        builder.HasKey(x => x.ConfigurationFileItemId);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Value).IsRequired();
    }
}