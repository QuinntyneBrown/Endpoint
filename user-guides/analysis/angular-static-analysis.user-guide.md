# angular-static-analysis

Perform static analysis on an Angular workspace.

## Synopsis

```bash
endpoint angular-static-analysis [options]
```

## Description

The `angular-static-analysis` command analyzes Angular workspaces, providing insights about components, services, modules, directives, pipes, routing, and potential issues. It automatically detects the Angular workspace root by looking for `angular.json`.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--directory` | `-d` | Directory to analyze | No | Current directory |
| `--output` | `-o` | Output file path | No | Console |
| `--no-templates` | - | Skip template analysis | No | `false` |
| `--no-styles` | - | Skip style analysis | No | `false` |
| `--no-issues` | - | Skip issue checking | No | `false` |
| `--no-routing` | - | Skip routing analysis | No | `false` |
| `--verbose` | `-v` | Show verbose output with full details | No | `false` |
| `--json` | - | Output results as JSON | No | `false` |

## Examples

### Analyze current Angular workspace

```bash
endpoint angular-static-analysis
```

### Analyze specific directory

```bash
endpoint angular-static-analysis -d ./frontend
```

### Verbose output

```bash
endpoint angular-static-analysis -v
```

### JSON output

```bash
endpoint angular-static-analysis --json -o analysis.json
```

### Skip certain analyses

```bash
endpoint angular-static-analysis --no-templates --no-styles
```

### Output to file

```bash
endpoint angular-static-analysis -o ./angular-report.txt
```

## Output Sections

### Summary

```
================================================================================
                        ANGULAR STATIC ANALYSIS REPORT
================================================================================

Workspace: /path/to/angular-project
Angular Version: 17.0.0
Standalone Mode: Yes
Analysis Date: 2024-01-15 12:00:00 UTC

--------------------------------------------------------------------------------
                                 SUMMARY
--------------------------------------------------------------------------------
  Components:      25  (OnPush: 20, Standalone: 25)
  Services:        12
  Modules:          3
  Directives:       5
  Pipes:            4
  Routes:          15

  Issues:           3  (Errors: 1, Warnings: 2)
```

### Components

```
--------------------------------------------------------------------------------
                               COMPONENTS
--------------------------------------------------------------------------------
  UserProfileComponent [standalone, OnPush]
    Selector: app-user-profile
    File: src/app/features/users/user-profile.component.ts
    Inputs: userId, showDetails
    Outputs: profileUpdated

  DashboardComponent [standalone, OnPush]
    Selector: app-dashboard
    File: src/app/features/dashboard/dashboard.component.ts
```

### Services

```
--------------------------------------------------------------------------------
                                SERVICES
--------------------------------------------------------------------------------
  UserService [providedIn: root]
    File: src/app/core/services/user.service.ts
    Dependencies: HttpClient, AuthService
    Methods: getUser, updateUser, deleteUser
```

### Issues

```
--------------------------------------------------------------------------------
                                 ISSUES
--------------------------------------------------------------------------------

  ERRORS:
    [!] [Performance] Component missing OnPush change detection
        File: src/app/components/data-table.component.ts
        Suggestion: Add changeDetection: ChangeDetectionStrategy.OnPush

  WARNINGS:
    [*] [BestPractice] Large component template (>100 lines)
        File: src/app/pages/admin/admin.component.html
        Suggestion: Consider breaking into smaller components
```

## Analyzed Elements

| Element | Analysis |
|---------|----------|
| **Components** | Standalone mode, change detection, inputs/outputs |
| **Services** | Injectable scope, dependencies, methods |
| **Modules** | Declarations, imports, exports, providers |
| **Directives** | Selector, inputs, outputs, standalone |
| **Pipes** | Pure/impure, standalone |
| **Routes** | Lazy loading, guards, resolvers |

## Issue Categories

| Category | Description |
|----------|-------------|
| Performance | Change detection, lazy loading |
| BestPractice | Angular style guide compliance |
| Deprecation | Deprecated APIs usage |
| Security | XSS, template injection |
| Accessibility | a11y issues |

## JSON Output Structure

```json
{
  "workspaceRoot": "/path/to/project",
  "angularVersion": "17.0.0",
  "isStandalone": true,
  "analyzedAt": "2024-01-15T12:00:00Z",
  "summary": {
    "totalComponents": 25,
    "onPushComponents": 20,
    "standaloneComponents": 25,
    "totalServices": 12,
    "totalModules": 3,
    "totalDirectives": 5,
    "totalPipes": 4,
    "totalRoutes": 15,
    "issueCount": 3,
    "errorCount": 1,
    "warningCount": 2
  },
  "components": [...],
  "services": [...],
  "modules": [...],
  "issues": [...]
}
```

## Common Use Cases

1. **Project Audits**: Understand Angular project structure
2. **Migration Planning**: Assess standalone component migration
3. **Performance Review**: Find change detection issues
4. **Code Quality**: Enforce Angular best practices

## Related Commands

- [static-analysis](./static-analysis.user-guide.md) - General analysis
- [scss-static-analysis](./scss-static-analysis.user-guide.md) - Style analysis

[Back to Analysis](./index.md) | [Back to Index](../index.md)
