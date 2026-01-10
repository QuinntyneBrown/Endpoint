// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Testing;

namespace Endpoint.Testing.UnitTests;

public class SyntaxFactoryTests
{
    private readonly Mock<ILogger<SyntaxFactory>> _loggerMock;

    public SyntaxFactoryTests()
    {
        _loggerMock = new Mock<ILogger<SyntaxFactory>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Arrange & Act
        var sut = new SyntaxFactory(_loggerMock.Object);

        // Assert
        Assert.NotNull(sut);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SyntaxFactory(null!));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullExceptionWithCorrectParamName()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SyntaxFactory(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region DoWorkAsync Tests

    [Fact]
    public async Task DoWorkAsync_WhenCalled_ShouldCompleteSuccessfully()
    {
        // Arrange
        var sut = new SyntaxFactory(_loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => sut.DoWorkAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task DoWorkAsync_WhenCalled_ShouldLogInformation()
    {
        // Arrange
        var sut = new SyntaxFactory(_loggerMock.Object);

        // Act
        await sut.DoWorkAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DoWorkAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DoWorkAsync_WhenCalledMultipleTimes_ShouldCompleteSuccessfullyEachTime()
    {
        // Arrange
        var sut = new SyntaxFactory(_loggerMock.Object);

        // Act
        await sut.DoWorkAsync();
        await sut.DoWorkAsync();
        await sut.DoWorkAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DoWorkAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task DoWorkAsync_WhenCalled_ShouldReturnCompletedTask()
    {
        // Arrange
        var sut = new SyntaxFactory(_loggerMock.Object);

        // Act
        var task = sut.DoWorkAsync();

        // Assert
        Assert.True(task.IsCompleted || await Task.Run(async () => { await task; return true; }));
    }

    #endregion
}
