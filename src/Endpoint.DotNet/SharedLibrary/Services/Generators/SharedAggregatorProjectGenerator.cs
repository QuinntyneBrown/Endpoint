// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.csproj aggregator project.
/// </summary>
public class SharedAggregatorProjectGenerator : ProjectGeneratorBase
{
    public SharedAggregatorProjectGenerator(
        ILogger<SharedAggregatorProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 100; // Run last

    public override bool ShouldGenerate(SharedLibraryConfig config) => true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var lib = context.LibraryName;
        var projectName = $"{context.SolutionName}.{lib}";
        var projectDirectory = FileSystem.Path.Combine(context.SharedDirectory, projectName);
        FileSystem.Directory.CreateDirectory(projectDirectory);
        var ns = $"{context.Namespace}.{lib}";

        Logger.LogInformation("Generating {ProjectName} aggregator", projectName);

        // Collect project references based on what was generated
        var projectReferences = new List<string>
        {
            $"../{lib}.Messaging.Abstractions/{lib}.Messaging.Abstractions.csproj",
            $"../{lib}.Domain/{lib}.Domain.csproj",
        };

        if (context.Config.Services.Count > 0)
        {
            projectReferences.Add($"../{lib}.Contracts/{lib}.Contracts.csproj");
        }

        if (context.Config.Protocols.Redis?.Enabled == true)
        {
            projectReferences.Add($"../{lib}.Messaging.Redis/{lib}.Messaging.Redis.csproj");
        }

        if (context.Config.Protocols.UdpMulticast?.Enabled == true)
        {
            projectReferences.Add($"../{lib}.Messaging.UdpMulticast/{lib}.Messaging.UdpMulticast.csproj");
        }

        if (context.Config.Protocols.AzureServiceBus?.Enabled == true)
        {
            projectReferences.Add($"../{lib}.Messaging.AzureServiceBus/{lib}.Messaging.AzureServiceBus.csproj");
        }

        if (context.Config.Protocols.Ccsds?.Enabled == true)
        {
            projectReferences.Add($"../{lib}.Messaging.Ccsds/{lib}.Messaging.Ccsds.csproj");
        }

        if (context.Config.Protocols?.Jsc?.Enabled == true)
        {
            projectReferences.Add($"../{lib}.Messaging.Jsc/{lib}.Messaging.Jsc.csproj");
        }

        if (HasMessagingInfrastructure(context.Config))
        {
            projectReferences.Add($"../{lib}.Messaging.Infrastructure/{lib}.Messaging.Infrastructure.csproj");
        }

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            projectReferences: projectReferences,
            cancellationToken: cancellationToken);

        // Generate a re-exporting file
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, $"{lib}Exports.cs"),
            GenerateExportsFile(ns, context.Config),
            cancellationToken);

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var lib = context.LibraryName;
        var projectName = $"{context.SolutionName}.{lib}";
        var basePath = $"src/{lib}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/{lib}Exports.cs");

        return Task.FromResult(preview);
    }

    private string GenerateExportsFile(string ns, SharedLibraryConfig config)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// Auto-generated code - Re-exports all shared library types");
        sb.AppendLine();

        // Re-export abstractions
        sb.AppendLine($"global using {ns}.Messaging.Abstractions;");
        sb.AppendLine($"global using {ns}.Domain;");

        if (config.Services.Count > 0)
        {
            foreach (var service in config.Services)
            {
                sb.AppendLine($"global using {ns}.Contracts.{service.Name};");
            }
        }

        if (config.Domain.StronglyTypedIds.Count > 0)
        {
            sb.AppendLine($"global using {ns}.Domain.Ids;");
        }

        if (config.Domain.ValueObjects.Count > 0)
        {
            sb.AppendLine($"global using {ns}.Domain.ValueObjects;");
        }

        if (config.Protocols.Redis?.Enabled == true)
        {
            sb.AppendLine($"global using {ns}.Messaging.Redis;");
        }

        if (config.Protocols.UdpMulticast?.Enabled == true)
        {
            sb.AppendLine($"global using {ns}.Messaging.UdpMulticast;");
        }

        if (config.Protocols.AzureServiceBus?.Enabled == true)
        {
            sb.AppendLine($"global using {ns}.Messaging.AzureServiceBus;");
        }

        if (config.Protocols.Ccsds?.Enabled == true)
        {
            sb.AppendLine($"global using {ns}.Messaging.Ccsds;");
            sb.AppendLine($"global using {ns}.Messaging.Ccsds.Packets;");
        }

        if (config.Protocols?.Jsc?.Enabled == true)
        {
            sb.AppendLine($"global using {ns}.Messaging.Jsc;");
        }

        if (HasMessagingInfrastructure(config))
        {
            sb.AppendLine($"global using {ns}.Messaging.Infrastructure;");
        }

        return sb.ToString();
    }

    private bool HasMessagingInfrastructure(SharedLibraryConfig config)
    {
        return config.MessagingInfrastructure?.IncludeRetryPolicies == true
            || config.MessagingInfrastructure?.IncludeCircuitBreaker == true
            || config.MessagingInfrastructure?.IncludeDeadLetterQueue == true
            || config.MessagingInfrastructure?.IncludeMessageValidation == true
            || config.MessagingInfrastructure?.IncludeDistributedTracing == true
            || config.MessagingInfrastructure?.IncludeMessageVersioning == true
            || config.MessagingInfrastructure?.IncludeSerializationHelpers == true
            || config.MessagingInfrastructure?.IncludeRepositoryInterfaces == true
            || config.MessagingInfrastructure?.IncludeEntityBaseClasses == true;
    }
}
