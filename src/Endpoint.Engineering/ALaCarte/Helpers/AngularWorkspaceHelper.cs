// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Helpers;

/// <summary>
/// Helper class for managing Angular workspaces and handling orphan Angular projects.
/// </summary>
public class AngularWorkspaceHelper
{
    private readonly ILogger _logger;

    public AngularWorkspaceHelper(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Finds orphan Angular library projects (folders with ng-package.json not referenced by any angular.json).
    /// </summary>
    /// <param name="directory">The root directory to search.</param>
    /// <returns>List of paths to orphan Angular library projects.</returns>
    public List<string> FindOrphanAngularProjects(string directory)
    {
        var orphanProjects = new List<string>();

        // Find all ng-package.json files (Angular library projects)
        var ngPackageFiles = Directory.GetFiles(directory, "ng-package.json", SearchOption.AllDirectories);

        // Find all angular.json files (Angular workspaces)
        var angularJsonFiles = Directory.GetFiles(directory, "angular.json", SearchOption.AllDirectories);

        // Get all project paths referenced by angular.json files
        var referencedProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var angularJsonPath in angularJsonFiles)
        {
            var projectPaths = GetProjectPathsFromAngularJson(angularJsonPath);
            foreach (var projectPath in projectPaths)
            {
                referencedProjects.Add(Path.GetFullPath(projectPath));
            }
        }

        // Check each ng-package.json to see if it's orphaned
        foreach (var ngPackagePath in ngPackageFiles)
        {
            var projectDir = Path.GetDirectoryName(ngPackagePath);
            if (projectDir == null) continue;

            var isReferenced = IsProjectReferenced(projectDir, referencedProjects, angularJsonFiles);
            if (!isReferenced)
            {
                _logger.LogInformation("Found orphan Angular project: {Path}", projectDir);
                orphanProjects.Add(projectDir);
            }
        }

        return orphanProjects;
    }

    /// <summary>
    /// Creates an Angular workspace for orphan projects.
    /// </summary>
    /// <param name="orphanProjectPath">Path to the orphan Angular library project.</param>
    /// <param name="customRoot">Optional custom root path for the angular.json configuration.
    /// When specified, this value is used as the "root" property and the angular.json is placed
    /// at the appropriate parent directory level to make this path valid.</param>
    /// <returns>Path to the created angular.json file, or null if creation failed.</returns>
    public string? CreateWorkspaceForOrphanProject(string orphanProjectPath, string? customRoot = null)
    {
        try
        {
            string parentDir;
            string relativePath;

            if (!string.IsNullOrEmpty(customRoot))
            {
                // When custom root is provided, calculate the workspace directory
                // by going up the path by the number of segments in customRoot
                var rootSegments = customRoot.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                parentDir = orphanProjectPath;

                // Go up the directory tree by the number of segments in customRoot
                for (int i = 0; i < rootSegments.Length; i++)
                {
                    var parent = Path.GetDirectoryName(parentDir);
                    if (parent == null)
                    {
                        _logger.LogError("Cannot determine workspace directory for custom root '{Root}' from: {Path}", customRoot, orphanProjectPath);
                        return null;
                    }
                    parentDir = parent;
                }

                // Use the custom root as the relative path
                relativePath = customRoot.Replace("\\", "/");
            }
            else
            {
                // Determine the best location for the workspace
                // Create it in the parent directory of the orphan project
                parentDir = Path.GetDirectoryName(orphanProjectPath)!;
                if (parentDir == null)
                {
                    _logger.LogError("Cannot determine parent directory for: {Path}", orphanProjectPath);
                    return null;
                }

                // Calculate relative path from workspace to project
                relativePath = Path.GetRelativePath(parentDir, orphanProjectPath).Replace("\\", "/");
            }

            var angularJsonPath = Path.Combine(parentDir, "angular.json");

            // If angular.json already exists in parent, add the project to it
            if (File.Exists(angularJsonPath))
            {
                AddProjectToExistingWorkspace(angularJsonPath, orphanProjectPath, customRoot);
                return angularJsonPath;
            }

            // Read ng-package.json to get library name
            var ngPackagePath = Path.Combine(orphanProjectPath, "ng-package.json");
            var libraryName = GetLibraryName(ngPackagePath, orphanProjectPath);

            // Create angular.json
            var angularJson = CreateAngularJson(libraryName, relativePath);
            File.WriteAllText(angularJsonPath, angularJson);

            // Create package.json if it doesn't exist
            var packageJsonPath = Path.Combine(parentDir, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                var packageJson = CreatePackageJson(libraryName);
                File.WriteAllText(packageJsonPath, packageJson);
            }

            _logger.LogInformation("Created Angular workspace at: {Path}", angularJsonPath);
            return angularJsonPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workspace for orphan project: {Path}", orphanProjectPath);
            return null;
        }
    }

