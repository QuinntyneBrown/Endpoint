// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.CodeReview.Cli.Commands;
using Endpoint.Engineering.CodeReview.Cli.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<DiffCommand>();
        services.AddSingleton<IGitService, GitService>();

        return services;
    }
}
