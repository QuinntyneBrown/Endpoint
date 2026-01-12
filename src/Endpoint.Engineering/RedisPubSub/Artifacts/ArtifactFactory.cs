// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Artifacts;
using Endpoint.Engineering.RedisPubSub.Models;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.RedisPubSub.Artifacts;

/// <summary>
/// Factory for creating messaging artifacts.
/// </summary>
public class ArtifactFactory : IArtifactFactory
{
    private readonly ILogger<ArtifactFactory> _logger;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileSystem">The file system abstraction.</param>
    public ArtifactFactory(ILogger<ArtifactFactory> logger, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public async Task<MessagingProjectModel> CreateMessagingProjectAsync(MessagingModel model, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating messaging project for solution: {SolutionName}", model.SolutionName);

        var projectModel = new MessagingProjectModel(model.SolutionName, model.Directory)
        {
            UseLz4Compression = model.UseLz4Compression
        };

        var messagesDirectory = _fileSystem.Path.Combine(projectModel.Directory, "Messages");
        var servicesDirectory = _fileSystem.Path.Combine(projectModel.Directory, "Services");

        // Add IMessage interface
        projectModel.Files.Add(CreateIMessageFile(projectModel.Namespace, messagesDirectory));

        // Add IDomainEvent interface
        projectModel.Files.Add(CreateIDomainEventFile(projectModel.Namespace, messagesDirectory));

        // Add ICommand interface
        projectModel.Files.Add(CreateICommandFile(projectModel.Namespace, messagesDirectory));

        // Add MessageHeader class
        projectModel.Files.Add(CreateMessageHeaderFile(projectModel.Namespace, messagesDirectory));

        // Add MessageEnvelope class
        projectModel.Files.Add(CreateMessageEnvelopeFile(projectModel.Namespace, messagesDirectory));

        // Add IMessageSerializer interface
        projectModel.Files.Add(CreateIMessageSerializerFile(projectModel.Namespace, servicesDirectory));

        // Add MessagePackMessageSerializer class
        projectModel.Files.Add(CreateMessagePackMessageSerializerFile(projectModel.Namespace, servicesDirectory, model.UseLz4Compression));

        // Add IMessageTypeRegistry interface
        projectModel.Files.Add(CreateIMessageTypeRegistryFile(projectModel.Namespace, servicesDirectory));

        // Add MessageTypeRegistry class
        projectModel.Files.Add(CreateMessageTypeRegistryFile(projectModel.Namespace, servicesDirectory));

        // Add ConfigureServices extension
        projectModel.Files.Add(CreateConfigureServicesFile(projectModel.Namespace, projectModel.Directory));

        return projectModel;
    }

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
            }
            """
        };
    }

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

    private static FileModel CreateMessageTypeRegistryFile(string @namespace, string directory)
    {
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

    private static FileModel CreateConfigureServicesFile(string @namespace, string directory)
    {
        return new FileModel("ConfigureServices", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using {{@namespace}}.Services;

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
            }
            """
        };
    }
}
