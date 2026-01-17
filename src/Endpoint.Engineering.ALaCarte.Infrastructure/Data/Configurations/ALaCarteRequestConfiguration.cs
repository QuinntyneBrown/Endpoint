// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Endpoint.Engineering.ALaCarte.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for ALaCarteRequest.
/// </summary>
public class ALaCarteRequestConfiguration : IEntityTypeConfiguration<ALaCarteRequest>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ALaCarteRequest> builder)
    {
        builder.HasKey(x => x.ALaCarteRequestId);

        builder.Property(x => x.OutputType)
            .IsRequired()
            .HasDefaultValue(OutputType.NotSpecified);

        builder.Property(x => x.Directory)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.SolutionName)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue("ALaCarte.sln");

        // Ignore Repositories collection as it's not stored directly - it's configuration data
        builder.Ignore(x => x.Repositories);
    }
}
