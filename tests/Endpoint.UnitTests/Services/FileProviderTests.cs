// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;
using System.IO.Abstractions.TestingHelpers;

namespace Endpoint.UnitTests.Services;

public class FileProviderTests
{
    [Fact]
    public void Get_ShouldReturnFileNotFound_WhenDepthEqualsPartsLength()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileProvider = new FileProvider(mockFileSystem);
        var directory = "path";

        // Act
        var result = fileProvider.Get("*.txt", directory, 1);

        // Assert
        Assert.Equal(Endpoint.Constants.FileNotFound, result);
    }

    [Fact]
    public void FileProvider_ShouldCreateInstance()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();

        // Act
        var fileProvider = new FileProvider(mockFileSystem);

        // Assert
        Assert.NotNull(fileProvider);
    }
}
