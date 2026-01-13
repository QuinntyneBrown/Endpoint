// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Endpoint.Engineering.Api;

public class ApiArtifactFactory: IApiArtifactFactory
{
    private readonly ILogger<ApiArtifactFactory> _logger;

    public ApiArtifactFactory(ILogger<ApiArtifactFactory> logger){
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

    }

    public async Task DoWorkAsync()
    {
        _logger.LogInformation("DoWorkAsync");

    }

}

