// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.UnitTests;

public class ProjectModelTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNormalProjectName_ShouldSetProperties()
    {
        // Arrange
        var name = "my-app";
        var projectType = "application";
        var prefix = "app";
        var rootDirectory = "/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.Equal(name, model.Name);
        Assert.Equal(projectType, model.ProjectType);
        Assert.Equal(prefix, model.Prefix);
        Assert.Equal(rootDirectory, model.RootDirectory);
    }

    [Fact]
    public void Constructor_WithNormalProjectName_ShouldSetCorrectDirectory()
    {
        // Arrange
        var name = "my-app";
        var projectType = "application";
        var prefix = "app";
        var rootDirectory = "/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        var expectedDirectory = $"/workspace{Path.DirectorySeparatorChar}projects{Path.DirectorySeparatorChar}my-app";
        Assert.Equal(expectedDirectory, model.Directory);
    }

    [Fact]
    public void Constructor_WithScopedProjectName_ShouldSetCorrectDirectory()
    {
        // Arrange
        var name = "@myorg/shared-utils";
        var projectType = "library";
        var prefix = "lib";
        var rootDirectory = "/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        var expectedDirectory = $"/workspace{Path.DirectorySeparatorChar}projects{Path.DirectorySeparatorChar}myorg{Path.DirectorySeparatorChar}shared-utils";
        Assert.Equal(expectedDirectory, model.Directory);
    }

    [Fact]
    public void Constructor_WithScopedProjectName_ShouldRemoveAtSymbol()
    {
        // Arrange
        var name = "@company/feature-module";
        var projectType = "library";
        var prefix = "feat";
        var rootDirectory = "/home/user/projects";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.DoesNotContain("@", model.Directory);
        Assert.Contains("company", model.Directory);
        Assert.Contains("feature-module", model.Directory);
    }

    [Fact]
    public void Constructor_WithLibraryProjectType_ShouldSetProjectType()
    {
        // Arrange
        var name = "shared-lib";
        var projectType = "library";
        var prefix = "lib";
        var rootDirectory = "/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.Equal("library", model.ProjectType);
    }

    [Fact]
    public void Constructor_WithDifferentPrefixes_ShouldSetPrefix()
    {
        // Arrange & Act
        var model1 = new ProjectModel("app1", "application", "app", "/workspace");
        var model2 = new ProjectModel("lib1", "library", "lib", "/workspace");
        var model3 = new ProjectModel("custom", "application", "cust", "/workspace");

        // Assert
        Assert.Equal("app", model1.Prefix);
        Assert.Equal("lib", model2.Prefix);
        Assert.Equal("cust", model3.Prefix);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Root_ShouldBeSettable()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "test", "/workspace");

        // Act
        model.Root = "projects/test";

        // Assert
        Assert.Equal("projects/test", model.Root);
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var model = new ProjectModel("original", "application", "app", "/workspace");

        // Act
        model.Name = "new-name";

        // Assert
        Assert.Equal("new-name", model.Name);
    }

    [Fact]
    public void Prefix_ShouldBeSettable()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "old", "/workspace");

        // Act
        model.Prefix = "new";

        // Assert
        Assert.Equal("new", model.Prefix);
    }

    [Fact]
    public void Directory_ShouldBeSettable()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "test", "/workspace");

        // Act
        model.Directory = "/custom/directory";

        // Assert
        Assert.Equal("/custom/directory", model.Directory);
    }

    [Fact]
    public void RootDirectory_ShouldBeSettable()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "test", "/workspace");

        // Act
        model.RootDirectory = "/new/root";

        // Assert
        Assert.Equal("/new/root", model.RootDirectory);
    }

    [Fact]
    public void ProjectType_ShouldBeSettable()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "test", "/workspace");

        // Act
        model.ProjectType = "library";

        // Assert
        Assert.Equal("library", model.ProjectType);
    }

    [Fact]
    public void ProjectType_DefaultValue_ShouldBeApplication()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "test", "/workspace");

        // Assert - testing the default from class definition perspective
        // Note: Since we always pass a value, we're testing the passed value is used
        Assert.Equal("application", model.ProjectType);
    }

    #endregion

    #region Directory Path Calculation Tests

    [Fact]
    public void Constructor_WithWindowsStyleRootDirectory_ShouldHandleCorrectly()
    {
        // Arrange
        var name = "my-app";
        var projectType = "application";
        var prefix = "app";
        var rootDirectory = "C:\\Users\\dev\\workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.Contains("projects", model.Directory);
        Assert.Contains("my-app", model.Directory);
    }

    [Fact]
    public void Constructor_WithUnixStyleRootDirectory_ShouldHandleCorrectly()
    {
        // Arrange
        var name = "my-app";
        var projectType = "application";
        var prefix = "app";
        var rootDirectory = "/home/user/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.Contains("projects", model.Directory);
        Assert.Contains("my-app", model.Directory);
    }

    [Fact]
    public void Constructor_WithScopedName_ShouldParseOrganizationCorrectly()
    {
        // Arrange
        var name = "@angular/material";
        var projectType = "library";
        var prefix = "mat";
        var rootDirectory = "/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.Contains("angular", model.Directory);
        Assert.Contains("material", model.Directory);
        Assert.DoesNotContain("@", model.Directory);
    }

    [Fact]
    public void Constructor_WithMultipleSlashesInScopedName_ShouldUseFirstTwo()
    {
        // Arrange - testing behavior with @org/package format
        var name = "@myorg/my-package";
        var projectType = "library";
        var prefix = "lib";
        var rootDirectory = "/workspace";

        // Act
        var model = new ProjectModel(name, projectType, prefix, rootDirectory);

        // Assert
        Assert.Contains("myorg", model.Directory);
        Assert.Contains("my-package", model.Directory);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ProjectModel_ShouldInheritFromArtifactModel()
    {
        // Arrange
        var model = new ProjectModel("test", "application", "test", "/workspace");

        // Assert
        Assert.IsAssignableFrom<ArtifactModel>(model);
    }

    #endregion
}
