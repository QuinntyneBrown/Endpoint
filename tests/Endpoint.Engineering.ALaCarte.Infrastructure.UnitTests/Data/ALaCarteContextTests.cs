// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Endpoint.Engineering.ALaCarte.Infrastructure.UnitTests.Data;

public class ALaCarteContextTests : IDisposable
{
    private readonly ALaCarteContext _context;

    public ALaCarteContextTests()
    {
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ALaCarteContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region DbSet Tests

    [Fact]
    public void RepositoryConfigurations_DbSet_IsNotNull()
    {
        // Assert
        Assert.NotNull(_context.RepositoryConfigurations);
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task CanAddRepositoryConfiguration()
    {
        // Arrange
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo",
            Branch = "main"
        };

        // Act
        _context.RepositoryConfigurations.Add(config);
        await _context.SaveChangesAsync();

        // Assert
        var savedConfig = await _context.RepositoryConfigurations.FindAsync(config.RepositoryConfigurationId);
        Assert.NotNull(savedConfig);
        Assert.Equal(config.Url, savedConfig.Url);
    }

    [Fact]
    public async Task CanUpdateRepositoryConfiguration()
    {
        // Arrange
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo",
            Branch = "main"
        };
        _context.RepositoryConfigurations.Add(config);
        await _context.SaveChangesAsync();

        // Act
        config.Branch = "develop";
        await _context.SaveChangesAsync();

        // Assert
        var updatedConfig = await _context.RepositoryConfigurations.FindAsync(config.RepositoryConfigurationId);
        Assert.Equal("develop", updatedConfig!.Branch);
    }

    [Fact]
    public async Task CanDeleteRepositoryConfiguration()
    {
        // Arrange
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo",
            Branch = "main"
        };
        _context.RepositoryConfigurations.Add(config);
        await _context.SaveChangesAsync();

        // Act
        _context.RepositoryConfigurations.Remove(config);
        await _context.SaveChangesAsync();

        // Assert
        var deletedConfig = await _context.RepositoryConfigurations.FindAsync(config.RepositoryConfigurationId);
        Assert.Null(deletedConfig);
    }

    [Fact]
    public async Task CanQueryRepositoryConfigurations()
    {
        // Arrange
        var config1 = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo1",
            Branch = "main"
        };
        var config2 = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo2",
            Branch = "develop"
        };
        _context.RepositoryConfigurations.AddRange(config1, config2);
        await _context.SaveChangesAsync();

        // Act
        var developConfigs = await _context.RepositoryConfigurations
            .Where(c => c.Branch == "develop")
            .ToListAsync();

        // Assert
        Assert.Single(developConfigs);
        Assert.Equal("repo2", developConfigs[0].Url.Split('/').Last());
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public async Task RepositoryConfiguration_Branch_HasDefaultValue()
    {
        // Arrange
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo"
        };

        // Act
        _context.RepositoryConfigurations.Add(config);
        await _context.SaveChangesAsync();

        // Assert
        var savedConfig = await _context.RepositoryConfigurations.FindAsync(config.RepositoryConfigurationId);
        Assert.Equal("main", savedConfig!.Branch);
    }

    #endregion

    #region Nullable Property Tests

    [Fact]
    public async Task RepositoryConfiguration_LocalDirectory_CanBeNull()
    {
        // Arrange
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo",
            LocalDirectory = null
        };

        // Act
        _context.RepositoryConfigurations.Add(config);
        await _context.SaveChangesAsync();

        // Assert
        var savedConfig = await _context.RepositoryConfigurations.FindAsync(config.RepositoryConfigurationId);
        Assert.Null(savedConfig!.LocalDirectory);
    }

    [Fact]
    public async Task RepositoryConfiguration_LocalDirectory_CanBeSet()
    {
        // Arrange
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo",
            LocalDirectory = "/local/path"
        };

        // Act
        _context.RepositoryConfigurations.Add(config);
        await _context.SaveChangesAsync();

        // Assert
        var savedConfig = await _context.RepositoryConfigurations.FindAsync(config.RepositoryConfigurationId);
        Assert.Equal("/local/path", savedConfig!.LocalDirectory);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Context_ImplementsIALaCarteContext()
    {
        // Assert
        Assert.IsAssignableFrom<Endpoint.Engineering.ALaCarte.Core.IALaCarteContext>(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsNumberOfEntriesWritten()
    {
        // Arrange
        var config1 = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo1"
        };
        var config2 = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo2"
        };
        _context.RepositoryConfigurations.AddRange(config1, config2);

        // Act
        var result = await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(2, result);
    }

    #endregion
}
