# Endpoint.MinimalApi.UnitTests

Unit tests for the Minimal API code generation module.

## Overview

This project contains unit tests for ASP.NET Core Minimal API generation, including:

- Endpoint generation
- Route handler generation
- Request/response model generation
- OpenAPI integration

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

# Run endpoint tests
dotnet test --filter "Category=Endpoints"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Endpoints | Minimal API endpoint generation tests |
| Handlers | Route handler generation tests |
| Models | Request/response model tests |

## Project Reference

Tests the **Endpoint.MinimalApi** project (`src/Endpoint.MinimalApi/`).

## License

This project is licensed under the MIT License.
