# Endpoint.Engineering.CodeReview.Cli

A command-line tool for performing git diff analysis between branches.

## Features

- Compare any branch with the repository's default branch
- Support for public GitHub, GitLab, and git repositories
- Save diffs to text files
- Leverage Microsoft Extensions for DI, Logging, and Configuration
- Built with System.CommandLine

## Limitations

- Currently supports only public repositories or repositories with cached credentials
- For private repositories, ensure git credentials are cached or use SSH keys
- The tool clones repositories to a temporary directory, which may take time for large repositories

## Installation

Install as a global .NET tool:

```bash
dotnet tool install --global Quinntyne.Endpoint.Engineering.CodeReview.Cli
```

## Usage

### Basic Usage

```bash
endpoint-code-review diff --url <repository-url> --branch <branch-name>
```

### Options

- `-u, --url` (Required): The URL of the git repository (GitHub, GitLab, or any git URL)
- `-b, --branch` (Required): The name of the branch to compare against the default branch
- `-o, --output` (Optional): The output file name (defaults to `diff.txt`)

### Examples

Compare a feature branch with the default branch:

```bash
endpoint-code-review diff --url https://github.com/QuinntyneBrown/Endpoint --branch feature-branch
```

Save diff to a custom file:

```bash
endpoint-code-review diff --url https://github.com/QuinntyneBrown/Endpoint --branch feature-branch --output my-changes.txt
```

## Development

### Build

```bash
dotnet build src/Endpoint.Engineering.CodeReview.Cli/Endpoint.Engineering.CodeReview.Cli.csproj
```

### Test

```bash
dotnet test tests/Endpoint.Engineering.CodeReview.Cli.UnitTests/Endpoint.Engineering.CodeReview.Cli.UnitTests.csproj
```

### Package

```bash
dotnet pack src/Endpoint.Engineering.CodeReview.Cli/Endpoint.Engineering.CodeReview.Cli.csproj
```

## Architecture

The CLI is built using:
- **System.CommandLine**: For command-line parsing and handling
- **Microsoft.Extensions.DependencyInjection**: For dependency injection
- **Microsoft.Extensions.Logging**: For structured logging
- **Microsoft.Extensions.Hosting**: For application lifecycle management
- **LibGit2Sharp**: For git operations
- **Serilog**: For logging implementation

## License

MIT License - See LICENSE.txt for details
