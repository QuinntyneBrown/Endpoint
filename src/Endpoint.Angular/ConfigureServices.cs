// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Artifacts;
using Endpoint.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.Angular;

public static class ConfigureServices
{
    public static void AddAngularServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileFactory,FileFactory>();
        services.AddArifactGenerator(typeof(ProjectModel).Assembly);
        services.AddSyntaxGenerator(typeof(ProjectModel).Assembly);
    }
}
