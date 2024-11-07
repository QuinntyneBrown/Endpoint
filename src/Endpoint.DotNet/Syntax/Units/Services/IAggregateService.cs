// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Units.Services;

public interface IAggregateService
{
    Task<ClassModel> AddAsync(string name, string properties, string directory, string microserviceName);

    Task CommandCreateAsync(string routeType, string name, string aggregate, string properties, string directory);

    Task QueryCreateAsync(string routeType, string name, string aggregate, string properties, string directory);
}
