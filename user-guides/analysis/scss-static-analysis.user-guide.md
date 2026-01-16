# scss-static-analysis

Perform static analysis on SCSS files.

## Synopsis

```bash
endpoint scss-static-analysis [options]
```

## Description

The `scss-static-analysis` command analyzes SCSS (Sass) stylesheets for issues, metrics, and best practices. It can analyze individual files or entire directories recursively.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--directory` | `-d` | Directory path to analyze | No | Current directory |
| `--file` | `-f` | Single SCSS file to analyze | No | - |
| `--recursive` | `-r` | Search directories recursively | No | `true` |
| `--output` | `-o` | Output format: `console`, `json`, or `summary` | No | `console` |
| `--errors-only` | - | Only show errors, hide warnings and info | No | `false` |
| `--fail-on-warnings` | - | Exit with non-zero code if warnings found | No | `false` |

## Examples

### Analyze current directory

```bash
endpoint scss-static-analysis
```

### Analyze a specific file

```bash
endpoint scss-static-analysis -f ./src/styles/main.scss
```

### Analyze a directory

```bash
endpoint scss-static-analysis -d ./src/styles
```

### Non-recursive analysis

```bash
endpoint scss-static-analysis -d ./src/styles -r false
```

### JSON output

```bash
endpoint scss-static-analysis -o json > scss-report.json
```

### Summary only

```bash
endpoint scss-static-analysis -o summary
```

### Errors only (strict mode)

```bash
endpoint scss-static-analysis --errors-only
```

### Fail on warnings

```bash
endpoint scss-static-analysis --fail-on-warnings
```

## Output Format

### Console Output

```
SCSS Static Analysis Results
Directory: /path/to/styles
============================================================

Files analyzed: 15
Total lines: 2,450

File: src/styles/components/_button.scss
----------------------------------------
  Lines: 85
  Selectors: 12
  Variables: 5
  Mixins: 2
  Max nesting depth: 3

  Issues:
    ✗ [SCSS001] Line 45: Selector nesting exceeds recommended depth (4 levels)
      > .container .wrapper .content .item { ... }
    ⚠ [SCSS002] Line 23: Color should use variable
      > color: #333333;
    ℹ [SCSS003] Line 10: Consider using shorthand property

Summary
----------------------------------------
  Errors: 1
  Warnings: 3
  Info: 2

Analysis completed with errors.
```

### JSON Output

```json
{
  "directoryPath": "/path/to/styles",
  "totalFiles": 15,
  "totalLines": 2450,
  "totalErrors": 1,
  "totalWarnings": 3,
  "fileResults": [
    {
      "filePath": "src/styles/components/_button.scss",
      "totalLines": 85,
      "selectorCount": 12,
      "variableCount": 5,
      "mixinCount": 2,
      "maxNestingDepth": 3,
      "hasIssues": true,
      "issues": [
        {
          "code": "SCSS001",
          "line": 45,
          "severity": "Error",
          "message": "Selector nesting exceeds recommended depth (4 levels)",
          "sourceSnippet": ".container .wrapper .content .item { ... }"
        }
      ]
    }
  ]
}
```

## Metrics Analyzed

| Metric | Description |
|--------|-------------|
| Lines | Total lines of code |
| Selectors | Number of CSS selectors |
| Variables | Number of SCSS variables |
| Mixins | Number of mixins defined |
| Max Nesting | Deepest selector nesting level |

## Issue Rules

### Errors

| Rule | Description |
|------|-------------|
| SCSS001 | Selector nesting exceeds maximum depth |
| SCSS002 | Invalid SCSS syntax |
| SCSS003 | Undefined variable reference |

### Warnings

| Rule | Description |
|------|-------------|
| SCSS010 | Hardcoded color should use variable |
| SCSS011 | Duplicate selectors |
| SCSS012 | Empty ruleset |
| SCSS013 | !important usage |

### Info

| Rule | Description |
|------|-------------|
| SCSS020 | Shorthand property available |
| SCSS021 | Vendor prefix can be automated |
| SCSS022 | Consider using calc() |

## Best Practices Checked

1. **Nesting Depth**: Max 3-4 levels
2. **Variables**: Use variables for colors, sizes
3. **Mixins**: Reusable patterns
4. **Organization**: Partial files, logical grouping
5. **Naming**: BEM or consistent convention

## CI/CD Integration

```yaml
# GitHub Actions
- name: SCSS Analysis
  run: |
    endpoint scss-static-analysis --fail-on-warnings -o json > scss-report.json
    cat scss-report.json
```

## Common Use Cases

1. **Style Audits**: Review CSS/SCSS quality
2. **Migration**: Analyze before refactoring
3. **CI/CD**: Enforce style standards
4. **Code Reviews**: Automated style checks

## Related Commands

- [angular-static-analysis](./angular-static-analysis.user-guide.md) - Angular analysis
- [static-analysis](./static-analysis.user-guide.md) - General analysis

[Back to Analysis](./index.md) | [Back to Index](../index.md)
