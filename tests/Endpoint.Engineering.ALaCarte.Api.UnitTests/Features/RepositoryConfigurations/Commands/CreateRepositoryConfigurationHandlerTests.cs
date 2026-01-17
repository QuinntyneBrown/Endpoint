// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.CreateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.UnitTests.Features.RepositoryConfigurations.Commands;

public class CreateRepositoryConfigurationHandlerTests
{
    private readonly Mock<ILogger<CreateRepositoryConfigurationHandler>> _mockLogger;

    public CreateRepositoryConfigurationHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateRepositoryConfigurationHandler>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CreateRepositoryConfigurationHandler(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockContext = new Mock<IALaCarteContext>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CreateRepositoryConfigurationHandler(mockContext.Object, null!));
    }

    #endregion

    #region Handle Tests

    [Fact]
    public async Task Handle_AddsNewRepositoryConfiguration()
    {
        // Arrange
        var configurations = new List<RepositoryConfiguration>();
        var mockDbSet = CreateMockDbSet(configurations);
        var mockContext = new Mock<IALaCarteContext>();
        mockContext.Setup(c => c.RepositoryConfigurations).Returns(mockDbSet.Object);

        var handler = new CreateRepositoryConfigurationHandler(mockContext.Object, _mockLogger.Object);
        var request = new CreateRepositoryConfigurationRequest
        {
            Url = "https://github.com/user/repo",
            Branch = "main",
            LocalDirectory = "/local/path"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.RepositoryConfigurationId);
        mockDbSet.Verify(d => d.Add(It.Is<RepositoryConfiguration>(c =>
            c.Url == request.Url &&
            c.Branch == request.Branch &&
            c.LocalDirectory == request.LocalDirectory)), Times.Once);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var configurations = new List<RepositoryConfiguration>();
        var mockDbSet = CreateMockDbSet(configurations);
        var mockContext = new Mock<IALaCarteContext>();
        mockContext.Setup(c => c.RepositoryConfigurations).Returns(mockDbSet.Object);

        var handler = new CreateRepositoryConfigurationHandler(mockContext.Object, _mockLogger.Object);
        var request = new CreateRepositoryConfigurationRequest
        {
            Url = "https://github.com/user/repo",
            Branch = "main"
        };

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GeneratesNewGuidForId()
    {
        // Arrange
        var configurations = new List<RepositoryConfiguration>();
        var mockDbSet = CreateMockDbSet(configurations);
        var mockContext = new Mock<IALaCarteContext>();
        mockContext.Setup(c => c.RepositoryConfigurations).Returns(mockDbSet.Object);

        var handler = new CreateRepositoryConfigurationHandler(mockContext.Object, _mockLogger.Object);
        var request = new CreateRepositoryConfigurationRequest { Url = "https://github.com/user/repo" };

        // Act
        var result1 = await handler.Handle(request, CancellationToken.None);
        var result2 = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotEqual(result1.RepositoryConfigurationId, result2.RepositoryConfigurationId);
    }

    [Fact]
    public async Task Handle_ReturnsResponseWithCorrectId()
    {
        // Arrange
        RepositoryConfiguration? addedConfig = null;
        var mockDbSet = new Mock<DbSet<RepositoryConfiguration>>();
        mockDbSet.Setup(d => d.Add(It.IsAny<RepositoryConfiguration>()))
            .Callback<RepositoryConfiguration>(c => addedConfig = c);

        var mockContext = new Mock<IALaCarteContext>();
        mockContext.Setup(c => c.RepositoryConfigurations).Returns(mockDbSet.Object);

        var handler = new CreateRepositoryConfigurationHandler(mockContext.Object, _mockLogger.Object);
        var request = new CreateRepositoryConfigurationRequest { Url = "https://github.com/user/repo" };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(addedConfig);
        Assert.Equal(addedConfig.RepositoryConfigurationId, result.RepositoryConfigurationId);
    }

    #endregion

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> list) where T : class
    {
        var queryable = list.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(list.Add);
        return mockSet;
    }
}
