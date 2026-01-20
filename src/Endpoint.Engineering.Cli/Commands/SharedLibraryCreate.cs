// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.DotNet.SharedLibrary.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("shared-library-create")]
public class SharedLibraryCreateRequest : IRequest
{
    [Option('c', "config", Required = true, HelpText = "Path to the YAML configuration file.")]
    public string ConfigPath { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output directory (overrides config).")]
    public string? OutputDirectory { get; set; }

    [Option("protocols", Required = false, HelpText = "Comma-separated list of protocols to enable (e.g., redis,ccsds,udp,azureservicebus).")]
    public string? Protocols { get; set; }

    [Option("serializers", Required = false, HelpText = "Comma-separated list of serializers to enable (e.g., messagepack,json,ccsds).")]
    public string? Serializers { get; set; }

    [Option("dry-run", Required = false, Default = false, HelpText = "Preview the generated structure without creating files.")]
    public bool DryRun { get; set; }

    [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output.")]
    public bool Verbose { get; set; }

    [Option('n', "library-name", Required = false, HelpText = "Name prefix for generated projects (default: 'Shared'). Example: 'Common' generates Common.Domain, Common.Contracts, etc.")]
    public string? LibraryName { get; set; }
}

public class SharedLibraryCreateRequestHandler : IRequestHandler<SharedLibraryCreateRequest>
{
    private readonly ILogger<SharedLibraryCreateRequestHandler> _logger;
    private readonly ISharedLibraryConfigLoader _configLoader;
    private readonly ISharedLibraryGeneratorService _generatorService;

    public SharedLibraryCreateRequestHandler(
        ILogger<SharedLibraryCreateRequestHandler> logger,
        ISharedLibraryConfigLoader configLoader,
        ISharedLibraryGeneratorService generatorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configLoader = configLoader ?? throw new ArgumentNullException(nameof(configLoader));
        _generatorService = generatorService ?? throw new ArgumentNullException(nameof(generatorService));
    }

    public async Task Handle(SharedLibraryCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting shared library generation from config: {ConfigPath}", request.ConfigPath);

        // Load configuration
        var config = await _configLoader.LoadFromFileAsync(request.ConfigPath, cancellationToken);

        // Apply CLI overrides
        ApplyCliOverrides(config, request);

        // Validate configuration
        var validationErrors = _configLoader.Validate(config);
        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                _logger.LogError("Configuration error: {Error}", error);
            }

            throw new InvalidOperationException($"Configuration validation failed with {validationErrors.Count} error(s).");
        }

        _logger.LogInformation("Configuration loaded and validated. Solution: {SolutionName}", config.Solution.Name);

        if (request.DryRun)
        {
            // Preview mode
            var preview = await _generatorService.PreviewAsync(config, cancellationToken);
            _logger.LogInformation("Preview mode - files that would be generated:");
            Console.WriteLine();
            Console.WriteLine("=== Shared Library Preview ===");
            Console.WriteLine($"Solution: {config.Solution.Name}");
            Console.WriteLine($"Output Path: {config.Solution.OutputPath}");
            Console.WriteLine();
            Console.WriteLine("Projects to generate:");
            foreach (var project in preview.Projects)
            {
                Console.WriteLine($"  - {project}");
            }

            Console.WriteLine();
            Console.WriteLine("Files to generate:");
            foreach (var file in preview.Files)
            {
                Console.WriteLine($"  - {file}");
            }

            Console.WriteLine();
            Console.WriteLine($"Total: {preview.Projects.Count} projects, {preview.Files.Count} files");
        }
        else
        {
            // Generate
            await _generatorService.GenerateAsync(config, cancellationToken);
            _logger.LogInformation("Shared library generated successfully at: {OutputPath}", config.Solution.OutputPath);

            Console.WriteLine();
            Console.WriteLine($"Shared library '{config.Solution.Name}' generated successfully!");
            Console.WriteLine($"Location: {config.Solution.OutputPath}");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine($"  1. cd {config.Solution.OutputPath}/{config.Solution.Name}");
            Console.WriteLine("  2. dotnet build");
        }
    }

    private void ApplyCliOverrides(SharedLibraryConfig config, SharedLibraryCreateRequest request)
    {
        // Override output directory
        if (!string.IsNullOrWhiteSpace(request.OutputDirectory))
        {
            config.Solution.OutputPath = request.OutputDirectory;
        }

        // Override library name
        if (!string.IsNullOrWhiteSpace(request.LibraryName))
        {
            config.Solution.LibraryName = request.LibraryName;
        }

        // Override protocols
        if (!string.IsNullOrWhiteSpace(request.Protocols))
        {
            var protocols = request.Protocols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Disable all protocols first
            if (config.Protocols.AzureServiceBus != null)
            {
                config.Protocols.AzureServiceBus.Enabled = false;
            }

            if (config.Protocols.Redis != null)
            {
                config.Protocols.Redis.Enabled = false;
            }

            if (config.Protocols.UdpMulticast != null)
            {
                config.Protocols.UdpMulticast.Enabled = false;
            }

            if (config.Protocols.Ccsds != null)
            {
                config.Protocols.Ccsds.Enabled = false;
            }

            // Enable selected protocols
            foreach (var protocol in protocols)
            {
                switch (protocol.ToLowerInvariant())
                {
                    case "redis":
                        config.Protocols.Redis ??= new RedisConfig();
                        config.Protocols.Redis.Enabled = true;
                        break;
                    case "ccsds":
                        config.Protocols.Ccsds ??= new CcsdsProtocolConfig();
                        config.Protocols.Ccsds.Enabled = true;
                        break;
                    case "udp":
                    case "udpmulticast":
                        config.Protocols.UdpMulticast ??= new UdpMulticastConfig();
                        config.Protocols.UdpMulticast.Enabled = true;
                        break;
                    case "azureservicebus":
                    case "servicebus":
                        config.Protocols.AzureServiceBus ??= new AzureServiceBusConfig();
                        config.Protocols.AzureServiceBus.Enabled = true;
                        break;
                }
            }
        }

        // Override serializers
        if (!string.IsNullOrWhiteSpace(request.Serializers))
        {
            var serializers = request.Serializers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Disable all serializers first
            if (config.Serializers.MessagePack != null)
            {
                config.Serializers.MessagePack.Enabled = false;
            }

            if (config.Serializers.Json != null)
            {
                config.Serializers.Json.Enabled = false;
            }

            if (config.Serializers.CcsdsBinary != null)
            {
                config.Serializers.CcsdsBinary.Enabled = false;
            }

            // Enable selected serializers
            foreach (var serializer in serializers)
            {
                switch (serializer.ToLowerInvariant())
                {
                    case "messagepack":
                        config.Serializers.MessagePack ??= new MessagePackConfig();
                        config.Serializers.MessagePack.Enabled = true;
                        break;
                    case "json":
                        config.Serializers.Json ??= new JsonSerializerConfig();
                        config.Serializers.Json.Enabled = true;
                        break;
                    case "ccsds":
                    case "ccsdsbinary":
                        config.Serializers.CcsdsBinary ??= new CcsdsBinaryConfig();
                        config.Serializers.CcsdsBinary.Enabled = true;
                        break;
                }
            }
        }
    }
}
