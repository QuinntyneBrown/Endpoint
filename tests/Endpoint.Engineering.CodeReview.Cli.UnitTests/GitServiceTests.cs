// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Endpoint.Engineering.CodeReview.Cli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.CodeReview.Cli.UnitTests;

public class GitServiceTests
{
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GitService(null!));
    }

    [Fact]
    public async Task GetDiffAsync_WithInvalidBranch_ThrowsInvalidOperationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GitService>>();
        var service = new GitService(loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GetDiffAsync("https://github.com/QuinntyneBrown/Endpoint", "non-existent-branch-xyz-123");
        });
    }

    // Note: Testing actual git operations requires a real repository or mocking LibGit2Sharp,
    // which is complex. For integration tests, we would use a real repository.
    // These unit tests focus on the basic validation and error handling.
}
