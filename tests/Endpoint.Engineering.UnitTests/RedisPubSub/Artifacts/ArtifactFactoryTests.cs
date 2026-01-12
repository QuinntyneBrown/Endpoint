// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.Engineering.RedisPubSub.Artifacts;
using Endpoint.Engineering.RedisPubSub.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Artifacts;

public class ArtifactFactoryTests
{
    private readonly Mock<ILogger<ArtifactFactory>> _loggerMock;
    private readonly MockFileSystem _fileSystem;
    private readonly ArtifactFactory _factory;

    public ArtifactFactoryTests()
    {
        _loggerMock = new Mock<ILogger<ArtifactFactory>>();
        _fileSystem = new MockFileSystem();
        _factory = new ArtifactFactory(_loggerMock.Object, _fileSystem);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldReturnProjectModel()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src",
            UseLz4Compression = true
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestApp.Messaging", result.Name);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldSetCorrectSolutionName()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "MyProject",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Equal("MyProject", result.SolutionName);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldSetUseLz4Compression()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src",
            UseLz4Compression = false
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.False(result.UseLz4Compression);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateIMessageFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "IMessage");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateIDomainEventFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "IDomainEvent");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateICommandFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "ICommand");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateMessageHeaderFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "MessageHeader");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateMessageEnvelopeFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "MessageEnvelope");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateIMessageSerializerFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "IMessageSerializer");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateMessagePackMessageSerializerFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "MessagePackMessageSerializer");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateIMessageTypeRegistryFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "IMessageTypeRegistry");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateMessageTypeRegistryFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "MessageTypeRegistry");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateConfigureServicesFile()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Files, f => f.Name == "ConfigureServices");
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldCreateTenFiles()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Equal(10, result.Files.Count);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_WithLz4Compression_ShouldIncludeCompressionInSerializer()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src",
            UseLz4Compression = true
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        var serializerFile = result.Files.First(f => f.Name == "MessagePackMessageSerializer");
        Assert.Contains("Lz4BlockArray", serializerFile.Body);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_WithoutLz4Compression_ShouldNotIncludeCompressionInSerializer()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src",
            UseLz4Compression = false
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        var serializerFile = result.Files.First(f => f.Name == "MessagePackMessageSerializer");
        Assert.DoesNotContain("Lz4BlockArray", serializerFile.Body);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_FilesShouldHaveCorrectNamespaces()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "MyApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        var messageHeaderFile = result.Files.First(f => f.Name == "MessageHeader");
        Assert.Contains("namespace MyApp.Messaging.Messages", messageHeaderFile.Body);
    }

    [Fact]
    public async Task CreateMessagingProjectAsync_ShouldAddMessagePackPackage()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestApp",
            Directory = "/test/src"
        };

        // Act
        var result = await _factory.CreateMessagingProjectAsync(model);

        // Assert
        Assert.Contains(result.Packages, p => p.Name == "MessagePack");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ArtifactFactory(null!, _fileSystem));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileSystemIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ArtifactFactory(_loggerMock.Object, null!));
    }
}
