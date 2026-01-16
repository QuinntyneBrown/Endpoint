// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Syntax;
using Moq;

namespace Endpoint.UnitTests.Syntax;

public class SyntaxGeneratorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServiceProviderIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SyntaxGenerator(null!));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidServiceProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act
        var syntaxGenerator = new SyntaxGenerator(mockServiceProvider.Object);

        // Assert
        Assert.NotNull(syntaxGenerator);
    }

    [Fact]
    public void SyntaxGenerator_ShouldImplementISyntaxGenerator()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act
        var syntaxGenerator = new SyntaxGenerator(mockServiceProvider.Object);

        // Assert
        Assert.IsAssignableFrom<ISyntaxGenerator>(syntaxGenerator);
    }
}
