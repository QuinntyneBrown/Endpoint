// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Endpoint.Engineering.Messaging.Models;

/// <summary>
/// Represents the complete messaging project definition.
/// This model follows AsyncAPI specification patterns for message-driven architectures.
/// </summary>
public class MessagingProjectDefinition
{
    /// <summary>
    /// Gets or sets the schema version of this definition file.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "https://endpoint.dev/schemas/messaging/v1.0.json";

    /// <summary>
    /// Gets or sets the project name (without .Messaging suffix).
    /// </summary>
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root namespace for the messaging project.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the description of the messaging project.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the version of the messaging contract.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets whether to use LZ4 compression with MessagePack.
    /// </summary>
    [JsonPropertyName("useLz4Compression")]
    public bool UseLz4Compression { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include Redis pub/sub support.
    /// </summary>
    [JsonPropertyName("includeRedisPubSub")]
    public bool IncludeRedisPubSub { get; set; } = true;

    /// <summary>
    /// Gets or sets the message definitions.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<MessageDefinition> Messages { get; set; } = [];

    /// <summary>
    /// Gets or sets channel definitions for pub/sub patterns.
    /// </summary>
    [JsonPropertyName("channels")]
    public List<ChannelDefinition>? Channels { get; set; }

    /// <summary>
    /// Gets the computed namespace.
    /// </summary>
    public string ComputedNamespace => Namespace ?? $"{ProjectName}.Messaging";
}

/// <summary>
/// Represents a channel/topic definition for pub/sub messaging.
/// </summary>
public class ChannelDefinition
{
    /// <summary>
    /// Gets or sets the channel name (e.g., "orders", "notifications").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the channel.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the message types that are published to this channel.
    /// </summary>
    [JsonPropertyName("publishedMessages")]
    public List<string>? PublishedMessages { get; set; }

    /// <summary>
    /// Gets or sets the message types that are subscribed from this channel.
    /// </summary>
    [JsonPropertyName("subscribedMessages")]
    public List<string>? SubscribedMessages { get; set; }
}
