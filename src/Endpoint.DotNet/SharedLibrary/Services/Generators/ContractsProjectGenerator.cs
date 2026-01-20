// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.Contracts project.
/// </summary>
public class ContractsProjectGenerator : ProjectGeneratorBase
{
    public ContractsProjectGenerator(
        ILogger<ContractsProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 2;

    public override bool ShouldGenerate(SharedLibraryConfig config)
        => config.Services.Count > 0;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Contracts";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Contracts";

        Logger.LogInformation("Generating {ProjectName}", projectName);

        var packageRefs = new List<string>
        {
            "<PackageReference Include=\"MessagePack\" Version=\"2.5.172\" />",
        };

        var abstractionsProject = $"{context.LibraryName}.Messaging.Abstractions";

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            packageReferences: packageRefs,
            projectReferences: new List<string>
            {
                $"../{abstractionsProject}/{abstractionsProject}.csproj",
            },
            cancellationToken: cancellationToken);

        // Generate contracts for each service
        foreach (var service in context.Config.Services)
        {
            var serviceDirectory = FileSystem.Path.Combine(projectDirectory, service.Name);
            FileSystem.Directory.CreateDirectory(serviceDirectory);

            // Generate events
            foreach (var evt in service.Events)
            {
                await WriteFileAsync(
                    FileSystem.Path.Combine(serviceDirectory, $"{evt.Name}.cs"),
                    GenerateEventClass(ns, service.Name, evt),
                    cancellationToken);
            }

            // Generate commands
            foreach (var cmd in service.Commands)
            {
                await WriteFileAsync(
                    FileSystem.Path.Combine(serviceDirectory, $"{cmd.Name}.cs"),
                    GenerateCommandClass(ns, service.Name, cmd),
                    cancellationToken);
            }
        }

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var projectName = $"{context.LibraryName}.Contracts";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");

        foreach (var service in context.Config.Services)
        {
            foreach (var evt in service.Events)
            {
                preview.Files.Add($"{basePath}/{service.Name}/{evt.Name}.cs");
            }

            foreach (var cmd in service.Commands)
            {
                preview.Files.Add($"{basePath}/{service.Name}/{cmd.Name}.cs");
            }
        }

        return Task.FromResult(preview);
    }

    private string GenerateEventClass(string ns, string serviceName, EventConfig evt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine("using MessagePack;");
        sb.AppendLine($"using {ns.Replace(".Contracts", ".Messaging.Abstractions")};");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns}.{serviceName};");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(evt.Description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {evt.Description}");
            sb.AppendLine("/// </summary>");
        }

        sb.AppendLine("[MessagePackObject]");
        sb.AppendLine($"public sealed class {evt.Name} : EventBase");
        sb.AppendLine("{");

        // Generate properties with MessagePack keys
        var keyIndex = 10; // Start after base class properties
        var baseClassProperties = new[] { "EventId", "Timestamp", "EventType" };
        foreach (var prop in evt.Properties)
        {
            var key = prop.Key ?? keyIndex++;
            if (!string.IsNullOrWhiteSpace(prop.Description))
            {
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {prop.Description}");
                sb.AppendLine($"    /// </summary>");
            }

            // Add 'new' keyword if property hides a base class member
            var newKeyword = baseClassProperties.Contains(prop.Name) ? "new " : "";
            sb.AppendLine($"    [Key({key})]");
            sb.AppendLine($"    public {newKeyword}{prop.Type} {prop.Name} {{ get; init; }}{GetDefaultValue(prop)}");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GenerateCommandClass(string ns, string serviceName, CommandConfig cmd)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine("using MessagePack;");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns}.{serviceName};");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(cmd.Description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {cmd.Description}");
            sb.AppendLine("/// </summary>");
        }

        sb.AppendLine("[MessagePackObject]");
        sb.AppendLine($"public sealed class {cmd.Name}");
        sb.AppendLine("{");

        // Generate properties with MessagePack keys
        var keyIndex = 0;
        foreach (var prop in cmd.Properties)
        {
            var key = prop.Key ?? keyIndex++;
            if (!string.IsNullOrWhiteSpace(prop.Description))
            {
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {prop.Description}");
                sb.AppendLine($"    /// </summary>");
            }

            sb.AppendLine($"    [Key({key})]");
            sb.AppendLine($"    public {prop.Type} {prop.Name} {{ get; init; }}{GetDefaultValue(prop)}");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GetDefaultValue(PropertyConfig prop)
    {
        if (!string.IsNullOrWhiteSpace(prop.DefaultValue))
        {
            return $" = {prop.DefaultValue};";
        }

        // Add default for reference types to avoid nullability warnings
        return prop.Type switch
        {
            "string" => " = string.Empty;",
            _ when prop.Type.EndsWith("[]") => $" = Array.Empty<{prop.Type.TrimEnd('[', ']')}>();",
            _ when prop.Type.StartsWith("List<") => $" = new();",
            _ when prop.Type.StartsWith("Dictionary<") => $" = new();",
            _ => string.Empty,
        };
    }
}
