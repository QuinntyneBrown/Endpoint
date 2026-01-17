// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Core.UnitTests;

public class ALaCarteServiceTests : IDisposable
{
    private readonly Mock<ILogger<ALaCarteService>> _mockLogger;
    private readonly ALaCarteService _service;
    private readonly string _testDirectory;

    public ALaCarteServiceTests()
    {
        _mockLogger = new Mock<ILogger<ALaCarteService>>();
        _service = new ALaCarteService(_mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"alacarte_test_{Guid.NewGuid():N}");
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
        Assert.Throws<ArgumentNullException>(() => new ALaCarteService(null!));
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // Act
        var service = new ALaCarteService(_mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region ProcessAsync - Basic Tests

    [Fact]
    public async Task ProcessAsync_WithEmptyRepositories_ReturnsSuccessResult()
    {
        // Arrange
        var request = new ALaCarteRequest
        {
            Directory = _testDirectory,
            Repositories = new List<RepositoryConfiguration>()
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(_testDirectory, result.OutputDirectory);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ProcessAsync_CreatesOutputDirectoryIfNotExists()
    {
        // Arrange
        var newDir = Path.Combine(_testDirectory, "new_output");
        var request = new ALaCarteRequest
        {
            Directory = newDir,
            Repositories = new List<RepositoryConfiguration>()
        };

        // Act
        await _service.ProcessAsync(request);

        // Assert
        Assert.True(Directory.Exists(newDir));
    }

    #endregion

    #region ProcessAsync - LocalDirectory Tests

    [Fact]
    public async Task ProcessAsync_WithLocalDirectory_CopiesFiles()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var destDir = Path.Combine(_testDirectory, "dest");
        var folderToCopy = Path.Combine(sourceDir, "myproject");
        Directory.CreateDirectory(folderToCopy);
        File.WriteAllText(Path.Combine(folderToCopy, "file.txt"), "test content");

        var request = new ALaCarteRequest
        {
            Directory = destDir,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = sourceDir,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "myproject",
                            To = "dest_project"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(destDir, "dest_project", "file.txt")));
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentLocalDirectory_AddsError()
    {
        // Arrange
        var request = new ALaCarteRequest
        {
            Directory = _testDirectory,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = "/nonexistent/path",
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration { From = "folder", To = "dest" }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("LocalDirectory does not exist"));
    }

    [Fact]
    public async Task ProcessAsync_WithMissingSourceFolder_AddsWarning()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        Directory.CreateDirectory(sourceDir);

        var request = new ALaCarteRequest
        {
            Directory = _testDirectory,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = sourceDir,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration { From = "nonexistent", To = "dest" }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Contains(result.Warnings, w => w.Contains("Source folder not found"));
    }

    #endregion

    #region ProcessAsync - Csproj Detection Tests

    [Fact]
    public async Task ProcessAsync_DetectsCsprojFiles()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var projectDir = Path.Combine(sourceDir, "MyProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "MyProject.csproj"), "<Project></Project>");

        var request = new ALaCarteRequest
        {
            Directory = _testDirectory,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = sourceDir,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration { From = "MyProject", To = "MyProject" }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.Single(result.CsprojFiles);
    }

    #endregion

    #region ProcessAsync - Skip .git Directory Tests

    [Fact]
    public async Task ProcessAsync_SkipsGitDirectory()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var projectDir = Path.Combine(sourceDir, "project");
        var gitDir = Path.Combine(projectDir, ".git");
        Directory.CreateDirectory(gitDir);
        File.WriteAllText(Path.Combine(gitDir, "config"), "git config");
        File.WriteAllText(Path.Combine(projectDir, "file.txt"), "content");

        var request = new ALaCarteRequest
        {
            Directory = _testDirectory,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = sourceDir,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration { From = "project", To = "output" }
                    }
                }
            }
        };

        // Act
        await _service.ProcessAsync(request);

        // Assert
        Assert.False(Directory.Exists(Path.Combine(_testDirectory, "output", ".git")));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "output", "file.txt")));
    }

    #endregion

    #region ProcessAsync - No Url Or LocalDirectory Tests

    [Fact]
    public async Task ProcessAsync_WithNeitherUrlNorLocalDirectory_AddsError()
    {
        // Arrange
        var request = new ALaCarteRequest
        {
            Directory = _testDirectory,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration { From = "folder", To = "dest" }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Either Url or LocalDirectory must be specified"));
    }

    #endregion

    #region TakeAsync - Basic Tests

    [Fact]
    public async Task TakeAsync_WithNonExistentFromDirectory_ReturnsError()
    {
        // Arrange
        var request = new ALaCarteTakeRequest
        {
            Directory = _testDirectory,
            FromDirectory = "/nonexistent/directory",
            FromPath = "folder"
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("FromDirectory does not exist"));
    }

    [Fact]
    public async Task TakeAsync_WithValidLocalDirectory_CopiesFolder()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var folderToCopy = Path.Combine(sourceDir, "mylib");
        Directory.CreateDirectory(folderToCopy);
        File.WriteAllText(Path.Combine(folderToCopy, "index.ts"), "export {};");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir,
            FromPath = "mylib"
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(destDir, "mylib", "index.ts")));
    }

    [Fact]
    public async Task TakeAsync_WithMissingFromPath_ReturnsError()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        Directory.CreateDirectory(sourceDir);

        var request = new ALaCarteTakeRequest
        {
            Directory = _testDirectory,
            FromDirectory = sourceDir,
            FromPath = "nonexistent"
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Source folder not found"));
    }

    #endregion

    #region TakeAsync - Skip Directories Tests

    [Fact]
    public async Task TakeAsync_SkipsNodeModules()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var folderToCopy = Path.Combine(sourceDir, "mylib");
        var nodeModules = Path.Combine(folderToCopy, "node_modules");
        Directory.CreateDirectory(nodeModules);
        File.WriteAllText(Path.Combine(nodeModules, "package.json"), "{}");
        File.WriteAllText(Path.Combine(folderToCopy, "index.ts"), "export {};");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir,
            FromPath = "mylib"
        };

        // Act
        await _service.TakeAsync(request);

        // Assert
        Assert.False(Directory.Exists(Path.Combine(destDir, "mylib", "node_modules")));
    }

    [Fact]
    public async Task TakeAsync_SkipsBinAndObj()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var folderToCopy = Path.Combine(sourceDir, "myproj");
        Directory.CreateDirectory(Path.Combine(folderToCopy, "bin"));
        Directory.CreateDirectory(Path.Combine(folderToCopy, "obj"));
        File.WriteAllText(Path.Combine(folderToCopy, "bin", "output.dll"), "binary");
        File.WriteAllText(Path.Combine(folderToCopy, "Program.cs"), "class Program {}");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir,
            FromPath = "myproj"
        };

        // Act
        await _service.TakeAsync(request);

        // Assert
        Assert.False(Directory.Exists(Path.Combine(destDir, "myproj", "bin")));
        Assert.False(Directory.Exists(Path.Combine(destDir, "myproj", "obj")));
        Assert.True(File.Exists(Path.Combine(destDir, "myproj", "Program.cs")));
    }

    #endregion

    #region TakeAsync - Project Type Detection Tests

    [Fact]
    public async Task TakeAsync_DetectsDotNetProject()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var folderToCopy = Path.Combine(sourceDir, "MyProject");
        Directory.CreateDirectory(folderToCopy);
        File.WriteAllText(Path.Combine(folderToCopy, "MyProject.csproj"), "<Project></Project>");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir,
            FromPath = "MyProject"
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.True(result.IsDotNetProject);
        Assert.Single(result.CsprojFiles);
    }

    [Fact]
    public async Task TakeAsync_DetectsAngularProject()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var folderToCopy = Path.Combine(sourceDir, "my-lib");
        Directory.CreateDirectory(folderToCopy);
        File.WriteAllText(Path.Combine(folderToCopy, "ng-package.json"), "{}");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir,
            FromPath = "my-lib"
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.True(result.IsAngularProject);
    }

    [Fact]
    public async Task TakeAsync_DetectsAngularWorkspace()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "source");
        var folderToCopy = Path.Combine(sourceDir, "workspace");
        Directory.CreateDirectory(folderToCopy);
        File.WriteAllText(Path.Combine(folderToCopy, "angular.json"), @"{""projects"":{}}");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir,
            FromPath = "workspace"
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.True(result.IsAngularProject);
        Assert.NotNull(result.AngularWorkspacePath);
    }

    #endregion

    #region TakeAsync - Without FromPath Tests

    [Fact]
    public async Task TakeAsync_WithoutFromPath_CopiesEntireFromDirectory()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDirectory, "mysource");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "file.txt"), "content");

        var destDir = Path.Combine(_testDirectory, "dest");

        var request = new ALaCarteTakeRequest
        {
            Directory = destDir,
            FromDirectory = sourceDir
        };

        // Act
        var result = await _service.TakeAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(destDir, "mysource", "file.txt")));
    }

    #endregion
}
