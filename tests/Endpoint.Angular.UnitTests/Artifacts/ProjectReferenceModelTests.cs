// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Artifacts;
using Endpoint.Artifacts;

namespace Endpoint.Angular.UnitTests.Artifacts;

public class ProjectReferenceModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var name = "MyProject";
        var referencedDirectory = "/path/to/project";
        var projectType = "library";

        // Act
        var projectRef = new ProjectReferenceModel(name, referencedDirectory, projectType);

        // Assert
        Assert.Equal(name, projectRef.Name);
        Assert.Equal(referencedDirectory, projectRef.ReferencedDirectory);
        Assert.Equal(projectType, projectRef.ProjectType);
    }

    [Fact]
    public void Constructor_WithDefaultProjectType_ShouldInitialize()
    {
        // Arrange
        var name = "MyProject";
        var referencedDirectory = "/path/to/project";

        // Act
        var projectRef = new ProjectReferenceModel(name, referencedDirectory);

        // Assert
        Assert.Equal(name, projectRef.Name);
        Assert.Equal(referencedDirectory, projectRef.ReferencedDirectory);
        Assert.Equal("application", projectRef.ProjectType);
    }

    [Fact]
    public void Name_ShouldBeSettableAndGettable()
    {
        // Arrange
        var projectRef = new ProjectReferenceModel("Initial", "/path", "app");
        var newName = "UpdatedName";

        // Act
        projectRef.Name = newName;

        // Assert
        Assert.Equal(newName, projectRef.Name);
    }

    [Fact]
    public void ReferencedDirectory_ShouldBeSettableAndGettable()
    {
        // Arrange
        var projectRef = new ProjectReferenceModel("MyProject", "/initial/path", "app");
        var newPath = "/updated/path";

        // Act
        projectRef.ReferencedDirectory = newPath;

        // Assert
        Assert.Equal(newPath, projectRef.ReferencedDirectory);
    }

    [Fact]
    public void ProjectType_ShouldBeSettableAndGettable()
    {
        // Arrange
        var projectRef = new ProjectReferenceModel("MyProject", "/path", "app");
        var newType = "library";

        // Act
        projectRef.ProjectType = newType;

        // Assert
        Assert.Equal(newType, projectRef.ProjectType);
    }

    [Fact]
    public void ProjectReferenceModel_ShouldInheritFromArtifactModel()
    {
        // Arrange
        var projectRef = new ProjectReferenceModel("MyProject", "/path", "app");

        // Assert
        Assert.IsAssignableFrom<ArtifactModel>(projectRef);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_ShouldInitialize()
    {
        // Arrange & Act
        var projectRef = new ProjectReferenceModel(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.Equal(string.Empty, projectRef.Name);
        Assert.Equal(string.Empty, projectRef.ReferencedDirectory);
        Assert.Equal(string.Empty, projectRef.ProjectType);
    }
}
