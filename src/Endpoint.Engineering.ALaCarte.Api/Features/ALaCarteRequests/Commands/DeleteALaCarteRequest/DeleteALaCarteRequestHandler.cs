// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.DeleteALaCarteRequest;

public class DeleteALaCarteRequestHandler : IRequestHandler<DeleteALaCarteRequestRequest>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<DeleteALaCarteRequestHandler> _logger;

    public DeleteALaCarteRequestHandler(IALaCarteContext context, ILogger<DeleteALaCarteRequestHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteALaCarteRequestRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting ALaCarte request with ID: {ALaCarteRequestId}", request.ALaCarteRequestId);

        var alaCarteRequest = await _context.ALaCarteRequests
            .FirstOrDefaultAsync(r => r.ALaCarteRequestId == request.ALaCarteRequestId, cancellationToken);

        if (alaCarteRequest == null)
        {
            throw new InvalidOperationException($"ALaCarte request with ID {request.ALaCarteRequestId} not found");
        }

        _context.ALaCarteRequests.Remove(alaCarteRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted ALaCarte request with ID: {ALaCarteRequestId}", request.ALaCarteRequestId);
    }
}
