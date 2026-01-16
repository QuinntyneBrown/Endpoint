// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.UnitTests.Artifacts;

public class FileFactoryTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FileFactory>>();

        // Act
        var fileFactory = new FileFactory(mockLogger.Object);

        // Assert
        Assert.NotNull(fileFactory);
    }

    [Fact]
    public void FileFactory_ShouldImplementIFileFactory()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FileFactory>>();

        // Act
        var fileFactory = new FileFactory(mockLogger.Object);

        // Assert
        Assert.IsAssignableFrom<IFileFactory>(fileFactory);
    }

    [Fact]
    public void CreateTemplate_ShouldThrowNotImplementedException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FileFactory>>();
        var fileFactory = new FileFactory(mockLogger.Object);

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            fileFactory.CreateTemplate("template", "name", "/dir", ".cs"));
    }

    [Fact]
    public void CreateTemplate_WithAllParameters_ShouldThrowNotImplementedException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FileFactory>>();
        var fileFactory = new FileFactory(mockLogger.Object);
        var tokens = new Dictionary<string, object> { { "key", "value" } };

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            fileFactory.CreateTemplate("template", "name", "/dir", ".cs", "filename", tokens));
    }
}
