// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Queries.GetALaCarteRequest;

public class GetALaCarteRequestHandler : IRequestHandler<GetALaCarteRequestRequest, GetALaCarteRequestResponse?>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<GetALaCarteRequestHandler> _logger;

    public GetALaCarteRequestHandler(IALaCarteContext context, ILogger<GetALaCarteRequestHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetALaCarteRequestResponse?> Handle(GetALaCarteRequestRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ALaCarte request with ID: {ALaCarteRequestId}", request.ALaCarteRequestId);

        var alaCarteRequest = await _context.ALaCarteRequests
            .Where(r => r.ALaCarteRequestId == request.ALaCarteRequestId)
            .Select(r => new GetALaCarteRequestResponse
            {
                ALaCarteRequestId = r.ALaCarteRequestId,
                OutputType = r.OutputType,
                Directory = r.Directory,
                SolutionName = r.SolutionName
            })
            .FirstOrDefaultAsync(cancellationToken);

        return alaCarteRequest;
    }
}
