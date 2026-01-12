// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.RedisPubSub.Models;

/// <summary>
/// Represents the message type registry model for Redis Pub/Sub messaging.
/// </summary>
public class MessageTypeRegistryModel
{
    /// <summary>
    /// Gets or sets the name of the registry class.
    /// </summary>
    public string Name { get; set; } = "MessageTypeRegistry";

    /// <summary>
    /// Gets or sets the namespace for the registry.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;
}
