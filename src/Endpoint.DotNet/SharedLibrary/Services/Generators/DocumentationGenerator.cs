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
/// Generator for comprehensive documentation at the solution root.
/// </summary>
public class DocumentationGenerator : ProjectGeneratorBase
{
    public DocumentationGenerator(
        ILogger<DocumentationGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    /// <inheritdoc />
    public override int Order => 1000; // Run last

    /// <inheritdoc />
    public override bool ShouldGenerate(SharedLibraryConfig config) =>
        config.Documentation?.Enabled == true;

    /// <inheritdoc />
    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Generating documentation...");

        var docsDir = FileSystem.Path.Combine(context.SolutionDirectory, context.Config.Documentation?.OutputFolder ?? "docs");
        FileSystem.Directory.CreateDirectory(docsDir);

        // Generate main README
        if (context.Config.Documentation?.GenerateReadme != false)
        {
            await GenerateReadmeAsync(context, docsDir, cancellationToken);
        }

        // Generate architecture guide
        if (context.Config.Documentation?.GenerateArchitectureGuide != false)
        {
            await GenerateArchitectureGuideAsync(context, docsDir, cancellationToken);
        }

        // Generate protocol documentation
        if (context.Config.Documentation?.GenerateProtocolDocs != false)
        {
            await GenerateProtocolDocsAsync(context, docsDir, cancellationToken);
        }

        // Generate extension guides
        if (context.Config.Documentation?.GenerateExtensionGuides != false)
        {
            await GenerateExtensionGuidesAsync(context, docsDir, cancellationToken);
        }

        // Generate API reference
        if (context.Config.Documentation?.GenerateApiReference != false)
        {
            await GenerateApiReferenceAsync(context, docsDir, cancellationToken);
        }

        Logger.LogInformation("Documentation generated successfully in {DocsDir}", docsDir);
    }

    /// <inheritdoc />
    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var docsDir = context.Config.Documentation?.OutputFolder ?? "docs";

        preview.Files.Add($"{docsDir}/README.md");
        preview.Files.Add($"{docsDir}/architecture/README.md");
        preview.Files.Add($"{docsDir}/architecture/project-structure.md");
        preview.Files.Add($"{docsDir}/protocols/README.md");

        if (context.Config.Protocols?.Jsc?.Enabled == true)
        {
            preview.Files.Add($"{docsDir}/protocols/jsc-protocol.md");
        }

        if (context.Config.Protocols?.Ccsds?.Enabled == true)
        {
            preview.Files.Add($"{docsDir}/protocols/ccsds-protocol.md");
        }

        if (context.Config.Protocols?.Redis?.Enabled == true)
        {
            preview.Files.Add($"{docsDir}/protocols/redis-pubsub.md");
        }

        if (context.Config.Protocols?.UdpMulticast?.Enabled == true)
        {
            preview.Files.Add($"{docsDir}/protocols/udp-multicast.md");
        }

        if (context.Config.Protocols?.AzureServiceBus?.Enabled == true)
        {
            preview.Files.Add($"{docsDir}/protocols/azure-servicebus.md");
        }

        preview.Files.Add($"{docsDir}/extending/README.md");
        preview.Files.Add($"{docsDir}/extending/adding-events.md");
        preview.Files.Add($"{docsDir}/extending/adding-protocols.md");
        preview.Files.Add($"{docsDir}/extending/custom-serializers.md");
        preview.Files.Add($"{docsDir}/api/README.md");

