// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Artifacts;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Angular.UnitTests.Artifacts;

public class ProjectGenerationStrategyTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCommandServiceIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ProjectGenerationStrategy>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProjectGenerationStrategy(null!, mockLogger.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var mockCommandService = new Mock<ICommandService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProjectGenerationStrategy(mockCommandService.Object, null!));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidParameters()
    {
        // Arrange
        var mockCommandService = new Mock<ICommandService>();
        var mockLogger = new Mock<ILogger<ProjectGenerationStrategy>>();

        // Act
        var strategy = new ProjectGenerationStrategy(mockCommandService.Object, mockLogger.Object);

        // Assert
        Assert.NotNull(strategy);
    }

    [Fact]
    public void ProjectGenerationStrategy_ShouldImplementIArtifactGenerationStrategy()
    {
        // Arrange
        var mockCommandService = new Mock<ICommandService>();
        var mockLogger = new Mock<ILogger<ProjectGenerationStrategy>>();

        // Act
        var strategy = new ProjectGenerationStrategy(mockCommandService.Object, mockLogger.Object);

        // Assert
        Assert.IsAssignableFrom<IArtifactGenerationStrategy<ProjectModel>>(strategy);
    }

    [Fact]
    public void CanHandle_WithProjectModel_ShouldReturnTrue()
    {
        // Arrange
        var mockCommandService = new Mock<ICommandService>();
        var mockLogger = new Mock<ILogger<ProjectGenerationStrategy>>();
        var strategy = new ProjectGenerationStrategy(mockCommandService.Object, mockLogger.Object);
        var projectModel = new ProjectModel("Test", null, "test", "/dir");

        // Act
        var result = strategy.CanHandle(projectModel);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Priority_ShouldReturnOne()
    {
        // Arrange
        var mockCommandService = new Mock<ICommandService>();
        var mockLogger = new Mock<ILogger<ProjectGenerationStrategy>>();
        var strategy = new ProjectGenerationStrategy(mockCommandService.Object, mockLogger.Object);

        // Act
        var priority = strategy.Priority;

        // Assert
        Assert.Equal(1, priority);
    }
}
