# Endpoint.Cli

The main command-line interface application for the Endpoint code generation framework. This tool provides developers with a powerful CLI to generate C# code, solutions, and frontend projects.

## Overview

Endpoint.Cli is the primary entry point for all Endpoint code generation operations. It is distributed as a .NET global tool and provides comprehensive commands for:

- Creating new solutions and projects
- Generating domain entities and DDD patterns
- Scaffolding API endpoints and controllers
- Creating frontend components (Angular, React, Lit)
- Automating common development tasks

## Installation

Install as a .NET global tool:

```bash
dotnet tool install -g Quinntyne.Endpoint.Cli
```

Or update to the latest version:

```bash
dotnet tool update -g Quinntyne.Endpoint.Cli
```

## Usage

After installation, the `endpoint` command is available globally:

```bash
# Create a new solution
endpoint solution create --name MyProject --pattern modern-web-app

# Generate a domain entity
endpoint generate entity --name Customer --domain Sales

# Generate CRUD operations
endpoint generate crud --entity Customer --endpoints all

# Generate Angular components
endpoint angular component --name CustomerList
```

## Project Structure

```
Endpoint.Cli/
├── Commands/           # CLI command implementations
├── Properties/         # Assembly properties
├── Program.cs          # Application entry point
├── EndpointOptions.cs  # CLI option definitions
└── appsettings.json    # Application configuration
```

## Dependencies

This CLI integrates multiple Endpoint modules:

- **Endpoint.Angular** - Angular code generation
- **Endpoint.DotNet** - .NET code generation
- **Endpoint.ModernWebAppPattern** - Modern web app templates
- **Endpoint.Testing** - Test generation utilities

## Configuration

The CLI uses Serilog for logging. Log levels can be configured via command-line options:

```bash
endpoint --log-level Debug generate entity --name MyEntity
```

## NuGet Package

- **Package ID**: `Quinntyne.Endpoint.Cli`
- **Tool Command**: `endpoint`
- **Version**: 0.3.1

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
