// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Models;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Models;

public class MessagingModelTests
{
    [Fact]
    public void ProjectName_ShouldBeSolutionNamePlusMessaging()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "MyApp"
        };

        // Act
        var projectName = model.ProjectName;

        // Assert
        Assert.Equal("MyApp.Messaging", projectName);
    }

    [Fact]
    public void Namespace_ShouldEqualProjectName()
    {
        // Arrange
        var model = new MessagingModel
        {
            SolutionName = "TestSolution"
        };

        // Act
        var @namespace = model.Namespace;

        // Assert
        Assert.Equal("TestSolution.Messaging", @namespace);
    }

    [Fact]
    public void UseLz4Compression_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var model = new MessagingModel();

        // Assert
        Assert.True(model.UseLz4Compression);
    }

    [Fact]
    public void UseLz4Compression_ShouldBeSettable()
    {
        // Arrange
        var model = new MessagingModel
        {
            UseLz4Compression = false
        };

        // Assert
        Assert.False(model.UseLz4Compression);
    }

    [Fact]
    public void Directory_ShouldBeSettable()
    {
        // Arrange
        var expectedDirectory = "/path/to/project";
        var model = new MessagingModel
        {
            Directory = expectedDirectory
        };

        // Assert
        Assert.Equal(expectedDirectory, model.Directory);
    }

    [Fact]
    public void SolutionName_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessagingModel();

        // Assert
        Assert.Equal(string.Empty, model.SolutionName);
    }
}
