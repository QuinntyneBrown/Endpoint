# Command Artifact Generation Template

This template provides guidelines for implementing CLI commands that generate a series of artifacts based on a specification and create a demo playground project.

## Overview

When implementing a new command that generates artifacts (like `messaging-add`, `signalr-add`, etc.), follow this structured approach to ensure consistency, testability, and documentation.

## Implementation Steps

### Step 1: Design the Models

Create models in `src/Endpoint.Engineering/{FeatureName}/Models/`:

1. **Main Model** - Represents the overall configuration (e.g., `MessagingModel`)
   - Include all configuration options
   - Use computed properties for derived values (e.g., `ProjectName`, `Namespace`)
   - Default values should be sensible

2. **Component Models** - Represent individual generated artifacts
   - One model per major component (e.g., `MessageHeaderModel`, `MessageEnvelopeModel`)
   - Include all properties needed for code generation

```csharp
// Example: src/Endpoint.Engineering/YourFeature/Models/YourFeatureModel.cs
namespace Endpoint.Engineering.YourFeature.Models;

public class YourFeatureModel
{
    public string SolutionName { get; set; } = string.Empty;
    public string ProjectName => $"{SolutionName}.YourFeature";
    public string Namespace => ProjectName;
    public string Directory { get; set; } = string.Empty;
    // Add feature-specific options
}
```

### Step 2: Create the Artifact Factory

Create artifacts in `src/Endpoint.Engineering/{FeatureName}/Artifacts/`:

1. **IArtifactFactory** - Interface defining factory methods
2. **ArtifactFactory** - Implementation that creates project models with files

```csharp
// Example: src/Endpoint.Engineering/YourFeature/Artifacts/IArtifactFactory.cs
public interface IArtifactFactory
{
    Task<YourProjectModel> CreateProjectAsync(YourFeatureModel model, CancellationToken cancellationToken = default);
}
```

The factory should:
- Create the project model
- Add all required NuGet packages
- Create file models for each generated class/interface
- Use string interpolation or templates for code generation

### Step 3: Create Generation Strategies

Create strategies in `src/Endpoint.Engineering/{FeatureName}/Artifacts/Strategies/`:

```csharp
// Example: YourProjectArtifactGenerationStrategy.cs
public class YourProjectArtifactGenerationStrategy : IArtifactGenerationStrategy<YourProjectModel>
{
    // Inject IProjectService and IArtifactGenerator

    public async Task GenerateAsync(YourProjectModel model, CancellationToken cancellationToken = default)
    {
        await _projectService.AddProjectAsync(model);
        foreach (var file in model.Files)
        {
            await _artifactGenerator.GenerateAsync(file, cancellationToken);
        }
        _projectService.AddToSolution(model);
    }
}
```

### Step 4: Register Services

Create `ConfigureServices.cs` in `src/Endpoint.Engineering/{FeatureName}/`:

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class YourFeatureConfigureServices
{
    public static IServiceCollection AddYourFeatureServices(this IServiceCollection services)
    {
        services.AddSingleton<IArtifactFactory, ArtifactFactory>();
        services.AddArifactGenerator(typeof(YourProjectModel).Assembly);
        return services;
    }
}
```

Add to `Program.cs`:
```csharp
services.AddYourFeatureServices();
```

### Step 5: Implement the CLI Command

Update the command in `src/Endpoint.Engineering.Cli/Commands/`:

```csharp
[Verb("your-feature-add")]
public class YourFeatureAddRequest : IRequest
{
    [Option('n', "name", HelpText = "Solution name")]
    public string? Name { get; set; }

