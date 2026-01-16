# Project Management Commands

Commands for managing .NET solutions, projects, references, and packages.

## Available Commands

| Command | Description |
|---------|-------------|
| [solution-create](./solution-create.user-guide.md) | Create a new .NET solution |
| [project-add](./project-add.user-guide.md) | Add a project to a solution |
| [reference-add](./reference-add.user-guide.md) | Add project references |
| [package-add](./package-add.user-guide.md) | Add NuGet packages |
| [migration-add](./migration-add.user-guide.md) | Add database migrations |

## Quick Examples

```bash
# Create a new solution
endpoint solution-create -n MyApp

# Add a class library project
endpoint project-add -n MyApp.Core -t classlib

# Add a NuGet package
endpoint package-add -n Newtonsoft.Json
```

[Back to Index](../index.md)
