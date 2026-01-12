// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;

namespace Endpoint.Engineering.RedisPubSub.Artifacts;

/// <summary>
/// Represents a messaging project model that includes MessagePack serialization support.
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
    /// <param name="solutionName">The name of the solution.</param>
    /// <param name="parentDirectory">The parent directory for the project.</param>
    public MessagingProjectModel(string solutionName, string parentDirectory)
        : base(DotNetProjectType.ClassLib, $"{solutionName}.Messaging", parentDirectory)
    {
        SolutionName = solutionName;

        // Add MessagePack package
        Packages.Add(new PackageModel("MessagePack", "2.6.100-alpha"));
    }

    /// <summary>
    /// Gets or sets the solution name.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use LZ4 compression with MessagePack.
    /// </summary>
    public bool UseLz4Compression { get; set; } = true;
}
