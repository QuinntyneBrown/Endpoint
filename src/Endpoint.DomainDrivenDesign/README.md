# Endpoint.DomainDrivenDesign

The Domain-Driven Design (DDD) module for the Endpoint framework. This library provides patterns, templates, and generators for implementing DDD architectures in .NET applications.

## Overview

Endpoint.DomainDrivenDesign provides the building blocks for creating rich domain models following DDD principles. It includes support for:

- **Aggregates**: Aggregate roots and entity clustering
- **Entities**: Domain entities with identity
- **Value Objects**: Immutable value types
- **Domain Events**: Event-driven communication
- **Repositories**: Data access abstractions
- **Domain Services**: Cross-aggregate operations

## Project Structure

```
Endpoint.DomainDrivenDesign/
├── Models/                          # DDD model definitions
├── DataContext.cs                   # Domain data context
├── IDataContext.cs                  # Data context interface
├── IDataContextProvider.cs          # Context provider interface
├── FileSystenDataContextProvider.cs # File-based context provider
└── Endpoint.DomainDrivenDesign.csproj
```

## Key Features

### Data Context

The module provides a data context abstraction for working with domain models:

```csharp
public interface IDataContext
{
    IEnumerable<AggregateRoot> Aggregates { get; }
    IEnumerable<Entity> Entities { get; }
    IEnumerable<ValueObject> ValueObjects { get; }
}
```

### File System Provider

Read domain models from configuration files:

```csharp
var provider = new FileSystemDataContextProvider(options);
var context = await provider.GetAsync();
```

## Usage

Register DDD services in your application:

```csharp
using Endpoint.DomainDrivenDesign;

services.AddDomainDrivenDesignServices();
```

Generate DDD artifacts through the CLI:

```bash
# Generate an aggregate root
endpoint generate aggregate --name Order --domain Sales

# Generate an entity
endpoint generate entity --name OrderItem --aggregate Order

# Generate a value object
endpoint generate valueobject --name Money --properties "Amount:decimal,Currency:string"

# Generate a repository
endpoint generate repository --aggregate Order
```

## Dependencies

- **Endpoint** (Core) - Foundation library
- **Microsoft.Extensions.Logging** - Logging infrastructure

## DDD Patterns Supported

| Pattern | Description |
|---------|-------------|
| Aggregate Root | Entry point for aggregate operations |
| Entity | Objects with identity |
| Value Object | Immutable objects without identity |
| Domain Event | Communicate state changes |
| Repository | Collection-like interface for aggregates |
| Domain Service | Operations spanning multiple aggregates |
| Specification | Business rule encapsulation |

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
