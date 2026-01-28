// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
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

    [Theory]
    [InlineData(null, "branch")]
    [InlineData("", "branch")]
    [InlineData("   ", "branch")]
    [InlineData("https://github.com/test/repo", null)]
    [InlineData("https://github.com/test/repo", "")]
    [InlineData("https://github.com/test/repo", "   ")]
    public async Task GetDiffAsync_WithInvalidParameters_ThrowsArgumentException(string? url, string? branch)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GitService>>();
        var service = new GitService(loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await service.GetDiffAsync(url!, branch!);
        });
    }
}
