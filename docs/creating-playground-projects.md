# Creating Playground Projects

This guide explains how to create and use playground projects in the Endpoint solution. Playground projects are standalone console applications that demonstrate specific features of the Endpoint code generation framework.

## Overview

Playground projects are located in the `playground/` directory and serve as:
- **Testing environments** for new features during development
- **Examples** demonstrating how to use Endpoint's code generation capabilities
- **Quick prototypes** for experimenting with different generation patterns

## Existing Playground Projects

| Project | Description |
|---------|-------------|
| `DddSolution` | Generates a Domain-Driven Design solution using MediatR |
| `FullStackSolution` | Creates a complete n-tier solution with Core, Infrastructure, and API projects |
| `SolutionFromPlantuml` | Parses PlantUML architecture diagrams and generates a solution |

## Creating a New Playground Project

### Step 1: Create the Project Structure

Create a new folder in the `playground/` directory:

```
playground/
├── DddSolution/
├── FullStackSolution/
├── SolutionFromPlantuml/
└── YourNewProject/          <-- Create this
    ├── YourNewProject.csproj
    └── Program.cs
```

### Step 2: Create the Project File

Create a `.csproj` file with the necessary references:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <NoWarn>1998,4014,SA1101,SA1600,SA1200,SA1633,1591,SA1309</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Endpoint references -->
    <ProjectReference Include="..\..\src\Endpoint\Endpoint.csproj" />
    <ProjectReference Include="..\..\src\Endpoint.DotNet\Endpoint.DotNet.csproj" />

    <!-- Add Endpoint.Cli if using command handlers via MediatR -->
    <ProjectReference Include="..\..\src\Endpoint.Cli\Endpoint.Cli.csproj" />

    <!-- Add other Endpoint projects as needed -->
    <!-- <ProjectReference Include="..\..\src\Endpoint.ModernWebAppPattern\Endpoint.ModernWebAppPattern.csproj" /> -->
  </ItemGroup>

  <ItemGroup>
    <!-- Required packages -->
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />

    <!-- Add MediatR if using command handlers -->
    <PackageReference Include="MediatR" Version="12.4.0" />

    <!-- Add System.CommandLine if you want CLI argument parsing -->
    <!-- <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" /> -->
  </ItemGroup>
</Project>
```

### Step 3: Implement Program.cs

There are two common patterns for playground projects:

#### Pattern A: Using MediatR (like DddSolution)

Use this pattern when you want to leverage existing CLI command handlers:

```csharp
// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Cli.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add MediatR - scan the assembly containing your request type
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<YourRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("Starting generation...");

    var request = new YourRequest
    {
        // Set properties
    };

    await mediator.Send(request);

    logger.LogInformation("Generation completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during generation");
}
```

#### Pattern B: Direct Service Usage (like FullStackSolution)

Use this pattern when you need more control over the generation process:

```csharp
// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

services.AddCoreServices(typeof(IArtifactGenerator).Assembly);
services.AddDotNetServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var solutionFactory = serviceProvider.GetRequiredService<ISolutionFactory>();
var projectFactory = serviceProvider.GetRequiredService<IProjectFactory>();
var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();

try
{
    logger.LogInformation("Creating solution...");

    // Create solution model
    var solution = new SolutionModel("MySolution", outputDirectory);

    // Add projects
    var coreProject = await projectFactory.CreateCore("MySolution", solution.SrcDirectory);
    solution.Projects.Add(coreProject);

    // Generate artifacts
    await artifactGenerator.GenerateAsync(solution);

    logger.LogInformation("Solution created successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error creating solution");
}
```

## Service Registration

The following extension methods register services:

| Method | Assembly | Services |
|--------|----------|----------|
| `AddCoreServices()` | Endpoint | Core artifact generation, syntax generation |
| `AddDotNetServices()` | Endpoint.DotNet | .NET-specific factories, parsers, PlantUML services |
| `AddModernWebAppPatternCoreServices()` | Endpoint.ModernWebAppPattern | Modern web app patterns |

## Running a Playground Project

### From Visual Studio
1. Right-click the playground project
2. Select "Set as Startup Project"
3. Press F5 or click Run

### From Command Line
```bash
cd playground/YourNewProject
dotnet run
```

### With Arguments (if using System.CommandLine)
```bash
dotnet run -- --name MySolution --directory C:\output
```

## Best Practices

1. **Use constants for configuration** - Define paths and names at the top of Program.cs for easy modification

2. **Clean output directories** - Delete existing output before generating to ensure clean state

3. **Provide helpful logging** - Log progress, configuration, and results for debugging

4. **Validate inputs** - Check that source directories exist before processing

5. **Display results** - Show the generated structure and next steps after completion

## Example: SolutionFromPlantuml

The `SolutionFromPlantuml` playground demonstrates parsing PlantUML files and generating a complete solution:

```csharp
// Configuration
const string SolutionName = "ToDo";
const string PlantUmlSourcePath = @"C:\demo-plantuml-files";
const string OutputDirectory = @"C:\demo-out";

// Create request
var request = new SolutionCreateFromPlantUmlRequest
{
    Name = SolutionName,
    PlantUmlSourcePath = PlantUmlSourcePath,
    Directory = OutputDirectory
};

// Execute via MediatR
await mediator.Send(request);
```

This generates a solution with:
- Core project containing entities, DTOs, and CQRS handlers
- Infrastructure project with DbContext
- API project with controllers

## Troubleshooting

### Build Errors
- Ensure all project references point to valid paths
- Check that NuGet packages are restored: `dotnet restore`

### Runtime Errors
- Verify source directories exist
- Check output directory permissions
- Enable detailed logging: `builder.SetMinimumLevel(LogLevel.Debug)`

### Missing Services
- Ensure all required `Add*Services()` methods are called
- Check that MediatR is scanning the correct assemblies
