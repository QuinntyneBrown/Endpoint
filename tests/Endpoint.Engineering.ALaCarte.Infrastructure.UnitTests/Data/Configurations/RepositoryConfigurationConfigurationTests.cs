// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Endpoint.Engineering.ALaCarte.Infrastructure.UnitTests.Data.Configurations;

public class RepositoryConfigurationConfigurationTests
{
    private readonly RepositoryConfigurationConfiguration _configuration;

    public RepositoryConfigurationConfigurationTests()
    {
        _configuration = new RepositoryConfigurationConfiguration();
    }

    [Fact]
    public void Configure_SetsCorrectPrimaryKey()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));

        // Assert
        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal(nameof(RepositoryConfiguration.RepositoryConfigurationId), primaryKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_SetsUrlMaxLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var urlProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.Url));

        // Assert
        Assert.NotNull(urlProperty);
        Assert.Equal(500, urlProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_SetsBranchMaxLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var branchProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.Branch));

        // Assert
        Assert.NotNull(branchProperty);
        Assert.Equal(200, branchProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_SetsBranchDefaultValue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var branchProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.Branch));

        // Assert
        Assert.NotNull(branchProperty);
        Assert.Equal("main", branchProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_SetsLocalDirectoryMaxLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var localDirProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.LocalDirectory));

        // Assert
        Assert.NotNull(localDirProperty);
        Assert.Equal(500, localDirProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_IgnoresFoldersCollection()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var foldersProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.Folders));

        // Assert - Folders should be ignored (not mapped)
        Assert.Null(foldersProperty);
    }

    [Fact]
    public void Configure_UrlIsNotRequired()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var urlProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.Url));

        // Assert
        Assert.NotNull(urlProperty);
        Assert.True(urlProperty.IsNullable);
    }

    [Fact]
    public void Configure_BranchIsRequired()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var branchProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.Branch));

        // Assert
        Assert.NotNull(branchProperty);
        Assert.False(branchProperty.IsNullable);
    }

    [Fact]
    public void Configure_LocalDirectoryIsNotRequired()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(RepositoryConfiguration));
        var localDirProperty = entityType?.FindProperty(nameof(RepositoryConfiguration.LocalDirectory));

        // Assert
        Assert.NotNull(localDirProperty);
        Assert.True(localDirProperty.IsNullable);
    }
}
