// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.DeleteRepositoryConfiguration;

public class DeleteRepositoryConfigurationRequest : IRequest
{
    public Guid RepositoryConfigurationId { get; set; }
}
