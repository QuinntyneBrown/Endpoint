// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.ModernWebAppPattern;
using Endpoint.ModernWebAppPattern.Artifacts;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddModernWebAppPatternCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<Endpoint.ModernWebAppPattern.Syntax.ISyntaxFactory, Endpoint.ModernWebAppPattern.Syntax.SyntaxFactory>();
        services.AddSingleton<Endpoint.DomainDrivenDesign.IDataContextProvider, Endpoint.DomainDrivenDesign.FileSystenDataContextProvider>();
        services.AddSingleton<IDataContextProvider, FileSystemDataContextProvider>();
        services.AddSingleton<IDataContext, DataContext>();
        services.AddSingleton<IArtifactFactory, ArtifactFactory>();
        services.AddSyntaxGenerator(typeof(FileSystemDataContextProvider).Assembly);
    }
}
