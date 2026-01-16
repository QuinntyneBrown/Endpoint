# ALaCarte Demo

This demo showcases the A La Carte functionality of the Endpoint tool, which allows you to clone git repositories, extract selected folders, and create a custom workspace.

## What This Demo Does

The demo demonstrates creating a custom workspace by combining code from multiple repositories:

1. **Commitments Repository** (https://github.com/QuinntyneBrown/Commitments, branch: master)
   - Extracts `src/Commitments.App/projects/commitments-app` to `src/ALaCarteWorkspace/projects`
   - Extracts `src/Commitments.Core` to `src/Commitments.Core`

2. **AWSSDK.Extensions Repository** (https://github.com/QuinntyneBrown/AWSSDK.Extensions, branch: main)
   - Extracts `AWSSDK.Extensions` folder to `AWSSDK.Extensions`

The output is a mixed .NET solution with Angular workspaces and other folders combined into a single workspace.

## How to Run

### Option 1: Using the Console App (Programmatic)

The console app demonstrates using the ALaCarte service programmatically:

```bash
cd playground/ALaCarteDemo/ALaCarteDemo
dotnet run
```

This will:
- Use the IALaCarteService directly through dependency injection
- Create the workspace in the `generated-output` folder
- Display detailed logging of the process

### Option 2: Using the Endpoint CLI Tool

You can also use the `endpoint` CLI tool directly:

1. Build and install the endpoint tool locally:
```bash
cd src/Endpoint.Engineering.Cli
dotnet pack
dotnet tool install --global --add-source ./nupkg Quinntyne.Endpoint.Engineering.Cli
```

2. Run the a-la-carte command with the config file:
```bash
cd playground/ALaCarteDemo/ALaCarteDemo
endpoint a-la-carte --config alacarte-config.json --verbose
```

Or use command-line options directly:
```bash
endpoint a-la-carte \
  --url https://github.com/QuinntyneBrown/Commitments \
  --branch master \
  --folders "src/Commitments.Core:src/Commitments.Core" \
  --output-type MixDotNetSolutionWithOtherFolders \
  --solution-name MyWorkspace.sln \
  --directory ./my-output \
  --verbose
```

## Generated Output

After running the demo, you'll find in the `generated-output` folder:

- **ALaCarteWorkspace.sln**: A .NET solution file containing discovered C# projects
- **src/Commitments.Core/**: The Commitments Core library (.NET project)
- **src/ALaCarteWorkspace/projects/**: Angular workspace with the commitments-app
- **AWSSDK.Extensions/**: AWS SDK Extensions library

## Configuration File

The `alacarte-config.json` file contains the configuration for the repositories and folder mappings. You can modify this file to customize:

- Repository URLs and branches
- Source and destination folder paths
- Output type (DotNetSolution, MixDotNetSolutionWithOtherFolders, NotSpecified)
- Solution name

## Output Types

- **DotNetSolution**: Creates a .NET solution with only C# projects
- **MixDotNetSolutionWithOtherFolders**: Creates a .NET solution with C# projects plus other folders (like Angular workspaces)
- **NotSpecified**: Just copies folders without creating a solution file

## Use Cases

The A La Carte feature is useful for:

1. Creating custom workspaces from multiple repositories
2. Building demo projects with specific features from different sources
3. Extracting and combining libraries from various projects
4. Creating template projects with pre-selected components
5. Building full-stack applications by combining backend and frontend code from different repos
