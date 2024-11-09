// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.DataModel;

public class DataModelContext : IDataModelContext
{
    private readonly ILogger<DataModelContext> _logger;

    public List<ServiceModel> ServiceModels { get; set; } = [];

}