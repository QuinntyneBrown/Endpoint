// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.CreateALaCarteRequest;

public class CreateALaCarteRequestRequest : IRequest<CreateALaCarteRequestResponse>
{
    public OutputType OutputType { get; set; } = OutputType.NotSpecified;
    public string Directory { get; set; } = string.Empty;
    public string SolutionName { get; set; } = "ALaCarte.sln";
}
