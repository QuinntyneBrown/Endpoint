# Endpoint.React.UnitTests

Unit tests for the React code generation module.

## Overview

This project contains unit tests for React-specific code generation, including:

- Component generation
- Hook generation
- TypeScript type generation
- JSX/TSX output

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
| Components | React component generation tests |
| Hooks | Custom hook generation tests |
| Types | TypeScript type generation tests |

## Project Reference

Tests the **Endpoint.React** project (`src/Endpoint.React/`).

## License

This project is licensed under the MIT License.
