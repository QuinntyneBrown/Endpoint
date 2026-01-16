# csharp-static-analysis

Perform C# specific static analysis on solutions, projects, or files.

## Synopsis

```bash
endpoint csharp-static-analysis [options]
```

## Description

The `csharp-static-analysis` command performs comprehensive static analysis on C# code. It analyzes naming conventions, code style, code quality, unused code, documentation, design patterns, performance, security, and maintainability.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--path` | `-p` | Path to analyze (solution, project, directory, or file) | No | Current directory |
| `--output` | `-o` | Output file path | No | Console |
| `--severity` | - | Minimum severity level: `info`, `warning`, `error`, or `all` | No | `all` |
| `--categories` | `-c` | Comma-separated categories to analyze | No | All categories |
| `--include-tests` | - | Include test files in analysis | No | `false` |
| `--max-issues` | - | Maximum number of issues to report (0 = unlimited) | No | `0` |
| `--json` | - | Output results in JSON format | No | `false` |

## Categories

| Category | Description |
|----------|-------------|
| `naming` | Naming conventions (PascalCase, camelCase, etc.) |
| `style` | Code style and formatting |
| `codequality` | Code quality issues |
| `unusedcode` | Unused variables, methods, classes |
| `documentation` | XML documentation completeness |
| `design` | Design pattern issues |
| `performance` | Performance concerns |
| `security` | Security vulnerabilities |
| `maintainability` | Code maintainability metrics |

## Examples

### Analyze current directory

```bash
endpoint csharp-static-analysis
```

### Analyze a specific solution

```bash
endpoint csharp-static-analysis -p ./MyApp.sln
```

### Analyze only naming issues

```bash
endpoint csharp-static-analysis -c naming
```

### Analyze multiple categories

```bash
endpoint csharp-static-analysis -c "naming,security,performance"
```

### Errors only

```bash
endpoint csharp-static-analysis --severity error
```

### Output to file

```bash
endpoint csharp-static-analysis -o ./analysis-report.txt
```

### JSON output

```bash
endpoint csharp-static-analysis --json -o ./analysis.json
```

### Include test files

```bash
endpoint csharp-static-analysis --include-tests
```

### Limit issues reported

```bash
endpoint csharp-static-analysis --max-issues 50
```

## Output Format

### Console Output

```
C# Static Analysis Results
==========================

Path: /path/to/solution
Target Type: Solution
Files Analyzed: 45
Analysis Time: 2.3s

ISSUES BY CATEGORY:
-------------------

Naming (5 issues):
  [CS0001] Class name should be PascalCase
    File: src/services/userService.cs:5
    Severity: Warning

  [CS0002] Private field should have underscore prefix
    File: src/Models/User.cs:12
    Severity: Info

Security (1 issue):
  [CS0101] Potential SQL injection vulnerability
    File: src/Data/UserRepository.cs:34
    Severity: Error

SUMMARY:
--------
  Total Files: 45
  Total Lines: 5,234
  Total Issues: 6
    Errors: 1
    Warnings: 3
    Info: 2
  Duration: 2,345ms
```

### JSON Output

```json
{
  "rootPath": "/path/to/solution",
  "targetType": "Solution",
  "success": false,
  "summary": {
    "totalFiles": 45,
    "totalLinesOfCode": 5234,
    "totalClasses": 32,
    "totalInterfaces": 15,
    "totalMethods": 189,
    "totalIssues": 6,
    "errorCount": 1,
    "warningCount": 3,
    "infoCount": 2,
    "analysisDurationMs": 2345
  },
  "issues": [
    {
      "filePath": "src/services/userService.cs",
      "line": 5,
      "column": 14,
      "severity": "Warning",
      "category": "Naming",
      "ruleId": "CS0001",
      "message": "Class name should be PascalCase"
    }
  ]
}
```

## Common Rule IDs

### Naming Rules

| Rule | Description |
|------|-------------|
| CS0001 | Class name should be PascalCase |
| CS0002 | Private field should have underscore prefix |
| CS0003 | Interface name should start with 'I' |
| CS0004 | Method name should be PascalCase |

### Security Rules

| Rule | Description |
|------|-------------|
| CS0101 | Potential SQL injection |
| CS0102 | Hardcoded credentials |
| CS0103 | Insecure random number generator |
| CS0104 | Missing input validation |

### Performance Rules

| Rule | Description |
|------|-------------|
| CS0201 | Use StringBuilder for concatenation in loops |
| CS0202 | Avoid boxing/unboxing |
| CS0203 | Consider using async/await |

## CI/CD Integration

```yaml
# GitHub Actions
- name: C# Static Analysis
  run: |
    endpoint csharp-static-analysis --severity warning --json -o analysis.json
    if [ $(jq '.summary.errorCount' analysis.json) -gt 0 ]; then exit 1; fi
```

## Related Commands

- [static-analysis](./static-analysis.user-guide.md) - General analysis
- [code-review](./code-review.user-guide.md) - Code review with diff

[Back to Analysis](./index.md) | [Back to Index](../index.md)
