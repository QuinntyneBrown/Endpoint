// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.UnitTests.Artifacts.Projects.Factories;

public class ProjectFactoryTests
{
    private class StubContext : IContext
    {
        public string WorkingDirectory { get; set; } = "/tmp";
        public bool Verbose { get; set; }

        private readonly Dictionary<Type, object> _items = new();

        public void Set<T>(T item)
            where T : class
        {
            _items[typeof(T)] = item;
        }

        public T Get<T>()
            where T : class
        {
            if (_items.TryGetValue(typeof(T), out var item))
            {
                return (T)item;
            }

            return null!;
        }
    }

    [Fact]
    public async Task Create_WithAngularType_ReturnsTypeScriptStandaloneProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("angular", "MyAngularApp", "/tmp/src");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyAngularApp", result.Name);
        Assert.Equal(DotNetProjectType.TypeScriptStandalone, result.DotNetProjectType);
        Assert.Equal(".esproj", result.Extension);
    }

    [Fact]
    public async Task Create_WithTsType_ReturnsTypeScriptStandaloneProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("ts", "MyTypeScriptApp", "/tmp/src");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyTypeScriptApp", result.Name);
        Assert.Equal(DotNetProjectType.TypeScriptStandalone, result.DotNetProjectType);
        Assert.Equal(".esproj", result.Extension);
    }

    [Fact]
    public async Task Create_WithWorkerType_ReturnsWorkerProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("worker", "MyWorkerApp", "/tmp/src");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyWorkerApp", result.Name);
        Assert.Equal(DotNetProjectType.Worker, result.DotNetProjectType);
        Assert.Equal(".csproj", result.Extension);
    }

    [Fact]
    public async Task Create_WithWebApiType_ReturnsWebApiProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("webapi", "MyWebApi", "/tmp/src");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyWebApi", result.Name);
        Assert.Equal(DotNetProjectType.WebApi, result.DotNetProjectType);
        Assert.Equal(".csproj", result.Extension);
    }

    [Fact]
    public async Task Create_WithClassLibType_ReturnsClassLibProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("classlib", "MyLibrary", "/tmp/src");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyLibrary", result.Name);
        Assert.Equal(DotNetProjectType.ClassLib, result.DotNetProjectType);
        Assert.Equal(".csproj", result.Extension);
    }

    [Fact]
    public async Task Create_WithXUnitType_ReturnsXUnitProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("xunit", "MyTests", "/tmp/tests");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyTests", result.Name);
        Assert.Equal(DotNetProjectType.XUnit, result.DotNetProjectType);
        Assert.Equal(".csproj", result.Extension);
    }

    [Fact]
    public async Task Create_WithUnknownType_ReturnsConsoleProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IProjectFactory>();

        // Act
        var result = await sut.Create("unknown", "MyApp", "/tmp/src");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyApp", result.Name);
        Assert.Equal(DotNetProjectType.Console, result.DotNetProjectType);
        Assert.Equal(".csproj", result.Extension);
    }
}
