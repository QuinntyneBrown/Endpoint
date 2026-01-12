// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Engineering.ModernWebAppPattern;
using Endpoint.Engineering.ModernWebAppPattern.Artifacts;

namespace Microsoft.Extensions.DependencyInjection;

public static class ModernWebAppPatternConfigureServices
{
    public static void AddModernWebAppPatternCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<Endpoint.Engineering.ModernWebAppPattern.Syntax.ISyntaxFactory, Endpoint.Engineering.ModernWebAppPattern.Syntax.SyntaxFactory>();
        services.AddSingleton<Endpoint.Engineering.DomainDrivenDesign.IDataContextProvider, Endpoint.Engineering.DomainDrivenDesign.FileSystenDataContextProvider>();
        services.AddSingleton<IDataContextProvider, FileSystemDataContextProvider>();
        services.AddSingleton<IDataContext, DataContext>();
        services.AddSingleton<IArtifactFactory, ArtifactFactory>();
        services.AddSyntaxGenerator(typeof(FileSystemDataContextProvider).Assembly);
    }
}
