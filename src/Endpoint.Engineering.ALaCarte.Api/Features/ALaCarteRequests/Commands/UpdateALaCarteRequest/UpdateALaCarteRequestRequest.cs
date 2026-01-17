// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;
using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.UpdateALaCarteRequest;

public class UpdateALaCarteRequestRequest : IRequest
{
    public Guid ALaCarteRequestId { get; set; }
    public OutputType OutputType { get; set; } = OutputType.NotSpecified;
    public string Directory { get; set; } = string.Empty;
    public string SolutionName { get; set; } = "ALaCarte.sln";
}
