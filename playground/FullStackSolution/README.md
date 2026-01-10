# FullStack Solution Generator

A command-line tool that uses Endpoint.Cli to generate a complete .NET solution with a clean architecture structure.

## Features

This tool creates a solution with the following structure:

```
{SolutionName}/
├── src/
│   ├── {SolutionName}.Core/              # Core domain logic (classlib)
│   ├── {SolutionName}.Infrastructure/    # Infrastructure layer (classlib, references Core)
│   └── {SolutionName}.Api/               # API layer (web project, references Infrastructure)
└── tests/
    ├── {SolutionName}.Core.Tests/        # Core project tests (xunit)
    ├── {SolutionName}.Infrastructure.Tests/  # Infrastructure project tests (xunit)
    └── {SolutionName}.Api.Tests/         # API project tests (xunit)
```

## Project References

- **Infrastructure** → references **Core**
- **Api** → references **Infrastructure**
- Each test project references its corresponding implementation project

## Usage

### Build the Tool

```bash
cd playground/FullStackSolution
dotnet build
```

### Run the Tool

```bash
dotnet run -- --name MyAwesomeApp --directory C:\projects
```

Or after publishing:

```bash
dotnet publish -c Release
.\bin\Release\net10.0\FullStackSolution.exe --name MyAwesomeApp --directory C:\projects
```

## Command-Line Options

- `--name` (required): The name of the solution to create
- `--directory` (optional): The directory where the solution will be created (defaults to current directory)

## Examples

### Create a solution in the current directory

```bash
dotnet run -- --name MyShop
```

This creates `MyShop/` in the current directory with the full structure.

### Create a solution in a specific directory

```bash
dotnet run -- --name MyShop --directory C:\projects
```

This creates `C:\projects\MyShop\` with the full structure.

## What Gets Generated

1. **Solution File** (.sln) with all projects configured
2. **Core Project** - Class library for domain models and business logic
3. **Infrastructure Project** - Class library for data access, external services, etc.
4. **API Project** - ASP.NET Core Web API project
5. **Test Projects** - Three xUnit test projects, one for each implementation project

All projects are configured with proper references and .NET 10 target framework.

## Requirements

- .NET 10 SDK
- Endpoint.Core and Endpoint.DotNet libraries
