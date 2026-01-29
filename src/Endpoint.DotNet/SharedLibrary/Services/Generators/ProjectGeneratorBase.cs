// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Base class for project generators.
/// </summary>
public abstract class ProjectGeneratorBase : IProjectGenerator
{
    protected readonly ILogger Logger;
    protected readonly IFileSystem FileSystem;
    protected readonly ICommandService CommandService;
    protected readonly ITemplateLocator TemplateLocator;
    protected readonly ITemplateProcessor TemplateProcessor;

    protected ProjectGeneratorBase(
        ILogger logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        CommandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        TemplateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        TemplateProcessor = templateProcessor ?? throw new ArgumentNullException(nameof(templateProcessor));
    }

    /// <inheritdoc />
    public abstract int Order { get; }

    /// <inheritdoc />
    public abstract bool ShouldGenerate(SharedLibraryConfig config);

    /// <inheritdoc />
    public abstract Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a project directory.
    /// </summary>
    protected string CreateProjectDirectory(GeneratorContext context, string projectName)
    {
        var projectDirectory = FileSystem.Path.Combine(context.SharedDirectory, projectName);
        FileSystem.Directory.CreateDirectory(projectDirectory);
        return projectDirectory;
    }

    /// <summary>
    /// Writes a file with content.
    /// </summary>
    protected async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken)
    {
        var directory = FileSystem.Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            FileSystem.Directory.CreateDirectory(directory);
        }

        await FileSystem.File.WriteAllTextAsync(filePath, content, cancellationToken);
        Logger.LogDebug("Created file: {FilePath}", filePath);
    }

    /// <summary>
    /// Adds a project to the solution.
    /// </summary>
    protected void AddProjectToSolution(GeneratorContext context, string projectPath)
    {
        var solutionFile = FileSystem.Path.Combine(context.SolutionDirectory, $"{context.SolutionName}.sln");
        CommandService.Start($"dotnet sln {solutionFile} add {projectPath}", context.SolutionDirectory);
        Logger.LogInformation("Added project to solution: {ProjectPath}", projectPath);
    }

    /// <summary>
    /// Generates a .csproj file.
    /// </summary>
    protected async Task GenerateCsprojAsync(
        string projectDirectory,
        string projectName,
        string targetFramework,
        List<string>? packageReferences = null,
        List<string>? projectReferences = null,
        CancellationToken cancellationToken = default)
    {
        var csprojContent = GenerateCsprojContent(projectName, targetFramework, packageReferences, projectReferences);
        var csprojPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        await WriteFileAsync(csprojPath, csprojContent, cancellationToken);
    }

    /// <summary>
    /// Generates the content of a .csproj file.
    /// </summary>
    protected string GenerateCsprojContent(
        string projectName,
        string targetFramework,
        List<string>? packageReferences = null,
        List<string>? projectReferences = null)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine($"    <TargetFramework>{targetFramework}</TargetFramework>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("  </PropertyGroup>");

        if (packageReferences != null && packageReferences.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("  <ItemGroup>");
            foreach (var package in packageReferences)
            {
                sb.AppendLine($"    {package}");
            }

            sb.AppendLine("  </ItemGroup>");
        }

        if (projectReferences != null && projectReferences.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("  <ItemGroup>");
            foreach (var project in projectReferences)
            {
                sb.AppendLine($"    <ProjectReference Include=\"{project}\" />");
            }

            sb.AppendLine("  </ItemGroup>");
        }

        sb.AppendLine();
        sb.AppendLine("</Project>");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the relative path from the solution for display.
    /// </summary>
    protected string GetRelativePath(GeneratorContext context, string fullPath)
    {
        if (fullPath.StartsWith(context.SolutionDirectory))
        {
            return fullPath.Substring(context.SolutionDirectory.Length).TrimStart(FileSystem.Path.DirectorySeparatorChar, FileSystem.Path.AltDirectorySeparatorChar);
        }

        return fullPath;
    }
}
