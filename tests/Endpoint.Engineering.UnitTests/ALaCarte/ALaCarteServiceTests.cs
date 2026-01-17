// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte;
using Endpoint.Engineering.ALaCarte.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.UnitTests.ALaCarte;

/// <summary>
/// Unit tests for ALaCarteService.
/// </summary>
public class ALaCarteServiceTests : IDisposable
{
    private readonly Mock<ILogger<ALaCarteService>> _loggerMock;
    private readonly ALaCarteService _service;
    private readonly string _tempDirectory;
    private readonly string _sourceDirectory;
    private readonly string _outputDirectory;

    public ALaCarteServiceTests()
    {
        _loggerMock = new Mock<ILogger<ALaCarteService>>();
        _service = new ALaCarteService(_loggerMock.Object);

        // Create temporary directories for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"ALaCarteServiceTests_{Guid.NewGuid():N}");
        _sourceDirectory = Path.Combine(_tempDirectory, "source");
        _outputDirectory = Path.Combine(_tempDirectory, "output");

        Directory.CreateDirectory(_sourceDirectory);
        Directory.CreateDirectory(_outputDirectory);
    }

    public void Dispose()
    {
        // Clean up temporary directories
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Ignore IO exceptions during cleanup (e.g., file in use)
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore access exceptions during cleanup
            }
        }
    }

    [Fact]
    public async Task ProcessAsync_WithLocalDirectory_CopiesFilesSuccessfully()
    {
        // Arrange
        var sourceFolder = Path.Combine(_sourceDirectory, "src", "MyLibrary");
        Directory.CreateDirectory(sourceFolder);

        // Create some test files
        File.WriteAllText(Path.Combine(sourceFolder, "Class1.cs"), "public class Class1 { }");
        File.WriteAllText(Path.Combine(sourceFolder, "Class2.cs"), "public class Class2 { }");

        var request = new ALaCarteRequest
        {
            Directory = _outputDirectory,
            OutputType = OutputType.NotSpecified,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = _sourceDirectory,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "src/MyLibrary",
                            To = "lib"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.Equal(_outputDirectory, result.OutputDirectory);

        var expectedFile1 = Path.Combine(_outputDirectory, "lib", "Class1.cs");
        var expectedFile2 = Path.Combine(_outputDirectory, "lib", "Class2.cs");

        Assert.True(File.Exists(expectedFile1), $"Expected file not found: {expectedFile1}");
        Assert.True(File.Exists(expectedFile2), $"Expected file not found: {expectedFile2}");
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentLocalDirectory_ReturnsError()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(_tempDirectory, "non_existent");

        var request = new ALaCarteRequest
        {
            Directory = _outputDirectory,
            OutputType = OutputType.NotSpecified,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = nonExistentDirectory,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "src",
                            To = "dest"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("LocalDirectory does not exist"));
    }

    [Fact]
    public async Task ProcessAsync_WithLocalDirectory_SkipsGitDirectories()
    {
        // Arrange
        var sourceFolder = Path.Combine(_sourceDirectory, "src", "MyProject");
        var gitFolder = Path.Combine(sourceFolder, ".git");
        Directory.CreateDirectory(sourceFolder);
        Directory.CreateDirectory(gitFolder);

        File.WriteAllText(Path.Combine(sourceFolder, "Program.cs"), "public class Program { }");
        File.WriteAllText(Path.Combine(gitFolder, "config"), "git config");

        var request = new ALaCarteRequest
        {
            Directory = _outputDirectory,
            OutputType = OutputType.NotSpecified,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = _sourceDirectory,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "src/MyProject",
                            To = "project"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success);
        var programFile = Path.Combine(_outputDirectory, "project", "Program.cs");
        var gitConfigFile = Path.Combine(_outputDirectory, "project", ".git", "config");

        Assert.True(File.Exists(programFile), "Program.cs should be copied");
        Assert.False(File.Exists(gitConfigFile), ".git directory should not be copied");
    }

    [Fact]
    public async Task ProcessAsync_WithLocalDirectory_TracksCsprojFiles()
    {
        // Arrange
        var sourceFolder = Path.Combine(_sourceDirectory, "src", "MyLibrary");
        Directory.CreateDirectory(sourceFolder);

        File.WriteAllText(Path.Combine(sourceFolder, "MyLibrary.csproj"), "<Project></Project>");
        File.WriteAllText(Path.Combine(sourceFolder, "Class1.cs"), "public class Class1 { }");

        var request = new ALaCarteRequest
        {
            Directory = _outputDirectory,
            OutputType = OutputType.NotSpecified,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = _sourceDirectory,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "src/MyLibrary",
                            To = "lib"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.CsprojFiles);
        Assert.Contains(result.CsprojFiles, p => p.EndsWith("MyLibrary.csproj"));
    }

    [Fact]
    public async Task ProcessAsync_WithMissingUrlAndLocalDirectory_ReturnsError()
    {
        // Arrange
        var request = new ALaCarteRequest
        {
            Directory = _outputDirectory,
            OutputType = OutputType.NotSpecified,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    // Neither Url nor LocalDirectory specified
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "src",
                            To = "dest"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("Either Url or LocalDirectory must be specified"));
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentSourceFolder_ReturnsWarning()
    {
        // Arrange
        var sourceFolder = Path.Combine(_sourceDirectory, "src", "Existing");
        Directory.CreateDirectory(sourceFolder);
        File.WriteAllText(Path.Combine(sourceFolder, "File.cs"), "public class File { }");

        var request = new ALaCarteRequest
        {
            Directory = _outputDirectory,
            OutputType = OutputType.NotSpecified,
            Repositories = new List<RepositoryConfiguration>
            {
                new RepositoryConfiguration
                {
                    LocalDirectory = _sourceDirectory,
                    Folders = new List<FolderConfiguration>
                    {
                        new FolderConfiguration
                        {
                            From = "src/NonExistent",
                            To = "dest"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.True(result.Success); // Success because errors are warnings
        Assert.NotEmpty(result.Warnings);
        Assert.Contains(result.Warnings, w => w.Contains("Source folder not found"));
    }
}
