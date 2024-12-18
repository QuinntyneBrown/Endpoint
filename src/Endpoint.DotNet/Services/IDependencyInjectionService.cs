// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.Services;

public interface IDependencyInjectionService
{
    Task Add(string interfaceName, string className, string directory, ServiceLifetime? serviceLifetime = null);

    Task AddHosted(string hostedServiceName, string directory);

    Task AddConfigureServices(string layer, string directory);
}
