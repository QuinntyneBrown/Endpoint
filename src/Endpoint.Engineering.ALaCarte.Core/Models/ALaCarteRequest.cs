// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Core.Models;

/// <summary>
/// Request model for the ALaCarte operation.
/// </summary>
public class ALaCarteRequest
{
    /// <summary>
    /// The unique identifier for the ALaCarte request.
    /// </summary>
    public Guid ALaCarteRequestId { get; set; }

    /// <summary>
    /// The list of repository configurations to process.
    /// </summary>
    public List<RepositoryConfiguration> Repositories { get; set; } = new();

    /// <summary>
    /// The output type for the operation.
    /// </summary>
    public OutputType OutputType { get; set; } = OutputType.NotSpecified;

    /// <summary>
    /// The output directory where files will be copied to.
    /// Defaults to the current directory if not specified.
    /// </summary>
    public string Directory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// The name of the .NET solution to create (optional).
    /// Defaults to "ALaCarte.sln".
    /// </summary>
    public string SolutionName { get; set; } = "ALaCarte.sln";
}
