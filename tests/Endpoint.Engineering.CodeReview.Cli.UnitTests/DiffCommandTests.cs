// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Endpoint.Engineering.CodeReview.Cli.Commands;
using Endpoint.Engineering.CodeReview.Cli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.CodeReview.Cli.UnitTests;

public class DiffCommandTests
{
    [Fact]
    public void Create_ShouldReturnCommand()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DiffCommand>>();
        var gitServiceMock = new Mock<IGitService>();
        var command = new DiffCommand(loggerMock.Object, gitServiceMock.Object);

        // Act
        var result = command.Create();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("diff", result.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallGitServiceAndSaveFile()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DiffCommand>>();
        var gitServiceMock = new Mock<IGitService>();
        var expectedDiff = "diff --git a/file.txt b/file.txt\nindex 123..456 789\n--- a/file.txt\n+++ b/file.txt\n@@ -1,1 +1,1 @@\n-old\n+new";
        
        gitServiceMock
            .Setup(x => x.GetDiffAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDiff);

        var command = new DiffCommand(loggerMock.Object, gitServiceMock.Object);
        var testOutputFile = Path.Combine(Path.GetTempPath(), $"test-diff-{Guid.NewGuid()}.txt");

        try
        {
            // Act
            await command.ExecuteAsync("https://github.com/test/repo", "feature-branch", testOutputFile);

            // Assert
            gitServiceMock.Verify(
                x => x.GetDiffAsync("https://github.com/test/repo", "feature-branch", It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.True(File.Exists(testOutputFile));
            var content = await File.ReadAllTextAsync(testOutputFile);
            Assert.Equal(expectedDiff, content);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testOutputFile))
            {
                File.Delete(testOutputFile);
            }
        }
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var gitServiceMock = new Mock<IGitService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiffCommand(null!, gitServiceMock.Object));
    }

    [Fact]
    public void Constructor_WithNullGitService_ThrowsArgumentNullException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DiffCommand>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiffCommand(loggerMock.Object, null!));
    }
}
