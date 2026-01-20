// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.Messaging.Abstractions project.
/// </summary>
public class AbstractionsProjectGenerator : ProjectGeneratorBase
{
    public AbstractionsProjectGenerator(
        ILogger<AbstractionsProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 0;

    public override bool ShouldGenerate(SharedLibraryConfig config) => true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.Abstractions";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.Abstractions";

        Logger.LogInformation("Generating {ProjectName}", projectName);

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            packageReferences: new List<string>
            {
                "<PackageReference Include=\"Microsoft.Extensions.DependencyInjection.Abstractions\" Version=\"9.0.0\" />",
            },
            cancellationToken: cancellationToken);

        // Generate IEvent interface
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "IEvent.cs"),
            GenerateIEventInterface(ns),
            cancellationToken);

        // Generate EventBase class
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "EventBase.cs"),
            GenerateEventBaseClass(ns),
            cancellationToken);

        // Generate IEventBus interface
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "IEventBus.cs"),
            GenerateIEventBusInterface(ns),
            cancellationToken);

        // Generate IMessageSerializer interface
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "IMessageSerializer.cs"),
            GenerateIMessageSerializerInterface(ns),
            cancellationToken);

        // Generate ServiceCollectionExtensions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "ServiceCollectionExtensions.cs"),
            GenerateServiceCollectionExtensions(ns),
            cancellationToken);

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var projectName = $"{context.LibraryName}.Messaging.Abstractions";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/IEvent.cs");
        preview.Files.Add($"{basePath}/EventBase.cs");
        preview.Files.Add($"{basePath}/IEventBus.cs");
        preview.Files.Add($"{basePath}/IMessageSerializer.cs");
        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        return Task.FromResult(preview);
    }

    private string GenerateIEventInterface(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Marker interface for events.
/// </summary>
public interface IEvent
{{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    Guid EventId {{ get; }}

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTimeOffset Timestamp {{ get; }}

    /// <summary>
    /// Gets the correlation ID for distributed tracing.
    /// </summary>
    string? CorrelationId {{ get; }}
}}
";
    }

    private string GenerateEventBaseClass(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Base class for events providing common functionality.
/// </summary>
public abstract class EventBase : IEvent
{{
    /// <inheritdoc />
    public Guid EventId {{ get; init; }} = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset Timestamp {{ get; init; }} = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string? CorrelationId {{ get; init; }}
}}
";
    }

    private string GenerateIEventBusInterface(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Interface for publishing and subscribing to events.
/// </summary>
public interface IEventBus
{{
    /// <summary>
    /// Publishes an event.
    /// </summary>
    /// <typeparam name=""TEvent"">The event type.</typeparam>
    /// <param name=""event"">The event to publish.</param>
    /// <param name=""cancellationToken"">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name=""TEvent"">The event type.</typeparam>
    /// <param name=""handler"">The handler to invoke when an event is received.</param>
    /// <param name=""cancellationToken"">Cancellation token.</param>
    Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes from events of a specific type.
    /// </summary>
    /// <typeparam name=""TEvent"">The event type.</typeparam>
    Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent;
}}
";
    }

    private string GenerateIMessageSerializerInterface(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Interface for message serialization.
/// </summary>
public interface IMessageSerializer
{{
    /// <summary>
    /// Serializes an object to bytes.
    /// </summary>
    /// <typeparam name=""T"">The type to serialize.</typeparam>
    /// <param name=""value"">The value to serialize.</param>
    /// <returns>Serialized bytes.</returns>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Deserializes bytes to an object.
    /// </summary>
    /// <typeparam name=""T"">The type to deserialize to.</typeparam>
    /// <param name=""data"">The data to deserialize.</param>
    /// <returns>Deserialized object.</returns>
    T? Deserialize<T>(byte[] data);

    /// <summary>
    /// Deserializes bytes to an object of a specific type.
    /// </summary>
    /// <param name=""data"">The data to deserialize.</param>
    /// <param name=""type"">The type to deserialize to.</param>
    /// <returns>Deserialized object.</returns>
    object? Deserialize(byte[] data, Type type);
}}
";
    }

    private string GenerateServiceCollectionExtensions(string ns)
    {
        return $@"// Auto-generated code
using Microsoft.Extensions.DependencyInjection;

namespace {ns};

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{{
    /// <summary>
    /// Adds messaging abstractions to the service collection.
    /// </summary>
    /// <param name=""services"">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessagingAbstractions(this IServiceCollection services)
    {{
        // Base abstractions don't register anything by default
        // This is here for consistency and future extensibility
        return services;
    }}
}}
";
    }
}
