// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts.Abstractions;

namespace Endpoint.Angular.Cli.UnitTests;

public class WorkspaceCreateRequestHandlerTests
{
    private readonly Mock<ILogger<WorkspaceCreateRequestHandler>> _mockLogger;
    private readonly Mock<IArtifactGenerator> _mockArtifactGenerator;

    public WorkspaceCreateRequestHandlerTests()
    {
        _mockLogger = new Mock<ILogger<WorkspaceCreateRequestHandler>>();
        _mockArtifactGenerator = new Mock<IArtifactGenerator>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new WorkspaceCreateRequestHandler(null!, _mockArtifactGenerator.Object));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullArtifactGenerator_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new WorkspaceCreateRequestHandler(_mockLogger.Object, null!));

        Assert.Equal("artifactGenerator", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Constructor_WithBothParametersNull_ThrowsArgumentNullExceptionForLogger()
    {
        // Arrange & Act & Assert
        // Logger is checked first, so it should throw for logger
        var exception = Assert.Throws<ArgumentNullException>(
            () => new WorkspaceCreateRequestHandler(null!, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region Handle Method - Logging Tests

    [Fact]
    public async Task Handle_WhenCalled_LogsInformation()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = "/test/directory"
        };

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("WorkspaceCreateRequestHandler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalled_LogsHandlerName()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = "/test/directory"
        };

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(WorkspaceCreateRequestHandler))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Handle Method - ArtifactGenerator Tests

    [Fact]
    public async Task Handle_WhenCalled_CallsGenerateAsyncOnArtifactGenerator()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = "/test/directory"
        };

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        _mockArtifactGenerator.Verify(
            x => x.GenerateAsync(It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalled_PassesWorkspaceModelToGenerateAsync()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = "/test/directory"
        };
        WorkspaceModel? capturedModel = null;

        _mockArtifactGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<object>()))
            .Callback<object>(model => capturedModel = model as WorkspaceModel)
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedModel);
        Assert.IsType<WorkspaceModel>(capturedModel);
    }

    #endregion

    #region Handle Method - WorkspaceModel Creation Tests

    [Fact]
    public async Task Handle_WhenCalled_CreatesWorkspaceModelWithCorrectName()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var expectedName = "MyTestWorkspace";
        var request = new WorkspaceCreateRequest
        {
            Name = expectedName,
            Directory = "/test/directory"
        };
        WorkspaceModel? capturedModel = null;

        _mockArtifactGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<object>()))
            .Callback<object>(model => capturedModel = model as WorkspaceModel)
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedModel);
        Assert.Equal(expectedName, capturedModel.Name);
    }

    [Fact]
    public async Task Handle_WhenCalled_CreatesWorkspaceModelWithCorrectDirectory()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var expectedDirectory = "/custom/workspace/directory";
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = expectedDirectory
        };
        WorkspaceModel? capturedModel = null;

        _mockArtifactGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<object>()))
            .Callback<object>(model => capturedModel = model as WorkspaceModel)
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedModel);
        Assert.Equal(expectedDirectory, capturedModel.RootDirectory);
    }

    [Fact]
    public async Task Handle_WhenCalled_CreatesWorkspaceModelWithVersion18_2_7()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = "/test/directory"
        };
        WorkspaceModel? capturedModel = null;

        _mockArtifactGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<object>()))
            .Callback<object>(model => capturedModel = model as WorkspaceModel)
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedModel);
        Assert.Equal("18.2.7", capturedModel.Version);
    }

    [Fact]
    public async Task Handle_WithDifferentWorkspaceNames_CreatesModelsWithCorrectNames()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var testNames = new[] { "Workspace1", "my-angular-app", "test_project_123" };

        foreach (var expectedName in testNames)
        {
            WorkspaceModel? capturedModel = null;
            _mockArtifactGenerator
                .Setup(x => x.GenerateAsync(It.IsAny<object>()))
                .Callback<object>(model => capturedModel = model as WorkspaceModel)
                .Returns(Task.CompletedTask);

            var request = new WorkspaceCreateRequest
            {
                Name = expectedName,
                Directory = "/test"
            };

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedModel);
            Assert.Equal(expectedName, capturedModel.Name);
        }
    }

    #endregion

    #region Handle Method - CancellationToken Tests

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = "TestWorkspace",
            Directory = "/test/directory"
        };
        using var cts = new CancellationTokenSource();

        // Act & Assert (no exception)
        await handler.Handle(request, cts.Token);

        _mockArtifactGenerator.Verify(x => x.GenerateAsync(It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region Handle Method - Edge Cases Tests

    [Fact]
    public async Task Handle_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = null,
            Directory = "/test/directory"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptyName_PassesEmptyNameToWorkspaceModel()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var request = new WorkspaceCreateRequest
        {
            Name = string.Empty,
            Directory = "/test/directory"
        };
        WorkspaceModel? capturedModel = null;

        _mockArtifactGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<object>()))
            .Callback<object>(model => capturedModel = model as WorkspaceModel)
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedModel);
        Assert.Equal(string.Empty, capturedModel.Name);
    }

    [Fact]
    public async Task Handle_VersionIsHardcoded_AlwaysReturns18_2_7()
    {
        // Arrange
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);
        var capturedModels = new List<WorkspaceModel>();

        _mockArtifactGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<object>()))
            .Callback<object>(model =>
            {
                if (model is WorkspaceModel wsModel)
                    capturedModels.Add(wsModel);
            })
            .Returns(Task.CompletedTask);

        // Act - Call multiple times with different parameters
        await handler.Handle(new WorkspaceCreateRequest { Name = "Workspace1", Directory = "/dir1" }, CancellationToken.None);
        await handler.Handle(new WorkspaceCreateRequest { Name = "Workspace2", Directory = "/dir2" }, CancellationToken.None);
        await handler.Handle(new WorkspaceCreateRequest { Name = "Workspace3", Directory = "/dir3" }, CancellationToken.None);

        // Assert - All should have version 18.2.7
        Assert.All(capturedModels, model => Assert.Equal("18.2.7", model.Version));
    }

    #endregion

    #region Handler Interface Implementation Tests

    [Fact]
    public void WorkspaceCreateRequestHandler_ImplementsIRequestHandler()
    {
        // Arrange & Act
        var handler = new WorkspaceCreateRequestHandler(_mockLogger.Object, _mockArtifactGenerator.Object);

        // Assert
        Assert.IsAssignableFrom<IRequestHandler<WorkspaceCreateRequest>>(handler);
    }

    #endregion
}
