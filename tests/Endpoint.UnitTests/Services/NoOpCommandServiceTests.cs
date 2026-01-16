// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;

namespace Endpoint.UnitTests.Services;

public class NoOpCommandServiceTests
{
    [Fact]
    public void NoOpCommandService_ShouldCreateInstance()
    {
        // Arrange & Act
        var commandService = new NoOpCommandService();

        // Assert
        Assert.NotNull(commandService);
    }

    [Fact]
    public void Start_ShouldReturnZero_WithArguments()
    {
        // Arrange
        var commandService = new NoOpCommandService();
        var arguments = "test command";

        // Act
        var result = commandService.Start(arguments);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_ShouldReturnZero_WithWorkingDirectory()
    {
        // Arrange
        var commandService = new NoOpCommandService();
        var arguments = "test command";
        var workingDirectory = "/test/path";

        // Act
        var result = commandService.Start(arguments, workingDirectory);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_ShouldReturnZero_WithWaitForExitTrue()
    {
        // Arrange
        var commandService = new NoOpCommandService();
        var arguments = "test command";

        // Act
        var result = commandService.Start(arguments, waitForExit: true);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_ShouldReturnZero_WithWaitForExitFalse()
    {
        // Arrange
        var commandService = new NoOpCommandService();
        var arguments = "test command";

        // Act
        var result = commandService.Start(arguments, waitForExit: false);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_ShouldReturnZero_WithAllParameters()
    {
        // Arrange
        var commandService = new NoOpCommandService();
        var arguments = "test command";
        var workingDirectory = "/test/path";

        // Act
        var result = commandService.Start(arguments, workingDirectory, true);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_ShouldReturnZero_WithEmptyArguments()
    {
        // Arrange
        var commandService = new NoOpCommandService();

        // Act
        var result = commandService.Start(string.Empty);

        // Assert
        Assert.Equal(0, result);
    }
}
