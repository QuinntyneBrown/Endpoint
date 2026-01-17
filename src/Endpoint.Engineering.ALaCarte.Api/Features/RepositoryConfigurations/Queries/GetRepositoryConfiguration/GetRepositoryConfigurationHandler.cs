// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;

public class GetRepositoryConfigurationHandler : IRequestHandler<GetRepositoryConfigurationRequest, GetRepositoryConfigurationResponse?>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<GetRepositoryConfigurationHandler> _logger;

    public GetRepositoryConfigurationHandler(IALaCarteContext context, ILogger<GetRepositoryConfigurationHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetRepositoryConfigurationResponse?> Handle(GetRepositoryConfigurationRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting repository configuration with ID: {RepositoryConfigurationId}", request.RepositoryConfigurationId);

        var configuration = await _context.RepositoryConfigurations
            .Where(rc => rc.RepositoryConfigurationId == request.RepositoryConfigurationId)
            .Select(rc => new GetRepositoryConfigurationResponse
            {
                RepositoryConfigurationId = rc.RepositoryConfigurationId,
                Url = rc.Url,
                Branch = rc.Branch,
                LocalDirectory = rc.LocalDirectory
            })
            .FirstOrDefaultAsync(cancellationToken);

        return configuration;
    }
}
