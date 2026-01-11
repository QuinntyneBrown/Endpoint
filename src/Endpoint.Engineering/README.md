# Endpoint.Engineering

The Engineering module for the Endpoint code generation framework. This library provides Domain-Driven Design patterns, templates, and the Modern Web App Pattern implementation.

## Overview

Endpoint.Engineering combines DDD principles with modern .NET patterns to generate complete, production-ready applications. It serves as the core engineering library that powers the Endpoint CLI.

## Project Structure

```
Endpoint.Engineering/
├── DomainDrivenDesign/                     # DDD patterns and templates
│   ├── Models/                             # DDD model definitions
│   │   ├── AggregateModel.cs               # Aggregate root model
│   │   ├── EntityModel.cs                  # Entity model
│   │   ├── BoundedContext.cs               # Bounded context model
│   │   ├── Command.cs                      # Command model
│   │   ├── Query.cs                        # Query model
│   │   └── Property.cs                     # Property model
│   ├── DataContext.cs                      # Data context
│   ├── IDataContext.cs                     # Data context interface
│   ├── IDataContextProvider.cs             # Context provider interface
│   └── FileSystenDataContextProvider.cs    # File-based provider
├── ModernWebAppPattern/                    # Modern Web App Pattern
│   ├── Artifacts/                          # Generation artifacts
│   ├── Extensions/                         # Extension methods
│   ├── Models/                             # Model definitions
│   ├── Syntax/                             # Syntax generators
│   ├── ConfigureServices.cs                # DI registration
│   └── DataContext.cs                      # Application data context
└── Endpoint.Engineering.csproj             # Project file
```

## Key Features

### Domain-Driven Design Support

- **Aggregates**: Generate aggregate roots with commands and queries
- **Entities**: Create domain entities with properties and relationships
- **Value Objects**: Define immutable value objects
- **Bounded Contexts**: Organize domain into bounded contexts
- **Commands & Queries**: CQRS pattern support

### Modern Web App Pattern

- **Clean Architecture**: Generate solutions following clean architecture
- **CQRS**: Command Query Responsibility Segregation
- **MediatR Integration**: Request/notification handling
- **Repository Pattern**: Data access abstraction
- **Validation**: FluentValidation integration

## Usage

### DomainDrivenDesign

```csharp
using Endpoint.Engineering.DomainDrivenDesign;
using Endpoint.Engineering.DomainDrivenDesign.Models;

// Create an aggregate with commands and queries
var (aggregate, dataContext) = AggregateModel.Create("Customer", "Sales");
```

### ModernWebAppPattern

```csharp
using Endpoint.Engineering.ModernWebAppPattern;

services.AddModernWebAppPatternCoreServices();
```

## Dependencies

- **Endpoint** - Core generation engine
- **Endpoint.DotNet** - .NET code generation

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
