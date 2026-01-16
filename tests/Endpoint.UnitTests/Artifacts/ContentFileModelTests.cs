// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;

namespace Endpoint.UnitTests.Artifacts;

public class ContentFileModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var content = "Test content";
        var name = "TestFile";
        var directory = "/home/user/project";
        var extension = ".cs";

        // Act
        var contentFileModel = new ContentFileModel(content, name, directory, extension);

        // Assert
        Assert.Equal(content, contentFileModel.Content);
        Assert.Equal(name, contentFileModel.Name);
        Assert.Equal(directory, contentFileModel.Directory);
        Assert.Equal(extension, contentFileModel.Extension);
    }

    [Fact]
    public void ContentFileModel_ShouldInheritFromFileModel()
    {
        // Arrange
        var contentFileModel = new ContentFileModel("content", "name", "/dir", ".cs");

        // Assert
        Assert.IsAssignableFrom<FileModel>(contentFileModel);
    }

    [Fact]
    public void Content_ShouldBeInitOnly()
    {
        // Arrange & Act
        var contentFileModel = new ContentFileModel("initial content", "name", "/dir", ".cs");

        // Assert - Content should be set through constructor
        Assert.Equal("initial content", contentFileModel.Content);
    }

    [Fact]
    public void Constructor_WithEmptyContent_ShouldInitialize()
    {
        // Arrange & Act
        var contentFileModel = new ContentFileModel(string.Empty, "name", "/dir", ".cs");

        // Assert
        Assert.Equal(string.Empty, contentFileModel.Content);
        Assert.NotNull(contentFileModel);
    }

    [Fact]
    public void Constructor_WithMultilineContent_ShouldPreserveContent()
    {
        // Arrange
        var multilineContent = "Line 1\nLine 2\nLine 3";

        // Act
        var contentFileModel = new ContentFileModel(multilineContent, "name", "/dir", ".cs");

        // Assert
        Assert.Equal(multilineContent, contentFileModel.Content);
    }

    [Fact]
    public void Constructor_ShouldComputePathCorrectly()
    {
        // Arrange
        var content = "Test content";
        var name = "TestFile";
        var directory = "/home/user/project";
        var extension = ".cs";

        // Act
        var contentFileModel = new ContentFileModel(content, name, directory, extension);

        // Assert
        var expectedPath = Path.Combine(directory, $"{name}{extension}");
        Assert.Equal(expectedPath, contentFileModel.Path);
    }
}
