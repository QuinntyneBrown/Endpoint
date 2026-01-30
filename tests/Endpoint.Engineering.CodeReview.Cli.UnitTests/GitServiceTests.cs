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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetDiffAsync_WithInvalidUrl_ThrowsArgumentException(string? url)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GitService>>();
        var service = new GitService(loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await service.GetDiffAsync(url!);
        });
    }
}
