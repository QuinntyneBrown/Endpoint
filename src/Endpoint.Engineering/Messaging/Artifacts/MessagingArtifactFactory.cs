// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text.Json;
using Endpoint.Artifacts;
using Endpoint.Engineering.Messaging.Models;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Messaging.Artifacts;

/// <summary>
/// Factory for creating messaging project artifacts.
/// </summary>
public class MessagingArtifactFactory : IMessagingArtifactFactory
{
    private readonly ILogger<MessagingArtifactFactory> _logger;
    private readonly IFileSystem _fileSystem;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingArtifactFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileSystem">The file system abstraction.</param>
    public MessagingArtifactFactory(ILogger<MessagingArtifactFactory> logger, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public async Task<MessagingProjectModel> CreateMessagingProjectFromFilesAsync(
        IEnumerable<string> definitionFilePaths,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        var allMessages = new List<MessageDefinition>();
        var allChannels = new List<ChannelDefinition>();
        MessagingProjectDefinition? primaryDefinition = null;

        foreach (var filePath in definitionFilePaths)
        {
            _logger.LogInformation("Reading message definition file: {FilePath}", filePath);

            if (!_fileSystem.File.Exists(filePath))
            {
                throw new FileNotFoundException($"Message definition file not found: {filePath}");
            }

            var json = await _fileSystem.File.ReadAllTextAsync(filePath, cancellationToken);
            var definition = JsonSerializer.Deserialize<MessagingProjectDefinition>(json, JsonOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize message definition file: {filePath}");

            primaryDefinition ??= definition;
            allMessages.AddRange(definition.Messages);

            if (definition.Channels != null)
            {
                allChannels.AddRange(definition.Channels);
            }
        }

        if (primaryDefinition == null)
        {
            throw new InvalidOperationException("No message definition files provided.");
        }

        // Merge all messages into the primary definition
        primaryDefinition.Messages = allMessages;
        if (allChannels.Count > 0)
        {
            primaryDefinition.Channels = allChannels;
        }

        return await CreateMessagingProjectAsync(primaryDefinition, outputDirectory, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MessagingProjectModel> CreateMessagingProjectAsync(
        MessagingProjectDefinition definition,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating messaging project: {ProjectName}", definition.ProjectName);

        var projectModel = new MessagingProjectModel(definition.ProjectName, outputDirectory, definition.IncludeRedisPubSub)
        {
            UseLz4Compression = definition.UseLz4Compression,
            Messages = definition.Messages,
            Channels = definition.Channels
        };

        var messagesDirectory = _fileSystem.Path.Combine(projectModel.Directory, "Messages");
        var servicesDirectory = _fileSystem.Path.Combine(projectModel.Directory, "Services");
        var pubSubDirectory = _fileSystem.Path.Combine(projectModel.Directory, "PubSub");

        // Add base interfaces
        projectModel.Files.Add(CreateIMessageFile(projectModel.Namespace, messagesDirectory));
        projectModel.Files.Add(CreateIDomainEventFile(projectModel.Namespace, messagesDirectory));
        projectModel.Files.Add(CreateICommandFile(projectModel.Namespace, messagesDirectory));
        projectModel.Files.Add(CreateIQueryFile(projectModel.Namespace, messagesDirectory));

        // Add message infrastructure
        projectModel.Files.Add(CreateMessageHeaderFile(projectModel.Namespace, messagesDirectory));
        projectModel.Files.Add(CreateMessageEnvelopeFile(projectModel.Namespace, messagesDirectory));

        // Add serialization services
        projectModel.Files.Add(CreateIMessageSerializerFile(projectModel.Namespace, servicesDirectory));
        projectModel.Files.Add(CreateMessagePackMessageSerializerFile(projectModel.Namespace, servicesDirectory, definition.UseLz4Compression));
        projectModel.Files.Add(CreateIMessageTypeRegistryFile(projectModel.Namespace, servicesDirectory));
        projectModel.Files.Add(CreateMessageTypeRegistryFile(projectModel.Namespace, servicesDirectory, definition.Messages));

        // Generate message classes from definitions
        foreach (var message in definition.Messages)
        {
            projectModel.Files.Add(CreateMessageFile(projectModel.Namespace, messagesDirectory, message));
        }

        // Add Redis pub/sub support if enabled
        if (definition.IncludeRedisPubSub)
        {
            projectModel.Files.Add(CreateIMessagePublisherFile(projectModel.Namespace, pubSubDirectory));
            projectModel.Files.Add(CreateRedisMessagePublisherFile(projectModel.Namespace, pubSubDirectory));
            projectModel.Files.Add(CreateIMessageSubscriberFile(projectModel.Namespace, pubSubDirectory));
            projectModel.Files.Add(CreateRedisMessageSubscriberFile(projectModel.Namespace, pubSubDirectory));
            projectModel.Files.Add(CreateRedisConnectionOptionsFile(projectModel.Namespace, pubSubDirectory));
        }

        // Add ConfigureServices extension
        projectModel.Files.Add(CreateConfigureServicesFile(projectModel.Namespace, projectModel.Directory, definition.IncludeRedisPubSub));

        return projectModel;
    }

    #region Base Interfaces

    private static FileModel CreateIMessageFile(string @namespace, string directory)
    {
        return new FileModel("IMessage", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// Marker interface for all messages.
            /// </summary>
            public interface IMessage
            {
            }
            """
        };
    }

    private static FileModel CreateIDomainEventFile(string @namespace, string directory)
    {
        return new FileModel("IDomainEvent", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// Domain event marker interface.
            /// </summary>
            public interface IDomainEvent : IMessage
            {
                /// <summary>
                /// Gets the aggregate ID.
                /// </summary>
                string AggregateId { get; }

                /// <summary>
                /// Gets the aggregate type.
                /// </summary>
                string AggregateType { get; }

                /// <summary>
                /// Gets the timestamp when the event occurred.
                /// </summary>
                DateTimeOffset OccurredAt { get; }
            }
            """
        };
    }

    private static FileModel CreateICommandFile(string @namespace, string directory)
    {
        return new FileModel("ICommand", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// Command marker interface.
            /// </summary>
            public interface ICommand : IMessage
            {
                /// <summary>
                /// Gets the target ID.
                /// </summary>
                string TargetId { get; }
            }
            """
        };
    }

    private static FileModel CreateIQueryFile(string @namespace, string directory)
    {
        return new FileModel("IQuery", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// Query marker interface.
            /// </summary>
            /// <typeparam name="TResult">The type of the query result.</typeparam>
            public interface IQuery<TResult> : IMessage
            {
            }
            """
        };
    }

    #endregion

    #region Message Infrastructure

    private static FileModel CreateMessageHeaderFile(string @namespace, string directory)
    {
        return new FileModel("MessageHeader", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using MessagePack;

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// Message header containing metadata for all messages.
            /// </summary>
            [MessagePackObject]
            public sealed class MessageHeader
            {
                /// <summary>
                /// Gets or sets the message type discriminator for deserialization.
                /// </summary>
                [Key(0)]
                public required string MessageType { get; init; }

                /// <summary>
                /// Gets or sets the message ID for idempotency.
                /// </summary>
                [Key(1)]
                public required string MessageId { get; init; }

                /// <summary>
                /// Gets or sets the correlation ID for distributed tracing.
                /// </summary>
                [Key(2)]
                public required string CorrelationId { get; init; }

                /// <summary>
                /// Gets or sets the causation ID for event chain tracking.
                /// </summary>
                [Key(3)]
                public required string CausationId { get; init; }

                /// <summary>
                /// Gets or sets the timestamp in Unix milliseconds.
                /// </summary>
                [Key(4)]
                public required long TimestampUnixMs { get; init; }

                /// <summary>
                /// Gets or sets the source service name.
                /// </summary>
                [Key(5)]
                public required string SourceService { get; init; }

                /// <summary>
                /// Gets or sets the schema version for evolution.
                /// </summary>
                [Key(6)]
                public int SchemaVersion { get; init; } = 1;

                /// <summary>
                /// Gets or sets the metadata dictionary for extensibility.
                /// </summary>
                [Key(7)]
                public Dictionary<string, string>? Metadata { get; init; }
            }
            """
        };
    }

    private static FileModel CreateMessageEnvelopeFile(string @namespace, string directory)
    {
        return new FileModel("MessageEnvelope", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using MessagePack;

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// Message envelope that wraps messages with header metadata.
            /// </summary>
            /// <typeparam name="TPayload">The type of the message payload.</typeparam>
            [MessagePackObject]
            public sealed class MessageEnvelope<TPayload> where TPayload : IMessage
            {
                /// <summary>
                /// Gets or sets the message header.
                /// </summary>
                [Key(0)]
                public MessageHeader Header { get; init; } = new()
                {
                    MessageType = string.Empty,
                    MessageId = string.Empty,
                    CorrelationId = string.Empty,
                    CausationId = string.Empty,
                    TimestampUnixMs = 0,
                    SourceService = string.Empty
                };

                /// <summary>
                /// Gets or sets the message payload.
                /// </summary>
                [Key(1)]
                public TPayload Payload { get; init; } = default!;

                /// <summary>
                /// Creates a new message envelope with the specified payload.
                /// </summary>
                /// <param name="payload">The message payload.</param>
                /// <param name="sourceService">The source service name.</param>
                /// <param name="correlationId">Optional correlation ID.</param>
                /// <param name="causationId">Optional causation ID.</param>
                /// <returns>A new message envelope.</returns>
                public static MessageEnvelope<TPayload> Create(
                    TPayload payload,
                    string sourceService,
                    string? correlationId = null,
                    string? causationId = null)
                {
                    var messageId = Guid.NewGuid().ToString();
                    return new MessageEnvelope<TPayload>
                    {
                        Header = new MessageHeader
                        {
                            MessageType = typeof(TPayload).Name,
                            MessageId = messageId,
                            CorrelationId = correlationId ?? messageId,
                            CausationId = causationId ?? messageId,
                            TimestampUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            SourceService = sourceService
                        },
                        Payload = payload
                    };
                }
            }
            """
        };
    }

    #endregion

    #region Serialization Services

    private static FileModel CreateIMessageSerializerFile(string @namespace, string directory)
    {
        return new FileModel("IMessageSerializer", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Messages;

            namespace {{@namespace}}.Services;

            /// <summary>
            /// Interface for message serialization.
            /// </summary>
            public interface IMessageSerializer
            {
                /// <summary>
                /// Serializes a message envelope to bytes.
                /// </summary>
                /// <typeparam name="T">The type of the message payload.</typeparam>
                /// <param name="envelope">The message envelope to serialize.</param>
                /// <returns>The serialized bytes.</returns>
                byte[] Serialize<T>(MessageEnvelope<T> envelope) where T : IMessage;

                /// <summary>
                /// Deserializes bytes to a message envelope.
                /// </summary>
                /// <typeparam name="T">The type of the message payload.</typeparam>
                /// <param name="data">The bytes to deserialize.</param>
                /// <returns>The deserialized message envelope, or null if deserialization fails.</returns>
                MessageEnvelope<T>? Deserialize<T>(ReadOnlyMemory<byte> data) where T : IMessage;

                /// <summary>
                /// Peeks at the header of a message without fully deserializing the payload.
                /// </summary>
                /// <param name="data">The bytes to peek.</param>
                /// <returns>A tuple containing the header and payload type, or null if peek fails.</returns>
                (MessageHeader Header, Type? PayloadType)? PeekHeader(ReadOnlyMemory<byte> data);
            }
            """
        };
    }

    private static FileModel CreateMessagePackMessageSerializerFile(string @namespace, string directory, bool useLz4Compression)
    {
        var compressionOption = useLz4Compression
            ? ".WithCompression(MessagePackCompression.Lz4BlockArray)"
            : string.Empty;

        return new FileModel("MessagePackMessageSerializer", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using MessagePack;
            using MessagePack.Resolvers;
            using {{@namespace}}.Messages;

            namespace {{@namespace}}.Services;

            /// <summary>
            /// MessagePack-based message serializer implementation.
            /// </summary>
            public sealed class MessagePackMessageSerializer : IMessageSerializer
            {
                private readonly IMessageTypeRegistry _registry;
                private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
                    .WithResolver(CompositeResolver.Create(
                        StandardResolver.Instance)){{compressionOption}}
                    .WithSecurity(MessagePackSecurity.UntrustedData);

                /// <summary>
                /// Initializes a new instance of the <see cref="MessagePackMessageSerializer"/> class.
                /// </summary>
                /// <param name="registry">The message type registry.</param>
                public MessagePackMessageSerializer(IMessageTypeRegistry registry)
                {
                    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
                }

                /// <inheritdoc/>
                public byte[] Serialize<T>(MessageEnvelope<T> envelope) where T : IMessage
                {
                    return MessagePackSerializer.Serialize(envelope, Options);
                }

                /// <inheritdoc/>
                public MessageEnvelope<T>? Deserialize<T>(ReadOnlyMemory<byte> data) where T : IMessage
                {
                    return MessagePackSerializer.Deserialize<MessageEnvelope<T>>(data, Options);
                }

                /// <inheritdoc/>
                public (MessageHeader Header, Type? PayloadType)? PeekHeader(ReadOnlyMemory<byte> data)
                {
                    try
                    {
                        var reader = new MessagePackReader(data);

                        var mapCount = reader.ReadMapHeader();
                        for (int i = 0; i < mapCount; i++)
                        {
                            var key = reader.ReadString();
                            if (key == "Header")
                            {
                                var header = MessagePackSerializer.Deserialize<MessageHeader>(ref reader, Options);
                                var payloadType = _registry.GetType(header.MessageType);
                                return (header, payloadType);
                            }
                            reader.Skip();
                        }
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            """
        };
    }

    private static FileModel CreateIMessageTypeRegistryFile(string @namespace, string directory)
    {
        return new FileModel("IMessageTypeRegistry", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Messages;

            namespace {{@namespace}}.Services;

            /// <summary>
            /// Interface for message type registration and lookup.
            /// </summary>
            public interface IMessageTypeRegistry
            {
                /// <summary>
                /// Registers a message type with a message type string.
                /// </summary>
                /// <typeparam name="T">The message type.</typeparam>
                /// <param name="messageType">The message type string.</param>
                void Register<T>(string messageType) where T : IMessage;

                /// <summary>
                /// Gets the CLR type for a message type string.
                /// </summary>
                /// <param name="messageType">The message type string.</param>
                /// <returns>The CLR type, or null if not found.</returns>
                Type? GetType(string messageType);

                /// <summary>
                /// Gets the message type string for a CLR type.
                /// </summary>
                /// <typeparam name="T">The message type.</typeparam>
                /// <returns>The message type string, or null if not found.</returns>
                string? GetMessageType<T>() where T : IMessage;

                /// <summary>
                /// Gets the message type string for a CLR type.
                /// </summary>
                /// <param name="type">The CLR type.</param>
                /// <returns>The message type string, or null if not found.</returns>
                string? GetMessageType(Type type);
            }
            """
        };
    }

    private static FileModel CreateMessageTypeRegistryFile(string @namespace, string directory, List<MessageDefinition> messages)
    {
        var registrations = string.Join(Environment.NewLine + "        ",
            messages.Select(m => $"Register<{m.Name}>(\"{m.ComputedMessageType}\");"));

        return new FileModel("MessageTypeRegistry", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Messages;

            namespace {{@namespace}}.Services;

            /// <summary>
            /// Implementation of message type registration and lookup.
            /// </summary>
            public sealed class MessageTypeRegistry : IMessageTypeRegistry
            {
                private readonly Dictionary<string, Type> _typeByName = new();
                private readonly Dictionary<Type, string> _nameByType = new();

                /// <summary>
                /// Initializes a new instance of the <see cref="MessageTypeRegistry"/> class.
                /// </summary>
                public MessageTypeRegistry()
                {
                    // Register all known message types
                    {{registrations}}
                }

                /// <inheritdoc/>
                public void Register<T>(string messageType) where T : IMessage
                {
                    _typeByName[messageType] = typeof(T);
                    _nameByType[typeof(T)] = messageType;
                }

                /// <inheritdoc/>
                public Type? GetType(string messageType)
                {
                    return _typeByName.GetValueOrDefault(messageType);
                }

                /// <inheritdoc/>
                public string? GetMessageType<T>() where T : IMessage
                {
                    return _nameByType.GetValueOrDefault(typeof(T));
                }

                /// <inheritdoc/>
                public string? GetMessageType(Type type)
                {
                    return _nameByType.GetValueOrDefault(type);
                }
            }
            """
        };
    }

    #endregion

    #region Message Generation

    private static FileModel CreateMessageFile(string @namespace, string directory, MessageDefinition message)
    {
        var interfaceType = message.Kind switch
        {
            MessageKind.Event => "IDomainEvent",
            MessageKind.Command => "ICommand",
            MessageKind.Query => "IMessage",
            _ => "IMessage"
        };

        var description = message.Description ?? $"Represents the {message.Name} message.";
        var properties = GenerateProperties(message);
        var additionalProperties = GenerateAdditionalInterfaceProperties(message);

        return new FileModel(message.Name, directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using MessagePack;

            namespace {{@namespace}}.Messages;

            /// <summary>
            /// {{description}}
            /// </summary>
            [MessagePackObject]
            public sealed class {{message.Name}} : {{interfaceType}}
            {
            {{properties}}{{additionalProperties}}}
            """
        };
    }

    private static string GenerateProperties(MessageDefinition message)
    {
        var properties = new List<string>();
        var keyIndex = 0;

        foreach (var prop in message.Properties)
        {
            var type = GetCSharpType(prop);
            var nullableMarker = prop.Nullable ? "?" : "";
            var requiredKeyword = prop.Required && !prop.Nullable ? "required " : "";
            var defaultValue = GetDefaultValue(prop);
            var description = prop.Description ?? $"Gets or sets the {prop.Name.ToLowerInvariant()}.";

            properties.Add($$"""
                /// <summary>
                /// {{description}}
                /// </summary>
                [Key({{keyIndex}})]
                public {{requiredKeyword}}{{type}}{{nullableMarker}} {{prop.Name}} { get; init; }{{defaultValue}}
            """);

            keyIndex++;
        }

        return string.Join(Environment.NewLine + Environment.NewLine, properties);
    }

    private static string GenerateAdditionalInterfaceProperties(MessageDefinition message)
    {
        if (message.Kind == MessageKind.Event)
        {
            var hasAggregateId = message.Properties.Any(p => p.Name == "AggregateId");
            var aggregateType = message.AggregateType ?? "Unknown";

            var additionalProps = new List<string>();

            if (!hasAggregateId)
            {
                additionalProps.Add($$"""

                /// <summary>
                /// Gets the aggregate ID.
                /// </summary>
                [IgnoreMember]
                public string AggregateId => {{GetAggregateIdExpression(message)}};
            """);
            }

            additionalProps.Add($$"""

                /// <summary>
                /// Gets the aggregate type.
                /// </summary>
                [IgnoreMember]
                public string AggregateType => "{{aggregateType}}";

                /// <summary>
                /// Gets the timestamp when the event occurred.
                /// </summary>
                [IgnoreMember]
                public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
            """);

            return string.Join(Environment.NewLine, additionalProps);
        }

        if (message.Kind == MessageKind.Command)
        {
            var hasTargetId = message.Properties.Any(p => p.Name == "TargetId");
            if (!hasTargetId)
            {
                return $$"""


                /// <summary>
                /// Gets the target ID.
                /// </summary>
                [IgnoreMember]
                public string TargetId => {{GetTargetIdExpression(message)}};
            """;
            }
        }

        return string.Empty;
    }

    private static string GetAggregateIdExpression(MessageDefinition message)
    {
        var idProp = message.Properties.FirstOrDefault(p =>
            p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
            (p.Type == "Guid" || p.Type == "string"));

        return idProp != null ? $"{idProp.Name}.ToString()" : "Guid.NewGuid().ToString()";
    }

    private static string GetTargetIdExpression(MessageDefinition message)
    {
        var idProp = message.Properties.FirstOrDefault(p =>
            p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
            (p.Type == "Guid" || p.Type == "string"));

        return idProp != null ? $"{idProp.Name}.ToString()" : "Guid.NewGuid().ToString()";
    }

    private static string GetCSharpType(MessagePropertyDefinition prop)
    {
        var baseType = prop.Type.ToLowerInvariant() switch
        {
            "string" => "string",
            "int" or "integer" => "int",
            "long" => "long",
            "short" => "short",
            "byte" => "byte",
            "bool" or "boolean" => "bool",
            "decimal" => "decimal",
            "double" => "double",
            "float" => "float",
            "guid" => "Guid",
            "datetime" => "DateTime",
            "datetimeoffset" => "DateTimeOffset",
            "timespan" => "TimeSpan",
            "byte[]" or "bytes" => "byte[]",
            _ => prop.Type
        };

        if (prop.IsCollection)
        {
            return prop.CollectionType switch
            {
                "Array" => $"{baseType}[]",
                "IEnumerable" => $"IEnumerable<{baseType}>",
                "IList" => $"IList<{baseType}>",
                _ => $"List<{baseType}>"
            };
        }

        return baseType;
    }

    private static string GetDefaultValue(MessagePropertyDefinition prop)
    {
        if (!string.IsNullOrEmpty(prop.DefaultValue))
        {
            return $" = {prop.DefaultValue};";
        }

        if (prop.IsCollection)
        {
            return prop.CollectionType switch
            {
                "Array" => $" = Array.Empty<{GetCSharpType(new MessagePropertyDefinition { Name = prop.Name, Type = prop.Type, Required = prop.Required, Nullable = prop.Nullable, Description = prop.Description, DefaultValue = prop.DefaultValue, IsCollection = false, CollectionType = prop.CollectionType })}>();",
                _ => " = [];"
            };
        }

        return "";
    }

    #endregion

    #region Redis Pub/Sub

    private static FileModel CreateIMessagePublisherFile(string @namespace, string directory)
    {
        return new FileModel("IMessagePublisher", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Messages;

            namespace {{@namespace}}.PubSub;

            /// <summary>
            /// Interface for publishing messages to Redis pub/sub.
            /// </summary>
            public interface IMessagePublisher
            {
                /// <summary>
                /// Publishes a message to the specified channel.
                /// </summary>
                /// <typeparam name="T">The type of the message payload.</typeparam>
                /// <param name="channel">The channel to publish to.</param>
                /// <param name="message">The message to publish.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation and contains the number of subscribers that received the message.</returns>
                Task<long> PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default) where T : IMessage;

                /// <summary>
                /// Publishes a message envelope to the specified channel.
                /// </summary>
                /// <typeparam name="T">The type of the message payload.</typeparam>
                /// <param name="channel">The channel to publish to.</param>
                /// <param name="envelope">The message envelope to publish.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation and contains the number of subscribers that received the message.</returns>
                Task<long> PublishEnvelopeAsync<T>(string channel, MessageEnvelope<T> envelope, CancellationToken cancellationToken = default) where T : IMessage;
            }
            """
        };
    }

    private static FileModel CreateRedisMessagePublisherFile(string @namespace, string directory)
    {
        return new FileModel("RedisMessagePublisher", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using Microsoft.Extensions.Logging;
            using StackExchange.Redis;
            using {{@namespace}}.Messages;
            using {{@namespace}}.Services;

            namespace {{@namespace}}.PubSub;

            /// <summary>
            /// Redis-based message publisher implementation.
            /// </summary>
            public sealed class RedisMessagePublisher : IMessagePublisher
            {
                private readonly IConnectionMultiplexer _redis;
                private readonly IMessageSerializer _serializer;
                private readonly ILogger<RedisMessagePublisher> _logger;
                private readonly string _serviceName;

                /// <summary>
                /// Initializes a new instance of the <see cref="RedisMessagePublisher"/> class.
                /// </summary>
                /// <param name="redis">The Redis connection multiplexer.</param>
                /// <param name="serializer">The message serializer.</param>
                /// <param name="logger">The logger.</param>
                /// <param name="options">The Redis connection options.</param>
                public RedisMessagePublisher(
                    IConnectionMultiplexer redis,
                    IMessageSerializer serializer,
                    ILogger<RedisMessagePublisher> logger,
                    RedisConnectionOptions options)
                {
                    _redis = redis ?? throw new ArgumentNullException(nameof(redis));
                    _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
                    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    _serviceName = options?.ServiceName ?? "Unknown";
                }

                /// <inheritdoc/>
                public async Task<long> PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default) where T : IMessage
                {
                    var envelope = MessageEnvelope<T>.Create(message, _serviceName);
                    return await PublishEnvelopeAsync(channel, envelope, cancellationToken);
                }

                /// <inheritdoc/>
                public async Task<long> PublishEnvelopeAsync<T>(string channel, MessageEnvelope<T> envelope, CancellationToken cancellationToken = default) where T : IMessage
                {
                    _logger.LogDebug("Publishing message {MessageType} to channel {Channel}", envelope.Header.MessageType, channel);

                    var data = _serializer.Serialize(envelope);
                    var subscriber = _redis.GetSubscriber();
                    var result = await subscriber.PublishAsync(RedisChannel.Literal(channel), data);

                    _logger.LogDebug("Message {MessageId} published to {SubscriberCount} subscribers", envelope.Header.MessageId, result);

                    return result;
                }
            }
            """
        };
    }

    private static FileModel CreateIMessageSubscriberFile(string @namespace, string directory)
    {
        return new FileModel("IMessageSubscriber", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Messages;

            namespace {{@namespace}}.PubSub;

            /// <summary>
            /// Interface for subscribing to messages from Redis pub/sub.
            /// </summary>
            public interface IMessageSubscriber
            {
                /// <summary>
                /// Subscribes to messages on the specified channel.
                /// </summary>
                /// <typeparam name="T">The type of the message payload.</typeparam>
                /// <param name="channel">The channel to subscribe to.</param>
                /// <param name="handler">The handler to invoke when a message is received.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                Task SubscribeAsync<T>(string channel, Func<MessageEnvelope<T>, Task> handler, CancellationToken cancellationToken = default) where T : IMessage;

                /// <summary>
                /// Unsubscribes from the specified channel.
                /// </summary>
                /// <param name="channel">The channel to unsubscribe from.</param>
                /// <param name="cancellationToken">The cancellation token.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default);
            }
            """
        };
    }

    private static FileModel CreateRedisMessageSubscriberFile(string @namespace, string directory)
    {
        return new FileModel("RedisMessageSubscriber", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using Microsoft.Extensions.Logging;
            using StackExchange.Redis;
            using {{@namespace}}.Messages;
            using {{@namespace}}.Services;

            namespace {{@namespace}}.PubSub;

            /// <summary>
            /// Redis-based message subscriber implementation.
            /// </summary>
            public sealed class RedisMessageSubscriber : IMessageSubscriber, IAsyncDisposable
            {
                private readonly IConnectionMultiplexer _redis;
                private readonly IMessageSerializer _serializer;
                private readonly ILogger<RedisMessageSubscriber> _logger;
                private readonly Dictionary<string, ChannelMessageQueue> _subscriptions = new();
                private readonly SemaphoreSlim _lock = new(1, 1);

                /// <summary>
                /// Initializes a new instance of the <see cref="RedisMessageSubscriber"/> class.
                /// </summary>
                /// <param name="redis">The Redis connection multiplexer.</param>
                /// <param name="serializer">The message serializer.</param>
                /// <param name="logger">The logger.</param>
                public RedisMessageSubscriber(
                    IConnectionMultiplexer redis,
                    IMessageSerializer serializer,
                    ILogger<RedisMessageSubscriber> logger)
                {
                    _redis = redis ?? throw new ArgumentNullException(nameof(redis));
                    _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
                    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                }

                /// <inheritdoc/>
                public async Task SubscribeAsync<T>(string channel, Func<MessageEnvelope<T>, Task> handler, CancellationToken cancellationToken = default) where T : IMessage
                {
                    await _lock.WaitAsync(cancellationToken);
                    try
                    {
                        _logger.LogInformation("Subscribing to channel {Channel} for message type {MessageType}", channel, typeof(T).Name);

                        var subscriber = _redis.GetSubscriber();
                        var queue = await subscriber.SubscribeAsync(RedisChannel.Literal(channel));

                        _subscriptions[channel] = queue;

                        _ = Task.Run(async () =>
                        {
                            await foreach (var message in queue)
                            {
                                try
                                {
                                    if (message.Message.HasValue)
                                    {
                                        var envelope = _serializer.Deserialize<T>((byte[])message.Message!);
                                        if (envelope != null)
                                        {
                                            await handler(envelope);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error processing message on channel {Channel}", channel);
                                }
                            }
                        }, cancellationToken);
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }

                /// <inheritdoc/>
                public async Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default)
                {
                    await _lock.WaitAsync(cancellationToken);
                    try
                    {
                        if (_subscriptions.TryGetValue(channel, out var queue))
                        {
                            await queue.UnsubscribeAsync();
                            _subscriptions.Remove(channel);
                            _logger.LogInformation("Unsubscribed from channel {Channel}", channel);
                        }
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }

                /// <inheritdoc/>
                public async ValueTask DisposeAsync()
                {
                    foreach (var subscription in _subscriptions.Values)
                    {
                        await subscription.UnsubscribeAsync();
                    }
                    _subscriptions.Clear();
                    _lock.Dispose();
                }
            }
            """
        };
    }

    private static FileModel CreateRedisConnectionOptionsFile(string @namespace, string directory)
    {
        return new FileModel("RedisConnectionOptions", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            namespace {{@namespace}}.PubSub;

            /// <summary>
            /// Configuration options for Redis connection.
            /// </summary>
            public class RedisConnectionOptions
            {
                /// <summary>
                /// Gets or sets the Redis connection string.
                /// </summary>
                public string ConnectionString { get; set; } = "localhost:6379";

                /// <summary>
                /// Gets or sets the service name for message headers.
                /// </summary>
                public string ServiceName { get; set; } = "Unknown";
            }
            """
        };
    }

    #endregion

    #region ConfigureServices

    private static FileModel CreateConfigureServicesFile(string @namespace, string directory, bool includeRedisPubSub)
    {
        var redisPubSubRegistrations = includeRedisPubSub
            ? """

                /// <summary>
                /// Adds Redis pub/sub messaging services to the service collection.
                /// </summary>
                /// <param name="services">The service collection.</param>
                /// <param name="configure">Action to configure Redis connection options.</param>
                /// <returns>The service collection for chaining.</returns>
                public static IServiceCollection AddMessagingWithRedis(
                    this IServiceCollection services,
                    Action<RedisConnectionOptions>? configure = null)
                {
                    services.AddMessagingServices();

                    var options = new RedisConnectionOptions();
                    configure?.Invoke(options);

                    services.AddSingleton(options);
                    services.AddSingleton<IConnectionMultiplexer>(sp =>
                        ConnectionMultiplexer.Connect(options.ConnectionString));
                    services.AddSingleton<IMessagePublisher, RedisMessagePublisher>();
                    services.AddSingleton<IMessageSubscriber, RedisMessageSubscriber>();

                    return services;
                }
            """
            : "";

        var additionalUsings = includeRedisPubSub
            ? $"""
            using StackExchange.Redis;
            using {@namespace}.PubSub;
            """
            : "";

        return new FileModel("ConfigureServices", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Services;
            {{additionalUsings}}

            namespace Microsoft.Extensions.DependencyInjection;

            /// <summary>
            /// Extension methods for configuring messaging services.
            /// </summary>
            public static class ConfigureServices
            {
                /// <summary>
                /// Adds messaging services to the service collection.
                /// </summary>
                /// <param name="services">The service collection.</param>
                /// <returns>The service collection for chaining.</returns>
                public static IServiceCollection AddMessagingServices(this IServiceCollection services)
                {
                    services.AddSingleton<IMessageTypeRegistry, MessageTypeRegistry>();
                    services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
                    return services;
                }
            {{redisPubSubRegistrations}}}
            """
        };
    }

    #endregion
}
