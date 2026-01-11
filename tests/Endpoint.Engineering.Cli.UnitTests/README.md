# Endpoint.Cli.UnitTests

Unit tests for the Endpoint command-line interface application.

## Overview

This project contains unit tests for the CLI functionality, including:

- Command parsing and execution
- Option validation
- Command routing
- Error handling

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
dotnet test --filter "ClassName=CommandTests"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Commands | CLI command execution tests |
| Options | Command-line option parsing tests |
| Integration | End-to-end CLI workflow tests |

## Project Reference

Tests the **Endpoint.Cli** project (`src/Endpoint.Cli/`).

## License

This project is licensed under the MIT License.
