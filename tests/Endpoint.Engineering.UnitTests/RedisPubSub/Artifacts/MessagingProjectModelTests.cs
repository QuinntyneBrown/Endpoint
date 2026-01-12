// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Artifacts;
using Endpoint.DotNet.Artifacts.Projects.Enums;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Artifacts;

public class MessagingProjectModelTests
{
    [Fact]
    public void Constructor_WithSolutionName_ShouldSetCorrectProjectName()
    {
        // Arrange
        var solutionName = "MyApplication";
        var parentDirectory = "/path/to/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.Equal("MyApplication.Messaging", model.Name);
    }

    [Fact]
    public void Constructor_WithSolutionName_ShouldSetSolutionName()
    {
        // Arrange
        var solutionName = "TestSolution";
        var parentDirectory = "/path/to/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.Equal(solutionName, model.SolutionName);
    }

    [Fact]
    public void Constructor_WithSolutionName_ShouldAddMessagePackPackage()
    {
        // Arrange
        var solutionName = "TestSolution";
        var parentDirectory = "/path/to/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.Contains(model.Packages, p => p.Name == "MessagePack");
    }

    [Fact]
    public void Constructor_WithSolutionName_ShouldSetProjectTypeToClassLib()
    {
        // Arrange
        var solutionName = "TestSolution";
        var parentDirectory = "/path/to/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.Equal(DotNetProjectType.ClassLib, model.DotNetProjectType);
    }

    [Fact]
    public void UseLz4Compression_ShouldDefaultToTrue()
    {
        // Arrange
        var solutionName = "TestSolution";
        var parentDirectory = "/path/to/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.True(model.UseLz4Compression);
    }

    [Fact]
    public void UseLz4Compression_ShouldBeSettable()
    {
        // Arrange
        var model = new MessagingProjectModel("Test", "/path")
        {
            UseLz4Compression = false
        };

        // Assert
        Assert.False(model.UseLz4Compression);
    }

    [Fact]
    public void DefaultConstructor_ShouldCreateEmptyModel()
    {
        // Arrange & Act
        var model = new MessagingProjectModel();

        // Assert
        Assert.Equal(string.Empty, model.SolutionName);
    }

    [Fact]
    public void Directory_ShouldBeSetCorrectly()
    {
        // Arrange
        var solutionName = "MyApp";
        var parentDirectory = "/home/user/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.Contains("MyApp.Messaging", model.Directory);
    }

    [Fact]
    public void Namespace_ShouldMatchProjectName()
    {
        // Arrange
        var solutionName = "MyApp";
        var parentDirectory = "/path/to/src";

        // Act
        var model = new MessagingProjectModel(solutionName, parentDirectory);

        // Assert
        Assert.Equal("MyApp.Messaging", model.Namespace);
    }
}
