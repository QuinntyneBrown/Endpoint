// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Services;

namespace Endpoint.DotNet.UnitTests.Services;

public class NamespaceProviderTests
{
    [Fact]
    public void Get_ShouldReturnNamespaceNotFound_WhenDirectoryIsNull()
    {
        // Arrange
        var namespaceProvider = new NamespaceProvider();

        // Act
        var result = namespaceProvider.Get(null);

        // Assert
        Assert.Equal("NamespaceNotFound", result);
    }

    [Fact]
    public void Get_ShouldReturnNamespaceNotFound_WhenDirectoryIsEmpty()
    {
        // Arrange
        var namespaceProvider = new NamespaceProvider();

        // Act
        var result = namespaceProvider.Get(string.Empty);

        // Assert
        Assert.Equal("NamespaceNotFound", result);
    }

    [Fact]
    public void Get_ShouldReturnNamespaceNotFound_WhenDepthExceedsPathLength()
    {
        // Arrange
        var namespaceProvider = new NamespaceProvider();

        // Act
        var result = namespaceProvider.Get("path", 10);

        // Assert
        Assert.Equal("NamespaceNotFound", result);
    }

    [Fact]
    public void NamespaceProvider_ShouldCreateInstance()
    {
        // Act
        var namespaceProvider = new NamespaceProvider();

        // Assert
        Assert.NotNull(namespaceProvider);
    }
}
