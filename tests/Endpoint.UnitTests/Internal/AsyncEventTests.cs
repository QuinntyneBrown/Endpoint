// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Internal;
using Microsoft.Build.Framework;
using Moq;

namespace Endpoint.UnitTests.Internal;

public class AsyncEventTests
{
    public class TestEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitialize()
    {
        // Arrange & Act
        var asyncEvent = new AsyncEvent<TestEventArgs>();

        // Assert
        Assert.NotNull(asyncEvent);
        Assert.False(asyncEvent.HasHandlers);
    }

    [Fact]
    public void AddHandler_WithAction_ShouldAddHandler()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        Action<TestEventArgs> handler = args => { };

        // Act
        asyncEvent.AddHandler(handler);

        // Assert
        Assert.True(asyncEvent.HasHandlers);
    }

    [Fact]
    public void AddHandler_WithAsyncFunc_ShouldAddHandler()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        Func<TestEventArgs, Task> handler = args => Task.CompletedTask;

        // Act
        asyncEvent.AddHandler(handler);

        // Assert
        Assert.True(asyncEvent.HasHandlers);
    }

    [Fact]
    public void AddHandler_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            asyncEvent.AddHandler((Action<TestEventArgs>)null!));
    }

    [Fact]
    public void AddHandler_WithNullAsyncFunc_ShouldThrowArgumentNullException()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            asyncEvent.AddHandler((Func<TestEventArgs, Task>)null!));
    }

    [Fact]
    public void RemoveHandler_WithAction_ShouldRemoveHandler()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        Action<TestEventArgs> handler = args => { };
        asyncEvent.AddHandler(handler);

        // Act
        asyncEvent.RemoveHandler(handler);

        // Assert
        Assert.False(asyncEvent.HasHandlers);
    }

    [Fact]
    public void RemoveHandler_WithAsyncFunc_ShouldRemoveHandler()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        Func<TestEventArgs, Task> handler = args => Task.CompletedTask;
        asyncEvent.AddHandler(handler);

        // Act
        asyncEvent.RemoveHandler(handler);

        // Assert
        Assert.False(asyncEvent.HasHandlers);
    }

    [Fact]
    public void RemoveHandler_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            asyncEvent.RemoveHandler((Action<TestEventArgs>)null!));
    }

    [Fact]
    public void RemoveHandler_WithNullAsyncFunc_ShouldThrowArgumentNullException()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            asyncEvent.RemoveHandler((Func<TestEventArgs, Task>)null!));
    }

    [Fact]
    public async Task InvokeAsync_WithNoHandlers_ShouldReturnImmediately()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        var eventArgs = new TestEventArgs { Message = "test" };

        // Act
        await asyncEvent.InvokeAsync(eventArgs);

        // Assert - Should complete without throwing
        Assert.False(asyncEvent.HasHandlers);
    }

    [Fact]
    public async Task InvokeAsync_WithActionHandler_ShouldInvokeHandler()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        var wasCalled = false;
        Action<TestEventArgs> handler = args => { wasCalled = true; };
        asyncEvent.AddHandler(handler);
        var eventArgs = new TestEventArgs { Message = "test" };

        // Act
        await asyncEvent.InvokeAsync(eventArgs);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithAsyncHandler_ShouldInvokeHandler()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        var wasCalled = false;
        Func<TestEventArgs, Task> handler = args => { wasCalled = true; return Task.CompletedTask; };
        asyncEvent.AddHandler(handler);
        var eventArgs = new TestEventArgs { Message = "test" };

        // Act
        await asyncEvent.InvokeAsync(eventArgs);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task TryInvokeAsync_WithNullEventArgs_ShouldThrowArgumentNullException()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        var mockLogger = new Mock<Microsoft.Build.Framework.ILogger>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await asyncEvent.TryInvokeAsync(null!, mockLogger.Object));
    }

    [Fact]
    public async Task TryInvokeAsync_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        var eventArgs = new TestEventArgs();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await asyncEvent.TryInvokeAsync(eventArgs, null!));
    }

    [Fact]
    public async Task TryInvokeAsync_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        var asyncEvent = new AsyncEvent<TestEventArgs>();
        var mockLogger = new Mock<Microsoft.Build.Framework.ILogger>();
        var eventArgs = new TestEventArgs { Message = "test" };

        // Act
        await asyncEvent.TryInvokeAsync(eventArgs, mockLogger.Object);

        // Assert - Should complete without throwing
        Assert.True(true);
    }
}
