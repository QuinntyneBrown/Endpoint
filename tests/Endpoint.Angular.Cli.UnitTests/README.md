# Endpoint.Angular.Cli.UnitTests

Unit tests for the Angular CLI code generation tool.

## Overview

This project contains unit tests for the Angular-specific CLI tool, including:

- Angular CLI command execution
- Component generation commands
- Service generation commands
- Workspace configuration

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

# Run command tests
dotnet test --filter "Category=Commands"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Commands | CLI command execution tests |
| Generation | Code generation output tests |
| Configuration | CLI configuration tests |

## Project Reference

Tests the **Endpoint.Angular.Cli** project (`src/Endpoint.Angular.Cli/`).

## License

This project is licensed under the MIT License.
