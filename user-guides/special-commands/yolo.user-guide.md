# yolo

Execute Claude AI for automated code generation and modifications.

## Synopsis

```bash
endpoint yolo [options]
```

## Description

The `yolo` command invokes Claude AI (Anthropic's AI assistant) to perform automated code generation, modifications, and other development tasks. It runs Claude in a non-interactive mode with automatic permission approval for file operations.

**Note**: This command requires Claude CLI to be installed and configured on your system.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--directory` | `-d` | Working directory for Claude | No | Current directory |
| `--prompt` | `-p` | Prompt/instruction for Claude | No | - |

## Examples

### Run Claude with a prompt

```bash
endpoint yolo -p "Add unit tests for the UserService class"
```

### Run Claude in specific directory

```bash
endpoint yolo -d ./src -p "Refactor the database access layer"
```

### Complex code generation

```bash
endpoint yolo -p "Create a REST API endpoint for managing products with full CRUD operations"
```

### Code review and fixes

```bash
endpoint yolo -p "Review the authentication module and fix any security issues"
```

## Command Execution

The command builds and executes:

```bash
claude --continue --print "<prompt>" --dangerously-skip-permissions --verbose --output-format
```

## Security

### Command Injection Protection

The command includes built-in protection against command injection attacks. The following characters are not allowed in prompts:

- `;` (semicolon)
- `&` (ampersand)
- `|` (pipe)
- `` ` `` (backtick)
- `$` (dollar sign)
- `<` `>` (redirects)
- `(` `)` `{` `}` (brackets)
- `*` `?` `[` `]` (globs)
- `~` (tilde)
- `'` (single quote)
- Newlines (`\n`, `\r`)

If dangerous characters are detected, the command will fail with an error.

### Safe Prompt Examples

```bash
# Good - simple prompts
endpoint yolo -p "Add logging to all service methods"
endpoint yolo -p "Create a new Customer entity with Name and Email properties"
endpoint yolo -p "Fix the null reference exception in OrderService"
```

### Unsafe Prompts (Will Be Rejected)

```bash
# Bad - contains dangerous characters
endpoint yolo -p "Run `rm -rf /`"  # Backticks
endpoint yolo -p "Do this; and that"  # Semicolon
endpoint yolo -p "Save to /tmp/$USER"  # Dollar sign
```

## Use Cases

### 1. Code Generation

```bash
endpoint yolo -p "Create a repository pattern implementation for the Order entity"
```

### 2. Refactoring

```bash
endpoint yolo -p "Refactor UserService to use dependency injection"
```

### 3. Bug Fixes

```bash
endpoint yolo -p "Fix the race condition in the caching layer"
```

### 4. Documentation

```bash
endpoint yolo -p "Add XML documentation comments to all public methods"
```

### 5. Test Generation

```bash
endpoint yolo -p "Generate unit tests for OrderService with 80% coverage"
```

## Prerequisites

1. **Claude CLI**: Must be installed (`npm install -g @anthropic-ai/claude-cli` or similar)
2. **API Key**: Claude API key must be configured
3. **Permissions**: Working directory must be writable

## Exit Codes

| Code | Description |
|------|-------------|
| 0 | Claude completed successfully |
| Non-zero | Claude encountered an error |

## Best Practices

1. **Be Specific**: Provide detailed prompts for better results
2. **Review Output**: Always review AI-generated code
3. **Version Control**: Commit before running yolo
4. **Incremental**: Make small, focused requests

## Cautions

- AI-generated code should always be reviewed
- Use version control to track and revert changes
- Test thoroughly after modifications
- Don't use for security-critical code without review

## Related Commands

- [code-review](../analysis/code-review.user-guide.md) - Review code changes
- [static-analysis](../analysis/static-analysis.user-guide.md) - Analyze generated code

[Back to Special Commands](./index.md) | [Back to Index](../index.md)
