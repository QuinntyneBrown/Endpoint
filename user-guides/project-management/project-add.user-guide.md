# project-add

Add a new project to an existing .NET solution.

## Synopsis

```bash
endpoint project-add [options]
```

## Description

The `project-add` command creates a new .NET project and adds it to the solution. It supports various project types and can automatically set up project references and solution folders.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the project | No | - |
| `--type` | `-t` | .NET project type | No | `classlib` |
| `--folder-name` | `-f` | Solution folder for the project | No | - |
| `--references` | `-r` | Project references (comma-separated) | No | - |
| `--metadata` | `-m` | Additional project metadata | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Project Types

| Type | Description |
|------|-------------|
| `classlib` | Class library (default) |
| `console` | Console application |
| `webapi` | ASP.NET Core Web API |
| `worker` | Background worker service |
| `web` | ASP.NET Core web application |
| `xunit` | xUnit test project |
| `nunit` | NUnit test project |
| `mstest` | MSTest test project |
| `blazorwasm` | Blazor WebAssembly |
| `blazorserver` | Blazor Server |

## Examples

### Add a class library

```bash
endpoint project-add -n MyApp.Core
```

### Add a Web API project

```bash
endpoint project-add -n MyApp.Api -t webapi
```

### Add project with references

```bash
endpoint project-add -n MyApp.Services -r "MyApp.Core,MyApp.Data"
```

### Add project to a solution folder

```bash
endpoint project-add -n MyApp.Infrastructure -f "Infrastructure"
```

### Add a test project

```bash
endpoint project-add -n MyApp.Tests -t xunit -r "MyApp.Core"
```

### Discover and add existing projects

When no name is provided, the command scans for existing .csproj files and adds them to the solution:

```bash
endpoint project-add -d ./src
```

## Auto-Discovery Mode

When called without a name, `project-add` recursively searches the directory for .csproj files and adds them to the nearest solution file. It skips common directories like:

- `.git`
- `node_modules`
- `bin` / `obj`
- `.vs` / `.vscode`
- `dist`
- `.angular`

## Generated Structure

For `endpoint project-add -n MyApp.Services`:

```
src/
└── MyApp.Services/
    ├── MyApp.Services.csproj
    └── Class1.cs
```

## Common Patterns

### Multi-Project Setup

```bash
# Core library
endpoint project-add -n MyApp.Core -t classlib

# Data access layer
endpoint project-add -n MyApp.Data -t classlib -r "MyApp.Core"

# Business services
endpoint project-add -n MyApp.Services -t classlib -r "MyApp.Core,MyApp.Data"

# API layer
endpoint project-add -n MyApp.Api -t webapi -r "MyApp.Services"

# Tests
endpoint project-add -n MyApp.Tests -t xunit -r "MyApp.Core,MyApp.Services"
```

### Clean Architecture Setup

```bash
# Domain layer
endpoint project-add -n MyApp.Domain -f "Domain"

# Application layer
endpoint project-add -n MyApp.Application -f "Application" -r "MyApp.Domain"

# Infrastructure
endpoint project-add -n MyApp.Infrastructure -f "Infrastructure" -r "MyApp.Application"

# Presentation
endpoint project-add -n MyApp.Api -f "Presentation" -r "MyApp.Application"
```

## Common Use Cases

1. **Layer Separation**: Add projects for different architectural layers
2. **Test Projects**: Add unit and integration test projects
3. **Shared Libraries**: Add common/shared library projects
4. **Microservices**: Add individual service projects

## Related Commands

- [solution-create](./solution-create.user-guide.md) - Create new solutions
- [reference-add](./reference-add.user-guide.md) - Add references between projects
- [package-add](./package-add.user-guide.md) - Add NuGet packages

[Back to Project Management](./index.md) | [Back to Index](../index.md)
