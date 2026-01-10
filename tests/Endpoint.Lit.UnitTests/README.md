# Endpoint.Lit.UnitTests

Unit tests for the Lit web components code generation module.

## Overview

This project contains unit tests for Lit-specific code generation, including:

- Web component generation
- TypeScript decorator generation
- Template literal generation
- Shadow DOM styling

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
| Components | Lit component generation tests |
| Decorators | TypeScript decorator tests |
| Templates | lit-html template tests |

## Project Reference

Tests the **Endpoint.Lit** project (`src/Endpoint.Lit/`).

## License

This project is licensed under the MIT License.
