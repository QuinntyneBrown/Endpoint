// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Artifacts;

namespace Endpoint.Angular.UnitTests.Artifacts;

public class ProjectFactoryTests
{
    [Fact]
    public void Create_ShouldReturnProjectModel()
    {
        // Arrange
        var projectFactory = new ProjectFactory();
        var name = "TestProject";
        var prefix = "test";
        var directory = "/home/user/projects";

        // Act
        var result = projectFactory.Create(name, prefix, directory);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ProjectModel>(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(prefix, result.Prefix);
        Assert.Equal(directory, result.Directory);
    }

    [Fact]
    public void Create_ShouldSetWorkspaceToNull()
    {
        // Arrange
        var projectFactory = new ProjectFactory();

        // Act
        var result = projectFactory.Create("TestProject", "test", "/dir");

        // Assert
        Assert.Null(result.Workspace);
    }

    [Fact]
    public void ProjectFactory_ShouldImplementIProjectFactory()
    {
        // Arrange & Act
        var projectFactory = new ProjectFactory();

        // Assert
        Assert.IsAssignableFrom<IProjectFactory>(projectFactory);
    }

    [Fact]
    public void Create_WithEmptyStrings_ShouldCreateProjectModel()
    {
        // Arrange
        var projectFactory = new ProjectFactory();

        // Act
        var result = projectFactory.Create(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Name);
        Assert.Equal(string.Empty, result.Prefix);
        Assert.Equal(string.Empty, result.Directory);
    }

    [Fact]
    public void Create_MultipleTimes_ShouldCreateDistinctInstances()
    {
        // Arrange
        var projectFactory = new ProjectFactory();

        // Act
        var result1 = projectFactory.Create("Project1", "p1", "/dir1");
        var result2 = projectFactory.Create("Project2", "p2", "/dir2");

        // Assert
        Assert.NotSame(result1, result2);
        Assert.NotEqual(result1.Name, result2.Name);
    }
}
