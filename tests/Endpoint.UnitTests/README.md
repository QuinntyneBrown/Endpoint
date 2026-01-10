# Endpoint.UnitTests

Unit tests for the core Endpoint library.

## Overview

This project contains unit tests for the foundation components of the Endpoint code generation framework, including:

- Object caching functionality
- String manipulation utilities
- Service implementations
- Artifact definitions

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

# Run specific test
dotnet test --filter "ClassName=ObjectCacheTests"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Caching | Object and string cache tests |
| Services | Core service tests |
| Utilities | Extension method tests |
| Artifacts | Artifact definition tests |

## Project Reference

Tests the **Endpoint** project (`src/Endpoint/`).

## License

This project is licensed under the MIT License.
