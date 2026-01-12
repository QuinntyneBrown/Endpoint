// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.Cli.Commands;
using Endpoint.Engineering.RedisPubSub.Artifacts;
using Endpoint.Engineering.RedisPubSub.Models;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.Cli.UnitTests.Commands;

public class MessagingAddRequestHandlerTests
{
    private readonly Mock<ILogger<MessagingAddRequestHandler>> _loggerMock;
    private readonly Mock<IFileProvider> _fileProviderMock;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<IArtifactFactory> _artifactFactoryMock;
    private readonly Mock<IArtifactGenerator> _artifactGeneratorMock;
    private readonly MessagingAddRequestHandler _handler;

    public MessagingAddRequestHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MessagingAddRequestHandler>>();
        _fileProviderMock = new Mock<IFileProvider>();
        _fileSystem = new MockFileSystem();
        _artifactFactoryMock = new Mock<IArtifactFactory>();
        _artifactGeneratorMock = new Mock<IArtifactGenerator>();

        _handler = new MessagingAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MessagingAddRequestHandler(
            null!,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileProviderIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MessagingAddRequestHandler(
            _loggerMock.Object,
            null!,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileSystemIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MessagingAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            null!,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenArtifactFactoryIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MessagingAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            null!,
            _artifactGeneratorMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenArtifactGeneratorIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MessagingAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            null!));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoSolutionFileFound()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Directory = "/test/path"
        };

        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(Endpoint.Constants.FileNotFound);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUseSolutionNameFromFile_WhenNameNotProvided()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/MyApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new MessagingProjectModel("MyApp", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateMessagingProjectAsync(It.Is<MessagingModel>(m => m.SolutionName == "MyApp"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateMessagingProjectAsync(
            It.Is<MessagingModel>(m => m.SolutionName == "MyApp"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseProvidedName_WhenNameIsProvided()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Name = "CustomName",
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/MyApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new MessagingProjectModel("CustomName", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateMessagingProjectAsync(It.Is<MessagingModel>(m => m.SolutionName == "CustomName"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateMessagingProjectAsync(
            It.Is<MessagingModel>(m => m.SolutionName == "CustomName"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassUseLz4Compression_ToMessagingModel()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Directory = "/test/path",
            UseLz4Compression = false
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new MessagingProjectModel("TestApp", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateMessagingProjectAsync(It.IsAny<MessagingModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateMessagingProjectAsync(
            It.Is<MessagingModel>(m => m.UseLz4Compression == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallArtifactGenerator_WithProjectModel()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new MessagingProjectModel("TestApp", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateMessagingProjectAsync(It.IsAny<MessagingModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactGeneratorMock.Verify(x => x.GenerateAsync(projectModel, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseSrcDirectory_WhenItExists()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        _fileSystem.AddDirectory("/test/path/src");

        var projectModel = new MessagingProjectModel("TestApp", "/test/path/src");
        _artifactFactoryMock.Setup(x => x.CreateMessagingProjectAsync(It.Is<MessagingModel>(m => m.Directory == "/test/path/src"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateMessagingProjectAsync(
            It.Is<MessagingModel>(m => m.Directory == "/test/path/src"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseSolutionDirectory_WhenSrcDoesNotExist()
    {
        // Arrange
        var request = new MessagingAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        // Note: Not adding /test/path/src to file system

        var projectModel = new MessagingProjectModel("TestApp", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateMessagingProjectAsync(It.Is<MessagingModel>(m => m.Directory == "/test/path"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateMessagingProjectAsync(
            It.Is<MessagingModel>(m => m.Directory == "/test/path"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class MessagingAddRequestTests
{
    [Fact]
    public void Directory_ShouldDefaultToCurrentDirectory()
    {
        // Arrange & Act
        var request = new MessagingAddRequest();

        // Assert
        Assert.Equal(Environment.CurrentDirectory, request.Directory);
    }

    [Fact]
    public void UseLz4Compression_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var request = new MessagingAddRequest();

        // Assert
        Assert.True(request.UseLz4Compression);
    }

    [Fact]
    public void Name_ShouldDefaultToNull()
    {
        // Arrange & Act
        var request = new MessagingAddRequest();

        // Assert
        Assert.Null(request.Name);
    }
}
