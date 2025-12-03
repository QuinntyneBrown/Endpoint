// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Endpoint.Core.Abstractions;

public class Generator : IGenerator
{
    private readonly ILogger<Generator> _logger;

    public Generator(ILogger<Generator> logger){
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task DoWorkAsync()
    {
        _logger.LogInformation("DoWorkAsync");

    }
}