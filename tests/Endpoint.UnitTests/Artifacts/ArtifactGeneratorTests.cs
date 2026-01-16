// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.UnitTests.Artifacts;

public class ArtifactGeneratorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ArtifactGenerator(null!, mockServiceProvider.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServiceProviderIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ArtifactGenerator>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ArtifactGenerator(mockLogger.Object, null!));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ArtifactGenerator>>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act
        var artifactGenerator = new ArtifactGenerator(mockLogger.Object, mockServiceProvider.Object);

        // Assert
        Assert.NotNull(artifactGenerator);
    }

    [Fact]
    public void ArtifactGenerator_ShouldImplementIArtifactGenerator()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ArtifactGenerator>>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act
        var artifactGenerator = new ArtifactGenerator(mockLogger.Object, mockServiceProvider.Object);

        // Assert
        Assert.IsAssignableFrom<IArtifactGenerator>(artifactGenerator);
    }
}
