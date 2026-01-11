// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.UnitTests.Artifacts.Solutions.Factories;

public class SolutionFactoryTests
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
    public async Task Create_WithAngularProjectType_CreatesTypeScriptStandaloneProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyAngularApp", "angular", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Single(result.Projects);
        
        var project = result.Projects[0];
        Assert.Equal("MyAngularApp", project.Name);
        Assert.Equal(DotNetProjectType.TypeScriptStandalone, project.DotNetProjectType);
        Assert.Equal(".esproj", project.Extension);
    }

    [Fact]
    public async Task Create_WithTsProjectType_CreatesTypeScriptStandaloneProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyTypeScriptApp", "ts", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Single(result.Projects);
        
        var project = result.Projects[0];
        Assert.Equal("MyTypeScriptApp", project.Name);
        Assert.Equal(DotNetProjectType.TypeScriptStandalone, project.DotNetProjectType);
        Assert.Equal(".esproj", project.Extension);
    }

    [Fact]
    public async Task Create_WithWorkerProjectType_CreatesWorkerProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyWorkerApp", "worker", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Single(result.Projects);
        
        var project = result.Projects[0];
        Assert.Equal("MyWorkerApp", project.Name);
        Assert.Equal(DotNetProjectType.Worker, project.DotNetProjectType);
        Assert.Equal(".csproj", project.Extension);
    }

    [Fact]
    public async Task Create_WithWebApiProjectType_CreatesWebApiProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyWebApi", "webapi", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Single(result.Projects);
        
        var project = result.Projects[0];
        Assert.Equal("MyWebApi", project.Name);
        Assert.Equal(DotNetProjectType.WebApi, project.DotNetProjectType);
        Assert.Equal(".csproj", project.Extension);
    }

    [Fact]
    public async Task Create_WithClassLibProjectType_CreatesClassLibProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyLibrary", "classlib", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Single(result.Projects);
        
        var project = result.Projects[0];
        Assert.Equal("MyLibrary", project.Name);
        Assert.Equal(DotNetProjectType.ClassLib, project.DotNetProjectType);
        Assert.Equal(".csproj", project.Extension);
    }

    [Fact]
    public async Task Create_SolutionNameOnly_CreatesEmptySolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Empty(result.Projects);
    }
}
