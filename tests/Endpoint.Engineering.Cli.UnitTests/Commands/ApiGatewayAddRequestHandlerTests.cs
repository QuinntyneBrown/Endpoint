// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.Engineering.Cli.Commands;
using Endpoint.Engineering.Api;
using Endpoint.Engineering.Api.Models;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.Cli.UnitTests.Commands;

public class ApiGatewayAddRequestHandlerTests
{
    private readonly Mock<ILogger<ApiGatewayAddRequestHandler>> _loggerMock;
    private readonly Mock<IFileProvider> _fileProviderMock;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<IApiArtifactFactory> _artifactFactoryMock;
    private readonly Mock<IArtifactGenerator> _artifactGeneratorMock;
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly ApiGatewayAddRequestHandler _handler;

    public ApiGatewayAddRequestHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ApiGatewayAddRequestHandler>>();
        _fileProviderMock = new Mock<IFileProvider>();
        _fileSystem = new MockFileSystem();
        _artifactFactoryMock = new Mock<IApiArtifactFactory>();
        _artifactGeneratorMock = new Mock<IArtifactGenerator>();
        _projectServiceMock = new Mock<IProjectService>();

        _handler = new ApiGatewayAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object,
            _projectServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiGatewayAddRequestHandler(
            null!,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object,
            _projectServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileProviderIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiGatewayAddRequestHandler(
            _loggerMock.Object,
            null!,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object,
            _projectServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileSystemIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiGatewayAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            null!,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object,
            _projectServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenArtifactFactoryIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiGatewayAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            null!,
            _artifactGeneratorMock.Object,
            _projectServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenArtifactGeneratorIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiGatewayAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            null!,
            _projectServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProjectServiceIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiGatewayAddRequestHandler(
            _loggerMock.Object,
            _fileProviderMock.Object,
            _fileSystem,
            _artifactFactoryMock.Object,
            _artifactGeneratorMock.Object,
            null!));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoSolutionFileFound()
    {
        // Arrange
        var request = new ApiGatewayAddRequest
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
        var request = new ApiGatewayAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/MyApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new ApiGatewayModel("MyApp", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateApiGatewayProjectAsync(It.Is<ApiGatewayInputModel>(m => m.SolutionName == "MyApp"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateApiGatewayProjectAsync(
            It.Is<ApiGatewayInputModel>(m => m.SolutionName == "MyApp"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseProvidedName_WhenNameIsProvided()
    {
        // Arrange
        var request = new ApiGatewayAddRequest
        {
            Name = "CustomName",
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/MyApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new ApiGatewayModel("CustomName", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateApiGatewayProjectAsync(It.Is<ApiGatewayInputModel>(m => m.SolutionName == "CustomName"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateApiGatewayProjectAsync(
            It.Is<ApiGatewayInputModel>(m => m.SolutionName == "CustomName"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallArtifactGenerator_WithProjectModel()
    {
        // Arrange
        var request = new ApiGatewayAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        var projectModel = new ApiGatewayModel("TestApp", "/test/path");
        _artifactFactoryMock.Setup(x => x.CreateApiGatewayProjectAsync(It.IsAny<ApiGatewayInputModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactGeneratorMock.Verify(x => x.GenerateAsync(projectModel), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseSrcDirectory_WhenItExists()
    {
        // Arrange
        var request = new ApiGatewayAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        _fileSystem.AddDirectory("/test/path/src");

        var projectModel = new ApiGatewayModel("TestApp", "/test/path/src");
        _artifactFactoryMock.Setup(x => x.CreateApiGatewayProjectAsync(It.Is<ApiGatewayInputModel>(m => m.Directory.Contains("src")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateApiGatewayProjectAsync(
            It.Is<ApiGatewayInputModel>(m => m.Directory.Contains("src")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseSrcDirectory_WhenSrcDoesNotExist()
    {
        // Arrange
        var request = new ApiGatewayAddRequest
        {
            Directory = "/test/path"
        };

        var solutionPath = "/test/path/TestApp.sln";
        _fileProviderMock.Setup(x => x.Get("*.sln", request.Directory, 0))
            .Returns(solutionPath);

        // Note: Not adding /test/path/src to file system

        var projectModel = new ApiGatewayModel("TestApp", "/test/path/src");
        _artifactFactoryMock.Setup(x => x.CreateApiGatewayProjectAsync(It.Is<ApiGatewayInputModel>(m => m.Directory.Contains("src")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectModel);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _artifactFactoryMock.Verify(x => x.CreateApiGatewayProjectAsync(
            It.Is<ApiGatewayInputModel>(m => m.Directory.Contains("src")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class ApiGatewayAddRequestTests
{
    [Fact]
    public void Directory_ShouldDefaultToCurrentDirectory()
    {
        // Arrange & Act
        var request = new ApiGatewayAddRequest();

        // Assert
        Assert.Equal(Environment.CurrentDirectory, request.Directory);
    }

    [Fact]
    public void Name_ShouldDefaultToNull()
    {
        // Arrange & Act
        var request = new ApiGatewayAddRequest();

        // Assert
        Assert.Null(request.Name);
    }
}
