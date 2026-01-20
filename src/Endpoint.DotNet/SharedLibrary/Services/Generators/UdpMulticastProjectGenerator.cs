// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.Messaging.UdpMulticast project.
/// </summary>
public class UdpMulticastProjectGenerator : ProjectGeneratorBase
{
    public UdpMulticastProjectGenerator(
        ILogger<UdpMulticastProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 11;

    public override bool ShouldGenerate(SharedLibraryConfig config)
        => config.Protocols.UdpMulticast?.Enabled == true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.UdpMulticast";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.UdpMulticast";

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
                "<PackageReference Include=\"Microsoft.Extensions.Hosting.Abstractions\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"MessagePack\" Version=\"2.5.172\" />",
            },
            projectReferences: new List<string>
            {
                $"../{abstractionsProject}/{abstractionsProject}.csproj",
            },
            cancellationToken: cancellationToken);

        var udpConfig = context.Config.Protocols.UdpMulticast!;

        // Generate UdpMulticastOptions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "UdpMulticastOptions.cs"),
            GenerateUdpMulticastOptions(ns, udpConfig),
            cancellationToken);

        // Generate UdpMulticastEventBus
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "UdpMulticastEventBus.cs"),
            GenerateUdpMulticastEventBus(ns),
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
        var projectName = $"{context.LibraryName}.Messaging.UdpMulticast";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/UdpMulticastOptions.cs");
        preview.Files.Add($"{basePath}/UdpMulticastEventBus.cs");
        preview.Files.Add($"{basePath}/MessagePackMessageSerializer.cs");
        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        return Task.FromResult(preview);
    }

    private string GenerateUdpMulticastOptions(string ns, UdpMulticastConfig config)
    {
        return $@"// Auto-generated code
using System.Net;

namespace {ns};

/// <summary>
/// Options for UDP multicast event bus.
/// </summary>
public class UdpMulticastOptions
{{
    /// <summary>
    /// Gets or sets the multicast group address.
    /// </summary>
    public string MulticastGroup {{ get; set; }} = ""{config.DefaultMulticastGroup}"";

    /// <summary>
    /// Gets or sets the multicast port.
    /// </summary>
    public int Port {{ get; set; }} = {config.DefaultPort};

    /// <summary>
    /// Gets or sets the time-to-live for multicast packets.
    /// </summary>
    public int Ttl {{ get; set; }} = {config.DefaultTtl};

    /// <summary>
    /// Gets or sets the local interface to bind to.
    /// </summary>
    public string? LocalInterface {{ get; set; }}

    /// <summary>
    /// Gets the multicast group as IPAddress.
    /// </summary>
    public IPAddress MulticastGroupAddress => IPAddress.Parse(MulticastGroup);
}}
";
    }

    private string GenerateUdpMulticastEventBus(string ns)
    {
        var abstractionsNs = ns.Replace(".UdpMulticast", ".Abstractions");
        return $@"// Auto-generated code
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using {abstractionsNs};

namespace {ns};

/// <summary>
/// UDP multicast-based event bus implementation.
/// </summary>
public class UdpMulticastEventBus : IEventBus, IAsyncDisposable
{{
    private readonly ILogger<UdpMulticastEventBus> _logger;
    private readonly UdpMulticastOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly UdpClient _sender;
    private readonly UdpClient _receiver;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveTask;
    private bool _disposed;

    public UdpMulticastEventBus(
        ILogger<UdpMulticastEventBus> logger,
        IOptions<UdpMulticastOptions> options,
        IMessageSerializer serializer)
    {{
        _logger = logger;
        _options = options.Value;
        _serializer = serializer;

        // Setup sender
        _sender = new UdpClient();
        _sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _options.Ttl);

        // Setup receiver
        _receiver = new UdpClient();
        _receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _receiver.Client.Bind(new IPEndPoint(IPAddress.Any, _options.Port));
        _receiver.JoinMulticastGroup(_options.MulticastGroupAddress);

        // Start receiving
        _receiveTask = ReceiveLoop(_cts.Token);
    }}

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {{
        var typeName = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? typeof(TEvent).Name;
        var typeNameBytes = System.Text.Encoding.UTF8.GetBytes(typeName);
        var eventData = _serializer.Serialize(@event);

        // Format: [4 bytes type name length][type name][event data]
        var packet = new byte[4 + typeNameBytes.Length + eventData.Length];
        BitConverter.GetBytes(typeNameBytes.Length).CopyTo(packet, 0);
        typeNameBytes.CopyTo(packet, 4);
        eventData.CopyTo(packet, 4 + typeNameBytes.Length);

        var endpoint = new IPEndPoint(_options.MulticastGroupAddress, _options.Port);

        _logger.LogDebug(""Publishing {{EventType}} to {{Endpoint}}"", typeof(TEvent).Name, endpoint);

        await _sender.SendAsync(packet, endpoint, cancellationToken);
    }}

    public Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {{
        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> {{ handler }},
            (_, list) => {{ list.Add(handler); return list; }});

        _logger.LogInformation(""Subscribed to {{EventType}}"", typeof(TEvent).Name);

        return Task.CompletedTask;
    }}

    public Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent
    {{
        _handlers.TryRemove(typeof(TEvent), out _);
        _logger.LogInformation(""Unsubscribed from {{EventType}}"", typeof(TEvent).Name);
        return Task.CompletedTask;
    }}

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {{
        while (!cancellationToken.IsCancellationRequested)
        {{
            try
            {{
                var result = await _receiver.ReceiveAsync(cancellationToken);
                var packet = result.Buffer;

                if (packet.Length < 4) continue;

                // Parse packet
                var typeNameLength = BitConverter.ToInt32(packet, 0);
                if (packet.Length < 4 + typeNameLength) continue;

                var typeName = System.Text.Encoding.UTF8.GetString(packet, 4, typeNameLength);
                var eventData = new byte[packet.Length - 4 - typeNameLength];
                Array.Copy(packet, 4 + typeNameLength, eventData, 0, eventData.Length);

                // Find type and handlers
                var eventType = Type.GetType(typeName);
                if (eventType == null) continue;

                if (_handlers.TryGetValue(eventType, out var handlers))
                {{
                    var @event = _serializer.Deserialize(eventData, eventType);
                    if (@event != null)
                    {{
                        foreach (var handler in handlers)
                        {{
                            try
                            {{
                                var invokeMethod = handler.GetType().GetMethod(""Invoke"");
                                if (invokeMethod != null)
                                {{
                                    var task = invokeMethod.Invoke(handler, new[] {{ @event, cancellationToken }}) as Task;
                                    if (task != null)
                                    {{
                                        await task;
                                    }}
                                }}
                            }}
                            catch (Exception ex)
                            {{
                                _logger.LogError(ex, ""Error handling event {{EventType}}"", eventType.Name);
                            }}
                        }}
                    }}
                }}
            }}
            catch (OperationCanceledException)
            {{
                break;
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, ""Error in UDP receive loop"");
            }}
        }}
    }}

    public async ValueTask DisposeAsync()
    {{
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();

        if (_receiveTask != null)
        {{
            try {{ await _receiveTask; }} catch {{ }}
        }}

        _receiver.DropMulticastGroup(_options.MulticastGroupAddress);
        _receiver.Close();
        _sender.Close();
        _cts.Dispose();
    }}
}}
";
    }

    private string GenerateMessagePackSerializer(string ns)
    {
        var abstractionsNs = ns.Replace(".UdpMulticast", ".Abstractions");
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
        var abstractionsNs = ns.Replace(".UdpMulticast", ".Abstractions");
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
    /// Adds UDP multicast event bus to the service collection.
    /// </summary>
    /// <param name=""services"">The service collection.</param>
    /// <param name=""configure"">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUdpMulticastEventBus(
        this IServiceCollection services,
        Action<UdpMulticastOptions>? configure = null)
    {{
        if (configure != null)
        {{
            services.Configure(configure);
        }}

        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IEventBus, UdpMulticastEventBus>();

        return services;
    }}
}}
";
    }
}
