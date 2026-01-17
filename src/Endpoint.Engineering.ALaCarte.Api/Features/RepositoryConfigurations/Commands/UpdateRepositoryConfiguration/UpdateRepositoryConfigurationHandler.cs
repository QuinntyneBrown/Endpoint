// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.UpdateRepositoryConfiguration;

public class UpdateRepositoryConfigurationHandler : IRequestHandler<UpdateRepositoryConfigurationRequest>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<UpdateRepositoryConfigurationHandler> _logger;

    public UpdateRepositoryConfigurationHandler(IALaCarteContext context, ILogger<UpdateRepositoryConfigurationHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateRepositoryConfigurationRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating repository configuration with ID: {RepositoryConfigurationId}", request.RepositoryConfigurationId);

        var repositoryConfiguration = await _context.RepositoryConfigurations
            .FirstOrDefaultAsync(rc => rc.RepositoryConfigurationId == request.RepositoryConfigurationId, cancellationToken);

        if (repositoryConfiguration == null)
        {
            throw new InvalidOperationException($"Repository configuration with ID {request.RepositoryConfigurationId} not found");
        }

        repositoryConfiguration.Url = request.Url;
        repositoryConfiguration.Branch = request.Branch;
        repositoryConfiguration.LocalDirectory = request.LocalDirectory;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated repository configuration with ID: {RepositoryConfigurationId}", request.RepositoryConfigurationId);
    }
}
