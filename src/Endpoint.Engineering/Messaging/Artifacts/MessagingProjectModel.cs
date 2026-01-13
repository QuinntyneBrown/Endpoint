// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.Engineering.Messaging.Models;

namespace Endpoint.Engineering.Messaging.Artifacts;

/// <summary>
/// Represents a messaging project model with MessagePack and Redis support.
/// </summary>
public class MessagingProjectModel : ProjectModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProjectModel"/> class.
    /// </summary>
    public MessagingProjectModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProjectModel"/> class.
    /// </summary>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="parentDirectory">The parent directory for the project.</param>
    /// <param name="includeRedisPubSub">Whether to include Redis pub/sub support.</param>
    public MessagingProjectModel(string projectName, string parentDirectory, bool includeRedisPubSub = true)
        : base(DotNetProjectType.ClassLib, $"{projectName}.Messaging", parentDirectory)
    {
        ProjectName = projectName;
        IncludeRedisPubSub = includeRedisPubSub;

        // Add MessagePack package
        Packages.Add(new PackageModel("MessagePack", "2.6.100-alpha"));

        // Add Redis packages if enabled
        if (includeRedisPubSub)
        {
            Packages.Add(new PackageModel("StackExchange.Redis", "2.8.16"));
        }

        // Add DI package for ConfigureServices
        Packages.Add(new PackageModel("Microsoft.Extensions.DependencyInjection.Abstractions", "9.0.0"));
        Packages.Add(new PackageModel("Microsoft.Extensions.Logging.Abstractions", "9.0.0"));
    }

    /// <summary>
    /// Gets or sets the project name (without .Messaging suffix).
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use LZ4 compression with MessagePack.
    /// </summary>
    public bool UseLz4Compression { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include Redis pub/sub support.
    /// </summary>
    public bool IncludeRedisPubSub { get; set; } = true;

    /// <summary>
    /// Gets or sets the message definitions for this project.
    /// </summary>
    public List<MessageDefinition> Messages { get; set; } = [];

    /// <summary>
    /// Gets or sets the channel definitions for this project.
    /// </summary>
    public List<ChannelDefinition>? Channels { get; set; }
}
