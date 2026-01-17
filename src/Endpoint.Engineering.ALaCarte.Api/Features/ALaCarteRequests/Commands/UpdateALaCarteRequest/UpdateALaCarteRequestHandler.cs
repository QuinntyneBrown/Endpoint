// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.UpdateALaCarteRequest;

public class UpdateALaCarteRequestHandler : IRequestHandler<UpdateALaCarteRequestRequest>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<UpdateALaCarteRequestHandler> _logger;

    public UpdateALaCarteRequestHandler(IALaCarteContext context, ILogger<UpdateALaCarteRequestHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateALaCarteRequestRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating ALaCarte request with ID: {ALaCarteRequestId}", request.ALaCarteRequestId);

        var alaCarteRequest = await _context.ALaCarteRequests
            .FirstOrDefaultAsync(r => r.ALaCarteRequestId == request.ALaCarteRequestId, cancellationToken);

        if (alaCarteRequest == null)
        {
            throw new InvalidOperationException($"ALaCarte request with ID {request.ALaCarteRequestId} not found");
        }

        alaCarteRequest.OutputType = request.OutputType;
        alaCarteRequest.Directory = request.Directory;
        alaCarteRequest.SolutionName = request.SolutionName;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated ALaCarte request with ID: {ALaCarteRequestId}", request.ALaCarteRequestId);
    }
}
