// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfigurations;
using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.UnitTests.Features.RepositoryConfigurations.Queries;

public class GetRepositoryConfigurationsHandlerTests
{
    private readonly Mock<ILogger<GetRepositoryConfigurationsHandler>> _mockLogger;

    public GetRepositoryConfigurationsHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetRepositoryConfigurationsHandler>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetRepositoryConfigurationsHandler(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockContext = new Mock<IALaCarteContext>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetRepositoryConfigurationsHandler(mockContext.Object, null!));
    }

    #endregion

    #region Handle Tests

    [Fact]
    public async Task Handle_ReturnsAllConfigurations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        context.RepositoryConfigurations.AddRange(
            new RepositoryConfiguration
            {
                RepositoryConfigurationId = Guid.NewGuid(),
                Url = "https://github.com/user/repo1",
                Branch = "main"
            },
            new RepositoryConfiguration
            {
                RepositoryConfigurationId = Guid.NewGuid(),
                Url = "https://github.com/user/repo2",
                Branch = "develop",
                LocalDirectory = "/local/path"
            }
        );
        await context.SaveChangesAsync();

        var handler = new GetRepositoryConfigurationsHandler(context, _mockLogger.Object);
        var request = new GetRepositoryConfigurationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RepositoryConfigurations);
        Assert.Equal(2, result.RepositoryConfigurations.Count);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoConfigurations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        var handler = new GetRepositoryConfigurationsHandler(context, _mockLogger.Object);
        var request = new GetRepositoryConfigurationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RepositoryConfigurations);
        Assert.Empty(result.RepositoryConfigurations);
    }

    [Fact]
    public async Task Handle_MapsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        context.RepositoryConfigurations.Add(new RepositoryConfiguration
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "feature",
            LocalDirectory = "/path/to/local"
        });
        await context.SaveChangesAsync();

        var handler = new GetRepositoryConfigurationsHandler(context, _mockLogger.Object);
        var request = new GetRepositoryConfigurationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var config = result.RepositoryConfigurations.Single();
        Assert.Equal(id, config.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/repo", config.Url);
        Assert.Equal("feature", config.Branch);
        Assert.Equal("/path/to/local", config.LocalDirectory);
    }

    #endregion
}
