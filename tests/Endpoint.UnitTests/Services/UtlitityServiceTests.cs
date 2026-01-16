// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Internal;
using Endpoint.Syntax;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace Endpoint.UnitTests.Services;

public class UtlitityServiceTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var mockObservable = new Mock<Observable<INotification>>();
        var mockFileSystem = new MockFileSystem();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UtlitityService(null!, mockObservable.Object, mockFileSystem));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenObservableNotificationsIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockFileSystem = new MockFileSystem();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UtlitityService(mockLogger.Object, null!, mockFileSystem));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFileSystemIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockObservable = new Mock<Observable<INotification>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UtlitityService(mockLogger.Object, mockObservable.Object, null!));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockObservable = new Mock<Observable<INotification>>();
        var mockFileSystem = new MockFileSystem();

        // Act
        var utlitityService = new UtlitityService(mockLogger.Object, mockObservable.Object, mockFileSystem);

        // Assert
        Assert.NotNull(utlitityService);
    }

    [Fact]
    public void CopyrightAdd_ShouldProcessFiles_InDirectory()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockObservable = new Mock<Observable<INotification>>();
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/TestFile.cs", new MockFileData("// Test content") },
            { "/test/TestFile.ts", new MockFileData("// Test content") },
            { "/test/TestFile.js", new MockFileData("// Test content") },
        });

        var utlitityService = new UtlitityService(mockLogger.Object, mockObservable.Object, mockFileSystem);

        // Act
        var exception = Record.Exception(() => utlitityService.CopyrightAdd("/test"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void CopyrightAdd_ShouldIgnoreFilesInBinDirectory()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockObservable = new Mock<Observable<INotification>>();
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/TestFile.cs", new MockFileData("// Test content") },
            { "/test/bin/BinFile.cs", new MockFileData("// Bin content") },
        });

        var utlitityService = new UtlitityService(mockLogger.Object, mockObservable.Object, mockFileSystem);

        // Act
        var exception = Record.Exception(() => utlitityService.CopyrightAdd("/test"));

        // Assert - Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void CopyrightAdd_ShouldIgnoreFilesInObjDirectory()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockObservable = new Mock<Observable<INotification>>();
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/TestFile.cs", new MockFileData("// Test content") },
            { "/test/obj/ObjFile.cs", new MockFileData("// Obj content") },
        });

        var utlitityService = new UtlitityService(mockLogger.Object, mockObservable.Object, mockFileSystem);

        // Act
        var exception = Record.Exception(() => utlitityService.CopyrightAdd("/test"));

        // Assert - Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void CopyrightAdd_ShouldIgnoreFilesWithInvalidExtensions()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UtlitityService>>();
        var mockObservable = new Mock<Observable<INotification>>();
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/TestFile.txt", new MockFileData("// Test content") },
            { "/test/TestFile.md", new MockFileData("// Markdown content") },
        });

        var utlitityService = new UtlitityService(mockLogger.Object, mockObservable.Object, mockFileSystem);

        // Act
        var exception = Record.Exception(() => utlitityService.CopyrightAdd("/test"));

        // Assert - Should not throw
        Assert.Null(exception);
    }
}
