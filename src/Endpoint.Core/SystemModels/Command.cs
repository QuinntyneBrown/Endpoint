// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Endpoint.Core.SystemModels;

public class Command
{

    public string Name { get; set; }

    public Request Request { get; set; }

    public Response Response { get; set; }

    public Handler Handler { get; set; }

    public string MicroserviceName { get; set; }

    public Microservice Microservice { get; set; }

    public string ProjectName { get; set; }

    public Project Project { get; set; }

    public string SolutionName { get; set; }

    public Solution Solution { get; set; }
}
