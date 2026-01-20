// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Configuration for a service and its messages.
/// </summary>
public class ServiceConfig
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of events.
    /// </summary>
    public List<EventConfig> Events { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of commands.
    /// </summary>
    public List<CommandConfig> Commands { get; set; } = new();
}

/// <summary>
/// Configuration for an event.
/// </summary>
public class EventConfig
{
    /// <summary>
    /// Gets or sets the event name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event properties.
    /// </summary>
    public List<PropertyConfig> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets the routing key for pub/sub.
    /// </summary>
    public string? RoutingKey { get; set; }
}

/// <summary>
/// Configuration for a command.
/// </summary>
public class CommandConfig
{
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command properties.
    /// </summary>
    public List<PropertyConfig> Properties { get; set; } = new();
}

/// <summary>
/// Configuration for a property.
/// </summary>
public class PropertyConfig
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property type.
    /// </summary>
    public string Type { get; set; } = "string";

    /// <summary>
    /// Gets or sets the MessagePack key (for ordering).
    /// </summary>
    public int? Key { get; set; }

    /// <summary>
    /// Gets or sets whether the property is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }
}
