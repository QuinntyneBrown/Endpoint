// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.RedisPubSub.Models;

/// <summary>
/// Represents the message envelope model for Redis Pub/Sub messaging.
/// </summary>
public class MessageEnvelopeModel
{
    /// <summary>
    /// Gets or sets the name of the envelope class.
    /// </summary>
    public string Name { get; set; } = "MessageEnvelope";

    /// <summary>
    /// Gets or sets the namespace for the envelope.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to include generic payload type.
    /// </summary>
    public bool GenericPayload { get; set; } = true;
}
