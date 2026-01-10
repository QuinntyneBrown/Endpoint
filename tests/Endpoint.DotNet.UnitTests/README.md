# Endpoint.DotNet.UnitTests

Unit tests for the .NET code generation module.

## Overview

This project contains unit tests for .NET-specific code generation, including:

- C# syntax generation
- Template rendering
- Solution/project scaffolding
- Code transformation

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

# Run syntax generation tests
dotnet test --filter "Category=Syntax"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Syntax | C# syntax generator tests |
| Templates | DotLiquid template tests |
| Artifacts | Code artifact generation tests |
| Services | Generation service tests |

## Project Reference

Tests the **Endpoint.DotNet** project (`src/Endpoint.DotNet/`).

## License

This project is licensed under the MIT License.
