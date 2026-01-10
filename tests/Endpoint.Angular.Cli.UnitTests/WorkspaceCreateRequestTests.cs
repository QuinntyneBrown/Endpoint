// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.Cli.UnitTests;

public class WorkspaceCreateRequestTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Constructor_Default_DirectoryEqualsEnvironmentCurrentDirectory()
    {
        // Arrange & Act
        var request = new WorkspaceCreateRequest();

        // Assert
        Assert.Equal(Environment.CurrentDirectory, request.Directory);
    }

    [Fact]
    public void Constructor_Default_NameIsNull()
    {
        // Arrange & Act
        var request = new WorkspaceCreateRequest();

        // Assert
        Assert.Null(request.Name);
    }

    [Fact]
    public void Constructor_Default_ProjectTypeIsNull()
    {
        // Arrange & Act
        var request = new WorkspaceCreateRequest();

        // Assert
        Assert.Null(request.ProjectType);
    }

    [Fact]
    public void Constructor_Default_ProjectNameIsNull()
    {
        // Arrange & Act
        var request = new WorkspaceCreateRequest();

        // Assert
        Assert.Null(request.ProjectName);
    }

    #endregion

    #region Name Property Tests

    [Fact]
    public void Name_GetSet_ReturnsSetValue()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedName = "TestWorkspace";

        // Act
        request.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, request.Name);
    }

    [Fact]
    public void Name_SetToEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();

        // Act
        request.Name = string.Empty;

        // Assert
        Assert.Equal(string.Empty, request.Name);
    }

    [Fact]
    public void Name_SetToNull_ReturnsNull()
    {
        // Arrange
        var request = new WorkspaceCreateRequest { Name = "InitialValue" };

        // Act
        request.Name = null;

        // Assert
        Assert.Null(request.Name);
    }

    [Fact]
    public void Name_SetWithSpecialCharacters_ReturnsValueWithSpecialCharacters()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedName = "Test-Workspace_123";

        // Act
        request.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, request.Name);
    }

    #endregion

    #region ProjectType Property Tests

    [Fact]
    public void ProjectType_GetSet_ReturnsSetValue()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedProjectType = "application";

        // Act
        request.ProjectType = expectedProjectType;

        // Assert
        Assert.Equal(expectedProjectType, request.ProjectType);
    }

    [Fact]
    public void ProjectType_SetToEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();

        // Act
        request.ProjectType = string.Empty;

        // Assert
        Assert.Equal(string.Empty, request.ProjectType);
    }

    [Fact]
    public void ProjectType_SetToNull_ReturnsNull()
    {
        // Arrange
        var request = new WorkspaceCreateRequest { ProjectType = "library" };

        // Act
        request.ProjectType = null;

        // Assert
        Assert.Null(request.ProjectType);
    }

    [Fact]
    public void ProjectType_SetToLibrary_ReturnsLibrary()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();

        // Act
        request.ProjectType = "library";

        // Assert
        Assert.Equal("library", request.ProjectType);
    }

    #endregion

    #region ProjectName Property Tests

    [Fact]
    public void ProjectName_GetSet_ReturnsSetValue()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedProjectName = "MyProject";

        // Act
        request.ProjectName = expectedProjectName;

        // Assert
        Assert.Equal(expectedProjectName, request.ProjectName);
    }

    [Fact]
    public void ProjectName_SetToEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();

        // Act
        request.ProjectName = string.Empty;

        // Assert
        Assert.Equal(string.Empty, request.ProjectName);
    }

    [Fact]
    public void ProjectName_SetToNull_ReturnsNull()
    {
        // Arrange
        var request = new WorkspaceCreateRequest { ProjectName = "InitialProject" };

        // Act
        request.ProjectName = null;

        // Assert
        Assert.Null(request.ProjectName);
    }

    [Fact]
    public void ProjectName_SetWithSpecialCharacters_ReturnsValueWithSpecialCharacters()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedProjectName = "my-angular-project";

        // Act
        request.ProjectName = expectedProjectName;

        // Assert
        Assert.Equal(expectedProjectName, request.ProjectName);
    }

    #endregion

    #region Directory Property Tests

    [Fact]
    public void Directory_GetSet_ReturnsSetValue()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedDirectory = "/custom/path/to/workspace";

        // Act
        request.Directory = expectedDirectory;

        // Assert
        Assert.Equal(expectedDirectory, request.Directory);
    }

    [Fact]
    public void Directory_SetToCustomPath_OverridesDefaultValue()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var customDirectory = "/tmp/test-workspace";

        // Act
        request.Directory = customDirectory;

        // Assert
        Assert.NotEqual(Environment.CurrentDirectory, request.Directory);
        Assert.Equal(customDirectory, request.Directory);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void WorkspaceCreateRequest_ImplementsIRequestInterface()
    {
        // Arrange & Act
        var request = new WorkspaceCreateRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest>(request);
    }

    [Fact]
    public void WorkspaceCreateRequest_CanBeCastToIRequest()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();

        // Act
        IRequest iRequest = request;

        // Assert
        Assert.NotNull(iRequest);
    }

    #endregion

    #region Combined Property Tests

    [Fact]
    public void WorkspaceCreateRequest_AllPropertiesCanBeSetTogether()
    {
        // Arrange
        var request = new WorkspaceCreateRequest();
        var expectedName = "TestWorkspace";
        var expectedProjectType = "application";
        var expectedProjectName = "TestProject";
        var expectedDirectory = "/test/directory";

        // Act
        request.Name = expectedName;
        request.ProjectType = expectedProjectType;
        request.ProjectName = expectedProjectName;
        request.Directory = expectedDirectory;

        // Assert
        Assert.Equal(expectedName, request.Name);
        Assert.Equal(expectedProjectType, request.ProjectType);
        Assert.Equal(expectedProjectName, request.ProjectName);
        Assert.Equal(expectedDirectory, request.Directory);
    }

    [Fact]
    public void WorkspaceCreateRequest_ObjectInitializerSyntax_SetsAllProperties()
    {
        // Arrange & Act
        var request = new WorkspaceCreateRequest
        {
            Name = "InitializerWorkspace",
            ProjectType = "library",
            ProjectName = "InitializerProject",
            Directory = "/initializer/path"
        };

        // Assert
        Assert.Equal("InitializerWorkspace", request.Name);
        Assert.Equal("library", request.ProjectType);
        Assert.Equal("InitializerProject", request.ProjectName);
        Assert.Equal("/initializer/path", request.Directory);
    }

    #endregion
}
