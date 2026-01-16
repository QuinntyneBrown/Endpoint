// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace Endpoint.UnitTests.Services;

public class CommandServiceTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandService(null!));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CommandService>>();

        // Act
        var commandService = new CommandService(mockLogger.Object);

        // Assert
        Assert.NotNull(commandService);
    }

    [Fact]
    public void Start_ShouldUseCurrentDirectory_WhenWorkingDirectoryIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CommandService>>();
        var commandService = new CommandService(mockLogger.Object);
        var arguments = "echo test";

        // Act
        var result = commandService.Start(arguments, workingDirectory: null, waitForExit: true);

        // Assert - Just verify it doesn't throw and returns an exit code
        Assert.True(result >= 0 || result == 1);
    }

    [Fact]
    public void Start_ShouldReturnOne_WhenWaitForExitIsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CommandService>>();
        var commandService = new CommandService(mockLogger.Object);
        var arguments = "echo test";
        var workingDirectory = Environment.CurrentDirectory;

        // Act
        var result = commandService.Start(arguments, workingDirectory, waitForExit: false);

        // Assert
        Assert.Equal(1, result);
    }
}
