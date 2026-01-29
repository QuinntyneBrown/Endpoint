// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.Services;
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

    [Fact]
    public async Task Create_WithMicroserviceProjectType_CreatesThreeProjects()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MySolution", result.Name);
        Assert.Equal(3, result.Projects.Count);
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_CreatesApiProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        var apiProject = result.Projects.FirstOrDefault(p => p.Name == "MyService.Api");
        Assert.NotNull(apiProject);
        Assert.Equal("MyService.Api", apiProject.Name);
        Assert.Equal(DotNetProjectType.WebApi, apiProject.DotNetProjectType);
        Assert.Equal(".csproj", apiProject.Extension);
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_CreatesCoreProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        var coreProject = result.Projects.FirstOrDefault(p => p.Name == "MyService.Core");
        Assert.NotNull(coreProject);
        Assert.Equal("MyService.Core", coreProject.Name);
        Assert.Equal(DotNetProjectType.ClassLib, coreProject.DotNetProjectType);
        Assert.Equal(".csproj", coreProject.Extension);
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_CreatesInfrastructureProject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        var infrastructureProject = result.Projects.FirstOrDefault(p => p.Name == "MyService.Infrastructure");
        Assert.NotNull(infrastructureProject);
        Assert.Equal("MyService.Infrastructure", infrastructureProject.Name);
        Assert.Equal(DotNetProjectType.ClassLib, infrastructureProject.DotNetProjectType);
        Assert.Equal(".csproj", infrastructureProject.Extension);
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_SetsUpCorrectDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        Assert.Equal(2, result.DependOns.Count);

        var infrastructureDependency = result.DependOns.FirstOrDefault(d =>
            d.Client.Name == "MyService.Infrastructure" &&
            d.Service.Name == "MyService.Core");
        Assert.NotNull(infrastructureDependency);

        var apiDependency = result.DependOns.FirstOrDefault(d =>
            d.Client.Name == "MyService.Api" &&
            d.Service.Name == "MyService.Infrastructure");
        Assert.NotNull(apiDependency);
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_CaseInsensitive()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "MICROSERVICE", "src", "/tmp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Projects.Count);
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_CoreProjectHasExpectedPackages()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        var coreProject = result.Projects.FirstOrDefault(p => p.Name == "MyService.Core");
        Assert.NotNull(coreProject);
        Assert.Contains(coreProject.Packages, p => p.Name == "MediatR");
        Assert.Contains(coreProject.Packages, p => p.Name == "FluentValidation");
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_InfrastructureProjectHasExpectedPackages()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        var infrastructureProject = result.Projects.FirstOrDefault(p => p.Name == "MyService.Infrastructure");
        Assert.NotNull(infrastructureProject);
        Assert.Contains(infrastructureProject.Packages, p => p.Name == "Microsoft.EntityFrameworkCore.Tools");
        Assert.Contains(infrastructureProject.Packages, p => p.Name == "Microsoft.EntityFrameworkCore.SqlServer");
    }

    [Fact]
    public async Task Create_WithMicroserviceProjectType_ApiProjectHasExpectedPackages()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDotNetServices();
        services.AddSingleton<IContext>(new StubContext());

        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<ISolutionFactory>();

        // Act
        var result = await sut.Create("MySolution", "MyService", "microservice", "src", "/tmp");

        // Assert
        var apiProject = result.Projects.FirstOrDefault(p => p.Name == "MyService.Api");
        Assert.NotNull(apiProject);
        Assert.Contains(apiProject.Packages, p => p.Name == "Swashbuckle.AspNetCore");
        Assert.Contains(apiProject.Packages, p => p.Name == "Serilog");
    }
}
