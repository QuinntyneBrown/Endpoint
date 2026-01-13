// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Endpoint.Engineering.Messaging.Models;

/// <summary>
/// Represents a message definition following industry standards (CloudEvents/AsyncAPI inspired).
/// </summary>
public class MessageDefinition
{
    /// <summary>
    /// Gets or sets the message name (used as the class name).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message type discriminator for serialization.
    /// Follows CloudEvents type format: {domain}.{version}.{eventType}
    /// Example: "com.example.order.v1.OrderCreated"
    /// </summary>
    [JsonPropertyName("messageType")]
    public string? MessageType { get; set; }

    /// <summary>
    /// Gets or sets the message kind (Event, Command, Query).
    /// </summary>
    [JsonPropertyName("kind")]
    public MessageKind Kind { get; set; } = MessageKind.Event;

    /// <summary>
    /// Gets or sets the description for XML documentation.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the schema version for message evolution.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the properties of the message.
    /// </summary>
    [JsonPropertyName("properties")]
    public List<MessagePropertyDefinition> Properties { get; set; } = [];

    /// <summary>
    /// Gets or sets the aggregate type for domain events.
    /// </summary>
    [JsonPropertyName("aggregateType")]
    public string? AggregateType { get; set; }

    /// <summary>
    /// Gets or sets the channel/topic where this message is published.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// Gets the computed message type using CloudEvents format.
    /// </summary>
    public string ComputedMessageType => MessageType ?? $"{Name}";
}

/// <summary>
/// Defines the kind of message.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageKind
{
    /// <summary>
    /// An event that represents something that happened.
    /// </summary>
    Event,

    /// <summary>
    /// A command that represents an intent to do something.
    /// </summary>
    Command,

    /// <summary>
    /// A query that represents a request for data.
    /// </summary>
    Query
}
