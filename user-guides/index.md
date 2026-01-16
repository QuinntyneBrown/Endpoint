# Endpoint CLI User Guides

Welcome to the Endpoint CLI documentation. This guide provides comprehensive documentation for all available commands in the Endpoint Engineering CLI tool.

## Quick Start

```bash
# Install the CLI (if not already installed)
dotnet tool install -g Endpoint.Engineering.Cli

# Get help for any command
endpoint <command> --help

# Example: Create a new solution
endpoint solution-create -n MyApp -d ./projects
```

## Command Categories

### [Code Generation](./code-generation/index.md)
Commands for creating C# classes, interfaces, entities, enums, records, and other code artifacts.

| Command | Description |
|---------|-------------|
| [class-create](./code-generation/class-create.user-guide.md) | Create a C# class |
| [interface-create](./code-generation/interface-create.user-guide.md) | Create a C# interface |
| [entity-create](./code-generation/entity-create.user-guide.md) | Create a database entity with DTO |
| [enum-create](./code-generation/enum-create.user-guide.md) | Create an enumeration |
| [record-create](./code-generation/record-create.user-guide.md) | Create a C# record |
| [aggregate-create](./code-generation/aggregate-create.user-guide.md) | Create a DDD aggregate |
| [controller-create](./code-generation/controller-create.user-guide.md) | Create an API controller |
| [service-create](./code-generation/service-create.user-guide.md) | Create a service class |
| [feature](./code-generation/feature.user-guide.md) | Create a complete CRUD feature |

### [Project Management](./project-management/index.md)
Commands for managing .NET solutions, projects, and references.

| Command | Description |
|---------|-------------|
| [solution-create](./project-management/solution-create.user-guide.md) | Create a new .NET solution |
| [project-add](./project-management/project-add.user-guide.md) | Add a project to a solution |
| [reference-add](./project-management/reference-add.user-guide.md) | Add project references |
| [package-add](./project-management/package-add.user-guide.md) | Add NuGet packages |
| [migration-add](./project-management/migration-add.user-guide.md) | Add database migrations |

### [Application Scaffolding](./application-scaffolding/index.md)
Commands for creating complete application templates and microservices.

| Command | Description |
|---------|-------------|
| [mwa-create](./application-scaffolding/mwa-create.user-guide.md) | Create a Modern Web Application |
| [ddd-app-create](./application-scaffolding/ddd-app-create.user-guide.md) | Create a DDD application |
| [react-app-create](./application-scaffolding/react-app-create.user-guide.md) | Create a React application |
| [event-driven-microservices-create](./application-scaffolding/event-driven-microservices-create.user-guide.md) | Create event-driven microservices |
| [worker-create](./application-scaffolding/worker-create.user-guide.md) | Create a worker/console application |

### [Analysis](./analysis/index.md)
Commands for code review and static analysis.

| Command | Description |
|---------|-------------|
| [code-review](./analysis/code-review.user-guide.md) | Perform code review with diff analysis |
| [static-analysis](./analysis/static-analysis.user-guide.md) | Run general static analysis |
| [csharp-static-analysis](./analysis/csharp-static-analysis.user-guide.md) | C# specific static analysis |
| [angular-static-analysis](./analysis/angular-static-analysis.user-guide.md) | Angular workspace analysis |
| [scss-static-analysis](./analysis/scss-static-analysis.user-guide.md) | SCSS static analysis |
| [code-parse](./analysis/code-parse.user-guide.md) | Parse code for LLM consumption |

### [File Operations](./file-operations/index.md)
Commands for file manipulation and transformation.

| Command | Description |
|---------|-------------|
| [file-move](./file-operations/file-move.user-guide.md) | Move files matching a pattern |
| [file-rename](./file-operations/file-rename.user-guide.md) | Rename files with pattern matching |
| [replace](./file-operations/replace.user-guide.md) | Search and replace in files |

### [Messaging](./messaging/index.md)
Commands for adding messaging infrastructure.

| Command | Description |
|---------|-------------|
| [messaging-add](./messaging/messaging-add.user-guide.md) | Add messaging to a solution |
| [messaging-project-add](./messaging/messaging-project-add.user-guide.md) | Add a messaging project |

### [Special Commands](./special-commands/index.md)
Advanced commands for AI-assisted development and repository operations.

| Command | Description |
|---------|-------------|
| [yolo](./special-commands/yolo.user-guide.md) | Execute Claude AI code generation |
| [a-la-carte](./special-commands/a-la-carte.user-guide.md) | Clone and extract from repositories |

## Global Options

All commands support the following global option:

| Option | Description |
|--------|-------------|
| `--log-level` | Set logging verbosity (Debug, Information, Warning, Error) |

## Common Patterns

### Directory Option
Most commands accept a `-d` or `--directory` option that specifies the target directory. If not provided, the current working directory is used.

```bash
# Use current directory
endpoint class-create -n MyClass

# Specify directory
endpoint class-create -n MyClass -d ./src/Models
```

### Name Option
Most creation commands use `-n` or `--name` to specify the entity name.

```bash
endpoint class-create -n Customer
endpoint interface-create -n ICustomerService
```

## Getting Help

For detailed help on any command:

```bash
endpoint <command> --help
```

## License

Copyright (c) Quinntyne Brown. All Rights Reserved.
Licensed under the MIT License.
