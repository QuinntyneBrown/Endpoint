# Endpoint.Engineering.ModernWebAppPattern

The Modern Web Application Pattern module for the Endpoint framework. This library combines Domain-Driven Design principles with modern .NET patterns to generate complete, production-ready web applications.

## Overview

Endpoint.Engineering.ModernWebAppPattern provides templates and generators for creating modern web applications following clean architecture principles. It integrates DDD patterns with ASP.NET Core best practices.

## Project Structure

```
Endpoint.Engineering/ModernWebAppPattern/
├── Artifacts/                              # Generation artifacts
├── Extensions/                             # Extension methods
├── Models/                                 # Model definitions
├── Syntax/                                 # Syntax generators
├── ConfigureServices.cs                    # DI registration
├── DataContext.cs                          # Application data context
├── IDataContext.cs                         # Data context interface
├── IDataContextProvider.cs                 # Context provider interface
├── FileSystemDataContextProvider.cs        # File-based provider
└── FileSystemDataContextProviderOptions.cs # Provider options
```

## Key Features

### Clean Architecture Layers

Generated solutions follow clean architecture:

```
Solution/
├── src/
│   ├── Core/           # Domain layer (entities, value objects)
│   ├── Application/    # Application layer (use cases, CQRS)
│   ├── Infrastructure/ # Infrastructure (EF Core, external services)
│   └── Api/            # Presentation (Minimal APIs/Controllers)
└── tests/
    ├── Core.Tests/
    ├── Application.Tests/
    └── Api.Tests/
```

### Service Registration

```csharp
using Endpoint.Engineering.ModernWebAppPattern;

services.AddModernWebAppPatternCoreServices();
```

### Data Context Provider

Configure the data context for generation:

```csharp
var options = new FileSystemDataContextProviderOptions
{
    ConfigurationPath = "./domain-config.json"
};

var provider = new FileSystemDataContextProvider(options, logger);
var context = await provider.GetAsync();
```

## Usage

Generate modern web applications through the CLI:

```bash
# Create a new solution with modern web app pattern
endpoint solution create --name MyApp --pattern modern-web-app

# Generate a complete feature
endpoint feature create --name Customer --pattern modern-web-app

# Generate with full CQRS
endpoint generate cqrs --entity Customer
```

## Generated Patterns

| Pattern | Description |
|---------|-------------|
| CQRS | Command Query Responsibility Segregation |
| MediatR | Request/notification handling |
| Repository | Data access abstraction |
| Unit of Work | Transaction management |
| Result Pattern | Explicit operation results |
| Validation | FluentValidation integration |

## Dependencies

- **Endpoint.Engineering.DomainDrivenDesign** - DDD patterns
- **Endpoint.DotNet** - .NET code generation

## Architecture Principles

- **Dependency Inversion**: All dependencies point inward
- **Single Responsibility**: Each class has one reason to change
- **Interface Segregation**: Clients depend only on what they use
- **Domain-Centric**: Business logic in the domain layer

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