    [Option('d', "directory", Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    // Add feature-specific options with defaults
}

public class YourFeatureAddRequestHandler : IRequestHandler<YourFeatureAddRequest>
{
    // Inject required services

    public async Task Handle(YourFeatureAddRequest request, CancellationToken cancellationToken)
    {
        // 1. Find the solution file using IFileProvider
        // 2. Determine solution name and directory
        // 3. Create the feature model
        // 4. Use artifact factory to create project model
        // 5. Use artifact generator to generate files
    }
}
```

### Step 6: Create Playground Demo

Create a playground project in `playground/{FeatureName}Demo/`:

**Project file (`{FeatureName}Demo.csproj`):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Endpoint\Endpoint.csproj" />
    <ProjectReference Include="..\..\src\Endpoint.DotNet\Endpoint.DotNet.csproj" />
    <ProjectReference Include="..\..\src\Endpoint.Engineering\Endpoint.Engineering.csproj" />
    <ProjectReference Include="..\..\src\Endpoint.Engineering.Cli\Endpoint.Engineering.Cli.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.4.0" />
    <!-- Other required packages -->
  </ItemGroup>
</Project>
```

**Program.cs:**
```csharp
// 1. Setup DI with all required services
// 2. Create a demo solution
// 3. Use MediatR to send the request
// 4. Log results and generated files
```

### Step 7: Create Unit Tests

Create tests following this structure:

```
tests/Endpoint.Engineering.UnitTests/{FeatureName}/
├── Models/
│   ├── YourFeatureModelTests.cs
│   └── ComponentModelTests.cs
├── Artifacts/
│   ├── YourProjectModelTests.cs
│   └── ArtifactFactoryTests.cs

tests/Endpoint.Engineering.Cli.UnitTests/Commands/
└── YourFeatureAddRequestHandlerTests.cs
```

**Test coverage requirements:**
- All model properties and defaults
- Factory methods and file generation
- Command handler logic
- Error handling
- Null parameter validation

**Minimum coverage target: 80%**

## Checklist

- [ ] **Models** created in `src/Endpoint.Engineering/{FeatureName}/Models/`
- [ ] **Artifact Factory** created with interface and implementation
- [ ] **Generation Strategies** created for artifact generation
- [ ] **ConfigureServices** extension method added
- [ ] **CLI Command** implemented with proper options
- [ ] **Playground Demo** project created
- [ ] **Unit Tests** with 80%+ coverage
- [ ] **Documentation** updated if needed

## File Structure

```
src/
├── Endpoint.Engineering/
│   └── {FeatureName}/
│       ├── Models/
│       │   ├── YourFeatureModel.cs
│       │   └── ComponentModels.cs
│       ├── Artifacts/
│       │   ├── IArtifactFactory.cs
│       │   ├── ArtifactFactory.cs
│       │   ├── YourProjectModel.cs
│       │   └── Strategies/
│       │       └── YourProjectArtifactGenerationStrategy.cs
│       └── ConfigureServices.cs
├── Endpoint.Engineering.Cli/
│   └── Commands/
│       └── YourFeatureAdd.cs

playground/
└── {FeatureName}Demo/
    ├── {FeatureName}Demo.csproj
    └── Program.cs

tests/
├── Endpoint.Engineering.UnitTests/
│   └── {FeatureName}/
│       ├── Models/
│       └── Artifacts/
└── Endpoint.Engineering.Cli.UnitTests/
    └── Commands/
        └── YourFeatureAddRequestHandlerTests.cs
```

## Example Implementation Reference

See the `messaging-add` command implementation:
- Models: `src/Endpoint.Engineering/RedisPubSub/Models/`
- Artifacts: `src/Endpoint.Engineering/RedisPubSub/Artifacts/`
- Command: `src/Endpoint.Engineering.Cli/Commands/MessagingAdd.cs`
- Playground: `playground/MessagingDemo/`

## Tips

1. **Use existing patterns** - Look at `ModernWebAppPattern` and `DomainDrivenDesign` for reference
2. **Code generation** - Use raw strings with interpolation for simple files, syntax models for complex C# code
3. **Package versions** - Use stable versions and document any alpha/beta packages
4. **Logging** - Add informative logging at each major step
5. **Validation** - Validate inputs early and provide clear error messages
6. **Testability** - Use interfaces and dependency injection for all services
