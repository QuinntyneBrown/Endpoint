# Special Commands

Advanced commands for AI-assisted development, repository operations, and automation.

## Available Commands

| Command | Description |
|---------|-------------|
| [yolo](./yolo.user-guide.md) | Execute Claude AI code generation |
| [a-la-carte](./a-la-carte.user-guide.md) | Clone and extract from repositories |

## Quick Examples

```bash
# Run Claude AI to help with code generation
endpoint yolo -p "Add unit tests for the UserService class"

# Clone specific folders from a repository
endpoint a-la-carte -u https://github.com/org/repo -f "src/lib:lib;src/core:core"
```

[Back to Index](../index.md)
