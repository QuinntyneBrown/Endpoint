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
    public void CreateTemplate_ShouldReturnTemplatedFileModel()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FileFactory>>();
        var fileFactory = new FileFactory(mockLogger.Object);

        // Act
        var result = fileFactory.CreateTemplate("template", "name", "/dir", ".cs");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TemplatedFileModel>(result);
        Assert.Equal("template", result.Template);
        Assert.Equal("name", result.Name);
        Assert.Equal("/dir", result.Directory);
        Assert.Equal(".cs", result.Extension);
    }

    [Fact]
    public void CreateTemplate_WithAllParameters_ShouldReturnTemplatedFileModel()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FileFactory>>();
        var fileFactory = new FileFactory(mockLogger.Object);
        var tokens = new Dictionary<string, object> { { "key", "value" } };

        // Act
        var result = fileFactory.CreateTemplate("template", "name", "/dir", ".cs", "filename", tokens);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TemplatedFileModel>(result);
        Assert.Equal("template", result.Template);
        Assert.Equal("name", result.Name);
        Assert.Equal("/dir", result.Directory);
        Assert.Equal(".cs", result.Extension);
        Assert.NotNull(result.Tokens);
        Assert.Contains("key", result.Tokens.Keys);
        Assert.Equal("value", result.Tokens["key"]);
    }
}
