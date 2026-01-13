// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Endpoint.Engineering.Messaging.Models;

/// <summary>
/// Represents a property definition for a message.
/// </summary>
public class MessagePropertyDefinition
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property type (e.g., "string", "int", "Guid", "DateTime", "decimal").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    /// <summary>
    /// Gets or sets whether the property is required.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the property is nullable.
    /// </summary>
    [JsonPropertyName("nullable")]
    public bool Nullable { get; set; }

    /// <summary>
    /// Gets or sets the property description for XML documentation.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default value for the property.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets whether the property is a collection.
    /// </summary>
    [JsonPropertyName("isCollection")]
    public bool IsCollection { get; set; }

    /// <summary>
    /// Gets or sets the collection type (e.g., "List", "IEnumerable", "Array").
    /// </summary>
    [JsonPropertyName("collectionType")]
    public string CollectionType { get; set; } = "List";
}
