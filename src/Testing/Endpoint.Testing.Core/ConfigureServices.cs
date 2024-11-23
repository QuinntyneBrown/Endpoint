// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Testing.Core;
using Endpoint.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddTestingCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ISyntaxFactory,SyntaxFactory>();
        services.AddSingleton<IArtifactFactory,ArtifactFactory>();
        services.AddSyntaxGenerator(typeof(SyntaxFactory).Assembly);
        services.AddArifactGenerator(typeof(ArtifactFactory).Assembly);
    }
}