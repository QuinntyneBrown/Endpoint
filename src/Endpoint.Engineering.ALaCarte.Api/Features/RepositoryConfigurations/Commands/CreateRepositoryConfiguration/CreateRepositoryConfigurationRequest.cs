// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.CreateRepositoryConfiguration;

public class CreateRepositoryConfigurationRequest : IRequest<CreateRepositoryConfigurationResponse>
{
    public string Url { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public string? LocalDirectory { get; set; }
}
