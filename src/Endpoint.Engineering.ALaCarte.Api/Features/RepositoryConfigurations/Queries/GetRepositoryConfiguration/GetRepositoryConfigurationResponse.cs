// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;

public class GetRepositoryConfigurationResponse
{
    public Guid RepositoryConfigurationId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string? LocalDirectory { get; set; }
}
