# a-la-carte

Clone git repositories and extract select folders into a new structure.

## Synopsis

```bash
endpoint a-la-carte [options]
```

## Description

The `a-la-carte` command clones git repositories and extracts specified folders to create a new folder structure. It's useful for cherry-picking code from multiple repositories, creating custom project templates, or combining libraries from different sources.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--config` | `-c` | Path to JSON configuration file | No | - |
| `--directory` | `-d` | Output directory | No | Current directory |
| `--output-type` | `-o` | Output type: `DotNetSolution`, `MixDotNetSolutionWithOtherFolders`, `NotSpecified` | No | `NotSpecified` |
| `--solution-name` | `-s` | Name of .NET solution to create | No | `ALaCarte.sln` |
| `--url` | `-u` | Git repository URL (single repo mode) | No | - |
| `--branch` | `-b` | Git branch (single repo mode) | No | `main` |
| `--folders` | `-f` | Folder mappings `from:to` (semicolon-separated) | No | - |
| `--verbose` | `-v` | Show verbose output | No | `false` |
| `--no-color` | - | Disable colorized output | No | `false` |

## Examples

### Single repository with folder mappings

```bash
endpoint a-la-carte -u https://github.com/org/repo -f "src/lib:lib;src/core:core"
```

### Using configuration file

```bash
endpoint a-la-carte -c ./ala-carte-config.json
```

### Create .NET solution from extracted code

```bash
endpoint a-la-carte -u https://github.com/org/repo -f "src/MyLib:MyLib" -o DotNetSolution -s MyCustomSolution
```

### Specify output directory

```bash
endpoint a-la-carte -u https://github.com/org/repo -f "src:extracted" -d ./output
```

### Verbose output

```bash
endpoint a-la-carte -c ./config.json -v
```

## Configuration File Format

```json
{
  "directory": "./output",
  "outputType": "DotNetSolution",
  "solutionName": "MyComposedProject.sln",
  "repositories": [
    {
      "url": "https://github.com/org/repo1",
      "branch": "main",
      "folders": [
        { "from": "src/lib", "to": "libs/repo1-lib" },
        { "from": "src/core", "to": "core" }
      ]
    },
    {
      "url": "https://github.com/org/repo2",
      "branch": "develop",
      "folders": [
        { "from": "packages/utils", "to": "libs/utils" }
      ]
    }
  ]
}
```

## Output Types

| Type | Description |
|------|-------------|
| `NotSpecified` | Copy folders as-is |
| `DotNetSolution` | Create .NET solution and add all .csproj files |
| `MixDotNetSolutionWithOtherFolders` | Create solution + preserve non-.NET folders |

## Folder Mapping Format

Single repository mode uses semicolon-separated mappings:

```
from1:to1;from2:to2;from3:to3
```

Examples:
- `src/lib:lib` - Copy `src/lib` to `lib`
- `packages:deps` - Copy `packages` to `deps`
- `src:src` - Copy `src` with same name

## Output Example

### Command

```bash
endpoint a-la-carte -c config.json
```

### Console Output

```
=== ALaCarte ===

=== Configuration ===

Output Directory: ./output
Output Type: DotNetSolution
Solution Name: MyProject.sln
Repositories: 2

  Repository: https://github.com/org/repo1
    Branch: main
    Folder Mappings:
      src/lib -> libs/repo1-lib
      src/core -> core

  Repository: https://github.com/org/repo2
    Branch: develop
    Folder Mappings:
      packages/utils -> libs/utils

Processing ALaCarte request...

=== Results ===

Output Directory: ./output
Solution Created: ./output/MyProject.sln
Projects Found: 5
  - libs/repo1-lib/Repo1.Lib.csproj
  - core/Repo1.Core.csproj
  - libs/utils/Utils.csproj
  - ...

ALaCarte operation completed successfully.
```

## Common Use Cases

### 1. Create Custom Library Collection

Extract useful libraries from multiple repos:

```json
{
  "outputType": "DotNetSolution",
  "solutionName": "MyUtilities.sln",
  "repositories": [
    {
      "url": "https://github.com/dotnet/runtime",
      "folders": [{ "from": "src/libraries/System.Text.Json", "to": "Json" }]
    }
  ]
}
```

### 2. Compose Microservices Template

```json
{
  "repositories": [
    {
      "url": "https://github.com/company/shared-libs",
      "folders": [{ "from": "src/common", "to": "src/Common" }]
    },
    {
      "url": "https://github.com/company/templates",
      "folders": [{ "from": "api-template", "to": "src/Api" }]
    }
  ]
}
```

### 3. Extract Documentation

```bash
endpoint a-la-carte -u https://github.com/org/project -f "docs:documentation" -o NotSpecified
```

## Best Practices

1. **Pin Branches**: Specify exact branches for reproducibility
2. **Document Config**: Keep config files in version control
3. **Verify Licenses**: Ensure extracted code licenses permit usage
4. **Test After**: Verify extracted projects build correctly

## Related Commands

- [solution-create](../project-management/solution-create.user-guide.md) - Create new solutions
- [project-add](../project-management/project-add.user-guide.md) - Add projects

[Back to Special Commands](./index.md) | [Back to Index](../index.md)
