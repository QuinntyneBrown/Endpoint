// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis;

public class CodeReviewServiceTests
{
    private readonly Mock<ILogger<CodeReviewService>> _loggerMock;
    private readonly Mock<IStaticAnalysisService> _staticAnalysisServiceMock;
    private readonly CodeReviewService _service;

    public CodeReviewServiceTests()
    {
        _loggerMock = new Mock<ILogger<CodeReviewService>>();
        _staticAnalysisServiceMock = new Mock<IStaticAnalysisService>();
        _service = new CodeReviewService(_loggerMock.Object, _staticAnalysisServiceMock.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CodeReviewService(null!, _staticAnalysisServiceMock.Object));
    }

    [Fact]
    public void Constructor_WithNullStaticAnalysisService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CodeReviewService(_loggerMock.Object, null!));
    }

    [Fact]
    public void FindGitRepositoryRoot_WithGitRepository_ReturnsRoot()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gitDir = Path.Combine(tempDir, ".git");
        var subDir = Path.Combine(tempDir, "src");

        try
        {
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(subDir);

            // Act
            var result = _service.FindGitRepositoryRoot(subDir);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tempDir, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void FindGitRepositoryRoot_WithoutGitRepository_ReturnsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(tempDir);

            // Act
            var result = _service.FindGitRepositoryRoot(tempDir);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ReviewAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            _service.ReviewAsync(nonExistentDir));
    }

    [Fact]
    public async Task ReviewAsync_WithNonGitDirectory_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(tempDir);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.ReviewAsync(tempDir));

            Assert.Contains("No git repository found", exception.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void FindGitRepositoryRoot_WithNestedDirectories_FindsRoot()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gitDir = Path.Combine(tempDir, ".git");
        var nestedDir = Path.Combine(tempDir, "level1", "level2", "level3");

        try
        {
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(nestedDir);

            // Act
            var result = _service.FindGitRepositoryRoot(nestedDir);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tempDir, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
