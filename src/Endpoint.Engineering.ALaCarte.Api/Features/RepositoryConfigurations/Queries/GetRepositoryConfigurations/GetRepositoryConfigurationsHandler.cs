// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfigurations;

public class GetRepositoryConfigurationsHandler : IRequestHandler<GetRepositoryConfigurationsRequest, GetRepositoryConfigurationsResponse>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<GetRepositoryConfigurationsHandler> _logger;

    public GetRepositoryConfigurationsHandler(IALaCarteContext context, ILogger<GetRepositoryConfigurationsHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetRepositoryConfigurationsResponse> Handle(GetRepositoryConfigurationsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all repository configurations");

        var configurations = await _context.RepositoryConfigurations
            .Select(rc => new RepositoryConfigurationDto
            {
                RepositoryConfigurationId = rc.RepositoryConfigurationId,
                Url = rc.Url,
                Branch = rc.Branch,
                LocalDirectory = rc.LocalDirectory
            })
            .ToListAsync(cancellationToken);

        return new GetRepositoryConfigurationsResponse
        {
            RepositoryConfigurations = configurations
        };
    }
}
