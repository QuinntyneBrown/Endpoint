// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.UnitTests;

public class WorkspaceModelTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeNameProperty()
    {
        // Arrange
        var name = "my-workspace";
        var version = "18.0.0";
        var rootDirectory = "/home/user/projects";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Equal(name, model.Name);
    }

    [Fact]
    public void Constructor_ShouldInitializeVersionProperty()
    {
        // Arrange
        var name = "my-workspace";
        var version = "17.3.0";
        var rootDirectory = "/home/user/projects";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Equal(version, model.Version);
    }

    [Fact]
    public void Constructor_ShouldInitializeRootDirectoryProperty()
    {
        // Arrange
        var name = "my-workspace";
        var version = "18.0.0";
        var rootDirectory = "/workspace";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Equal(rootDirectory, model.RootDirectory);
    }

    [Fact]
    public void Constructor_ShouldComputeDirectoryCorrectly()
    {
        // Arrange
        var name = "angular-app";
        var version = "18.0.0";
        var rootDirectory = "/home/user/projects";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        var expectedDirectory = Path.Combine(rootDirectory, name);
        Assert.Equal(expectedDirectory, model.Directory);
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyProjectsList()
    {
        // Arrange
        var name = "my-workspace";
        var version = "18.0.0";
        var rootDirectory = "/workspace";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.NotNull(model.Projects);
        Assert.Empty(model.Projects);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Version_ShouldBeSettable()
    {
        // Arrange
        var model = new WorkspaceModel("test", "17.0.0", "/workspace");

        // Act
        model.Version = "18.0.0";

        // Assert
        Assert.Equal("18.0.0", model.Version);
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var model = new WorkspaceModel("original", "18.0.0", "/workspace");

        // Act
        model.Name = "new-name";

        // Assert
        Assert.Equal("new-name", model.Name);
    }

    [Fact]
    public void RootDirectory_ShouldBeSettable()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/original");

        // Act
        model.RootDirectory = "/new/path";

        // Assert
        Assert.Equal("/new/path", model.RootDirectory);
    }

    [Fact]
    public void Directory_ShouldBeSettable()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");

        // Act
        model.Directory = "/custom/directory";

        // Assert
        Assert.Equal("/custom/directory", model.Directory);
    }

    [Fact]
    public void Projects_ShouldBeSettable()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");
        var projects = new List<ProjectModel>
        {
            new("app1", "application", "app", "/workspace"),
            new("lib1", "library", "lib", "/workspace")
        };

        // Act
        model.Projects = projects;

        // Assert
        Assert.Equal(2, model.Projects.Count);
    }

    #endregion

    #region Directory Path Computation Tests

    [Fact]
    public void Directory_ShouldCombineRootDirectoryAndName()
    {
        // Arrange
        var name = "my-app";
        var version = "18.0.0";
        var rootDirectory = "/projects";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Equal(Path.Combine("/projects", "my-app"), model.Directory);
    }

    [Fact]
    public void Directory_WithWindowsPath_ShouldWorkCorrectly()
    {
        // Arrange
        var name = "angular-workspace";
        var version = "18.0.0";
        var rootDirectory = "C:\\Users\\dev\\projects";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Contains("angular-workspace", model.Directory);
        Assert.StartsWith(rootDirectory, model.Directory);
    }

    [Fact]
    public void Directory_WithUnixPath_ShouldWorkCorrectly()
    {
        // Arrange
        var name = "angular-workspace";
        var version = "18.0.0";
        var rootDirectory = "/home/user/dev";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Contains("angular-workspace", model.Directory);
        Assert.StartsWith(rootDirectory, model.Directory);
    }

    [Fact]
    public void Directory_WithEmptyName_ShouldEqualRootDirectory()
    {
        // Arrange
        var name = "";
        var version = "18.0.0";
        var rootDirectory = "/workspace";

        // Act
        var model = new WorkspaceModel(name, version, rootDirectory);

        // Assert
        Assert.Equal(Path.Combine("/workspace", ""), model.Directory);
    }

    #endregion

    #region Projects List Tests

    [Fact]
    public void Projects_ShouldAllowAddingProjects()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");

        // Act
        model.Projects.Add(new ProjectModel("app1", "application", "app", "/workspace"));
        model.Projects.Add(new ProjectModel("lib1", "library", "lib", "/workspace"));

        // Assert
        Assert.Equal(2, model.Projects.Count);
    }

    [Fact]
    public void Projects_ShouldAllowRemovingProjects()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");
        var project = new ProjectModel("app1", "application", "app", "/workspace");
        model.Projects.Add(project);

        // Act
        model.Projects.Remove(project);

        // Assert
        Assert.Empty(model.Projects);
    }

    [Fact]
    public void Projects_ShouldAllowClearingAllProjects()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");
        model.Projects.Add(new ProjectModel("app1", "application", "app", "/workspace"));
        model.Projects.Add(new ProjectModel("lib1", "library", "lib", "/workspace"));

        // Act
        model.Projects.Clear();

        // Assert
        Assert.Empty(model.Projects);
    }

    [Fact]
    public void Projects_ShouldMaintainInsertionOrder()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");

        // Act
        model.Projects.Add(new ProjectModel("first", "application", "app", "/workspace"));
        model.Projects.Add(new ProjectModel("second", "library", "lib", "/workspace"));
        model.Projects.Add(new ProjectModel("third", "application", "app", "/workspace"));

        // Assert
        Assert.Equal("first", model.Projects[0].Name);
        Assert.Equal("second", model.Projects[1].Name);
        Assert.Equal("third", model.Projects[2].Name);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void WorkspaceModel_ShouldInheritFromArtifactModel()
    {
        // Arrange
        var model = new WorkspaceModel("test", "18.0.0", "/workspace");

        // Assert
        Assert.IsAssignableFrom<ArtifactModel>(model);
    }

    #endregion

    #region Version Format Tests

    [Fact]
    public void Constructor_WithSemanticVersion_ShouldStoreCorrectly()
    {
        // Arrange
        var version = "18.2.1";

        // Act
        var model = new WorkspaceModel("test", version, "/workspace");

        // Assert
        Assert.Equal("18.2.1", model.Version);
    }

    [Fact]
    public void Constructor_WithPreReleaseVersion_ShouldStoreCorrectly()
    {
        // Arrange
        var version = "19.0.0-rc.1";

        // Act
        var model = new WorkspaceModel("test", version, "/workspace");

        // Assert
        Assert.Equal("19.0.0-rc.1", model.Version);
    }

    [Fact]
    public void Constructor_WithBetaVersion_ShouldStoreCorrectly()
    {
        // Arrange
        var version = "18.0.0-beta.5";

        // Act
        var model = new WorkspaceModel("test", version, "/workspace");

        // Assert
        Assert.Equal("18.0.0-beta.5", model.Version);
    }

    #endregion
}
