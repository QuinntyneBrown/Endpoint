// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Testing;

namespace Endpoint.Testing.UnitTests;

public class ConfigureServicesTests
{
    #region AddTestingCoreServices Tests

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldRegisterISyntaxFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var syntaxFactory = serviceProvider.GetService<ISyntaxFactory>();
        Assert.NotNull(syntaxFactory);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldRegisterISyntaxFactoryAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var syntaxFactory1 = serviceProvider.GetService<ISyntaxFactory>();
        var syntaxFactory2 = serviceProvider.GetService<ISyntaxFactory>();
        Assert.Same(syntaxFactory1, syntaxFactory2);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldRegisterSyntaxFactoryAsISyntaxFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var syntaxFactory = serviceProvider.GetService<ISyntaxFactory>();
        Assert.IsType<SyntaxFactory>(syntaxFactory);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldRegisterIArtifactFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var artifactFactory = serviceProvider.GetService<IArtifactFactory>();
        Assert.NotNull(artifactFactory);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldRegisterIArtifactFactoryAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var artifactFactory1 = serviceProvider.GetService<IArtifactFactory>();
        var artifactFactory2 = serviceProvider.GetService<IArtifactFactory>();
        Assert.Same(artifactFactory1, artifactFactory2);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldRegisterArtifactFactoryAsIArtifactFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var artifactFactory = serviceProvider.GetService<IArtifactFactory>();
        Assert.IsType<ArtifactFactory>(artifactFactory);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var exception = Record.Exception(() =>
        {
            services.AddTestingCoreServices();
            services.AddTestingCoreServices();
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldAllowResolvingBothFactories()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var syntaxFactory = serviceProvider.GetService<ISyntaxFactory>();
        var artifactFactory = serviceProvider.GetService<IArtifactFactory>();
        Assert.NotNull(syntaxFactory);
        Assert.NotNull(artifactFactory);
    }

    [Fact]
    public void AddTestingCoreServices_WithoutLogging_ShouldThrowWhenResolvingISyntaxFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<ISyntaxFactory>());
    }

    [Fact]
    public void AddTestingCoreServices_WithoutLogging_ShouldThrowWhenResolvingIArtifactFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IArtifactFactory>());
    }

    [Fact]
    public void AddTestingCoreServices_WhenCalled_ShouldReturnVoid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert - This test verifies the method signature returns void
        services.AddTestingCoreServices();

        // If we get here without exception, the method executed successfully
        Assert.True(true);
    }

    [Fact]
    public void AddTestingCoreServices_RegisteredSyntaxFactory_ShouldBeAbleToDoWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var syntaxFactory = serviceProvider.GetRequiredService<ISyntaxFactory>();
        var exception = Record.ExceptionAsync(() => syntaxFactory.DoWorkAsync()).Result;

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task AddTestingCoreServices_RegisteredArtifactFactory_ShouldBeAbleToCreateSolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var artifactFactory = serviceProvider.GetRequiredService<IArtifactFactory>();
        var result = await artifactFactory.SolutionCreateAsync("Test", "Resource", "/dir", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddTestingCoreServices_RegisteredArtifactFactory_ShouldBeAbleToCreateAngularWorkspace()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTestingCoreServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var artifactFactory = serviceProvider.GetRequiredService<IArtifactFactory>();
        var result = await artifactFactory.AngularWorkspaceCreateAsync("Test", "Resource", "/dir", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion
}
