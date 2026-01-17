// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.DeleteALaCarteRequest;

public class DeleteALaCarteRequestRequest : IRequest
{
    public Guid ALaCarteRequestId { get; set; }
}
