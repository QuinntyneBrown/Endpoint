// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.CreateRepositoryConfiguration;

public class CreateRepositoryConfigurationHandler : IRequestHandler<CreateRepositoryConfigurationRequest, CreateRepositoryConfigurationResponse>
{
    private readonly IALaCarteContext _context;
    private readonly ILogger<CreateRepositoryConfigurationHandler> _logger;

    public CreateRepositoryConfigurationHandler(IALaCarteContext context, ILogger<CreateRepositoryConfigurationHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateRepositoryConfigurationResponse> Handle(CreateRepositoryConfigurationRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating repository configuration");

        var repositoryConfiguration = new RepositoryConfiguration
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = request.Url,
            Branch = request.Branch,
            LocalDirectory = request.LocalDirectory
        };

        _context.RepositoryConfigurations.Add(repositoryConfiguration);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created repository configuration with ID: {RepositoryConfigurationId}", repositoryConfiguration.RepositoryConfigurationId);

        return new CreateRepositoryConfigurationResponse
        {
            RepositoryConfigurationId = repositoryConfiguration.RepositoryConfigurationId
        };
    }
}
