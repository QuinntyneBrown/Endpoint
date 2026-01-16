// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Artifacts;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Angular.UnitTests.Artifacts;

public class WorkspaceGenerationStrategyTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var mockCommandService = new Mock<ICommandService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WorkspaceGenerationStrategy(null!, mockCommandService.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCommandServiceIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<WorkspaceGenerationStrategy>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WorkspaceGenerationStrategy(mockLogger.Object, null!));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<WorkspaceGenerationStrategy>>();
        var mockCommandService = new Mock<ICommandService>();

        // Act
        var strategy = new WorkspaceGenerationStrategy(mockLogger.Object, mockCommandService.Object);

        // Assert
        Assert.NotNull(strategy);
    }

    [Fact]
    public void WorkspaceGenerationStrategy_ShouldImplementIArtifactGenerationStrategy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<WorkspaceGenerationStrategy>>();
        var mockCommandService = new Mock<ICommandService>();

        // Act
        var strategy = new WorkspaceGenerationStrategy(mockLogger.Object, mockCommandService.Object);

        // Assert
        Assert.IsAssignableFrom<IArtifactGenerationStrategy<WorkspaceModel>>(strategy);
    }
}
