# solution-create

Create a new .NET solution with optional initial project.

## Synopsis

```bash
endpoint solution-create [options]
```

## Description

The `solution-create` command generates a new .NET solution file along with an optional initial project. It sets up the solution structure with proper folder organization and opens VS Code automatically upon completion.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the solution | No | - |
| `--project-name` | `-p` | Name of the initial project | No | `Worker.Console` |
| `--folder-name` | `-f` | Solution folder name for the project | No | - |
| `--no-service-create` | - | Skip creating service classes | No | `false` |
| `--project-type` | `-t` | Type of initial project | No | `worker` |
| `--directory` | `-d` | Target directory | No | Current directory |

## Project Types

| Type | Description |
|------|-------------|
| `worker` | Background worker/console application |
| `classlib` | Class library |
| `webapi` | ASP.NET Core Web API |
| `console` | Console application |

## Examples

### Create a basic solution

```bash
endpoint solution-create -n MyApplication
```

Creates:
```
MyApplication/
├── MyApplication.sln
└── src/
    └── Worker.Console/
        └── Worker.Console.csproj
```

### Create a solution with custom project

```bash
endpoint solution-create -n ECommerce -p "ECommerce.Api" -t webapi
```

Creates:
```
ECommerce/
├── ECommerce.sln
└── src/
    └── ECommerce.Api/
        └── ECommerce.Api.csproj
```

### Create a solution without services

```bash
endpoint solution-create -n DataLibrary --no-service-create -t classlib
```

### Specify output directory

```bash
endpoint solution-create -n MyProject -d ./projects
```

### Create with folder organization

```bash
endpoint solution-create -n MyApp -p "MyApp.Core" -f "Core"
```

Creates with solution folder:
```
MyApp/
├── MyApp.sln
└── src/
    └── Core/
        └── MyApp.Core/
            └── MyApp.Core.csproj
```

## Solution Structure

The generated solution follows this structure:

```
{SolutionName}/
├── {SolutionName}.sln
├── src/
│   └── {Projects...}
├── tests/
│   └── {Test Projects...}
└── .gitignore
```

## Automatic Actions

After creation, the command:
1. Creates the solution file
2. Generates the initial project
3. Sets up proper folder structure
4. Opens VS Code in the solution directory

## Common Use Cases

1. **New Projects**: Start a new .NET project from scratch
2. **Microservices**: Create isolated service solutions
3. **Libraries**: Create reusable library solutions
4. **Full-Stack Apps**: Create backend solutions for full-stack applications

## Best Practices

- Use meaningful solution names that reflect the project's purpose
- Start with a minimal initial project
- Use solution folders to organize multiple projects
- Add projects incrementally using `project-add`

## Related Commands

- [project-add](./project-add.user-guide.md) - Add projects to the solution
- [ddd-app-create](../application-scaffolding/ddd-app-create.user-guide.md) - Create DDD applications
- [mwa-create](../application-scaffolding/mwa-create.user-guide.md) - Create Modern Web Applications

[Back to Project Management](./index.md) | [Back to Index](../index.md)
