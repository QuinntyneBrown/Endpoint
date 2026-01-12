// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.RedisPubSub.Models;

/// <summary>
/// Represents the message serializer model for Redis Pub/Sub messaging.
/// </summary>
public class MessageSerializerModel
{
    /// <summary>
    /// Gets or sets the name of the serializer class.
    /// </summary>
    public string Name { get; set; } = "MessagePackMessageSerializer";

    /// <summary>
    /// Gets or sets the namespace for the serializer.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use LZ4 compression.
    /// </summary>
    public bool UseLz4Compression { get; set; } = true;
}
