# file-move

Move files matching a search pattern to a destination directory.

## Synopsis

```bash
endpoint file-move [options]
```

## Description

The `file-move` command moves files matching a specified search pattern to a destination directory. It recursively searches through subdirectories and moves all matching files.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--searchPattern` | `-s` | File pattern to match (e.g., `*.cs`, `*.json`) | No | - |
| `--destination` | `-f` | Destination directory | Yes | - |
| `--directory` | `-d` | Source directory to search | No | Current directory |

## Examples

### Move all C# files to a new directory

```bash
endpoint file-move -s "*.cs" -f ./src/NewFolder
```

### Move specific file types

```bash
endpoint file-move -s "*.json" -f ./config -d ./src
```

### Move files with specific naming pattern

```bash
endpoint file-move -s "*Service.cs" -f ./Services
```

### Move test files

```bash
endpoint file-move -s "*Tests.cs" -f ./tests -d ./src
```

## Search Pattern Syntax

| Pattern | Matches |
|---------|---------|
| `*` | Any sequence of characters |
| `?` | Any single character |
| `*.cs` | All C# files |
| `*Test*.cs` | Files containing "Test" |
| `User*.cs` | Files starting with "User" |

## Behavior

- Searches recursively through all subdirectories
- Moves files to the flat destination directory
- Original file names are preserved
- Existing files with same name may be overwritten

## Common Use Cases

1. **Reorganization**: Move files during project restructuring
2. **Consolidation**: Gather scattered files into one location
3. **Separation**: Move tests, configs, or specific file types

## Example Scenarios

### Consolidate configuration files

```bash
endpoint file-move -s "appsettings*.json" -f ./config
```

### Move all interfaces

```bash
endpoint file-move -s "I*.cs" -f ./Interfaces
```

### Move generated files

```bash
endpoint file-move -s "*.generated.cs" -f ./Generated
```

## Caution

- Ensure destination directory exists
- Back up important files before bulk operations
- Review matches before moving large numbers of files

## Related Commands

- [file-rename](./file-rename.user-guide.md) - Rename files with patterns
- [replace](./replace.user-guide.md) - Search and replace in files

[Back to File Operations](./index.md) | [Back to Index](../index.md)
