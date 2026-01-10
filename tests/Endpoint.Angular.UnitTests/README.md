# Endpoint.Angular.UnitTests

Unit tests for the Angular code generation module.

## Overview

This project contains unit tests for Angular-specific code generation, including:

- Component generation
- Service generation
- Module scaffolding
- JSON configuration manipulation

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

# Run component tests
dotnet test --filter "Category=Components"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Components | Angular component generation tests |
| Services | Angular service generation tests |
| Modules | Module scaffolding tests |
| Configuration | JSON manipulation tests |

## Project Reference

Tests the **Endpoint.Angular** project (`src/Endpoint.Angular/`).

## License

This project is licensed under the MIT License.
