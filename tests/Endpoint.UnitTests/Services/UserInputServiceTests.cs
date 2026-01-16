// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace Endpoint.UnitTests.Services;

public class UserInputServiceTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var mockCommandService = new Mock<ICommandService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserInputService(null!, mockFileSystem, mockCommandService.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFileSystemIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserInputService>>();
        var mockCommandService = new Mock<ICommandService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserInputService(mockLogger.Object, null!, mockCommandService.Object));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserInputService>>();
        var mockFileSystem = new MockFileSystem();
        var mockCommandService = new Mock<ICommandService>();

        // Act
        var userInputService = new UserInputService(mockLogger.Object, mockFileSystem, mockCommandService.Object);

        // Assert
        Assert.NotNull(userInputService);
    }
}
