// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Queries.GetALaCarteRequests;

public class GetALaCarteRequestsResponse
{
    public List<ALaCarteRequestDto> ALaCarteRequests { get; set; } = new();
}

public class ALaCarteRequestDto
{
    public Guid ALaCarteRequestId { get; set; }
    public OutputType OutputType { get; set; }
    public string Directory { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
}
