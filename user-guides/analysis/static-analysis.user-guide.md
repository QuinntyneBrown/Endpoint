# static-analysis

Perform general static analysis on a codebase based on specification rules.

## Synopsis

```bash
endpoint static-analysis [options]
```

## Description

The `static-analysis` command performs static analysis on your codebase by applying specification rules. It automatically detects the project type (Git repository, .NET solution, Angular workspace, or Node environment) and applies appropriate rules.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--path` | `-p` | Path to analyze (file or directory) | No | Current directory |
| `--verbose` | `-v` | Show verbose output including informational messages | No | `false` |
| `--json` | - | Output results in JSON format | No | `false` |
| `--fail-on-warning` | - | Exit with error code if warnings are found | No | `false` |

## Examples

### Run analysis on current directory

```bash
endpoint static-analysis
```

### Analyze a specific path

```bash
endpoint static-analysis -p ./src
```

### Get JSON output

```bash
endpoint static-analysis --json > analysis.json
```

### Verbose output

```bash
endpoint static-analysis -v
```

### Fail on warnings (strict mode)

```bash
endpoint static-analysis --fail-on-warning
```

## Output Sections

### Header

```
=== Endpoint Static Analysis ===

Project Type: DotNetSolution
Root Directory: /path/to/project
```

### Violations

```
=== VIOLATIONS (2) ===

[RULE001] Missing required file: README.md
  Spec: project-structure.md
  Fix: Create a README.md file in the project root

[RULE002] Invalid namespace convention
  File: src/Services/userService.cs:1
  Spec: naming-conventions.md
  Fix: Use PascalCase for namespaces
```

### Warnings

```
=== WARNINGS (1) ===

[WARN001] Large file detected
  File: src/Models/LegacyModel.cs
  Spec: code-quality.md
  Recommendation: Consider splitting into smaller files
```

### Info (with --verbose)

```
=== INFO (5) ===

[INFO] Found 25 source files
  File: src/

[INFO] Detected .NET 8.0 project
  File: src/MyApp.csproj
```

### Summary

```
=== SUMMARY ===

FAILED: 2 violation(s), 1 warning(s).
```

## Supported Project Types

| Type | Detection |
|------|-----------|
| Git Repository | `.git` folder |
| .NET Solution | `*.sln` file |
| Angular Workspace | `angular.json` file |
| Node Environment | `package.json` file |

## Exit Codes

| Code | Description |
|------|-------------|
| 0 | Analysis passed |
| 1 | Violations found or error occurred |

## JSON Output Format

```json
{
  "passed": false,
  "totalFilesAnalyzed": 25,
  "timestamp": "2024-01-15T12:00:00Z",
  "violations": [
    {
      "ruleId": "RULE001",
      "message": "Missing required file: README.md",
      "specSource": "project-structure.md",
      "suggestedFix": "Create a README.md file"
    }
  ],
  "warnings": [],
  "info": []
}
```

## Common Use Cases

1. **CI/CD Pipelines**: Automated quality checks
2. **Pre-Commit Hooks**: Validate before commits
3. **Code Quality**: Enforce team standards
4. **Onboarding**: Check project setup

## CI/CD Integration

```yaml
# GitHub Actions
- name: Static Analysis
  run: endpoint static-analysis --fail-on-warning
```

## Related Commands

- [code-review](./code-review.user-guide.md) - Code review with diff
- [csharp-static-analysis](./csharp-static-analysis.user-guide.md) - C# specific
- [angular-static-analysis](./angular-static-analysis.user-guide.md) - Angular specific

[Back to Analysis](./index.md) | [Back to Index](../index.md)
