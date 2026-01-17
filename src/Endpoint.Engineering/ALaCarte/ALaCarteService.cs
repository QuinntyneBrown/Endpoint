// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text;
using Endpoint.Engineering.ALaCarte.Helpers;
using Endpoint.Engineering.ALaCarte.Models;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte;

/// <summary>
/// Service for cloning git repositories, extracting select folders,
/// and creating a new folder structure based on mapping configuration.
/// </summary>
public class ALaCarteService : IALaCarteService
{
    private readonly ILogger<ALaCarteService> _logger;

    public ALaCarteService(ILogger<ALaCarteService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ALaCarteResult> ProcessAsync(ALaCarteRequest request, CancellationToken cancellationToken = default)
    {
        var result = new ALaCarteResult
        {
            OutputDirectory = request.Directory
        };

        // Ensure output directory exists
        if (!Directory.Exists(request.Directory))
        {
            Directory.CreateDirectory(request.Directory);
        }

        // Create a temporary directory for cloning
        var tempDir = Path.Combine(Path.GetTempPath(), $"alacarte_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        _logger.LogInformation("Created temporary directory: {TempDir}", tempDir);

        try
        {
            // Process each repository
            foreach (var repo in request.Repositories)
            {
                await ProcessRepositoryAsync(repo, tempDir, request.Directory, result, cancellationToken);
            }

            // Sanitize all .csproj files
            SanitizeAllCsprojFiles(request.Directory, result);

            // Create .NET solution if required
            if (request.OutputType == OutputType.DotNetSolution ||
                request.OutputType == OutputType.MixDotNetSolutionWithOtherFolders)
            {
                await CreateDotNetSolutionAsync(request.Directory, request.SolutionName, result, cancellationToken);
            }

            // Handle orphan Angular projects
            HandleOrphanAngularProjects(request.Directory, result);

            result.Success = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ALaCarte request");
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.Success = false;
        }
        finally
        {
            // Clean up temporary directory
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                    _logger.LogInformation("Deleted temporary directory: {TempDir}", tempDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary directory: {TempDir}", tempDir);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ALaCarteTakeResult> TakeAsync(ALaCarteTakeRequest request, CancellationToken cancellationToken = default)
    {
        var result = new ALaCarteTakeResult
        {
            OutputDirectory = request.Directory
        };

        // Ensure output directory exists
        if (!Directory.Exists(request.Directory))
        {
            Directory.CreateDirectory(request.Directory);
        }

        // Check if FromDirectory is provided (local directory copy mode)
        if (!string.IsNullOrEmpty(request.FromDirectory))
        {
            return await TakeFromLocalDirectoryAsync(request, result, cancellationToken);
        }

        // Otherwise, proceed with Git clone mode
        return await TakeFromGitRepositoryAsync(request, result, cancellationToken);
    }

    private async Task<ALaCarteTakeResult> TakeFromLocalDirectoryAsync(
        ALaCarteTakeRequest request,
        ALaCarteTakeResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Directory.Exists(request.FromDirectory))
            {
                result.Errors.Add($"FromDirectory does not exist: {request.FromDirectory}");
                result.Success = false;
                return result;
            }

            _logger.LogInformation("Copying from local directory: {FromDirectory}", request.FromDirectory);

            // Determine the source path
            string sourcePath;
            if (!string.IsNullOrEmpty(request.FromPath))
            {
                sourcePath = Path.Combine(request.FromDirectory, request.FromPath.TrimStart('/', '\\'));
                if (!Directory.Exists(sourcePath))
                {
                    result.Errors.Add($"Source folder not found in FromDirectory: {request.FromPath}");
                    result.Success = false;
                    return result;
                }
            }
            else
            {
                sourcePath = request.FromDirectory;
            }

            // Determine the destination folder name (use the folder name from the source)
            var folderName = Path.GetFileName(sourcePath.TrimEnd('/', '\\'));
            var destPath = Path.Combine(request.Directory, folderName);

            _logger.LogInformation("Copying folder: {From} -> {To}", sourcePath, destPath);

            // Copy the folder
            CopyDirectoryForTake(sourcePath, destPath, result);
            result.CopiedFolderPath = destPath;

            // Detect project type and handle accordingly
            await DetectAndHandleProjectType(request.Directory, destPath, request.SolutionName, request.Root, result, cancellationToken);

            // Sanitize .csproj files if any were found
            if (result.CsprojFiles.Count > 0)
            {
                SanitizeCsprojFilesForTake(request.Directory, result);
            }

            result.Success = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Take request from local directory");
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.Success = false;
        }

        return result;
    }

    private async Task<ALaCarteTakeResult> TakeFromGitRepositoryAsync(
        ALaCarteTakeRequest request,
        ALaCarteTakeResult result,
        CancellationToken cancellationToken)
    {
        // Create a temporary directory for cloning
        var tempDir = Path.Combine(Path.GetTempPath(), $"alacarte_take_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        _logger.LogInformation("Created temporary directory for Take operation: {TempDir}", tempDir);

        try
        {
            // Clone the repository
            var repoName = GetRepositoryName(request.Url);
            var cloneDir = Path.Combine(tempDir, repoName);

            _logger.LogInformation("Cloning repository: {Url} (branch: {Branch})", request.Url, request.Branch);

            var cloneSuccess = await CloneRepositoryAsync(request.Url, request.Branch, cloneDir, cancellationToken);
            if (!cloneSuccess)
            {
                result.Errors.Add($"Failed to clone repository: {request.Url}");
                result.Success = false;
                return result;
            }

            // Locate the source folder
            var sourcePath = Path.Combine(cloneDir, request.FromPath.TrimStart('/', '\\'));

            if (!Directory.Exists(sourcePath))
            {
                result.Errors.Add($"Source folder not found in repository: {request.FromPath}");
                result.Success = false;
                return result;
            }

            // Determine the destination folder name (use the folder name from the source)
            var folderName = Path.GetFileName(sourcePath.TrimEnd('/', '\\'));
            var destPath = Path.Combine(request.Directory, folderName);

            _logger.LogInformation("Copying folder: {From} -> {To}", request.FromPath, destPath);

            // Copy the folder
            CopyDirectoryForTake(sourcePath, destPath, result);
            result.CopiedFolderPath = destPath;

            // Detect project type and handle accordingly
            await DetectAndHandleProjectType(request.Directory, destPath, request.SolutionName, request.Root, result, cancellationToken);

            // Sanitize .csproj files if any were found
            if (result.CsprojFiles.Count > 0)
            {
                SanitizeCsprojFilesForTake(request.Directory, result);
            }

            result.Success = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Take request");
            result.Errors.Add($"Unexpected error: {ex.Message}");
            result.Success = false;
        }
        finally
        {
            // Clean up temporary directory
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                    _logger.LogInformation("Deleted temporary directory: {TempDir}", tempDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary directory: {TempDir}", tempDir);
            }
        }

        return result;
    }

    private void CopyDirectoryForTake(string sourceDir, string destDir, ALaCarteTakeResult result)
    {
        // Create destination directory
        Directory.CreateDirectory(destDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, overwrite: true);

            // Track .csproj files
            if (file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                result.CsprojFiles.Add(destFile);
            }
        }

        // Recursively copy subdirectories
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);

            // Skip .git, node_modules, bin, obj directories
            if (dirName.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("node_modules", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("obj", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var destSubDir = Path.Combine(destDir, dirName);
            CopyDirectoryForTake(subDir, destSubDir, result);
        }
    }

    private async Task DetectAndHandleProjectType(
        string outputDir,
        string copiedFolderPath,
        string? solutionName,
        string? angularRoot,
        ALaCarteTakeResult result,
        CancellationToken cancellationToken)
    {
        // Check for .csproj files (C# project)
        var csprojFiles = Directory.GetFiles(copiedFolderPath, "*.csproj", SearchOption.AllDirectories);
        if (csprojFiles.Length > 0)
        {
            result.IsDotNetProject = true;
            _logger.LogInformation("Detected .NET project with {Count} .csproj file(s)", csprojFiles.Length);

            // Add any .csproj files found in subdirectories
            foreach (var csproj in csprojFiles)
            {
                if (!result.CsprojFiles.Contains(csproj))
                {
                    result.CsprojFiles.Add(csproj);
                }
            }

            // Create or update solution
            await CreateOrUpdateSolutionForTake(outputDir, solutionName, result, cancellationToken);
        }

        // Check for Angular project (angular.json or ng-package.json)
        var hasAngularJson = File.Exists(Path.Combine(copiedFolderPath, "angular.json"));
        var hasNgPackageJson = File.Exists(Path.Combine(copiedFolderPath, "ng-package.json"));
        var ngPackageFiles = Directory.GetFiles(copiedFolderPath, "ng-package.json", SearchOption.AllDirectories);

        if (hasAngularJson || hasNgPackageJson || ngPackageFiles.Length > 0)
        {
            result.IsAngularProject = true;
            _logger.LogInformation("Detected Angular project");

            // Handle Angular workspace
            await HandleAngularProjectForTake(outputDir, copiedFolderPath, result, angularRoot);
        }
    }

    private async Task CreateOrUpdateSolutionForTake(
        string directory,
        string? solutionName,
        ALaCarteTakeResult result,
        CancellationToken cancellationToken)
    {
        if (result.CsprojFiles.Count == 0)
        {
            _logger.LogInformation("No .csproj files found, skipping solution creation");
            return;
        }

        // Determine solution name
        var effectiveSolutionName = solutionName ?? Path.GetFileName(result.CopiedFolderPath);
        var solutionPath = Path.Combine(directory, effectiveSolutionName);

        // Ensure solution name ends with .sln
        if (!solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            solutionPath += ".sln";
        }

        try
        {
            // Check if solution already exists
            var solutionExists = File.Exists(solutionPath);

            if (!solutionExists)
            {
                // Create the solution
                var solutionNameWithoutExtension = Path.GetFileNameWithoutExtension(solutionPath);
                var exitCode = await RunDotNetCommandAsync(
                    $"new sln --name \"{solutionNameWithoutExtension}\"",
                    directory,
                    cancellationToken);

                if (exitCode != 0)
                {
                    result.Errors.Add("Failed to create .NET solution");
                    return;
                }

                _logger.LogInformation("Created new solution: {Path}", solutionPath);
            }
            else
            {
                _logger.LogInformation("Updating existing solution: {Path}", solutionPath);
            }

            // Add all .csproj files to the solution
            foreach (var csprojPath in result.CsprojFiles)
            {
                var relativePath = Path.GetRelativePath(directory, csprojPath);
                var exitCode = await RunDotNetCommandAsync(
                    $"sln \"{solutionPath}\" add \"{relativePath}\"",
                    directory,
                    cancellationToken);

                if (exitCode != 0)
                {
                    result.Warnings.Add($"Failed to add project to solution: {relativePath}");
                }
                else
                {
                    _logger.LogInformation("Added project to solution: {Project}", relativePath);
                }
            }

            result.SolutionPath = solutionPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating .NET solution");
            result.Errors.Add($"Failed to create/update .NET solution: {ex.Message}");
        }
    }

    private Task HandleAngularProjectForTake(string outputDir, string copiedFolderPath, ALaCarteTakeResult result, string? angularRoot)
    {
        var helper = new AngularWorkspaceHelper(_logger);

        // Check if the copied folder already has an angular.json
        var angularJsonPath = Path.Combine(copiedFolderPath, "angular.json");
        if (File.Exists(angularJsonPath))
        {
            result.AngularWorkspacePath = angularJsonPath;
            _logger.LogInformation("Angular workspace already exists: {Path}", angularJsonPath);
            return Task.CompletedTask;
        }

        // Find orphan Angular projects and create workspaces for them
        var orphanProjects = helper.FindOrphanAngularProjects(copiedFolderPath);

        foreach (var orphanProject in orphanProjects)
        {
            var workspacePath = helper.CreateWorkspaceForOrphanProject(orphanProject, angularRoot);
            if (workspacePath != null)
            {
                result.AngularWorkspacePath = workspacePath;
                _logger.LogInformation("Created Angular workspace: {Path}", workspacePath);
            }
        }

        // If still no workspace, check if we need to create one at output directory level
        if (result.AngularWorkspacePath == null)
        {
            var ngPackageFiles = Directory.GetFiles(copiedFolderPath, "ng-package.json", SearchOption.AllDirectories);
            if (ngPackageFiles.Length > 0)
            {
                var firstNgPackage = ngPackageFiles[0];
                var projectDir = Path.GetDirectoryName(firstNgPackage);
                if (projectDir != null)
                {
                    var workspacePath = helper.CreateWorkspaceForOrphanProject(projectDir, angularRoot);
                    if (workspacePath != null)
                    {
                        result.AngularWorkspacePath = workspacePath;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private void SanitizeCsprojFilesForTake(string directory, ALaCarteTakeResult result)
    {
        var sanitizer = new CsprojSanitizer(_logger);

        foreach (var csprojPath in result.CsprojFiles)
        {
            if (File.Exists(csprojPath))
            {
                sanitizer.Sanitize(csprojPath);
            }
        }

        _logger.LogInformation("Sanitized {Count} .csproj files", result.CsprojFiles.Count);
    }

    private async Task ProcessRepositoryAsync(
        RepositoryConfiguration repo,
        string tempDir,
        string outputDir,
        ALaCarteResult result,
        CancellationToken cancellationToken)
    {
        string sourceDir;

        // Check if LocalDirectory is specified (local directory copy mode)
        if (!string.IsNullOrEmpty(repo.LocalDirectory))
        {
            // Use local directory as source
            if (!Directory.Exists(repo.LocalDirectory))
            {
                result.Errors.Add($"LocalDirectory does not exist: {repo.LocalDirectory}");
                _logger.LogError("LocalDirectory does not exist: {LocalDirectory}", repo.LocalDirectory);
                return;
            }

            _logger.LogInformation("Copying from local directory: {LocalDirectory}", repo.LocalDirectory);
            sourceDir = repo.LocalDirectory;
        }
        else if (!string.IsNullOrEmpty(repo.Url))
        {
            // Clone from Git repository
            var repoName = GetRepositoryName(repo.Url);
            var cloneDir = Path.Combine(tempDir, repoName);

            _logger.LogInformation("Cloning repository: {Url} (branch: {Branch})", repo.Url, repo.Branch);

            // Clone the repository
            var cloneSuccess = await CloneRepositoryAsync(repo.Url, repo.Branch, cloneDir, cancellationToken);
            if (!cloneSuccess)
            {
                result.Errors.Add($"Failed to clone repository: {repo.Url}");
                return;
            }

            sourceDir = cloneDir;
        }
        else
        {
            result.Errors.Add("Either Url or LocalDirectory must be specified in repository configuration");
            _logger.LogError("Neither Url nor LocalDirectory specified in repository configuration");
            return;
        }

        // Process each folder configuration
        foreach (var folderConfig in repo.Folders)
        {
            ProcessFolderConfiguration(sourceDir, outputDir, folderConfig, result);
        }
    }

    private async Task<bool> CloneRepositoryAsync(
        string url,
        string branch,
        string targetDir,
        CancellationToken cancellationToken)
    {
        try
        {
            var arguments = $"clone --depth 1 --branch {branch} \"{url}\" \"{targetDir}\"";
            var exitCode = await RunGitCommandAsync(arguments, Path.GetTempPath(), cancellationToken);

            if (exitCode != 0)
            {
                _logger.LogError("Git clone failed with exit code {ExitCode}", exitCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning repository: {Url}", url);
            return false;
        }
    }

    private void ProcessFolderConfiguration(
        string cloneDir,
        string outputDir,
        FolderConfiguration folderConfig,
        ALaCarteResult result)
    {
        var sourcePath = Path.Combine(cloneDir, folderConfig.From.TrimStart('/', '\\'));
        var destPath = Path.Combine(outputDir, folderConfig.To.TrimStart('/', '\\'));

        if (!Directory.Exists(sourcePath))
        {
            result.Warnings.Add($"Source folder not found: {folderConfig.From}");
            _logger.LogWarning("Source folder not found: {Path}", sourcePath);
            return;
        }

        _logger.LogInformation("Copying folder: {From} -> {To}", folderConfig.From, folderConfig.To);

        try
        {
            CopyDirectory(sourcePath, destPath, result);

            // Track custom Angular root mappings if specified
            if (!string.IsNullOrEmpty(folderConfig.Root))
            {
                result.AngularRootMappings[Path.GetFullPath(destPath)] = folderConfig.Root;
                _logger.LogInformation("Registered Angular root mapping: {Path} -> {Root}", destPath, folderConfig.Root);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying folder: {From} -> {To}", folderConfig.From, folderConfig.To);
            result.Errors.Add($"Failed to copy folder {folderConfig.From}: {ex.Message}");
        }
    }

    private void CopyDirectory(string sourceDir, string destDir, ALaCarteResult result)
    {
        // Create destination directory
        Directory.CreateDirectory(destDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, overwrite: true);

            // Track .csproj files
            if (file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                result.CsprojFiles.Add(destFile);
            }
        }

        // Recursively copy subdirectories
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);

            // Skip .git directories
            if (dirName.Equals(".git", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var destSubDir = Path.Combine(destDir, dirName);
            CopyDirectory(subDir, destSubDir, result);
        }
    }

    private void SanitizeAllCsprojFiles(string directory, ALaCarteResult result)
    {
        var sanitizer = new CsprojSanitizer(_logger);

        // Find all .csproj files in the output directory
        var csprojFiles = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);

        foreach (var csprojPath in csprojFiles)
        {
            sanitizer.Sanitize(csprojPath);

            // Update the result list with any newly found .csproj files
            if (!result.CsprojFiles.Contains(csprojPath))
            {
                result.CsprojFiles.Add(csprojPath);
            }
        }

        _logger.LogInformation("Sanitized {Count} .csproj files", csprojFiles.Length);
    }

    private async Task CreateDotNetSolutionAsync(
        string directory,
        string solutionName,
        ALaCarteResult result,
        CancellationToken cancellationToken)
    {
        if (result.CsprojFiles.Count == 0)
        {
            _logger.LogInformation("No .csproj files found, skipping solution creation");
            return;
        }

        var solutionPath = Path.Combine(directory, solutionName);

        // Ensure solution name ends with .sln
        if (!solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            solutionPath += ".sln";
        }

        try
        {
            // Create the solution
            var solutionNameWithoutExtension = Path.GetFileNameWithoutExtension(solutionPath);
            var exitCode = await RunDotNetCommandAsync(
                $"new sln --name \"{solutionNameWithoutExtension}\"",
                directory,
                cancellationToken);

            if (exitCode != 0)
            {
                result.Errors.Add("Failed to create .NET solution");
                return;
            }

            // Add all .csproj files to the solution
            foreach (var csprojPath in result.CsprojFiles)
            {
                var relativePath = Path.GetRelativePath(directory, csprojPath);
                exitCode = await RunDotNetCommandAsync(
                    $"sln \"{solutionPath}\" add \"{relativePath}\"",
                    directory,
                    cancellationToken);

                if (exitCode != 0)
                {
                    result.Warnings.Add($"Failed to add project to solution: {relativePath}");
                }
            }

            result.SolutionPath = solutionPath;
            _logger.LogInformation("Created .NET solution: {Path} with {Count} projects",
                solutionPath, result.CsprojFiles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating .NET solution");
            result.Errors.Add($"Failed to create .NET solution: {ex.Message}");
        }
    }

    private void HandleOrphanAngularProjects(string directory, ALaCarteResult result)
    {
        var helper = new AngularWorkspaceHelper(_logger);

        var orphanProjects = helper.FindOrphanAngularProjects(directory);

        foreach (var orphanProject in orphanProjects)
        {
            // Look up custom root mapping for this project or any of its parent directories
            string? customRoot = null;
            var projectFullPath = Path.GetFullPath(orphanProject);

            foreach (var mapping in result.AngularRootMappings)
            {
                // Check if orphan project is within a mapped directory
                if (projectFullPath.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase) ||
                    mapping.Key.StartsWith(projectFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    customRoot = mapping.Value;
                    _logger.LogInformation("Using custom Angular root '{Root}' for project: {Path}", customRoot, orphanProject);
                    break;
                }
            }

            var workspacePath = helper.CreateWorkspaceForOrphanProject(orphanProject, customRoot);
            if (workspacePath != null && !result.AngularWorkspacesCreated.Contains(workspacePath))
            {
                result.AngularWorkspacesCreated.Add(workspacePath);
            }
        }

        if (result.AngularWorkspacesCreated.Count > 0)
        {
            _logger.LogInformation("Created {Count} Angular workspace(s) for orphan projects",
                result.AngularWorkspacesCreated.Count);
        }
    }

    private async Task<int> RunGitCommandAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        return await RunProcessAsync("git", arguments, workingDirectory, cancellationToken);
    }

    private async Task<int> RunDotNetCommandAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        return await RunProcessAsync("dotnet", arguments, workingDirectory, cancellationToken);
    }

    private async Task<int> RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogWarning(
                "{FileName} command failed: {Arguments}. Error: {Error}",
                fileName,
                arguments,
                error.ToString().Trim());
        }

        return process.ExitCode;
    }

    private static string GetRepositoryName(string url)
    {
        // Extract repository name from URL
        var uri = url.TrimEnd('/');
        var lastSlash = uri.LastIndexOf('/');
        var name = lastSlash >= 0 ? uri.Substring(lastSlash + 1) : uri;

        // Remove .git extension if present
        if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(0, name.Length - 4);
        }

        return name;
    }
}
