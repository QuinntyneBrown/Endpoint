// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Internal;

namespace Endpoint.UnitTests.Internal;

public class CompletedTaskTests
{
    [Fact]
    public void Instance_ShouldNotBeNull()
    {
        // Arrange & Act
        var instance = CompletedTask.Instance;

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void Instance_ShouldBeCompleted()
    {
        // Arrange & Act
        var instance = CompletedTask.Instance;

        // Assert
        Assert.True(instance.IsCompleted);
    }

    [Fact]
    public void Instance_ShouldBeCompletedSuccessfully()
    {
        // Arrange & Act
        var instance = CompletedTask.Instance;

        // Assert
        Assert.True(instance.IsCompletedSuccessfully);
    }

    [Fact]
    public void Instance_ShouldNotBeFaulted()
    {
        // Arrange & Act
        var instance = CompletedTask.Instance;

        // Assert
        Assert.False(instance.IsFaulted);
    }

    [Fact]
    public void Instance_ShouldNotBeCanceled()
    {
        // Arrange & Act
        var instance = CompletedTask.Instance;

        // Assert
        Assert.False(instance.IsCanceled);
    }

    [Fact]
    public void Instance_ShouldBeAssignableToTask()
    {
        // Arrange & Act
        var instance = CompletedTask.Instance;

        // Assert
        Assert.IsAssignableFrom<Task>(instance);
    }
}
