// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.DeleteRepositoryConfiguration;

public class DeleteRepositoryConfigurationHandler : IRequestHandler<DeleteRepositoryConfigurationRequest>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<DeleteRepositoryConfigurationHandler> _logger;

    public DeleteRepositoryConfigurationHandler(IALaCarteContext context, ILogger<DeleteRepositoryConfigurationHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteRepositoryConfigurationRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting repository configuration with ID: {RepositoryConfigurationId}", request.RepositoryConfigurationId);

        var repositoryConfiguration = await _context.RepositoryConfigurations
            .FirstOrDefaultAsync(rc => rc.RepositoryConfigurationId == request.RepositoryConfigurationId, cancellationToken);

        if (repositoryConfiguration == null)
        {
            throw new InvalidOperationException($"Repository configuration with ID {request.RepositoryConfigurationId} not found");
        }

        _context.RepositoryConfigurations.Remove(repositoryConfiguration);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted repository configuration with ID: {RepositoryConfigurationId}", request.RepositoryConfigurationId);
    }
}
