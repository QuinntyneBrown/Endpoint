// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;

public class GetRepositoryConfigurationRequest : IRequest<GetRepositoryConfigurationResponse?>
{
    public Guid RepositoryConfigurationId { get; set; }
}
