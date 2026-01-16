// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Syntax;
using Endpoint.Services;
using Endpoint.Syntax;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Angular.UnitTests.Syntax;

public class FunctionSyntaxGenerationStrategyTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var mockSyntaxGenerator = new Mock<ISyntaxGenerator>();
        var mockNamingConverter = new Mock<INamingConventionConverter>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new FunctionSyntaxGenerationStrategy(mockSyntaxGenerator.Object, mockNamingConverter.Object, null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNamingConventionConverterIsNull()
    {
        // Arrange
        var mockSyntaxGenerator = new Mock<ISyntaxGenerator>();
        var mockLogger = new Mock<ILogger<FunctionSyntaxGenerationStrategy>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new FunctionSyntaxGenerationStrategy(mockSyntaxGenerator.Object, null!, mockLogger.Object));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Arrange
        var mockSyntaxGenerator = new Mock<ISyntaxGenerator>();
        var mockNamingConverter = new Mock<INamingConventionConverter>();
        var mockLogger = new Mock<ILogger<FunctionSyntaxGenerationStrategy>>();

        // Act
        var strategy = new FunctionSyntaxGenerationStrategy(
            mockSyntaxGenerator.Object,
            mockNamingConverter.Object,
            mockLogger.Object);

        // Assert
        Assert.NotNull(strategy);
    }

    [Fact]
    public void FunctionSyntaxGenerationStrategy_ShouldImplementISyntaxGenerationStrategy()
    {
        // Arrange
        var mockSyntaxGenerator = new Mock<ISyntaxGenerator>();
        var mockNamingConverter = new Mock<INamingConventionConverter>();
        var mockLogger = new Mock<ILogger<FunctionSyntaxGenerationStrategy>>();

        // Act
        var strategy = new FunctionSyntaxGenerationStrategy(
            mockSyntaxGenerator.Object,
            mockNamingConverter.Object,
            mockLogger.Object);

        // Assert
        Assert.IsAssignableFrom<ISyntaxGenerationStrategy<FunctionModel>>(strategy);
    }
}
