// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;

namespace Endpoint.Engineering.Api;

/// <summary>
/// Represents an API Gateway project model that includes YARP reverse proxy support.
/// </summary>
public class ApiGatewayModel : ProjectModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayModel"/> class.
    /// </summary>
    public ApiGatewayModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayModel"/> class.
    /// </summary>
    /// <param name="solutionName">The name of the solution.</param>
    /// <param name="parentDirectory">The parent directory for the project.</param>
    public ApiGatewayModel(string solutionName, string parentDirectory)
        : base(DotNetProjectType.Web, $"{solutionName}.ApiGateway", parentDirectory)
    {
        SolutionName = solutionName;

        // Add YARP package for reverse proxy
        Packages.Add(new PackageModel("Yarp.ReverseProxy", "2.2.0"));
        
        // Add JWT authentication support
        Packages.Add(new PackageModel("Microsoft.AspNetCore.Authentication.JwtBearer", "9.0.0"));
    }

    /// <summary>
    /// Gets or sets the solution name.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;
}
