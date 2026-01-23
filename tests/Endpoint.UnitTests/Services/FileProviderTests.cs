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

    [Fact]
    public void Get_ShouldReturnSlnFile_WhenSearchingForSln()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/solution/MySolution.sln", new MockFileData("") }
        });
        var fileProvider = new FileProvider(mockFileSystem);

        // Act
        var result = fileProvider.Get("*.sln", "/solution");

        // Assert
        Assert.Equal("/solution/MySolution.sln", result);
    }

    [Fact]
    public void Get_ShouldReturnSlnxFile_WhenSearchingForSlnAndOnlySlnxExists()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/solution/MySolution.slnx", new MockFileData("") }
        });
        var fileProvider = new FileProvider(mockFileSystem);

        // Act
        var result = fileProvider.Get("*.sln", "/solution");

        // Assert
        Assert.Equal("/solution/MySolution.slnx", result);
    }

    [Fact]
    public void Get_ShouldPreferSlnOverSlnx_WhenBothExist()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/solution/MySolution.sln", new MockFileData("") },
            { "/solution/MySolution.slnx", new MockFileData("") }
        });
        var fileProvider = new FileProvider(mockFileSystem);

        // Act
        var result = fileProvider.Get("*.sln", "/solution");

        // Assert
        Assert.Equal("/solution/MySolution.sln", result);
    }
}
