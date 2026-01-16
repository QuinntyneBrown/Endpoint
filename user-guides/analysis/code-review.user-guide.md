# code-review

Perform a code review by comparing branches and running static analysis.

## Synopsis

```bash
endpoint code-review [options]
```

## Description

The `code-review` command performs an automated code review by comparing the current branch against a target branch (usually `main`). It shows changed files, displays diffs, and optionally runs static analysis on the changed files to identify potential issues.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--directory` | `-d` | Directory containing the git repository | No | Current directory |
| `--target-branch` | `-t` | Target branch to compare against | No | `main` |
| `--analyze` | `-a` | Run static analysis on changed files | No | `true` |
| `--verbose` | `-v` | Show verbose output including full diff | No | `false` |
| `--no-color` | - | Disable colorized output | No | `false` |

## Examples

### Basic code review against main

```bash
endpoint code-review
```

### Review against a specific branch

```bash
endpoint code-review -t develop
```

### Full verbose review with diff

```bash
endpoint code-review -v
```

### Review without static analysis

```bash
endpoint code-review -a false
```

### Review in a specific directory

```bash
endpoint code-review -d ./my-project
```

### Review with plain text output

```bash
endpoint code-review --no-color > review.txt
```

## Output Sections

### Repository Information

```
Repository: /path/to/repo
Current Branch: feature/my-feature
Target Branch: main
```

### Changed Files

```
=== Changed Files (5) ===

  src/Services/UserService.cs
  src/Models/User.cs
  src/Controllers/UsersController.cs
  tests/UserServiceTests.cs
  README.md
```

### Diff (with --verbose)

```
=== Diff ===

diff --git a/src/Services/UserService.cs b/src/Services/UserService.cs
--- a/src/Services/UserService.cs
+++ b/src/Services/UserService.cs
@@ -10,6 +10,10 @@ public class UserService
+    public async Task<User> CreateAsync(CreateUserRequest request)
+    {
+        // Implementation
+    }
```

### Static Analysis Results

```
=== Static Analysis Results ===

Files Analyzed: 4

VIOLATIONS (1):
  [SA001] Missing null check in UserService.CreateAsync
    File: src/Services/UserService.cs:15
    Fix: Add ArgumentNullException.ThrowIfNull(request)

WARNINGS (2):
  [SA002] Method exceeds recommended line count
    File: src/Services/UserService.cs:20
    Recommendation: Consider breaking into smaller methods
```

### Summary

```
=== Summary ===

Review FAILED: 1 violation(s) found.
```

## Exit Codes

| Code | Description |
|------|-------------|
| 0 | Review passed (no violations) |
| 1 | Review failed (violations found) |

## Common Use Cases

1. **Pre-PR Reviews**: Review changes before creating pull request
2. **CI/CD Integration**: Automated review in pipelines
3. **Quality Gates**: Enforce code quality standards
4. **Team Reviews**: Generate review reports for team discussion

## CI/CD Integration

### GitHub Actions

```yaml
- name: Code Review
  run: |
    endpoint code-review -t main --no-color
  continue-on-error: false
```

### Azure DevOps

```yaml
- script: endpoint code-review -t main --no-color
  displayName: 'Run Code Review'
  failOnStderr: true
```

## Best Practices

- Run before creating pull requests
- Address violations before merging
- Review warnings for potential improvements
- Use verbose mode for detailed investigation

## Related Commands

- [static-analysis](./static-analysis.user-guide.md) - General static analysis
- [csharp-static-analysis](./csharp-static-analysis.user-guide.md) - C# specific analysis

[Back to Analysis](./index.md) | [Back to Index](../index.md)
