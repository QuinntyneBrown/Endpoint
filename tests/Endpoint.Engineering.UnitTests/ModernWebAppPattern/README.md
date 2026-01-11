# Endpoint.ModernWebAppPattern.UnitTests

Unit tests for the Modern Web Application Pattern code generation module.

## Overview

This project contains unit tests for modern web app pattern generation, including:

- Clean architecture layer generation
- CQRS pattern generation
- Solution structure scaffolding
- Integration with DDD patterns

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

# Run architecture tests
dotnet test --filter "Category=Architecture"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Architecture | Clean architecture generation tests |
| CQRS | Command/query generation tests |
| DataContext | Data context provider tests |
| Integration | DDD integration tests |

## Project Reference

Tests the **Endpoint.ModernWebAppPattern** project (`src/Endpoint.ModernWebAppPattern/`).

## License

This project is licensed under the MIT License.
