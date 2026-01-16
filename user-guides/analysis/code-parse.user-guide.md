# code-parse

Parse code and generate LLM-optimized summaries for AI consumption.

## Synopsis

```bash
endpoint code-parse [options]
```

## Description

The `code-parse` command parses source code files and generates optimized summaries designed for consumption by Large Language Models (LLMs) like ChatGPT, Claude, or other AI assistants. It extracts code structure while reducing token count based on the specified efficiency level.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name for the output file | No | - |
| `--directory` | `-d` | Comma-separated list of directories to parse | No | Current directory |
| `--output` | `-o` | Output file path | No | Console (and clipboard) |
| `--efficiency` | `-e` | Token efficiency level (0-100) | No | `50` |
| `--ignore-tests` | - | Ignore test files and test projects | No | `false` |
| `--tests-only` | - | Only parse test files and test projects | No | `false` |

## Efficiency Levels

| Level | Description |
|-------|-------------|
| `0` | Verbatim - Full code with all details |
| `25` | Low compression - Most code with minor reduction |
| `50` | Medium - Balanced summary (default) |
| `75` | High compression - Signatures and key logic |
| `100` | Minimal - Only structure and signatures |

## Examples

### Parse current directory

```bash
endpoint code-parse
```

### Parse with high efficiency

```bash
endpoint code-parse -e 80
```

### Parse specific directories

```bash
endpoint code-parse -d "./src,./lib"
```

### Parse ignoring tests

```bash
endpoint code-parse --ignore-tests
```

### Parse tests only

```bash
endpoint code-parse --tests-only
```

### Save to file

```bash
endpoint code-parse -o ./code-summary.txt
```

### Parse with custom name

```bash
endpoint code-parse -n "MyProject-Summary" -o ./summaries/
```

## Output Format

The output is optimized for LLM context windows:

```
=== CODE SUMMARY ===
Root: /path/to/project
Efficiency: 50
Files: 25

=== src/Services/UserService.cs ===
```csharp
public class UserService : IUserService
{
    // Dependencies
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    // Methods
    public async Task<User> GetByIdAsync(Guid id) { ... }
    public async Task<User> CreateAsync(CreateUserRequest request) { ... }
    public async Task UpdateAsync(UpdateUserRequest request) { ... }
    public async Task DeleteAsync(Guid id) { ... }
}
```

=== src/Models/User.cs ===
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```
```

## Automatic Clipboard Copy

The parsed output is automatically copied to your clipboard, making it easy to paste into:

- ChatGPT
- Claude
- GitHub Copilot Chat
- Other AI assistants

## Supported File Types

| Language | Extensions |
|----------|------------|
| C# | `.cs` |
| TypeScript | `.ts`, `.tsx` |
| JavaScript | `.js`, `.jsx` |
| Python | `.py` |
| Java | `.java` |
| Go | `.go` |
| Rust | `.rs` |
| JSON | `.json` |
| YAML | `.yaml`, `.yml` |

## Test File Detection

The command automatically detects test files by:

- File names: `*Test.cs`, `*Tests.cs`, `*Spec.ts`
- Directories: `test/`, `tests/`, `__tests__/`
- Project types: xUnit, NUnit, MSTest, Jest, pytest

## Common Use Cases

1. **AI Assistance**: Provide code context to AI assistants
2. **Code Documentation**: Generate code summaries
3. **Onboarding**: Quick codebase overview
4. **Code Review**: Share code structure for review

## Best Practices

- Use efficiency 50-75 for most AI queries
- Use efficiency 0-25 when precise code is needed
- Exclude tests unless specifically needed
- Parse only relevant directories

## Example Workflow

```bash
# Parse core code for AI assistance
endpoint code-parse -d ./src -e 70 --ignore-tests

# Paste into ChatGPT with your question
# "Based on this code, how would I add caching to UserService?"
```

## Related Commands

- [static-analysis](./static-analysis.user-guide.md) - Analyze code quality
- [code-review](./code-review.user-guide.md) - Review code changes

[Back to Analysis](./index.md) | [Back to Index](../index.md)
