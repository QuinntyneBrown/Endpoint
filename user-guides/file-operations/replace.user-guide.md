# replace

Search and replace text across multiple files.

## Synopsis

```bash
endpoint replace [options]
```

## Description

The `replace` command performs search and replace operations across multiple files matching a specified pattern. This is useful for bulk text updates like namespace changes, renaming, or configuration updates.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--search` | `-s` | Text to search for | No | - |
| `--replace` | `-r` | Text to replace with | No | - |
| `--pattern` | `-p` | File pattern to match (e.g., `*.cs`) | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Examples

### Replace namespace

```bash
endpoint replace -s "OldNamespace" -r "NewNamespace" -p "*.cs"
```

### Replace in specific file types

```bash
endpoint replace -s "localhost:5000" -r "api.example.com" -p "*.json"
```

### Replace class names

```bash
endpoint replace -s "CustomerManager" -r "CustomerService" -p "*.cs"
```

### Replace in specific directory

```bash
endpoint replace -s "v1" -r "v2" -p "*.ts" -d ./src/api
```

## Common Use Cases

### 1. Namespace Refactoring

```bash
# Change project namespace
endpoint replace -s "MyCompany.OldProject" -r "MyCompany.NewProject" -p "*.cs"
```

### 2. Configuration Updates

```bash
# Update connection strings
endpoint replace -s "Server=olddb" -r "Server=newdb" -p "appsettings*.json"
```

### 3. API Endpoint Changes

```bash
# Update API base URLs
endpoint replace -s "/api/v1/" -r "/api/v2/" -p "*.ts"
```

### 4. Import Path Updates

```bash
# Update import paths
endpoint replace -s "from '@old/'" -r "from '@new/'" -p "*.ts"
```

### 5. Class Renaming

```bash
# Rename classes across codebase
endpoint replace -s "UserRepository" -r "UserDataAccess" -p "*.cs"
```

## File Pattern Examples

| Pattern | Matches |
|---------|---------|
| `*.cs` | All C# files |
| `*.ts` | All TypeScript files |
| `*.json` | All JSON files |
| `appsettings*.json` | appsettings files |
| `*Controller.cs` | Controller files |

## Behavior

- Recursively searches through subdirectories
- Case-sensitive matching
- Replaces all occurrences in each file
- Modifies files in place

## Best Practices

1. **Backup First**: Use version control or backup before bulk changes
2. **Test Pattern**: Verify file pattern matches expected files
3. **Review Changes**: Use `git diff` to review modifications
4. **Incremental**: Start with specific directories, then expand

## Workflow Example

```bash
# 1. Check what files will be affected
find . -name "*.cs" | xargs grep -l "OldNamespace"

# 2. Perform replacement
endpoint replace -s "OldNamespace" -r "NewNamespace" -p "*.cs"

# 3. Review changes
git diff

# 4. Commit if satisfied
git add -A && git commit -m "Renamed namespace"
```

## Caution

- Changes are immediate and cannot be undone without version control
- Test on a small subset first
- Be specific with search terms to avoid unintended replacements
- Consider word boundaries for partial matches

## Related Commands

- [file-rename](./file-rename.user-guide.md) - Rename file names
- [file-move](./file-move.user-guide.md) - Move files

[Back to File Operations](./index.md) | [Back to Index](../index.md)
