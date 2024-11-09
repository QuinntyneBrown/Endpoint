// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Endpoint.DomainDrivenDesign.Core;

public class DataContextProvider : IDataContextProvider
{
    private readonly ILogger<DataContextProvider> _logger;

    public DataContextProvider(ILogger<DataContextProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<IDataContext> GetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DoWorkAsync");

        throw new NotImplementedException();

    }

}

