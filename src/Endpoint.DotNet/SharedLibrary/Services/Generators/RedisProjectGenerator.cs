// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.Messaging.Redis project.
/// </summary>
public class RedisProjectGenerator : ProjectGeneratorBase
{
    public RedisProjectGenerator(
        ILogger<RedisProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 10;

    public override bool ShouldGenerate(SharedLibraryConfig config)
        => config.Protocols.Redis?.Enabled == true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.Redis";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.Redis";

        Logger.LogInformation("Generating {ProjectName}", projectName);

        var abstractionsProject = $"{context.LibraryName}.Messaging.Abstractions";

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            packageReferences: new List<string>
            {
                "<PackageReference Include=\"Microsoft.Extensions.DependencyInjection.Abstractions\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"Microsoft.Extensions.Logging.Abstractions\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"Microsoft.Extensions.Options\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"StackExchange.Redis\" Version=\"2.7.33\" />",
                "<PackageReference Include=\"MessagePack\" Version=\"2.5.172\" />",
            },
            projectReferences: new List<string>
            {
                $"../{abstractionsProject}/{abstractionsProject}.csproj",
            },
            cancellationToken: cancellationToken);

        // Generate RedisOptions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "RedisEventBusOptions.cs"),
            GenerateRedisOptions(ns, context.Config.Protocols.Redis!),
            cancellationToken);

        // Generate RedisEventBus
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "RedisEventBus.cs"),
            GenerateRedisEventBus(ns),
            cancellationToken);

        // Generate MessagePackSerializer implementation
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "MessagePackMessageSerializer.cs"),
            GenerateMessagePackSerializer(ns),
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
        var projectName = $"{context.LibraryName}.Messaging.Redis";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/RedisEventBusOptions.cs");
        preview.Files.Add($"{basePath}/RedisEventBus.cs");
        preview.Files.Add($"{basePath}/MessagePackMessageSerializer.cs");
        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        return Task.FromResult(preview);
    }

    private string GenerateRedisOptions(string ns, RedisConfig config)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Options for Redis event bus.
/// </summary>
public class RedisEventBusOptions
{{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    public string ConnectionString {{ get; set; }} = ""localhost:6379"";

    /// <summary>
    /// Gets or sets the channel prefix.
    /// </summary>
    public string ChannelPrefix {{ get; set; }} = ""{config.ChannelPrefix}"";
}}
";
    }

    private string GenerateRedisEventBus(string ns)
    {
        var abstractionsNs = ns.Replace(".Redis", ".Abstractions");
        return $@"// Auto-generated code
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using {abstractionsNs};

namespace {ns};

/// <summary>
/// Redis-based event bus implementation.
/// </summary>
public class RedisEventBus : IEventBus, IAsyncDisposable
{{
    private readonly ILogger<RedisEventBus> _logger;
    private readonly RedisEventBusOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ConnectionMultiplexer _connection;
    private readonly ISubscriber _subscriber;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private bool _disposed;

    public RedisEventBus(
        ILogger<RedisEventBus> logger,
        IOptions<RedisEventBusOptions> options,
        IMessageSerializer serializer)
    {{
        _logger = logger;
        _options = options.Value;
        _serializer = serializer;
        _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
        _subscriber = _connection.GetSubscriber();
    }}

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {{
        var channel = GetChannel<TEvent>();
        var data = _serializer.Serialize(@event);

        _logger.LogDebug(""Publishing {{EventType}} to channel {{Channel}}"", typeof(TEvent).Name, channel);

        await _subscriber.PublishAsync(RedisChannel.Literal(channel), data);
    }}

    public async Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {{
        var channel = GetChannel<TEvent>();

        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> {{ handler }},
            (_, list) => {{ list.Add(handler); return list; }});

        _logger.LogInformation(""Subscribing to {{EventType}} on channel {{Channel}}"", typeof(TEvent).Name, channel);

        await _subscriber.SubscribeAsync(RedisChannel.Literal(channel), async (_, message) =>
        {{
            try
            {{
                var @event = _serializer.Deserialize<TEvent>(message!);
                if (@event != null)
                {{
                    await handler(@event, cancellationToken);
                }}
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, ""Error handling event {{EventType}}"", typeof(TEvent).Name);
            }}
        }});
    }}

    public async Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent
    {{
        var channel = GetChannel<TEvent>();
        _handlers.TryRemove(typeof(TEvent), out _);

        _logger.LogInformation(""Unsubscribing from {{EventType}} on channel {{Channel}}"", typeof(TEvent).Name, channel);

        await _subscriber.UnsubscribeAsync(RedisChannel.Literal(channel));
    }}

    private string GetChannel<TEvent>() =>
        $""{{_options.ChannelPrefix}}:{{typeof(TEvent).Name}}"";

    public async ValueTask DisposeAsync()
    {{
        if (_disposed) return;
        _disposed = true;

        await _connection.CloseAsync();
        _connection.Dispose();
    }}
}}
";
    }

    private string GenerateMessagePackSerializer(string ns)
    {
        var abstractionsNs = ns.Replace(".Redis", ".Abstractions");
        return $@"// Auto-generated code
using MessagePack;
using {abstractionsNs};

namespace {ns};

/// <summary>
/// MessagePack-based message serializer.
/// </summary>
public class MessagePackMessageSerializer : IMessageSerializer
{{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public byte[] Serialize<T>(T value) =>
        MessagePackSerializer.Serialize(value, Options);

    public T? Deserialize<T>(byte[] data) =>
        MessagePackSerializer.Deserialize<T>(data, Options);

    public object? Deserialize(byte[] data, Type type) =>
        MessagePackSerializer.Deserialize(type, data, Options);
}}
";
    }

    private string GenerateServiceCollectionExtensions(string ns)
    {
        var abstractionsNs = ns.Replace(".Redis", ".Abstractions");
        return $@"// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using {abstractionsNs};

namespace {ns};

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{{
    /// <summary>
    /// Adds Redis event bus to the service collection.
    /// </summary>
    /// <param name=""services"">The service collection.</param>
    /// <param name=""configure"">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisEventBus(
        this IServiceCollection services,
        Action<RedisEventBusOptions>? configure = null)
    {{
        if (configure != null)
        {{
            services.Configure(configure);
        }}

        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IEventBus, RedisEventBus>();

        return services;
    }}
}}
";
    }
}
