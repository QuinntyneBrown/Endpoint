// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Engineering.Testing.Cli.Commands;
using Endpoint.Engineering.Testing.Cli.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddTestingCliServices(this IServiceCollection services)
    {
        // Commands
        services.AddSingleton<UnitTestsGenerateCommand>();

        // Services
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ITypeScriptParser, TypeScriptParser>();
        services.AddSingleton<IAngularTestGeneratorService, AngularTestGeneratorService>();

        return services;
    }
}
