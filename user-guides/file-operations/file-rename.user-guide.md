# file-rename

Rename files by replacing file extensions or suffixes.

## Synopsis

```bash
endpoint file-rename [options]
```

## Description

The `file-rename` command renames files by replacing specified endings (extensions or suffixes) with new values. It recursively processes all matching files in the specified directory.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--old` | `-o` | Old ending to replace | No | - |
| `--new` | `-n` | New ending to use | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Examples

### Change file extensions

```bash
endpoint file-rename -o ".txt" -n ".md"
```

This renames:
- `README.txt` → `README.md`
- `docs/guide.txt` → `docs/guide.md`

### Change file suffixes

```bash
endpoint file-rename -o "Service.cs" -n "Svc.cs"
```

This renames:
- `UserService.cs` → `UserSvc.cs`
- `OrderService.cs` → `OrderSvc.cs`

### Rename in specific directory

```bash
endpoint file-rename -o ".bak" -n ".backup" -d ./data
```

### Rename test file conventions

```bash
endpoint file-rename -o "Tests.cs" -n "Test.cs"
```

## How It Works

1. Recursively finds all files ending with the old value
2. Replaces the ending with the new value
3. Renames the file in place

## Common Use Cases

1. **Extension Changes**: Convert between file formats
2. **Naming Conventions**: Update to new naming standards
3. **Suffix Changes**: Update file suffix patterns
4. **Migrations**: Rename files during project migrations

## Example Scenarios

### Convert TypeScript to JavaScript extensions

```bash
endpoint file-rename -o ".ts" -n ".js"
```

### Update test file naming

```bash
endpoint file-rename -o ".spec.ts" -n ".test.ts"
```

### Rename backup files

```bash
endpoint file-rename -o ".old" -n ".backup"
```

### Change component suffix

```bash
endpoint file-rename -o ".component.ts" -n ".cmp.ts"
```

## Behavior Notes

- Searches recursively through all subdirectories
- Only renames files, not directories
- Case-sensitive matching
- Atomic rename operations

## Caution

- This operation is destructive and cannot be undone
- Test with a small subset first
- Consider backing up before bulk renames
- Version control helps recover from mistakes

## Related Commands

- [file-move](./file-move.user-guide.md) - Move files
- [replace](./replace.user-guide.md) - Replace content in files

[Back to File Operations](./index.md) | [Back to Index](../index.md)
