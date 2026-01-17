// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.UnitTests.Features.RepositoryConfigurations.Queries;

public class GetRepositoryConfigurationHandlerTests
{
    private readonly Mock<ILogger<GetRepositoryConfigurationHandler>> _mockLogger;

    public GetRepositoryConfigurationHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetRepositoryConfigurationHandler>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetRepositoryConfigurationHandler(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockContext = new Mock<IALaCarteContext>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetRepositoryConfigurationHandler(mockContext.Object, null!));
    }

    #endregion

    #region Handle Tests

    [Fact]
    public async Task Handle_ReturnsConfiguration_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        var existingConfig = new RepositoryConfiguration
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "develop",
            LocalDirectory = "/local/path"
        };
        context.RepositoryConfigurations.Add(existingConfig);
        await context.SaveChangesAsync();

        var handler = new GetRepositoryConfigurationHandler(context, _mockLogger.Object);
        var request = new GetRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/repo", result.Url);
        Assert.Equal("develop", result.Branch);
        Assert.Equal("/local/path", result.LocalDirectory);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        var handler = new GetRepositoryConfigurationHandler(context, _mockLogger.Object);
        var request = new GetRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = Guid.NewGuid()
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectConfiguration_WhenMultipleExist()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        context.RepositoryConfigurations.AddRange(
            new RepositoryConfiguration
            {
                RepositoryConfigurationId = id1,
                Url = "https://github.com/user/repo1",
                Branch = "main"
            },
            new RepositoryConfiguration
            {
                RepositoryConfigurationId = id2,
                Url = "https://github.com/user/repo2",
                Branch = "develop"
            }
        );
        await context.SaveChangesAsync();

        var handler = new GetRepositoryConfigurationHandler(context, _mockLogger.Object);
        var request = new GetRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id2
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id2, result.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/repo2", result.Url);
    }

    #endregion
}
