# take

Take a folder from a git/gitlab repository and copy it to a target directory.

## Synopsis

```bash
endpoint take [options]
```

## Description

The `take` command clones a git repository and extracts a specific folder from it to a target directory. It's designed for quickly grabbing a single project, library, or component from a repository without cloning the entire repo. 

The command automatically detects project types:
- **\.NET Projects**: If the folder contains `.csproj` files, it will create or update a `.sln` solution file
- **Angular Projects**: If the folder contains `angular.json` or `ng-package.json`, it will create or update an Angular workspace

This is a simplified version of the `a-la-carte` command, focused on taking a single folder from one repository.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--url` | `-u` | Git/GitLab repository URL | No* | - |
| `--branch` | `-b` | Branch to clone from | No | `main` |
| `--from` | `-f` | Path to the folder in the repository to copy | No* | - |
| `--directory` | `-d` | Target directory for output | No | Current directory |
| `--solution` | `-s` | Name of the .NET solution to create/update | No | Folder name |

\* If no options are provided, the command will enter **interactive mode** where you can provide a JSON configuration.

## Usage Modes

### Command-line Mode

Provide options directly on the command line:

```bash
endpoint take -u <repo-url> -b <branch> -f <folder-path> [options]
```

### Interactive Mode

Run without parameters to enter interactive JSON mode:

```bash
endpoint take
```

You'll be prompted to provide a JSON configuration:

```json
{
  "url": "https://github.com/username/repository",
  "branch": "main",
  "fromPath": "src/ProjectFolder",
  "directory": "./output",
  "solutionName": "MySolution"
}
```

## Examples

### Take a .NET project

```bash
endpoint take -u https://github.com/user/repo -b main -f src/MyProject
```

Creates:
- Copies `src/MyProject` folder to current directory
- Detects `.csproj` files
- Creates a solution file automatically

### Take with custom solution name

```bash
endpoint take -u https://github.com/user/repo -f src/MyLib -s CustomSolution
```

Creates:
- Copies folder
- Creates `CustomSolution.sln` and adds all projects

### Take to specific directory

```bash
endpoint take -u https://github.com/user/repo -f src/Core -d ./libs/core
```

Copies the `src/Core` folder to `./libs/core`

### Take from a specific branch

```bash
endpoint take -u https://github.com/user/repo -b develop -f src/Feature
```

Takes the folder from the `develop` branch

### Take an Angular project

```bash
endpoint take -u https://github.com/user/repo -f projects/my-angular-lib
```

If the folder contains `angular.json` or `ng-package.json`, it will:
- Copy the Angular project
- Create/update Angular workspace configuration

### Interactive mode with JSON

```bash
endpoint take
```

Then provide JSON configuration:

```json
{
  "url": "https://github.com/QuinntyneBrown/AWSSDK.Extensions",
  "branch": "main",
  "fromPath": "src/AWSSDK.Extensions",
  "solutionName": "MyAwsSdk"
}
```

## Output Example

### Command

```bash
endpoint take -u https://github.com/user/repo -b main -f src/MyProject -s MySolution
```

### Console Output

```
Taking folder 'src/MyProject' from repository 'https://github.com/user/repo' (branch: main)
Successfully copied folder to: /path/to/current/directory/MyProject
Solution created/updated: /path/to/current/directory/MySolution.sln
Found .csproj: MyProject/MyProject.csproj
```

## Project Type Detection

The `take` command automatically detects and handles different project types:

### .NET Projects

When `.csproj` files are detected:
- Creates or updates a `.sln` solution file
- Adds all discovered projects to the solution
- Logs each `.csproj` file found

### Angular Projects

When `angular.json` or `ng-package.json` is detected:
- Creates or updates Angular workspace configuration
- Preserves Angular project structure
- Logs workspace path

## Common Use Cases

### 1. Extract a Library from a Monorepo

```bash
endpoint take -u https://github.com/company/monorepo -f packages/shared-lib -d ./libs
```

### 2. Grab a Template Project

```bash
endpoint take -u https://github.com/templates/dotnet -f templates/api-template -s MyApi
```

### 3. Copy a Component or Module

```bash
endpoint take -u https://github.com/user/repo -f src/components/DataGrid
```

### 4. Extract Sample Code

```bash
endpoint take -u https://github.com/examples/repo -b samples -f examples/authentication
```

### 5. Get a Specific Project Version

```bash
endpoint take -u https://github.com/user/repo -b v2.0 -f src/LegacyProject
```

## Best Practices

1. **Specify Branch**: Always specify the branch to ensure consistency
2. **Verify Licenses**: Check that the code you're taking has an appropriate license
3. **Test After Taking**: Verify the extracted project builds correctly
4. **Use Solution Names**: Provide meaningful solution names for .NET projects
5. **Check Dependencies**: Ensure all dependencies are included or available

## Differences from a-la-carte

| Feature | take | a-la-carte |
|---------|------|------------|
| **Repositories** | Single repository | Multiple repositories |
| **Folders** | Single folder | Multiple folders with mappings |
| **Configuration** | Simple options or JSON | Complex JSON configuration |
| **Use Case** | Quick single folder extraction | Complex multi-repo composition |
| **Output Types** | Auto-detected | Configurable (DotNetSolution, etc.) |

Use `take` when you need one folder from one repository. Use `a-la-carte` when composing from multiple repositories or multiple folders.

## Error Handling

Common errors and solutions:

### Repository URL Required

```
Repository URL is required.
```

**Solution**: Provide the `-u` option or enter interactive mode with valid JSON.

### From Path Required

```
From path is required.
```

**Solution**: Provide the `-f` option with the path to the folder in the repository.

### Path Not Found

```
Error: Failed to take folder from repository.
Error: Path 'src/NonExistent' not found in repository
```

**Solution**: Verify the folder path exists in the specified branch of the repository.

### Invalid Repository URL

```
Error: Failed to take folder from repository.
Error: Unable to clone repository
```

**Solution**: Check that the URL is correct and you have access to the repository.

## Related Commands

- [a-la-carte](./a-la-carte.user-guide.md) - Clone and extract from multiple repositories
- [solution-create](../project-management/solution-create.user-guide.md) - Create new .NET solutions
- [project-add](../project-management/project-add.user-guide.md) - Add projects to solutions

## See Also

For more complex scenarios involving multiple repositories and advanced folder mappings, see the [a-la-carte command](./a-la-carte.user-guide.md).

[Back to Special Commands](./index.md) | [Back to Index](../index.md)