    private bool IsProjectReferenced(string projectDir, HashSet<string> referencedProjects, string[] angularJsonFiles)
    {
        var projectDirFullPath = Path.GetFullPath(projectDir);

        // Check direct references
        if (referencedProjects.Contains(projectDirFullPath))
        {
            return true;
        }

        // Check if any angular.json is in a parent directory
        foreach (var angularJsonPath in angularJsonFiles)
        {
            var workspaceDir = Path.GetDirectoryName(angularJsonPath);
            if (workspaceDir != null && projectDirFullPath.StartsWith(workspaceDir, StringComparison.OrdinalIgnoreCase))
            {
                // Check if this workspace references the project
                var projectPaths = GetProjectPathsFromAngularJson(angularJsonPath);
                foreach (var projectPath in projectPaths)
                {
                    if (Path.GetFullPath(projectPath).Equals(projectDirFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private List<string> GetProjectPathsFromAngularJson(string angularJsonPath)
    {
        var projectPaths = new List<string>();

        try
        {
            var content = File.ReadAllText(angularJsonPath);
            var jsonNode = JsonNode.Parse(content);
            var projects = jsonNode?["projects"]?.AsObject();

            if (projects == null) return projectPaths;

            var workspaceDir = Path.GetDirectoryName(angularJsonPath) ?? "";

            foreach (var project in projects)
            {
                var projectConfig = project.Value;
                var root = projectConfig?["root"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(root))
                {
                    var fullPath = Path.Combine(workspaceDir, root);
                    projectPaths.Add(fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing angular.json: {Path}", angularJsonPath);
        }

        return projectPaths;
    }

    private void AddProjectToExistingWorkspace(string angularJsonPath, string projectPath, string? customRoot = null)
    {
        try
        {
            var content = File.ReadAllText(angularJsonPath);
            var jsonNode = JsonNode.Parse(content);

            if (jsonNode == null) return;

            var projects = jsonNode["projects"]?.AsObject();
            if (projects == null)
            {
                projects = new JsonObject();
                jsonNode["projects"] = projects;
            }

            var workspaceDir = Path.GetDirectoryName(angularJsonPath) ?? "";
            var relativePath = !string.IsNullOrEmpty(customRoot)
                ? customRoot.Replace("\\", "/")
                : Path.GetRelativePath(workspaceDir, projectPath).Replace("\\", "/");

            var ngPackagePath = Path.Combine(projectPath, "ng-package.json");
            var libraryName = GetLibraryName(ngPackagePath, projectPath);

            // Check if project already exists
            if (projects.ContainsKey(libraryName))
            {
                _logger.LogInformation("Project {Name} already exists in workspace", libraryName);
                return;
            }

            var projectConfig = CreateLibraryProjectConfig(relativePath);
            projects.Add(libraryName, JsonNode.Parse(projectConfig));

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(angularJsonPath, jsonNode.ToJsonString(options));

            _logger.LogInformation("Added project {Name} to existing workspace: {Path}", libraryName, angularJsonPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding project to existing workspace: {Path}", angularJsonPath);
        }
    }

    private string GetLibraryName(string ngPackagePath, string projectPath)
    {
        try
        {
            if (File.Exists(ngPackagePath))
            {
                var content = File.ReadAllText(ngPackagePath);
                var jsonNode = JsonNode.Parse(content);
                var libNode = jsonNode?["lib"];

                if (libNode != null)
                {
                    var entryFile = libNode["entryFile"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(entryFile))
                    {
                        // Try to extract name from entry file path
                        var name = Path.GetFileNameWithoutExtension(entryFile);
                        if (name != "public-api" && name != "index")
                        {
                            return name;
                        }
                    }
                }
            }

            // Check for package.json in the project directory
            var packageJsonPath = Path.Combine(projectPath, "package.json");
            if (File.Exists(packageJsonPath))
            {
                var content = File.ReadAllText(packageJsonPath);
                var jsonNode = JsonNode.Parse(content);
                var name = jsonNode?["name"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(name))
                {
                    return name.Replace("@", "").Replace("/", "-");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading library name from: {Path}", ngPackagePath);
        }

        // Fall back to directory name
        return Path.GetFileName(projectPath) ?? "library";
    }

    private string CreateAngularJson(string projectName, string projectPath)
    {
        var projectConfig = CreateLibraryProjectConfig(projectPath);

        return $$"""
{
  "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
  "version": 1,
  "newProjectRoot": "projects",
  "projects": {
    "{{projectName}}": {{projectConfig}}
  }
}
""";
    }

    private string CreateLibraryProjectConfig(string projectPath)
    {
        return $$"""
{
      "projectType": "library",
      "root": "{{projectPath}}",
      "sourceRoot": "{{projectPath}}/src",
      "prefix": "lib",
      "architect": {
        "build": {
          "builder": "@angular-devkit/build-angular:ng-packagr",
          "options": {
            "project": "{{projectPath}}/ng-package.json"
          },
          "configurations": {
            "production": {
              "tsConfig": "{{projectPath}}/tsconfig.lib.prod.json"
            },
            "development": {
              "tsConfig": "{{projectPath}}/tsconfig.lib.json"
            }
          },
          "defaultConfiguration": "production"
        }
      }
    }
""";
    }

    private string CreatePackageJson(string projectName)
    {
        return $$"""
{
  "name": "{{projectName}}-workspace",
  "version": "0.0.0",
  "scripts": {
    "build": "ng build"
  },
  "private": true,
  "dependencies": {
    "@angular/common": "^17.0.0",
    "@angular/core": "^17.0.0"
  },
  "devDependencies": {
    "@angular-devkit/build-angular": "^17.0.0",
    "@angular/cli": "^17.0.0",
    "@angular/compiler-cli": "^17.0.0",
    "ng-packagr": "^17.0.0",
    "typescript": "~5.2.2"
  }
}
""";
    }
}
