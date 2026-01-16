// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;

namespace Endpoint.UnitTests.Services;

public class ClipboardServiceTests
{
    [Fact]
    public void ClipboardService_ShouldCreateInstance()
    {
        // Arrange & Act
        var clipboardService = new ClipboardService();

        // Assert
        Assert.NotNull(clipboardService);
    }

    [Fact]
    public void ClipboardService_ShouldImplementIClipboardService()
    {
        // Arrange & Act
        var clipboardService = new ClipboardService();

        // Assert
        Assert.IsAssignableFrom<IClipboardService>(clipboardService);
    }
}
