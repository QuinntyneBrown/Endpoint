// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.CreateALaCarteRequest;

public class CreateALaCarteRequestHandler : IRequestHandler<CreateALaCarteRequestRequest, CreateALaCarteRequestResponse>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<CreateALaCarteRequestHandler> _logger;

    public CreateALaCarteRequestHandler(IALaCarteContext context, ILogger<CreateALaCarteRequestHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateALaCarteRequestResponse> Handle(CreateALaCarteRequestRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating ALaCarte request");

        var alaCarteRequest = new ALaCarteRequest
        {
            ALaCarteRequestId = Guid.NewGuid(),
            OutputType = request.OutputType,
            Directory = request.Directory,
            SolutionName = request.SolutionName
        };

        _context.ALaCarteRequests.Add(alaCarteRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created ALaCarte request with ID: {ALaCarteRequestId}", alaCarteRequest.ALaCarteRequestId);

        return new CreateALaCarteRequestResponse
        {
            ALaCarteRequestId = alaCarteRequest.ALaCarteRequestId
        };
    }
}