        return Task.FromResult(preview);
    }

    private async Task GenerateReadmeAsync(GeneratorContext context, string docsDir, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {context.SolutionName} Documentation");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine($"This shared library was generated using the Endpoint CLI `shared-library-create` command. It provides a complete messaging and domain infrastructure for microservices architecture.");
        sb.AppendLine();
        sb.AppendLine("## Generated Configuration");
        sb.AppendLine();
        sb.AppendLine("The library was generated with the following configuration:");
        sb.AppendLine();
        sb.AppendLine($"- **Solution Name**: {context.SolutionName}");
        sb.AppendLine($"- **Root Namespace**: {context.Namespace}");
        sb.AppendLine($"- **Target Framework**: {context.TargetFramework}");
        sb.AppendLine($"- **Library Name Prefix**: {context.LibraryName}");
        sb.AppendLine();

        // Document enabled protocols
        sb.AppendLine("### Enabled Protocols");
        sb.AppendLine();
        var protocols = new List<string>();
        if (context.Config.Protocols?.Redis?.Enabled == true) protocols.Add("Redis Pub/Sub");
        if (context.Config.Protocols?.UdpMulticast?.Enabled == true) protocols.Add("UDP Multicast");
        if (context.Config.Protocols?.AzureServiceBus?.Enabled == true) protocols.Add("Azure Service Bus");
        if (context.Config.Protocols?.Ccsds?.Enabled == true) protocols.Add("CCSDS Space Packets");
        if (context.Config.Protocols?.Jsc?.Enabled == true) protocols.Add("JSC Protocol (JSC-35199)");

        if (protocols.Count > 0)
        {
            foreach (var protocol in protocols)
            {
                sb.AppendLine($"- {protocol}");
            }
        }
        else
        {
            sb.AppendLine("- None (abstractions only)");
        }

        sb.AppendLine();

        // Document enabled serializers
        sb.AppendLine("### Enabled Serializers");
        sb.AppendLine();
        var serializers = new List<string>();
        if (context.Config.Serializers?.MessagePack?.Enabled == true) serializers.Add("MessagePack");
        if (context.Config.Serializers?.Json?.Enabled == true) serializers.Add("JSON (System.Text.Json)");
        if (context.Config.Serializers?.CcsdsBinary?.Enabled == true) serializers.Add("CCSDS Binary (bit-level)");

        if (serializers.Count > 0)
        {
            foreach (var serializer in serializers)
            {
                sb.AppendLine($"- {serializer}");
            }
        }
        else
        {
            sb.AppendLine("- MessagePack (default)");
        }

        sb.AppendLine();

        // Document messaging infrastructure
        if (HasMessagingInfrastructure(context.Config))
        {
            sb.AppendLine("### Messaging Infrastructure");
            sb.AppendLine();
            if (context.Config.MessagingInfrastructure?.IncludeRetryPolicies == true)
                sb.AppendLine("- Retry Policies with exponential backoff");
            if (context.Config.MessagingInfrastructure?.IncludeCircuitBreaker == true)
                sb.AppendLine("- Circuit Breaker pattern");
            if (context.Config.MessagingInfrastructure?.IncludeDeadLetterQueue == true)
                sb.AppendLine("- Dead Letter Queue support");
            if (context.Config.MessagingInfrastructure?.IncludeMessageValidation == true)
                sb.AppendLine("- Message Validation framework");
            if (context.Config.MessagingInfrastructure?.IncludeDistributedTracing == true)
                sb.AppendLine("- Distributed Tracing support");
            if (context.Config.MessagingInfrastructure?.IncludeMessageVersioning == true)
                sb.AppendLine("- Message Versioning support");
            sb.AppendLine();
        }

        // Quick start
        sb.AppendLine("## Quick Start");
        sb.AppendLine();
        sb.AppendLine("### Building the Solution");
        sb.AppendLine();
        sb.AppendLine("```bash");
        sb.AppendLine($"cd {context.SolutionName}");
        sb.AppendLine("dotnet build");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### Adding to Your Service");
        sb.AppendLine();
        sb.AppendLine("Reference the aggregator project to get all shared library functionality:");
        sb.AppendLine();
        sb.AppendLine("```xml");
        sb.AppendLine($"<ProjectReference Include=\"../{context.LibraryName}/{context.SolutionName}.{context.LibraryName}/{context.SolutionName}.{context.LibraryName}.csproj\" />");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("Or reference individual projects for granular control:");
        sb.AppendLine();
        sb.AppendLine("```xml");
        sb.AppendLine($"<ProjectReference Include=\"../{context.LibraryName}/{context.LibraryName}.Contracts/{context.LibraryName}.Contracts.csproj\" />");
        sb.AppendLine($"<ProjectReference Include=\"../{context.LibraryName}/{context.LibraryName}.Domain/{context.LibraryName}.Domain.csproj\" />");
        sb.AppendLine("```");
        sb.AppendLine();

        // Table of contents
        sb.AppendLine("## Documentation");
        sb.AppendLine();
        sb.AppendLine("- [Architecture Guide](./architecture/README.md) - Project structure and design decisions");
        sb.AppendLine("- [Protocol Documentation](./protocols/README.md) - Details on messaging protocols");
        sb.AppendLine("- [Extension Guides](./extending/README.md) - How to extend the library");
        sb.AppendLine("- [API Reference](./api/README.md) - Interface and class documentation");
        sb.AppendLine();

        sb.AppendLine("## Services and Events");
        sb.AppendLine();
        if (context.Config.Services?.Count > 0)
        {
            foreach (var service in context.Config.Services)
            {
                sb.AppendLine($"### {service.Name}");
                if (!string.IsNullOrEmpty(service.Description))
                {
                    sb.AppendLine();
                    sb.AppendLine(service.Description);
                }

                sb.AppendLine();

                if (service.Events?.Count > 0)
                {
                    sb.AppendLine("**Events:**");
                    foreach (var evt in service.Events)
                    {
                        sb.AppendLine($"- `{evt.Name}`");
                    }

                    sb.AppendLine();
                }

                if (service.Commands?.Count > 0)
                {
                    sb.AppendLine("**Commands:**");
                    foreach (var cmd in service.Commands)
                    {
                        sb.AppendLine($"- `{cmd.Name}`");
                    }

                    sb.AppendLine();
                }
            }
        }
        else
        {
            sb.AppendLine("No services defined in configuration.");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*This documentation was auto-generated by the Endpoint CLI `shared-library-create` command.*");

        var readmePath = FileSystem.Path.Combine(docsDir, "README.md");
        await WriteFileAsync(readmePath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateArchitectureGuideAsync(GeneratorContext context, string docsDir, CancellationToken cancellationToken)
    {
        var archDir = FileSystem.Path.Combine(docsDir, "architecture");
        FileSystem.Directory.CreateDirectory(archDir);

        // Main architecture README
        var sb = new StringBuilder();
        sb.AppendLine("# Architecture Guide");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("This shared library follows a layered architecture designed for maximum flexibility and maintainability in microservices environments.");
        sb.AppendLine();
        sb.AppendLine("## Design Principles");
        sb.AppendLine();
        sb.AppendLine("1. **Abstraction over Implementation**: Core interfaces define contracts; implementations can be swapped");
        sb.AppendLine("2. **Protocol Agnostic**: Events and commands are serialized using pluggable serializers");
        sb.AppendLine("3. **Domain-Driven Design**: Strongly-typed IDs and value objects enforce domain rules");
        sb.AppendLine("4. **Dependency Injection**: All services are registered via extension methods");
        sb.AppendLine();
        sb.AppendLine("## Layer Dependencies");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                    Aggregator Project                        │");
        sb.AppendLine($"│                ({context.SolutionName}.{context.LibraryName})                │");
        sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
        sb.AppendLine("                              │");
        sb.AppendLine("              ┌───────────────┼───────────────┐");
        sb.AppendLine("              │               │               │");
        sb.AppendLine("              ▼               ▼               ▼");
        sb.AppendLine("┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐");
        sb.AppendLine($"│   {context.LibraryName}.Domain   │ │  {context.LibraryName}.Contracts │ │ Protocol Projects │");
        sb.AppendLine("│ (IDs, Results)  │ │    (Events)     │ │  (Event Buses)  │");
        sb.AppendLine("└─────────────────┘ └─────────────────┘ └─────────────────┘");
        sb.AppendLine("              │               │               │");
        sb.AppendLine("              └───────────────┼───────────────┘");
        sb.AppendLine("                              │");
        sb.AppendLine("                              ▼");
        sb.AppendLine("              ┌─────────────────────────────────┐");
        sb.AppendLine($"              │  {context.LibraryName}.Messaging.Abstractions  │");
        sb.AppendLine("              │     (Core Interfaces)           │");
        sb.AppendLine("              └─────────────────────────────────┘");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Projects");
        sb.AppendLine();
        sb.AppendLine($"- [Project Structure](./project-structure.md) - Detailed breakdown of each project");
        sb.AppendLine();

        var readmePath = FileSystem.Path.Combine(archDir, "README.md");
        await WriteFileAsync(readmePath, sb.ToString(), cancellationToken);

        // Project structure document
        sb.Clear();
        sb.AppendLine("# Project Structure");
        sb.AppendLine();
        sb.AppendLine("## Directory Layout");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine($"{context.SolutionName}/");
        sb.AppendLine($"├── {context.SolutionName}.sln");
        sb.AppendLine("├── docs/                                    # This documentation");
        sb.AppendLine("└── src/");
        sb.AppendLine($"    └── {context.LibraryName}/");
        sb.AppendLine($"        ├── {context.SolutionName}.{context.LibraryName}/         # Aggregator");
        sb.AppendLine($"        ├── {context.LibraryName}.Messaging.Abstractions/    # Core interfaces");
        sb.AppendLine($"        ├── {context.LibraryName}.Domain/                    # Domain primitives");
        sb.AppendLine($"        ├── {context.LibraryName}.Contracts/                 # Event definitions");

        if (context.Config.Protocols?.Redis?.Enabled == true)
            sb.AppendLine($"        ├── {context.LibraryName}.Messaging.Redis/           # Redis implementation");
        if (context.Config.Protocols?.UdpMulticast?.Enabled == true)
            sb.AppendLine($"        ├── {context.LibraryName}.Messaging.UdpMulticast/    # UDP implementation");
        if (context.Config.Protocols?.AzureServiceBus?.Enabled == true)
            sb.AppendLine($"        ├── {context.LibraryName}.Messaging.AzureServiceBus/ # Azure SB implementation");
        if (context.Config.Protocols?.Ccsds?.Enabled == true)
            sb.AppendLine($"        ├── {context.LibraryName}.Messaging.Ccsds/           # CCSDS implementation");
        if (context.Config.Protocols?.Jsc?.Enabled == true)
            sb.AppendLine($"        ├── {context.LibraryName}.Messaging.Jsc/             # JSC implementation");
        if (HasMessagingInfrastructure(context.Config))
            sb.AppendLine($"        └── {context.LibraryName}.Messaging.Infrastructure/  # Advanced features");

        sb.AppendLine("```");
        sb.AppendLine();

        // Detailed project descriptions
        sb.AppendLine("## Project Descriptions");
        sb.AppendLine();

        sb.AppendLine($"### {context.SolutionName}.{context.LibraryName} (Aggregator)");
        sb.AppendLine();
        sb.AppendLine("The aggregator project provides a single entry point for consuming applications. It re-exports all types from the underlying projects.");
        sb.AppendLine();
        sb.AppendLine("**Usage:**");
        sb.AppendLine("```csharp");
        sb.AppendLine($"using {context.Namespace};");
        sb.AppendLine("// All types are available through this single namespace");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine($"### {context.LibraryName}.Messaging.Abstractions");
        sb.AppendLine();
        sb.AppendLine("Contains core interfaces that define the messaging contracts:");
        sb.AppendLine();
        sb.AppendLine("| Interface | Purpose |");
        sb.AppendLine("|-----------|---------|");
        sb.AppendLine("| `IEvent` | Base interface for all events |");
        sb.AppendLine("| `IEventBus` | Publish/subscribe operations |");
        sb.AppendLine("| `IMessageSerializer` | Serialization/deserialization |");
        sb.AppendLine("| `IEventHandler<T>` | Event handler interface |");
        sb.AppendLine();

        sb.AppendLine($"### {context.LibraryName}.Domain");
        sb.AppendLine();
        sb.AppendLine("Contains domain primitives including:");
        sb.AppendLine();
        sb.AppendLine("- **Strongly-Typed IDs**: Type-safe identifiers that prevent mixing different ID types");
        sb.AppendLine("- **Value Objects**: Immutable objects defined by their values");
        sb.AppendLine("- **Result Pattern**: Railway-oriented programming for error handling");
        sb.AppendLine();

        sb.AppendLine($"### {context.LibraryName}.Contracts");
        sb.AppendLine();
        sb.AppendLine("Contains event and command definitions organized by service:");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine($"{context.LibraryName}.Contracts/");

        if (context.Config.Services?.Count > 0)
        {
            foreach (var service in context.Config.Services)
            {
                sb.AppendLine($"└── {service.Name}/");
                if (service.Events?.Count > 0)
                {
                    foreach (var evt in service.Events)
                    {
                        sb.AppendLine($"    ├── {evt.Name}.cs");
                    }
                }
            }
        }

        sb.AppendLine("```");
        sb.AppendLine();

        var structurePath = FileSystem.Path.Combine(archDir, "project-structure.md");
        await WriteFileAsync(structurePath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateProtocolDocsAsync(GeneratorContext context, string docsDir, CancellationToken cancellationToken)
    {
        var protocolDir = FileSystem.Path.Combine(docsDir, "protocols");
        FileSystem.Directory.CreateDirectory(protocolDir);

        // Main protocols README
        var sb = new StringBuilder();
        sb.AppendLine("# Protocol Documentation");
        sb.AppendLine();
        sb.AppendLine("This section documents the messaging protocols implemented in this shared library.");
        sb.AppendLine();
        sb.AppendLine("## Available Protocols");
        sb.AppendLine();

        if (context.Config.Protocols?.Redis?.Enabled == true)
            sb.AppendLine("- [Redis Pub/Sub](./redis-pubsub.md)");
        if (context.Config.Protocols?.UdpMulticast?.Enabled == true)
            sb.AppendLine("- [UDP Multicast](./udp-multicast.md)");
        if (context.Config.Protocols?.AzureServiceBus?.Enabled == true)
            sb.AppendLine("- [Azure Service Bus](./azure-servicebus.md)");
        if (context.Config.Protocols?.Ccsds?.Enabled == true)
            sb.AppendLine("- [CCSDS Space Packets](./ccsds-protocol.md)");
        if (context.Config.Protocols?.Jsc?.Enabled == true)
            sb.AppendLine("- [JSC Protocol (JSC-35199)](./jsc-protocol.md)");

        sb.AppendLine();

        var readmePath = FileSystem.Path.Combine(protocolDir, "README.md");
        await WriteFileAsync(readmePath, sb.ToString(), cancellationToken);

        // Generate individual protocol docs
        if (context.Config.Protocols?.Jsc?.Enabled == true)
        {
            await GenerateJscProtocolDocAsync(context, protocolDir, cancellationToken);
        }

        if (context.Config.Protocols?.Ccsds?.Enabled == true)
        {
            await GenerateCcsdsProtocolDocAsync(context, protocolDir, cancellationToken);
        }

        if (context.Config.Protocols?.Redis?.Enabled == true)
        {
            await GenerateRedisProtocolDocAsync(context, protocolDir, cancellationToken);
        }

        if (context.Config.Protocols?.UdpMulticast?.Enabled == true)
        {
            await GenerateUdpProtocolDocAsync(context, protocolDir, cancellationToken);
        }

        if (context.Config.Protocols?.AzureServiceBus?.Enabled == true)
        {
            await GenerateAzureServiceBusDocAsync(context, protocolDir, cancellationToken);
        }
    }

    private async Task GenerateJscProtocolDocAsync(GeneratorContext context, string protocolDir, CancellationToken cancellationToken)
    {
        var jscConfig = context.Config.Protocols?.Jsc;
        var sb = new StringBuilder();

        sb.AppendLine("# JSC Protocol (JSC-35199)");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("The JSC (Johnson Space Center) protocol follows the JSC-35199 specification for inter-MCC (Mission Control Center) communication. It provides a standardized message format for reliable data exchange between ground systems.");
        sb.AppendLine();

        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine($"- **Source MCC ID**: {jscConfig?.SourceMccId}");
        sb.AppendLine($"- **Default Destination MCC ID**: {jscConfig?.DefaultDestinationMccId}");
        sb.AppendLine($"- **Protocol Version**: {jscConfig?.ProtocolVersion}");
        sb.AppendLine($"- **CRC-32 Enabled**: {jscConfig?.IncludeCrc32}");
        sb.AppendLine();

        sb.AppendLine("## Message Structure");
        sb.AppendLine();
        sb.AppendLine("### Primary Header (16 bytes)");
        sb.AppendLine();
        sb.AppendLine("| Field | Size | Description |");
        sb.AppendLine("|-------|------|-------------|");
        sb.AppendLine("| Sync Word | 4 bytes | `0x1ACFFC1D` - Magic synchronization pattern |");
        sb.AppendLine("| Version | 1 byte | Protocol version |");
        sb.AppendLine("| Message Type | 1 byte | Type code for message routing |");
        sb.AppendLine("| Message ID | 2 bytes | Unique message identifier |");
        sb.AppendLine("| Source MCC ID | 2 bytes | Originating MCC identifier |");
        sb.AppendLine("| Destination MCC ID | 2 bytes | Target MCC identifier |");
        sb.AppendLine("| Priority | 1 byte | Message priority (0-255) |");
        sb.AppendLine("| Flags | 1 byte | Control flags |");
        sb.AppendLine("| Secondary Header Length | 2 bytes | Length of secondary header |");
        sb.AppendLine();

        sb.AppendLine("### Secondary Headers");
        sb.AppendLine();
        sb.AppendLine("The secondary header type is determined by the message category:");
        sb.AppendLine();
        sb.AppendLine("#### Common Secondary Header (8 bytes)");
        sb.AppendLine("| Field | Size | Description |");
        sb.AppendLine("|-------|------|-------------|");
        sb.AppendLine("| Timestamp | 8 bytes | UTC timestamp in ticks |");
        sb.AppendLine();

        sb.AppendLine("#### Command Secondary Header (24 bytes)");
        sb.AppendLine("| Field | Size | Description |");
        sb.AppendLine("|-------|------|-------------|");
        sb.AppendLine("| Timestamp | 8 bytes | UTC timestamp |");
        sb.AppendLine("| Command Sequence | 4 bytes | Command sequence number |");
        sb.AppendLine("| Execution Time | 8 bytes | Scheduled execution time |");
        sb.AppendLine("| Target System | 2 bytes | Target system identifier |");
        sb.AppendLine("| Reserved | 2 bytes | Reserved for future use |");
        sb.AppendLine();

        sb.AppendLine("#### Telemetry Secondary Header (16 bytes)");
        sb.AppendLine("| Field | Size | Description |");
        sb.AppendLine("|-------|------|-------------|");
        sb.AppendLine("| Timestamp | 8 bytes | Acquisition timestamp |");
        sb.AppendLine("| Sample Rate | 4 bytes | Samples per second |");
        sb.AppendLine("| Quality Indicator | 2 bytes | Data quality flags |");
        sb.AppendLine("| Reserved | 2 bytes | Reserved for future use |");
        sb.AppendLine();

        sb.AppendLine("### CRC-32 Checksum");
        sb.AppendLine();
        sb.AppendLine("When enabled, a 4-byte CRC-32 checksum is appended to each message using the IEEE 802.3 polynomial (`0xEDB88320`).");
        sb.AppendLine();

        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("### Registering JSC Messaging");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("services.AddJscMessaging(options =>");
        sb.AppendLine("{");
        sb.AppendLine($"    options.SourceMccId = {jscConfig?.SourceMccId};");
        sb.AppendLine($"    options.DefaultDestinationMccId = {jscConfig?.DefaultDestinationMccId};");
        sb.AppendLine($"    options.IncludeCrc32 = {jscConfig?.IncludeCrc32.ToString().ToLower()};");
        sb.AppendLine("});");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### Creating and Sending Messages");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("var serializer = serviceProvider.GetRequiredService<JscMessageSerializer>();");
        sb.AppendLine();
        sb.AppendLine("var message = new JscMessage");
        sb.AppendLine("{");
        sb.AppendLine("    MessageType = JscMessageType.Telemetry,");
        sb.AppendLine("    Priority = 128,");
        sb.AppendLine("    UserData = telemetryData");
        sb.AppendLine("};");
        sb.AppendLine();
        sb.AppendLine("byte[] bytes = serializer.Serialize(message);");
        sb.AppendLine("```");
        sb.AppendLine();

        if (jscConfig?.MessageTypes?.Count > 0)
        {
            sb.AppendLine("## Configured Message Types");
            sb.AppendLine();
            sb.AppendLine("| Name | Type Code | Secondary Header | Description |");
            sb.AppendLine("|------|-----------|------------------|-------------|");

            foreach (var msgType in jscConfig.MessageTypes)
            {
                sb.AppendLine($"| {msgType.Name} | 0x{msgType.TypeCode:X2} | {msgType.SecondaryHeaderType} | {msgType.Description ?? "-"} |");
            }

            sb.AppendLine();
        }

        var jscDocPath = FileSystem.Path.Combine(protocolDir, "jsc-protocol.md");
        await WriteFileAsync(jscDocPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateCcsdsProtocolDocAsync(GeneratorContext context, string protocolDir, CancellationToken cancellationToken)
    {
        var ccsdsConfig = context.Config.Protocols?.Ccsds;
        var sb = new StringBuilder();

        sb.AppendLine("# CCSDS Space Packets Protocol");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("The CCSDS (Consultative Committee for Space Data Systems) protocol implements the Space Packet Protocol as defined in CCSDS 133.0-B-1. It provides bit-level serialization for space telemetry and telecommand data.");
        sb.AppendLine();

        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine($"- **Spacecraft ID**: {ccsdsConfig?.SpacecraftId}");
        sb.AppendLine($"- **Include Secondary Header**: {ccsdsConfig?.IncludeSecondaryHeader}");
        sb.AppendLine($"- **Secondary Header Format**: {ccsdsConfig?.SecondaryHeaderFormat} (CUC or CDS)");
        sb.AppendLine();

        sb.AppendLine("## Packet Structure");
        sb.AppendLine();
        sb.AppendLine("### Primary Header (6 bytes)");
        sb.AppendLine();
        sb.AppendLine("| Field | Bits | Description |");
        sb.AppendLine("|-------|------|-------------|");
        sb.AppendLine("| Version | 3 | Packet version (always 0) |");
        sb.AppendLine("| Type | 1 | 0=TM, 1=TC |");
        sb.AppendLine("| Secondary Header Flag | 1 | 1 if secondary header present |");
        sb.AppendLine("| APID | 11 | Application Process ID |");
        sb.AppendLine("| Sequence Flags | 2 | 11=unsegmented |");
        sb.AppendLine("| Sequence Count | 14 | Packet sequence counter |");
        sb.AppendLine("| Data Length | 16 | Data field length - 1 |");
        sb.AppendLine();

        sb.AppendLine("### Secondary Header (CUC Format)");
        sb.AppendLine();
        sb.AppendLine("When using CCSDS Unsegmented Time Code (CUC):");
        sb.AppendLine();
        sb.AppendLine("| Field | Bytes | Description |");
        sb.AppendLine("|-------|-------|-------------|");
        sb.AppendLine("| Coarse Time | 4 | Seconds since epoch |");
        sb.AppendLine("| Fine Time | 2 | Fractional seconds |");
        sb.AppendLine();

        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("### Creating Packets");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("var packet = new CcsdsPacket");
        sb.AppendLine("{");
        sb.AppendLine("    Apid = 100,");
        sb.AppendLine("    PacketType = CcsdsPacketType.Telemetry,");
        sb.AppendLine("    HasSecondaryHeader = true,");
        sb.AppendLine("    Data = dataBytes");
        sb.AppendLine("};");
        sb.AppendLine();
        sb.AppendLine("var serializer = serviceProvider.GetRequiredService<CcsdsSerializer>();");
        sb.AppendLine("byte[] bytes = serializer.Serialize(packet);");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### Bit Packing");
        sb.AppendLine();
        sb.AppendLine("The library provides `BitPacker` and `BitUnpacker` classes for arbitrary bit-width field access:");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("var packer = new BitPacker();");
        sb.AppendLine("packer.PackBits(value: 7, bitWidth: 3);   // Pack 3-bit value");
        sb.AppendLine("packer.PackBits(value: 1, bitWidth: 1);   // Pack 1-bit flag");
        sb.AppendLine("packer.PackBits(value: 100, bitWidth: 11); // Pack 11-bit APID");
        sb.AppendLine("byte[] result = packer.ToArray();");
        sb.AppendLine("```");
        sb.AppendLine();

        if (context.Config.CcsdsPackets?.Count > 0)
        {
            sb.AppendLine("## Configured Packets");
            sb.AppendLine();

            foreach (var packet in context.Config.CcsdsPackets)
            {
                sb.AppendLine($"### {packet.Name}");
                sb.AppendLine();
                sb.AppendLine($"- **APID**: {packet.Apid}");
                sb.AppendLine($"- **Type**: {(packet.PacketType == 0 ? "Telemetry" : "Telecommand")}");
                sb.AppendLine($"- **Secondary Header**: {packet.HasSecondaryHeader}");

                if (!string.IsNullOrEmpty(packet.Description))
                {
                    sb.AppendLine($"- **Description**: {packet.Description}");
                }

                sb.AppendLine();

                if (packet.Fields?.Count > 0)
                {
                    sb.AppendLine("| Field | Type | Bits | Description |");
                    sb.AppendLine("|-------|------|------|-------------|");

                    foreach (var field in packet.Fields)
                    {
                        sb.AppendLine($"| {field.Name} | {field.DataType} | {field.BitSize} | {field.Description ?? "-"} |");
                    }

                    sb.AppendLine();
                }
            }
        }

        var ccsdsDocPath = FileSystem.Path.Combine(protocolDir, "ccsds-protocol.md");
        await WriteFileAsync(ccsdsDocPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateRedisProtocolDocAsync(GeneratorContext context, string protocolDir, CancellationToken cancellationToken)
    {
        var redisConfig = context.Config.Protocols?.Redis;
        var sb = new StringBuilder();

        sb.AppendLine("# Redis Pub/Sub Protocol");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("Redis Pub/Sub provides real-time messaging using the Redis server's built-in publish/subscribe mechanism. It's ideal for scenarios requiring low-latency event distribution within a controlled network.");
        sb.AppendLine();

        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine($"- **Channel Prefix**: `{redisConfig?.ChannelPrefix}`");
        sb.AppendLine();

        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("### Service Registration");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("services.AddRedisEventBus(options =>");
        sb.AppendLine("{");
        sb.AppendLine("    options.ConnectionString = \"localhost:6379\";");
        sb.AppendLine($"    options.ChannelPrefix = \"{redisConfig?.ChannelPrefix}\";");
        sb.AppendLine("});");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### Publishing Events");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public class MyService");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly IEventBus _eventBus;");
        sb.AppendLine();
        sb.AppendLine("    public async Task DoSomethingAsync()");
        sb.AppendLine("    {");
        sb.AppendLine("        await _eventBus.PublishAsync(new OrderCreated { OrderId = Guid.NewGuid() });");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### Subscribing to Events");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public class OrderCreatedHandler : IEventHandler<OrderCreated>");
        sb.AppendLine("{");
        sb.AppendLine("    public Task HandleAsync(OrderCreated @event, CancellationToken ct)");
        sb.AppendLine("    {");
        sb.AppendLine("        Console.WriteLine($\"Order created: {@event.OrderId}\");");
        sb.AppendLine("        return Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Channel Naming");
        sb.AppendLine();
        sb.AppendLine($"Events are published to channels using the pattern: `{redisConfig?.ChannelPrefix}:{{EventTypeName}}`");
        sb.AppendLine();
        sb.AppendLine($"For example, `OrderCreated` would be published to: `{redisConfig?.ChannelPrefix}:OrderCreated`");

        var redisDocPath = FileSystem.Path.Combine(protocolDir, "redis-pubsub.md");
        await WriteFileAsync(redisDocPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateUdpProtocolDocAsync(GeneratorContext context, string protocolDir, CancellationToken cancellationToken)
    {
        var udpConfig = context.Config.Protocols?.UdpMulticast;
        var sb = new StringBuilder();

        sb.AppendLine("# UDP Multicast Protocol");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("UDP Multicast provides high-throughput, low-latency messaging using IP multicast groups. It's ideal for real-time data distribution where some message loss is acceptable (e.g., telemetry, market data).");
        sb.AppendLine();

        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine($"- **Default Multicast Group**: `{udpConfig?.DefaultMulticastGroup}`");
        sb.AppendLine($"- **Default Port**: `{udpConfig?.DefaultPort}`");
        sb.AppendLine($"- **Default TTL**: `{udpConfig?.DefaultTtl}`");
        sb.AppendLine();

        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("### Service Registration");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("services.AddUdpMulticastEventBus(options =>");
        sb.AppendLine("{");
        sb.AppendLine($"    options.MulticastGroup = \"{udpConfig?.DefaultMulticastGroup}\";");
        sb.AppendLine($"    options.Port = {udpConfig?.DefaultPort};");
        sb.AppendLine($"    options.Ttl = {udpConfig?.DefaultTtl};");
        sb.AppendLine("});");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Network Considerations");
        sb.AppendLine();
        sb.AppendLine("- **TTL (Time To Live)**: Controls how many network hops the packets can traverse");
        sb.AppendLine("  - TTL=1: Local subnet only");
        sb.AppendLine("  - TTL=32: Reasonable for enterprise networks");
        sb.AppendLine("  - TTL=255: Maximum (unrestricted)");
        sb.AppendLine();
        sb.AppendLine("- **Multicast Groups**: Use addresses in the `239.0.0.0/8` range for organization-local scope");
        sb.AppendLine();
        sb.AppendLine("- **Message Size**: UDP has a maximum datagram size. Large messages should be fragmented or use a different protocol.");

        var udpDocPath = FileSystem.Path.Combine(protocolDir, "udp-multicast.md");
        await WriteFileAsync(udpDocPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateAzureServiceBusDocAsync(GeneratorContext context, string protocolDir, CancellationToken cancellationToken)
    {
        var asbConfig = context.Config.Protocols?.AzureServiceBus;
        var sb = new StringBuilder();

        sb.AppendLine("# Azure Service Bus Protocol");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("Azure Service Bus provides enterprise-grade, reliable messaging with features like guaranteed delivery, dead-lettering, and message sessions.");
        sb.AppendLine();

        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine($"- **Default Topic**: `{asbConfig?.DefaultTopic}`");
        sb.AppendLine($"- **Use Sessions**: `{asbConfig?.UseSessions}`");
        sb.AppendLine();

        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("### Service Registration");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("services.AddAzureServiceBusEventBus(options =>");
        sb.AppendLine("{");
        sb.AppendLine("    options.ConnectionString = \"your-connection-string\";");
        sb.AppendLine($"    options.TopicName = \"{asbConfig?.DefaultTopic}\";");
        sb.AppendLine("    options.SubscriptionName = \"my-service\";");
        sb.AppendLine("});");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Features");
        sb.AppendLine();
        sb.AppendLine("- **Guaranteed Delivery**: Messages are persisted until acknowledged");
        sb.AppendLine("- **Dead Letter Queue**: Failed messages are moved to a dead letter queue for analysis");
        sb.AppendLine("- **Message Sessions**: Ensures ordered processing for related messages");
        sb.AppendLine("- **Scheduled Delivery**: Messages can be scheduled for future delivery");

        var asbDocPath = FileSystem.Path.Combine(protocolDir, "azure-servicebus.md");
        await WriteFileAsync(asbDocPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateExtensionGuidesAsync(GeneratorContext context, string docsDir, CancellationToken cancellationToken)
    {
        var extendingDir = FileSystem.Path.Combine(docsDir, "extending");
        FileSystem.Directory.CreateDirectory(extendingDir);

        // Main extending README
        var sb = new StringBuilder();
        sb.AppendLine("# Extension Guides");
        sb.AppendLine();
        sb.AppendLine("This section explains how to extend the shared library for your specific needs.");
        sb.AppendLine();
        sb.AppendLine("## Guides");
        sb.AppendLine();
        sb.AppendLine("- [Adding New Events](./adding-events.md) - How to add new events and commands");
        sb.AppendLine("- [Adding New Protocols](./adding-protocols.md) - How to implement custom protocols");
        sb.AppendLine("- [Custom Serializers](./custom-serializers.md) - How to create custom serializers");
        sb.AppendLine();

        var readmePath = FileSystem.Path.Combine(extendingDir, "README.md");
        await WriteFileAsync(readmePath, sb.ToString(), cancellationToken);

        // Adding events guide
        await GenerateAddingEventsGuideAsync(context, extendingDir, cancellationToken);

        // Adding protocols guide
        await GenerateAddingProtocolsGuideAsync(context, extendingDir, cancellationToken);

        // Custom serializers guide
        await GenerateCustomSerializersGuideAsync(context, extendingDir, cancellationToken);
    }

    private async Task GenerateAddingEventsGuideAsync(GeneratorContext context, string extendingDir, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Adding New Events");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine($"Events are defined in the `{context.LibraryName}.Contracts` project. Each service has its own namespace and folder.");
        sb.AppendLine();

        sb.AppendLine("## Step-by-Step Guide");
        sb.AppendLine();
        sb.AppendLine("### 1. Identify the Service");
        sb.AppendLine();
        sb.AppendLine("Determine which service owns this event. Events should be grouped by the service that publishes them.");
        sb.AppendLine();

        sb.AppendLine("### 2. Create the Event Class");
        sb.AppendLine();
        sb.AppendLine($"Create a new class in `{context.LibraryName}.Contracts/{{ServiceName}}/`:");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine($"using {context.Namespace}.Messaging.Abstractions;");
        sb.AppendLine("using MessagePack;");
        sb.AppendLine();
        sb.AppendLine($"namespace {context.Namespace}.Contracts.MyService;");
        sb.AppendLine();
        sb.AppendLine("[MessagePackObject]");
        sb.AppendLine("public class MyNewEvent : EventBase");
        sb.AppendLine("{");
        sb.AppendLine("    [Key(0)]");
        sb.AppendLine("    public Guid EntityId { get; set; }");
        sb.AppendLine();
        sb.AppendLine("    [Key(1)]");
        sb.AppendLine("    public string Description { get; set; } = string.Empty;");
        sb.AppendLine();
        sb.AppendLine("    [Key(2)]");
        sb.AppendLine("    public DateTime OccurredAt { get; set; }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### 3. MessagePack Key Considerations");
        sb.AppendLine();
        sb.AppendLine("- **Keys must be unique** within the class");
        sb.AppendLine("- **Keys are immutable** - once published, never change a key number");
        sb.AppendLine("- **Start from 0** and increment sequentially");
        sb.AppendLine("- **Add new properties at the end** with the next available key");
        sb.AppendLine();

        sb.AppendLine("### 4. Backward Compatibility");
        sb.AppendLine();
        sb.AppendLine("When modifying events:");
        sb.AppendLine();
        sb.AppendLine("| Action | Safe? | Notes |");
        sb.AppendLine("|--------|-------|-------|");
        sb.AppendLine("| Add new property | ✅ | Use next available key |");
        sb.AppendLine("| Remove property | ⚠️ | Old messages may fail |");
        sb.AppendLine("| Rename property | ✅ | Key stays the same |");
        sb.AppendLine("| Change key number | ❌ | Breaks deserialization |");
        sb.AppendLine("| Change property type | ❌ | Breaks deserialization |");
        sb.AppendLine();

        sb.AppendLine("### 5. Publishing the Event");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public class MyService");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly IEventBus _eventBus;");
        sb.AppendLine();
        sb.AppendLine("    public async Task DoWorkAsync()");
        sb.AppendLine("    {");
        sb.AppendLine("        // ... business logic ...");
        sb.AppendLine();
        sb.AppendLine("        await _eventBus.PublishAsync(new MyNewEvent");
        sb.AppendLine("        {");
        sb.AppendLine("            EntityId = Guid.NewGuid(),");
        sb.AppendLine("            Description = \"Something happened\",");
        sb.AppendLine("            OccurredAt = DateTime.UtcNow");
        sb.AppendLine("        });");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### 6. Handling the Event");
        sb.AppendLine();
        sb.AppendLine("Create a handler in your consuming service:");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public class MyNewEventHandler : IEventHandler<MyNewEvent>");
        sb.AppendLine("{");
        sb.AppendLine("    public Task HandleAsync(MyNewEvent @event, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Handle the event");
        sb.AppendLine("        return Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");

        var eventsPath = FileSystem.Path.Combine(extendingDir, "adding-events.md");
        await WriteFileAsync(eventsPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateAddingProtocolsGuideAsync(GeneratorContext context, string extendingDir, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Adding New Protocols");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("To add a new messaging protocol, you need to implement the `IEventBus` interface and provide a message serializer.");
        sb.AppendLine();

        sb.AppendLine("## Implementation Steps");
        sb.AppendLine();

        sb.AppendLine("### 1. Create a New Project");
        sb.AppendLine();
        sb.AppendLine($"Create a new class library: `{context.LibraryName}.Messaging.MyProtocol`");
        sb.AppendLine();
        sb.AppendLine("Add project references:");
        sb.AppendLine();
        sb.AppendLine("```xml");
        sb.AppendLine("<ItemGroup>");
        sb.AppendLine($"  <ProjectReference Include=\"..\\{context.LibraryName}.Messaging.Abstractions\\{context.LibraryName}.Messaging.Abstractions.csproj\" />");
        sb.AppendLine("</ItemGroup>");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### 2. Create Options Class");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public class MyProtocolOptions");
        sb.AppendLine("{");
        sb.AppendLine("    public string ConnectionString { get; set; } = string.Empty;");
        sb.AppendLine("    public int Timeout { get; set; } = 30000;");
        sb.AppendLine("    // Add protocol-specific options");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### 3. Implement IEventBus");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine($"using {context.Namespace}.Messaging.Abstractions;");
        sb.AppendLine();
        sb.AppendLine("public class MyProtocolEventBus : IEventBus");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly MyProtocolOptions _options;");
        sb.AppendLine("    private readonly IMessageSerializer _serializer;");
        sb.AppendLine();
        sb.AppendLine("    public MyProtocolEventBus(");
        sb.AppendLine("        IOptions<MyProtocolOptions> options,");
        sb.AppendLine("        IMessageSerializer serializer)");
        sb.AppendLine("    {");
        sb.AppendLine("        _options = options.Value;");
        sb.AppendLine("        _serializer = serializer;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)");
        sb.AppendLine("        where TEvent : IEvent");
        sb.AppendLine("    {");
        sb.AppendLine("        var bytes = _serializer.Serialize(@event);");
        sb.AppendLine("        // Send bytes using your protocol");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public Task SubscribeAsync<TEvent>(");
        sb.AppendLine("        Func<TEvent, CancellationToken, Task> handler,");
        sb.AppendLine("        CancellationToken ct = default)");
        sb.AppendLine("        where TEvent : IEvent");
        sb.AppendLine("    {");
        sb.AppendLine("        // Implement subscription logic");
        sb.AppendLine("        // Call handler when messages arrive");
        sb.AppendLine("        return Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### 4. Create Extension Method");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public static class ServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddMyProtocolEventBus(");
        sb.AppendLine("        this IServiceCollection services,");
        sb.AppendLine("        Action<MyProtocolOptions> configure)");
        sb.AppendLine("    {");
        sb.AppendLine("        services.Configure(configure);");
        sb.AppendLine("        services.AddSingleton<IEventBus, MyProtocolEventBus>();");
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### 5. Update the Aggregator Project");
        sb.AppendLine();
        sb.AppendLine($"Add a reference to your new project in `{context.SolutionName}.{context.LibraryName}.csproj`:");
        sb.AppendLine();
        sb.AppendLine("```xml");
        sb.AppendLine("<ItemGroup>");
        sb.AppendLine($"  <ProjectReference Include=\"..\\{context.LibraryName}.Messaging.MyProtocol\\{context.LibraryName}.Messaging.MyProtocol.csproj\" />");
        sb.AppendLine("</ItemGroup>");
        sb.AppendLine("```");

        var protocolsPath = FileSystem.Path.Combine(extendingDir, "adding-protocols.md");
        await WriteFileAsync(protocolsPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateCustomSerializersGuideAsync(GeneratorContext context, string extendingDir, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Custom Serializers");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("The `IMessageSerializer` interface allows you to implement custom serialization formats for your messages.");
        sb.AppendLine();

        sb.AppendLine("## Interface Definition");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public interface IMessageSerializer");
        sb.AppendLine("{");
        sb.AppendLine("    byte[] Serialize<T>(T message);");
        sb.AppendLine("    T Deserialize<T>(byte[] data);");
        sb.AppendLine("    object Deserialize(byte[] data, Type type);");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Example: Protocol Buffers Serializer");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("using Google.Protobuf;");
        sb.AppendLine();
        sb.AppendLine("public class ProtobufMessageSerializer : IMessageSerializer");
        sb.AppendLine("{");
        sb.AppendLine("    public byte[] Serialize<T>(T message)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (message is IMessage protoMessage)");
        sb.AppendLine("        {");
        sb.AppendLine("            return protoMessage.ToByteArray();");
        sb.AppendLine("        }");
        sb.AppendLine("        throw new ArgumentException(\"Message must implement IMessage\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public T Deserialize<T>(byte[] data)");
        sb.AppendLine("    {");
        sb.AppendLine("        var parser = (MessageParser<T>)typeof(T)");
        sb.AppendLine("            .GetProperty(\"Parser\", BindingFlags.Public | BindingFlags.Static)");
        sb.AppendLine("            ?.GetValue(null);");
        sb.AppendLine("        return parser!.ParseFrom(data);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public object Deserialize(byte[] data, Type type)");
        sb.AppendLine("    {");
        sb.AppendLine("        var method = GetType().GetMethod(nameof(Deserialize), new[] { typeof(byte[]) });");
        sb.AppendLine("        var generic = method!.MakeGenericMethod(type);");
        sb.AppendLine("        return generic.Invoke(this, new object[] { data })!;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Registering Custom Serializers");
        sb.AppendLine();
        sb.AppendLine("Register your serializer in the DI container:");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("services.AddSingleton<IMessageSerializer, ProtobufMessageSerializer>();");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Considerations");
        sb.AppendLine();
        sb.AppendLine("When implementing custom serializers, consider:");
        sb.AppendLine();
        sb.AppendLine("- **Performance**: Serialization/deserialization is on the hot path");
        sb.AppendLine("- **Compatibility**: Ensure all services use compatible serializer versions");
        sb.AppendLine("- **Schema Evolution**: Plan for how to handle message schema changes");
        sb.AppendLine("- **Error Handling**: Provide clear error messages for serialization failures");

        var serializersPath = FileSystem.Path.Combine(extendingDir, "custom-serializers.md");
        await WriteFileAsync(serializersPath, sb.ToString(), cancellationToken);
    }

    private async Task GenerateApiReferenceAsync(GeneratorContext context, string docsDir, CancellationToken cancellationToken)
    {
        var apiDir = FileSystem.Path.Combine(docsDir, "api");
        FileSystem.Directory.CreateDirectory(apiDir);

        var sb = new StringBuilder();
        sb.AppendLine("# API Reference");
        sb.AppendLine();
        sb.AppendLine("## Core Interfaces");
        sb.AppendLine();

        sb.AppendLine("### IEvent");
        sb.AppendLine();
        sb.AppendLine("Base interface for all events.");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public interface IEvent");
        sb.AppendLine("{");
        sb.AppendLine("    Guid EventId { get; }");
        sb.AppendLine("    DateTime Timestamp { get; }");
        sb.AppendLine("    string EventType { get; }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### IEventBus");
        sb.AppendLine();
        sb.AppendLine("Interface for publish/subscribe operations.");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public interface IEventBus");
        sb.AppendLine("{");
        sb.AppendLine("    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)");
        sb.AppendLine("        where TEvent : IEvent;");
        sb.AppendLine();
        sb.AppendLine("    Task SubscribeAsync<TEvent>(");
        sb.AppendLine("        Func<TEvent, CancellationToken, Task> handler,");
        sb.AppendLine("        CancellationToken cancellationToken = default)");
        sb.AppendLine("        where TEvent : IEvent;");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### IMessageSerializer");
        sb.AppendLine();
        sb.AppendLine("Interface for message serialization.");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public interface IMessageSerializer");
        sb.AppendLine("{");
        sb.AppendLine("    byte[] Serialize<T>(T message);");
        sb.AppendLine("    T Deserialize<T>(byte[] data);");
        sb.AppendLine("    object Deserialize(byte[] data, Type type);");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("### IEventHandler<TEvent>");
        sb.AppendLine();
        sb.AppendLine("Interface for event handlers.");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public interface IEventHandler<in TEvent> where TEvent : IEvent");
        sb.AppendLine("{");
        sb.AppendLine("    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Domain Types");
        sb.AppendLine();

        sb.AppendLine("### Result<T>");
        sb.AppendLine();
        sb.AppendLine("Railway-oriented result type for error handling.");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("public readonly struct Result<T>");
        sb.AppendLine("{");
        sb.AppendLine("    public bool IsSuccess { get; }");
        sb.AppendLine("    public bool IsFailure => !IsSuccess;");
        sb.AppendLine("    public T Value { get; }");
        sb.AppendLine("    public Error Error { get; }");
        sb.AppendLine();
        sb.AppendLine("    public static Result<T> Success(T value);");
        sb.AppendLine("    public static Result<T> Failure(Error error);");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        if (context.Config.Domain?.StronglyTypedIds?.Count > 0)
        {
            sb.AppendLine("### Strongly-Typed IDs");
            sb.AppendLine();

            foreach (var id in context.Config.Domain.StronglyTypedIds)
            {
                sb.AppendLine($"#### {id.Name}");
                sb.AppendLine();
                sb.AppendLine($"Underlying type: `{id.UnderlyingType}`");
                sb.AppendLine();
            }
        }

        if (context.Config.Domain?.ValueObjects?.Count > 0)
        {
            sb.AppendLine("### Value Objects");
            sb.AppendLine();

            foreach (var vo in context.Config.Domain.ValueObjects)
            {
                sb.AppendLine($"#### {vo.Name}");
                sb.AppendLine();
                if (vo.Properties?.Count > 0)
                {
                    sb.AppendLine("Properties:");
                    foreach (var prop in vo.Properties)
                    {
                        sb.AppendLine($"- `{prop.Name}`: {prop.Type}");
                    }

                    sb.AppendLine();
                }
            }
        }

        var apiPath = FileSystem.Path.Combine(apiDir, "README.md");
        await WriteFileAsync(apiPath, sb.ToString(), cancellationToken);
    }

    private bool HasMessagingInfrastructure(SharedLibraryConfig config)
    {
        return config.MessagingInfrastructure?.IncludeRetryPolicies == true
            || config.MessagingInfrastructure?.IncludeCircuitBreaker == true
            || config.MessagingInfrastructure?.IncludeDeadLetterQueue == true
            || config.MessagingInfrastructure?.IncludeMessageValidation == true
            || config.MessagingInfrastructure?.IncludeDistributedTracing == true
            || config.MessagingInfrastructure?.IncludeMessageVersioning == true;
    }
}
