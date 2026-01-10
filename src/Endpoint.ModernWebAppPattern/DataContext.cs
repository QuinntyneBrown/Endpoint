// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Endpoint.ModernWebAppPattern;

using Microservice = Models.Microservice;

using DddDataContext = DomainDrivenDesign.Core.DataContext;

public class DataContext : DddDataContext, IDataContext
{
    private readonly ILogger<DataContext> _logger;

    public DataContext()
    {

    }

    public List<Microservice> Microservices { get; set; } = [];
}

