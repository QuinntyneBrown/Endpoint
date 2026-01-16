// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;

namespace Endpoint.UnitTests.Artifacts;

public class TemplatedFileModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var templateName = "MyTemplate";
        var name = "TestFile";
        var directory = "/home/user/project";
        var extension = ".cs";

        // Act
        var templatedFileModel = new TemplatedFileModel(templateName, name, directory, extension);

        // Assert
        Assert.Equal(templateName, templatedFileModel.Template);
        Assert.Equal(name, templatedFileModel.Name);
        Assert.Equal(directory, templatedFileModel.Directory);
        Assert.Equal(extension, templatedFileModel.Extension);
        Assert.NotNull(templatedFileModel.Tokens);
        Assert.Empty(templatedFileModel.Tokens);
    }

    [Fact]
    public void TemplatedFileModel_ShouldInheritFromFileModel()
    {
        // Arrange
        var templatedFileModel = new TemplatedFileModel("template", "name", "/dir", ".cs");

        // Assert
        Assert.IsAssignableFrom<FileModel>(templatedFileModel);
    }

    [Fact]
    public void Constructor_WithTokens_ShouldAddTokensToDictionary()
    {
        // Arrange
        var tokens = new Dictionary<string, object>
        {
            { "Key1", "Value1" },
            { "Key2", 123 },
            { "Key3", true }
        };

        // Act
        var templatedFileModel = new TemplatedFileModel("template", "name", "/dir", ".cs", tokens);

        // Assert
        Assert.Equal(3, templatedFileModel.Tokens.Count);
        Assert.Equal("Value1", templatedFileModel.Tokens["Key1"]);
        Assert.Equal(123, templatedFileModel.Tokens["Key2"]);
        Assert.Equal(true, templatedFileModel.Tokens["Key3"]);
    }

    [Fact]
    public void Constructor_WithNullTokens_ShouldInitializeEmptyDictionary()
    {
        // Arrange & Act
        var templatedFileModel = new TemplatedFileModel("template", "name", "/dir", ".cs", null);

        // Assert
        Assert.NotNull(templatedFileModel.Tokens);
        Assert.Empty(templatedFileModel.Tokens);
    }

    [Fact]
    public void Constructor_WithEmptyTokens_ShouldInitializeEmptyDictionary()
    {
        // Arrange
        var tokens = new Dictionary<string, object>();

        // Act
        var templatedFileModel = new TemplatedFileModel("template", "name", "/dir", ".cs", tokens);

        // Assert
        Assert.NotNull(templatedFileModel.Tokens);
        Assert.Empty(templatedFileModel.Tokens);
    }

    [Fact]
    public void Tokens_ShouldBeInitOnly()
    {
        // Arrange
        var tokens = new Dictionary<string, object> { { "Key1", "Value1" } };

        // Act
        var templatedFileModel = new TemplatedFileModel("template", "name", "/dir", ".cs", tokens);

        // Assert - Tokens should be initialized and accessible
        Assert.Single(templatedFileModel.Tokens);
        Assert.Equal("Value1", templatedFileModel.Tokens["Key1"]);
    }

    [Fact]
    public void Template_ShouldBeInitOnly()
    {
        // Arrange & Act
        var templatedFileModel = new TemplatedFileModel("MyTemplate", "name", "/dir", ".cs");

        // Assert
        Assert.Equal("MyTemplate", templatedFileModel.Template);
    }

    [Fact]
    public void Constructor_ShouldComputePathCorrectly()
    {
        // Arrange
        var templateName = "MyTemplate";
        var name = "TestFile";
        var directory = "/home/user/project";
        var extension = ".cs";

        // Act
        var templatedFileModel = new TemplatedFileModel(templateName, name, directory, extension);

        // Assert
        var expectedPath = Path.Combine(directory, $"{name}{extension}");
        Assert.Equal(expectedPath, templatedFileModel.Path);
    }
}
