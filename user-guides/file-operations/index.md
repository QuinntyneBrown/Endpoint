# File Operations Commands

Commands for file manipulation, moving, renaming, and content transformation.

## Available Commands

| Command | Description |
|---------|-------------|
| [file-move](./file-move.user-guide.md) | Move files matching a pattern |
| [file-rename](./file-rename.user-guide.md) | Rename files with pattern matching |
| [replace](./replace.user-guide.md) | Search and replace in files |

## Quick Examples

```bash
# Move all .cs files to a new directory
endpoint file-move -s "*.cs" -f ./src/NewFolder

# Rename files from .txt to .md
endpoint file-rename -o ".txt" -n ".md"

# Replace text in all files
endpoint replace -s "oldNamespace" -r "newNamespace" -p "*.cs"
```

[Back to Index](../index.md)
