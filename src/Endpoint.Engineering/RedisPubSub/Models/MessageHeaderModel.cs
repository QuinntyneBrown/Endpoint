// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.RedisPubSub.Models;

/// <summary>
/// Represents the message header model for Redis Pub/Sub messaging.
/// </summary>
public class MessageHeaderModel
{
    /// <summary>
    /// Gets or sets the message type discriminator for deserialization.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message ID for idempotency.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation ID for distributed tracing.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the causation ID for event chain tracking.
    /// </summary>
    public string CausationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp in Unix milliseconds.
    /// </summary>
    public long TimestampUnixMs { get; set; }

    /// <summary>
    /// Gets or sets the source service name.
    /// </summary>
    public string SourceService { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version for evolution.
    /// </summary>
    public int SchemaVersion { get; set; } = 1;
}
