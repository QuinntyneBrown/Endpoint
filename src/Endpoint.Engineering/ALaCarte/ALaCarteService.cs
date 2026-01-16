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

    private async Task ProcessRepositoryAsync(
        RepositoryConfiguration repo,
        string tempDir,
        string outputDir,
        ALaCarteResult result,
        CancellationToken cancellationToken)
    {
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

        // Process each folder configuration
        foreach (var folderConfig in repo.Folders)
        {
            ProcessFolderConfiguration(cloneDir, outputDir, folderConfig, result);
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
            var workspacePath = helper.CreateWorkspaceForOrphanProject(orphanProject);
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
