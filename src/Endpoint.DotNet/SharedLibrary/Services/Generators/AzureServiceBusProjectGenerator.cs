// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.Messaging.AzureServiceBus project.
/// </summary>
public class AzureServiceBusProjectGenerator : ProjectGeneratorBase
{
    public AzureServiceBusProjectGenerator(
        ILogger<AzureServiceBusProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 12;

    public override bool ShouldGenerate(SharedLibraryConfig config)
        => config.Protocols.AzureServiceBus?.Enabled == true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.AzureServiceBus";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.AzureServiceBus";

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
                "<PackageReference Include=\"Azure.Messaging.ServiceBus\" Version=\"7.17.5\" />",
                "<PackageReference Include=\"MessagePack\" Version=\"2.5.172\" />",
            },
            projectReferences: new List<string>
            {
                $"../{abstractionsProject}/{abstractionsProject}.csproj",
            },
            cancellationToken: cancellationToken);

        var asbConfig = context.Config.Protocols.AzureServiceBus!;

        // Generate AzureServiceBusOptions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "AzureServiceBusOptions.cs"),
            GenerateAzureServiceBusOptions(ns, asbConfig),
            cancellationToken);

        // Generate AzureServiceBusEventBus
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "AzureServiceBusEventBus.cs"),
            GenerateAzureServiceBusEventBus(ns),
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
        var projectName = $"{context.LibraryName}.Messaging.AzureServiceBus";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/AzureServiceBusOptions.cs");
        preview.Files.Add($"{basePath}/AzureServiceBusEventBus.cs");
        preview.Files.Add($"{basePath}/MessagePackMessageSerializer.cs");
        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        return Task.FromResult(preview);
    }

    private string GenerateAzureServiceBusOptions(string ns, AzureServiceBusConfig config)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Options for Azure Service Bus event bus.
/// </summary>
public class AzureServiceBusOptions
{{
    /// <summary>
    /// Gets or sets the Azure Service Bus connection string.
    /// </summary>
    public string ConnectionString {{ get; set; }} = string.Empty;

    /// <summary>
    /// Gets or sets the default topic name.
    /// </summary>
    public string TopicName {{ get; set; }} = ""{config.DefaultTopic}"";

    /// <summary>
    /// Gets or sets the subscription name for this consumer.
    /// </summary>
    public string SubscriptionName {{ get; set; }} = string.Empty;

    /// <summary>
    /// Gets or sets whether to use sessions.
    /// </summary>
    public bool UseSessions {{ get; set; }} = {config.UseSessions.ToString().ToLowerInvariant()};
}}
";
    }

    private string GenerateAzureServiceBusEventBus(string ns)
    {
        var abstractionsNs = ns.Replace(".AzureServiceBus", ".Abstractions");
        return $@"// Auto-generated code
using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using {abstractionsNs};

namespace {ns};

/// <summary>
/// Azure Service Bus-based event bus implementation.
/// </summary>
public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
{{
    private readonly ILogger<AzureServiceBusEventBus> _logger;
    private readonly AzureServiceBusOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ConcurrentDictionary<Type, ServiceBusProcessor> _processors = new();
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private bool _disposed;

    public AzureServiceBusEventBus(
        ILogger<AzureServiceBusEventBus> logger,
        IOptions<AzureServiceBusOptions> options,
        IMessageSerializer serializer)
    {{
        _logger = logger;
        _options = options.Value;
        _serializer = serializer;
        _client = new ServiceBusClient(_options.ConnectionString);
        _sender = _client.CreateSender(_options.TopicName);
    }}

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {{
        var data = _serializer.Serialize(@event);
        var message = new ServiceBusMessage(data)
        {{
            ContentType = ""application/x-msgpack"",
            Subject = typeof(TEvent).Name,
            MessageId = @event.EventId.ToString(),
            CorrelationId = @event.CorrelationId ?? string.Empty,
        }};

        _logger.LogDebug(""Publishing {{EventType}} to topic {{Topic}}"", typeof(TEvent).Name, _options.TopicName);

        await _sender.SendMessageAsync(message, cancellationToken);
    }}

    public async Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {{
        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> {{ handler }},
            (_, list) => {{ list.Add(handler); return list; }});

        if (!_processors.ContainsKey(typeof(TEvent)))
        {{
            var processor = _client.CreateProcessor(
                _options.TopicName,
                _options.SubscriptionName,
                new ServiceBusProcessorOptions
                {{
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 10,
                }});

            processor.ProcessMessageAsync += async args =>
            {{
                if (args.Message.Subject == typeof(TEvent).Name)
                {{
                    try
                    {{
                        var @event = _serializer.Deserialize<TEvent>(args.Message.Body.ToArray());
                        if (@event != null && _handlers.TryGetValue(typeof(TEvent), out var handlers))
                        {{
                            foreach (var h in handlers)
                            {{
                                if (h is Func<TEvent, CancellationToken, Task> typedHandler)
                                {{
                                    await typedHandler(@event, args.CancellationToken);
                                }}
                            }}
                        }}

                        await args.CompleteMessageAsync(args.Message);
                    }}
                    catch (Exception ex)
                    {{
                        _logger.LogError(ex, ""Error handling event {{EventType}}"", typeof(TEvent).Name);
                        await args.AbandonMessageAsync(args.Message);
                    }}
                }}
            }};

            processor.ProcessErrorAsync += args =>
            {{
                _logger.LogError(args.Exception, ""Error processing Service Bus messages"");
                return Task.CompletedTask;
            }};

            _processors[typeof(TEvent)] = processor;
            await processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation(""Subscribed to {{EventType}} on topic {{Topic}}"", typeof(TEvent).Name, _options.TopicName);
        }}
    }}

    public async Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent
    {{
        _handlers.TryRemove(typeof(TEvent), out _);

        if (_processors.TryRemove(typeof(TEvent), out var processor))
        {{
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }}

        _logger.LogInformation(""Unsubscribed from {{EventType}}"", typeof(TEvent).Name);
    }}

    public async ValueTask DisposeAsync()
    {{
        if (_disposed) return;
        _disposed = true;

        foreach (var processor in _processors.Values)
        {{
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }}

        _processors.Clear();
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }}
}}
";
    }

    private string GenerateMessagePackSerializer(string ns)
    {
        var abstractionsNs = ns.Replace(".AzureServiceBus", ".Abstractions");
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
        var abstractionsNs = ns.Replace(".AzureServiceBus", ".Abstractions");
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
    /// Adds Azure Service Bus event bus to the service collection.
    /// </summary>
    /// <param name=""services"">The service collection.</param>
    /// <param name=""configure"">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureServiceBusEventBus(
        this IServiceCollection services,
        Action<AzureServiceBusOptions>? configure = null)
    {{
        if (configure != null)
        {{
            services.Configure(configure);
        }}

        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

        return services;
    }}
}}
";
    }
}
