// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Testing.Core;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.Angular.Artifacts;

namespace Endpoint.Testing.Core.UnitTests;

public class ArtifactFactoryTests
{
    private readonly Mock<ILogger<ArtifactFactory>> _loggerMock;

    public ArtifactFactoryTests()
    {
        _loggerMock = new Mock<ILogger<ArtifactFactory>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Arrange & Act
        var sut = new ArtifactFactory(_loggerMock.Object);

        // Assert
        Assert.NotNull(sut);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ArtifactFactory(null!));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullExceptionWithCorrectParamName()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ArtifactFactory(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region SolutionCreateAsync Tests

    [Fact]
    public async Task SolutionCreateAsync_WithValidParameters_ShouldReturnSolutionModel()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.SolutionCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SolutionModel>(result);
    }

    [Fact]
    public async Task SolutionCreateAsync_WithValidParameters_ShouldReturnSolutionModelWithCorrectName()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.SolutionCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal(systemName, result.Name);
    }

    [Fact]
    public async Task SolutionCreateAsync_WithValidParameters_ShouldReturnSolutionModelWithCorrectDirectory()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.SolutionCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal(directory, result.Directory);
    }

    [Fact]
    public async Task SolutionCreateAsync_WithValidParameters_ShouldAddProjectToSolution()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.SolutionCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Single(result.Projects);
    }

    [Fact]
    public async Task SolutionCreateAsync_WithValidParameters_ShouldAddProjectWithCorrectName()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.SolutionCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal($"{systemName}.Api", result.Projects.First().Name);
    }

    [Fact]
    public async Task SolutionCreateAsync_WhenCalled_ShouldLogInformation()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);

        // Act
        await sut.SolutionCreateAsync("System", "Resource", "/dir", CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SolutionCreateAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SolutionCreateAsync_WithCancellationToken_ShouldNotThrowWhenNotCancelled()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var cts = new CancellationTokenSource();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            sut.SolutionCreateAsync("System", "Resource", "/dir", cts.Token));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SolutionCreateAsync_ShouldReturnSolutionWithSrcDirectory()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var directory = "/test/directory";

        // Act
        var result = await sut.SolutionCreateAsync(systemName, "Resource", directory, CancellationToken.None);

        // Assert
        Assert.NotNull(result.SrcDirectory);
        Assert.Contains("src", result.SrcDirectory);
    }

    #endregion

    #region AngularWorkspaceCreateAsync Tests

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithValidParameters_ShouldReturnWorkspaceModel()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<WorkspaceModel>(result);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithValidParameters_ShouldReturnWorkspaceModelWithCorrectName()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal($"{systemName}.App", result.Name);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithValidParameters_ShouldAddProjectToWorkspace()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Single(result.Projects);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithValidParameters_ShouldAddProjectWithCorrectName()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal("app", result.Projects.First().Name);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithValidParameters_ShouldAddProjectWithApplicationType()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal("application", result.Projects.First().ProjectType);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WhenCalled_ShouldLogInformation()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);

        // Act
        await sut.AngularWorkspaceCreateAsync("System", "Resource", "/dir", CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Angular Workspace Create Async")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_ShouldCallSolutionCreateAsync()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);

        // Act
        await sut.AngularWorkspaceCreateAsync("System", "Resource", "/dir", CancellationToken.None);

        // Assert - verify both logs were called (one for Solution, one for Angular Workspace)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SolutionCreateAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_ShouldSetRootDirectoryFromSolutionSrcDirectory()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var directory = "/test/directory";
        var systemName = "TestSystem";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, "Resource", directory, CancellationToken.None);

        // Assert
        Assert.NotNull(result.RootDirectory);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithCancellationToken_ShouldNotThrowWhenNotCancelled()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var cts = new CancellationTokenSource();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            sut.AngularWorkspaceCreateAsync("System", "Resource", "/dir", cts.Token));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task AngularWorkspaceCreateAsync_WithValidParameters_ShouldAddProjectWithCorrectPrefix()
    {
        // Arrange
        var sut = new ArtifactFactory(_loggerMock.Object);
        var systemName = "TestSystem";
        var resourceName = "TestResource";
        var directory = "/test/directory";

        // Act
        var result = await sut.AngularWorkspaceCreateAsync(systemName, resourceName, directory, CancellationToken.None);

        // Assert
        Assert.Equal("app", result.Projects.First().Prefix);
    }

    #endregion
}
