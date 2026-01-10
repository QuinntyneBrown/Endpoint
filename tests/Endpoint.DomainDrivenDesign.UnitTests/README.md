# Endpoint.DomainDrivenDesign.UnitTests

Unit tests for the Domain-Driven Design code generation module.

## Overview

This project contains unit tests for DDD pattern generation, including:

- Aggregate root generation
- Entity generation
- Value object generation
- Repository patterns
- Domain service generation

## Test Framework

- **xUnit** - Primary testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run aggregate tests
dotnet test --filter "Category=Aggregates"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Aggregates | Aggregate root generation tests |
| Entities | Entity generation tests |
| ValueObjects | Value object generation tests |
| Repositories | Repository pattern tests |
| DataContext | Data context provider tests |

## Project Reference

Tests the **Endpoint.DomainDrivenDesign** project (`src/Endpoint.DomainDrivenDesign/`).

## License

This project is licensed under the MIT License.
