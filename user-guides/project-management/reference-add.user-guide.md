# reference-add

Add project references between .NET projects.

## Synopsis

```bash
endpoint reference-add [options]
```

## Description

The `reference-add` command adds project references from one project to another. This is useful for establishing dependencies between projects in a solution.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the project to reference | No | - |
| `--directory` | `-d` | Directory containing the source project | No | Current directory |

## Examples

### Add a single reference

```bash
# In the MyApp.Api project directory
endpoint reference-add -n MyApp.Core
```

### Add reference with full path

```bash
endpoint reference-add -n "../MyApp.Core/MyApp.Core.csproj"
```

### Specify source project directory

```bash
endpoint reference-add -n MyApp.Services -d ./src/MyApp.Api
```

## How It Works

The command:
1. Locates the project file (.csproj) in the specified directory
2. Finds the referenced project by name
3. Adds a `<ProjectReference>` element to the source project

## Generated Reference

The command adds to the .csproj file:

```xml
<ItemGroup>
  <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
</ItemGroup>
```

## Common Patterns

### Layered Architecture

```bash
# API references Services
cd src/MyApp.Api
endpoint reference-add -n MyApp.Services

# Services references Core
cd ../MyApp.Services
endpoint reference-add -n MyApp.Core

# Data references Core
cd ../MyApp.Data
endpoint reference-add -n MyApp.Core
```

### Test Project References

```bash
# Test project references the project being tested
cd tests/MyApp.Core.Tests
endpoint reference-add -n MyApp.Core
```

## Common Use Cases

1. **Dependency Management**: Set up project dependencies
2. **Layered Architecture**: Connect architectural layers
3. **Test Projects**: Reference production code from tests
4. **Shared Libraries**: Reference common libraries

## Best Practices

- Avoid circular references
- Keep references unidirectional (higher layers reference lower)
- Use abstractions to reduce tight coupling
- Reference only what's needed

## Related Commands

- [project-add](./project-add.user-guide.md) - Add projects with references
- [package-add](./package-add.user-guide.md) - Add NuGet package references

[Back to Project Management](./index.md) | [Back to Index](../index.md)
