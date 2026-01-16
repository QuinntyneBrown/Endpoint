# take

Take a folder from a GitHub or GitLab repository and copy it to your local directory.

## Synopsis

```bash
endpoint take -u <url> [-d <directory>] [-s <solution-name>]
```

## Description

The `take` command clones a specific folder from a GitHub or GitLab repository. Simply provide the full URL to the folder you want to copy, and the command will:

1. Parse the URL to extract the repository URL, branch, and folder path
2. Clone the repository
3. Copy the specified folder to your target directory
4. Automatically create/update a .NET solution if .csproj files are found
5. Automatically create/update an Angular workspace if Angular projects are found

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--url` | `-u` | Full GitHub or GitLab URL to the folder | Yes | - |
| `--directory` | `-d` | Target directory where the folder will be copied | No | Current directory |
| `--solution` | `-s` | Name of the .NET solution to create/update | No | Auto-generated from folder name |

## URL Format

The command supports GitHub and GitLab URLs in the following formats:

### GitHub
```
https://github.com/owner/repo/tree/branch/path/to/folder
```

### GitLab
```
https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
```

### Self-Hosted GitLab
```
https://gitlab.example.com/owner/repo/-/tree/branch/path/to/folder
```

The URL is automatically parsed to extract:
- **Repository URL**: The base git repository URL
- **Branch**: The git branch to clone from
- **Folder Path**: The path within the repository to the folder you want

## Examples

### Take a specific folder from GitHub

```bash
endpoint take -u https://github.com/dotnet/aspnetcore/tree/main/src/Mvc
```

This will:
1. Clone the aspnetcore repository (main branch)
2. Copy the `src/Mvc` folder to your current directory
3. Create a solution if .csproj files are found

### Take from a feature branch

```bash
endpoint take -u https://github.com/company/repo/tree/feature/new-api/src/Services
```

This handles branch names with slashes (e.g., `feature/new-api`).

### Specify output directory

```bash
endpoint take -u https://github.com/owner/repo/tree/main/libs/core -d ./my-libs
```

The folder will be copied to `./my-libs/core`.

### Specify solution name

```bash
endpoint take -u https://github.com/owner/repo/tree/main/src/Project -s MyCustomSolution
```

Creates or updates a solution named `MyCustomSolution.sln`.

### Take from GitLab

```bash
endpoint take -u https://gitlab.com/company/project/-/tree/develop/packages/utils
```

Works the same way with GitLab URLs.

## Output

The command provides detailed logging:

```
Parsed URL - Repository: https://github.com/owner/repo, Branch: main, Folder: src/Mvc
Taking folder 'src/Mvc' from repository 'https://github.com/owner/repo' (branch: main)
Cloning repository: https://github.com/owner/repo (branch: main)
Copying folder: src/Mvc -> ./Mvc
Detected .NET project with 3 .csproj file(s)
Created new solution: ./Mvc.sln
Added project to solution: Mvc/Mvc.csproj
Successfully copied folder to: /current/directory
```

## Use Cases

### 1. Extract a Library for Reuse

Copy a useful library from another project:

```bash
endpoint take -u https://github.com/company/shared/tree/main/src/Common
```

### 2. Start a New Project from Template

Use a folder as a project template:

```bash
endpoint take -u https://github.com/templates/api/tree/main/api-template -d ./my-new-api
```

### 3. Study Code Samples

Grab example code to learn from:

```bash
endpoint take -u https://github.com/dotnet/samples/tree/main/core/getting-started
```

### 4. Copy Microservice Components

Extract a specific microservice:

```bash
endpoint take -u https://github.com/company/services/tree/main/services/auth-service
```

## Project Type Detection

The command automatically detects project types and handles them appropriately:

### .NET Projects
- Searches for .csproj files
- Creates or updates a .NET solution
- Adds all found projects to the solution
- Sanitizes project files to remove repository-specific references

### Angular Projects
- Detects angular.json or ng-package.json files
- Creates Angular workspace if needed
- Handles orphan Angular library projects

## Notes

- The command uses shallow cloning (depth 1) for efficiency
- Temporary clone directories are automatically cleaned up
- Binary folders (bin, obj, node_modules) are excluded from the copy
- The `.git` directory is not copied

## Best Practices

1. **Review licenses**: Always check that you have permission to use the code
2. **Pin branches**: Use specific branch names (not just 'main') for reproducibility
3. **Clean up**: The copied folder may need adjustments (package versions, namespaces, etc.)
4. **Test builds**: Always build and test after taking code to ensure it works in your environment

## Common Errors

### Invalid URL Format

```
Invalid URL format. The URL must be a GitHub or GitLab URL to a folder.
GitHub example: https://github.com/owner/repo/tree/branch/path/to/folder
GitLab example: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
```

**Solution**: Make sure you're using the full URL format with `/tree/` or `/-/tree/`.

### Source Folder Not Found

```
Source folder not found in repository: src/NonExistent
```

**Solution**: Verify the folder path exists in the specified branch of the repository.

### Git Clone Failed

```
Failed to clone repository: https://github.com/owner/private-repo
```

**Solution**: Ensure you have access to the repository and that Git credentials are configured.

## Related Commands

- [a-la-carte](./a-la-carte.user-guide.md) - Take multiple folders from multiple repositories
- [solution-create](../project-management/solution-create.user-guide.md) - Create .NET solutions
- [project-add](../project-management/project-add.user-guide.md) - Add projects to solutions

[Back to Special Commands](./index.md) | [Back to Index](../index.md)
