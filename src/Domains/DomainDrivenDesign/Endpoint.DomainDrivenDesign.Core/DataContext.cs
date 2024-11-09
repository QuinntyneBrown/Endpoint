// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Microsoft.Extensions.Logging;

namespace Endpoint.DomainDrivenDesign.Core;

public class DataContext : IDataContext
{
    private readonly ILogger<DataContext> _logger;

    public DataContext(ILogger<DataContext> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
    
        _logger = logger;
    }

    public string ProductName { get; set; } = string.Empty;

    public List<BoundedContext> BoundedContexts { get; set; } = [];

}

