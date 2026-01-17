// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Endpoint.Engineering.Cli.Commands;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.Cli.UnitTests.Commands;

public class CommandIssueCreateRequestHandlerTests
{
    private readonly Mock<ILogger<CommandIssueCreateRequestHandler>> _loggerMock;
    private readonly Mock<IClipboardService> _clipboardServiceMock;
    private readonly CommandIssueCreateRequestHandler _handler;

    public CommandIssueCreateRequestHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CommandIssueCreateRequestHandler>>();
        _clipboardServiceMock = new Mock<IClipboardService>();

        _handler = new CommandIssueCreateRequestHandler(
            _loggerMock.Object,
            _clipboardServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandIssueCreateRequestHandler(
            null!,
            _clipboardServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenClipboardServiceIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandIssueCreateRequestHandler(
            _loggerMock.Object,
            null!));
    }

    [Fact]
    public async Task Handle_ShouldSetClipboardText_WithFormattedIssueTemplate()
    {
        // Arrange
        var request = new CommandIssueCreateRequest
        {
            Prompt = "Code a Rocket Ship",
            Type = "code"
        };

        string capturedText = null;
        _clipboardServiceMock.Setup(x => x.SetText(It.IsAny<string>()))
            .Callback<string>(text => capturedText = text);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _clipboardServiceMock.Verify(x => x.SetText(It.IsAny<string>()), Times.Once);
        Assert.NotNull(capturedText);
        Assert.Contains("In Endpoint.Engineering.Cli", capturedText);
        Assert.Contains("Create a new command called \"code\"", capturedText);
        Assert.Contains("Code a Rocket Ship", capturedText);
        Assert.Contains("Add Unit Tests so that there is 80% Test coverage", capturedText);
    }

    [Fact]
    public async Task Handle_ShouldIncludePromptText_InOutputTemplate()
    {
        // Arrange
        var request = new CommandIssueCreateRequest
        {
            Prompt = "Build a Web API",
            Type = "api"
        };

        string capturedText = null;
        _clipboardServiceMock.Setup(x => x.SetText(It.IsAny<string>()))
            .Callback<string>(text => capturedText = text);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Contains("Build a Web API", capturedText);
        Assert.Contains("Create a new command called \"api\"", capturedText);
    }

    [Fact]
    public async Task Handle_ShouldIncludeTypeInPlaygroundPath()
    {
        // Arrange
        var request = new CommandIssueCreateRequest
        {
            Prompt = "Test feature",
            Type = "feature-test"
        };

        string capturedText = null;
        _clipboardServiceMock.Setup(x => x.SetText(It.IsAny<string>()))
            .Callback<string>(text => capturedText = text);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Contains("playground folder demonstrating the functionality of feature-test", capturedText);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var request = new CommandIssueCreateRequest
        {
            Prompt = "Test",
            Type = "test"
        };

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}

public class CommandIssueCreateRequestTests
{
    [Fact]
    public void Prompt_ShouldBeSettable()
    {
        // Arrange & Act
        var request = new CommandIssueCreateRequest
        {
            Prompt = "Test prompt"
        };

        // Assert
        Assert.Equal("Test prompt", request.Prompt);
    }

    [Fact]
    public void Type_ShouldBeSettable()
    {
        // Arrange & Act
        var request = new CommandIssueCreateRequest
        {
            Type = "test-type"
        };

        // Assert
        Assert.Equal("test-type", request.Type);
    }
}
