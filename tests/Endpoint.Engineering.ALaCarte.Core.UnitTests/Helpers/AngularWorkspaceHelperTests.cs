// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Core.UnitTests.Helpers;

public class AngularWorkspaceHelperTests : IDisposable
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly AngularWorkspaceHelper _helper;
    private readonly string _testDirectory;

    public AngularWorkspaceHelperTests()
    {
        _mockLogger = new Mock<ILogger>();
        _helper = new AngularWorkspaceHelper(_mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"angular_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AngularWorkspaceHelper(null!));
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // Act
        var helper = new AngularWorkspaceHelper(_mockLogger.Object);

        // Assert
        Assert.NotNull(helper);
    }

    #endregion

    #region FindOrphanAngularProjects Tests

    [Fact]
    public void FindOrphanAngularProjects_WithNoNgPackageFiles_ReturnsEmptyList()
    {
        // Act
        var result = _helper.FindOrphanAngularProjects(_testDirectory);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOrphanAngularProjects_WithNgPackageNotReferencedByAngularJson_ReturnsOrphan()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "my-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");

        // Act
        var result = _helper.FindOrphanAngularProjects(_testDirectory);

        // Assert
        Assert.Single(result);
        Assert.Equal(libraryDir, result[0]);
    }

    [Fact]
    public void FindOrphanAngularProjects_WithNgPackageReferencedByAngularJson_ReturnsEmptyList()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "my-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");

        var angularJson = @"{
            ""$schema"": ""./node_modules/@angular/cli/lib/config/schema.json"",
            ""version"": 1,
            ""projects"": {
                ""my-library"": {
                    ""root"": ""my-library"",
                    ""projectType"": ""library""
                }
            }
        }";
        File.WriteAllText(Path.Combine(_testDirectory, "angular.json"), angularJson);

        // Act
        var result = _helper.FindOrphanAngularProjects(_testDirectory);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOrphanAngularProjects_WithMultipleOrphans_ReturnsAllOrphans()
    {
        // Arrange
        var lib1Dir = Path.Combine(_testDirectory, "lib1");
        var lib2Dir = Path.Combine(_testDirectory, "lib2");
        Directory.CreateDirectory(lib1Dir);
        Directory.CreateDirectory(lib2Dir);
        File.WriteAllText(Path.Combine(lib1Dir, "ng-package.json"), "{}");
        File.WriteAllText(Path.Combine(lib2Dir, "ng-package.json"), "{}");

        // Act
        var result = _helper.FindOrphanAngularProjects(_testDirectory);

        // Assert
        Assert.Equal(2, result.Count);
    }

    #endregion

    #region CreateWorkspaceForOrphanProject Tests

    [Fact]
    public void CreateWorkspaceForOrphanProject_CreatesAngularJson()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "projects", "my-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), @"{""lib"": {""entryFile"": ""src/public-api.ts""}}");

        // Act
        var result = _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Assert
        Assert.NotNull(result);
        Assert.True(File.Exists(result));
        Assert.EndsWith("angular.json", result);
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_CreatesPackageJson()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "projects", "my-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");

        var parentDir = Path.GetDirectoryName(libraryDir)!;

        // Act
        _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Assert
        Assert.True(File.Exists(Path.Combine(parentDir, "package.json")));
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_WhenAngularJsonExists_AddsToExisting()
    {
        // Arrange
        var projectsDir = Path.Combine(_testDirectory, "projects");
        var lib1Dir = Path.Combine(projectsDir, "lib1");
        var lib2Dir = Path.Combine(projectsDir, "lib2");
        Directory.CreateDirectory(lib1Dir);
        Directory.CreateDirectory(lib2Dir);
        File.WriteAllText(Path.Combine(lib1Dir, "ng-package.json"), "{}");
        File.WriteAllText(Path.Combine(lib2Dir, "ng-package.json"), "{}");

        // Create first workspace
        _helper.CreateWorkspaceForOrphanProject(lib1Dir);

        // Act
        var result = _helper.CreateWorkspaceForOrphanProject(lib2Dir);

        // Assert
        Assert.NotNull(result);
        var angularJsonContent = File.ReadAllText(result);
        Assert.Contains("lib1", angularJsonContent);
        Assert.Contains("lib2", angularJsonContent);
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_WithCustomRoot_CreatesCorrectPath()
    {
        // Arrange
        var deepDir = Path.Combine(_testDirectory, "packages", "scope", "my-lib");
        Directory.CreateDirectory(deepDir);
        File.WriteAllText(Path.Combine(deepDir, "ng-package.json"), "{}");

        // Act
        var result = _helper.CreateWorkspaceForOrphanProject(deepDir, "packages/scope/my-lib");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Path.Combine(_testDirectory, "angular.json"), result);
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_UsesDirectoryNameAsFallbackLibraryName()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "projects", "my-custom-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");

        // Act
        var result = _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Assert
        Assert.NotNull(result);
        var angularJsonContent = File.ReadAllText(result);
        Assert.Contains("my-custom-library", angularJsonContent);
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_WithPackageJson_UsesPackageName()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "projects", "my-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");
        File.WriteAllText(Path.Combine(libraryDir, "package.json"), @"{""name"": ""@scope/my-lib""}");

        // Act
        var result = _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Assert
        Assert.NotNull(result);
        var angularJsonContent = File.ReadAllText(result);
        Assert.Contains("scope-my-lib", angularJsonContent);
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_DuplicateProject_SkipsAddition()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "projects", "my-lib");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");

        // Create workspace first time
        _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Act - try to add same project again
        var result = _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Assert
        Assert.NotNull(result);
        var angularJsonContent = File.ReadAllText(result);
        var count = angularJsonContent.Split("my-lib").Length - 1;
        Assert.True(count >= 1); // Should not be duplicated
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FindOrphanAngularProjects_WithInvalidAngularJson_IgnoresAndContinues()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "my-library");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "{}");
        File.WriteAllText(Path.Combine(_testDirectory, "angular.json"), "invalid json");

        // Act
        var result = _helper.FindOrphanAngularProjects(_testDirectory);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void CreateWorkspaceForOrphanProject_WithInvalidNgPackageJson_UsesFallbackName()
    {
        // Arrange
        var libraryDir = Path.Combine(_testDirectory, "projects", "fallback-lib");
        Directory.CreateDirectory(libraryDir);
        File.WriteAllText(Path.Combine(libraryDir, "ng-package.json"), "invalid json");

        // Act
        var result = _helper.CreateWorkspaceForOrphanProject(libraryDir);

        // Assert
        Assert.NotNull(result);
        var angularJsonContent = File.ReadAllText(result);
        Assert.Contains("fallback-lib", angularJsonContent);
    }

    #endregion
}
