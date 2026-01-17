// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Queries.GetALaCarteRequest;

public class GetALaCarteRequestRequest : IRequest<GetALaCarteRequestResponse?>
{
    public Guid ALaCarteRequestId { get; set; }
}
