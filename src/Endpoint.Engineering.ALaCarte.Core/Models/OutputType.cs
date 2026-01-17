// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Core.Models;

/// <summary>
/// Specifies the output type for the ALaCarte operation.
/// </summary>
public enum OutputType
{
    /// <summary>
    /// Not specified - no solution or workspace created.
    /// </summary>
    NotSpecified,

    /// <summary>
    /// Create a .NET solution containing all discovered .csproj files.
    /// </summary>
    DotNetSolution,

    /// <summary>
    /// Create a mixed structure with .NET solution and other folders.
    /// </summary>
    MixDotNetSolutionWithOtherFolders
}
