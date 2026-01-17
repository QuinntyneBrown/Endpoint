// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Queries.GetALaCarteRequests;

public class GetALaCarteRequestsHandler : IRequestHandler<GetALaCarteRequestsRequest, GetALaCarteRequestsResponse>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<GetALaCarteRequestsHandler> _logger;

    public GetALaCarteRequestsHandler(IALaCarteContext context, ILogger<GetALaCarteRequestsHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetALaCarteRequestsResponse> Handle(GetALaCarteRequestsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all ALaCarte requests");

        var requests = await _context.ALaCarteRequests
            .Select(r => new ALaCarteRequestDto
            {
                ALaCarteRequestId = r.ALaCarteRequestId,
                OutputType = r.OutputType,
                Directory = r.Directory,
                SolutionName = r.SolutionName
            })
            .ToListAsync(cancellationToken);

        return new GetALaCarteRequestsResponse
        {
            ALaCarteRequests = requests
        };
    }
}
