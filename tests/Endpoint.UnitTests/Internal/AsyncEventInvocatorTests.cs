// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Internal;

namespace Endpoint.UnitTests.Internal;

public class AsyncEventInvocatorTests
{
    public class TestEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_WithAction_ShouldInitialize()
    {
        // Arrange
        Action<TestEventArgs> handler = args => { };

        // Act
        var invocator = new AsyncEventInvocator<TestEventArgs>(handler, null);

        // Assert - Should not throw
        Assert.True(true);
    }

    [Fact]
    public void Constructor_WithAsyncHandler_ShouldInitialize()
    {
        // Arrange
        Func<TestEventArgs, Task> handler = args => Task.CompletedTask;

        // Act
        var invocator = new AsyncEventInvocator<TestEventArgs>(null, handler);

        // Assert - Should not throw
        Assert.True(true);
    }

    [Fact]
    public void WrapsHandler_WithAction_ShouldReturnTrue_WhenSameHandler()
    {
        // Arrange
        Action<TestEventArgs> handler = args => { };
        var invocator = new AsyncEventInvocator<TestEventArgs>(handler, null);

        // Act
        var result = invocator.WrapsHandler(handler);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WrapsHandler_WithAction_ShouldReturnFalse_WhenDifferentHandler()
    {
        // Arrange
        Action<TestEventArgs> handler1 = args => { };
        Action<TestEventArgs> handler2 = args => { };
        var invocator = new AsyncEventInvocator<TestEventArgs>(handler1, null);

        // Act
        var result = invocator.WrapsHandler(handler2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WrapsHandler_WithAsyncFunc_ShouldReturnTrue_WhenSameHandler()
    {
        // Arrange
        Func<TestEventArgs, Task> handler = args => Task.CompletedTask;
        var invocator = new AsyncEventInvocator<TestEventArgs>(null, handler);

        // Act
        var result = invocator.WrapsHandler(handler);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WrapsHandler_WithAsyncFunc_ShouldReturnFalse_WhenDifferentHandler()
    {
        // Arrange
        Func<TestEventArgs, Task> handler1 = args => Task.CompletedTask;
        Func<TestEventArgs, Task> handler2 = args => Task.CompletedTask;
        var invocator = new AsyncEventInvocator<TestEventArgs>(null, handler1);

        // Act
        var result = invocator.WrapsHandler(handler2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InvokeAsync_WithAction_ShouldInvokeHandler()
    {
        // Arrange
        var wasCalled = false;
        Action<TestEventArgs> handler = args => { wasCalled = true; };
        var invocator = new AsyncEventInvocator<TestEventArgs>(handler, null);
        var eventArgs = new TestEventArgs();

        // Act
        await invocator.InvokeAsync(eventArgs);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithAsyncFunc_ShouldInvokeHandler()
    {
        // Arrange
        var wasCalled = false;
        Func<TestEventArgs, Task> handler = args => { wasCalled = true; return Task.CompletedTask; };
        var invocator = new AsyncEventInvocator<TestEventArgs>(null, handler);
        var eventArgs = new TestEventArgs();

        // Act
        await invocator.InvokeAsync(eventArgs);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithNoHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invocator = new AsyncEventInvocator<TestEventArgs>(null, null);
        var eventArgs = new TestEventArgs();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await invocator.InvokeAsync(eventArgs));
    }
}
