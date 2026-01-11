// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;

namespace Endpoint.UnitTests.Services;

public class FileProviderTests
{
    [Fact]
    public void Get_ShouldReturnFileNotFound_WhenDepthEqualsPartsLength()
    {
        // Arrange
        var fileProvider = new FileProvider();
        var directory = "path";

        // Act
        var result = fileProvider.Get("*.txt", directory, 1);

        // Assert
        Assert.Equal(Endpoint.Constants.FileNotFound, result);
    }

    [Fact]
    public void FileProvider_ShouldCreateInstance()
    {
        // Act
        var fileProvider = new FileProvider();

        // Assert
        Assert.NotNull(fileProvider);
    }
}
