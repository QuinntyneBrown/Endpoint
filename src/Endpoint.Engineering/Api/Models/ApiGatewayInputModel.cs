// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.Api.Models;

/// <summary>
/// Represents the input model for generating an API Gateway project.
/// </summary>
public class ApiGatewayInputModel
{
    /// <summary>
    /// Gets or sets the solution name.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name (defaults to {SolutionName}.ApiGateway).
    /// </summary>
    public string ProjectName => $"{SolutionName}.ApiGateway";

    /// <summary>
    /// Gets or sets the namespace for the API Gateway project.
    /// </summary>
    public string Namespace => ProjectName;

    /// <summary>
    /// Gets or sets the directory where the project will be created.
    /// </summary>
    public string Directory { get; set; } = string.Empty;
}
