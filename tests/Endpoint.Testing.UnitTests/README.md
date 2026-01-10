# Endpoint.Testing.UnitTests

Unit tests for the Testing utilities and test generation module.

## Overview

This project contains unit tests for the test generation infrastructure, including:

- Test artifact factory
- Syntax factory for test code
- Angular workspace generation strategy
- Test project scaffolding

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

# Run factory tests
dotnet test --filter "Category=Factory"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Factory | Artifact and syntax factory tests |
| Angular | Angular workspace generation tests |
| Scaffolding | Test project scaffolding tests |

## Project Reference

Tests the **Endpoint.Testing** project (`src/Endpoint.Testing/`).

## License

This project is licensed under the MIT License.
