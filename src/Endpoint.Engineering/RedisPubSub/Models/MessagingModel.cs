// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.RedisPubSub.Models;

/// <summary>
/// Represents the complete messaging model for generating a messaging project.
/// </summary>
public class MessagingModel
{
    /// <summary>
    /// Gets or sets the solution name.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name (defaults to {SolutionName}.Messaging).
    /// </summary>
    public string ProjectName => $"{SolutionName}.Messaging";

    /// <summary>
    /// Gets or sets the namespace for the messaging project.
    /// </summary>
    public string Namespace => ProjectName;

    /// <summary>
    /// Gets or sets the directory where the project will be created.
    /// </summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use LZ4 compression with MessagePack.
    /// </summary>
    public bool UseLz4Compression { get; set; } = true;
}
