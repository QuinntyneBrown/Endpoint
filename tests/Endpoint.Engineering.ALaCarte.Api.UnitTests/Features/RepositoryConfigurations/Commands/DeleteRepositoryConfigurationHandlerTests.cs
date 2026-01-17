// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.DeleteRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.UnitTests.Features.RepositoryConfigurations.Commands;

public class DeleteRepositoryConfigurationHandlerTests
{
    private readonly Mock<ILogger<DeleteRepositoryConfigurationHandler>> _mockLogger;

    public DeleteRepositoryConfigurationHandlerTests()
    {
        _mockLogger = new Mock<ILogger<DeleteRepositoryConfigurationHandler>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeleteRepositoryConfigurationHandler(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockContext = new Mock<IALaCarteContext>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeleteRepositoryConfigurationHandler(mockContext.Object, null!));
    }

    #endregion

    #region Handle Tests

    [Fact]
    public async Task Handle_DeletesExistingConfiguration()
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
            Branch = "main"
        };
        context.RepositoryConfigurations.Add(existingConfig);
        await context.SaveChangesAsync();

        var handler = new DeleteRepositoryConfigurationHandler(context, _mockLogger.Object);
        var request = new DeleteRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id
        };

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedConfig = await context.RepositoryConfigurations.FindAsync(id);
        Assert.Null(deletedConfig);
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenConfigurationNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ALaCarteContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ALaCarteContext(options);
        var handler = new DeleteRepositoryConfigurationHandler(context, _mockLogger.Object);
        var request = new DeleteRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(request, CancellationToken.None));

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_SavesChanges()
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
            Branch = "main"
        };
        context.RepositoryConfigurations.Add(existingConfig);
        await context.SaveChangesAsync();

        var handler = new DeleteRepositoryConfigurationHandler(context, _mockLogger.Object);
        var request = new DeleteRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id
        };

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert - Verify deletion persisted by creating new context
        using var verifyContext = new ALaCarteContext(options);
        var savedConfig = await verifyContext.RepositoryConfigurations.FindAsync(id);
        Assert.Null(savedConfig);
    }

    #endregion
}
