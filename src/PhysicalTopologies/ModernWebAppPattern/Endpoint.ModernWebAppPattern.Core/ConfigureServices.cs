// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core;
using Endpoint.ModernWebAppPattern.Core;
using Endpoint.ModernWebAppPattern.Core.Artifacts;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddModernWebAppPatternCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<Endpoint.ModernWebAppPattern.Core.Syntax.ISyntaxFactory, Endpoint.ModernWebAppPattern.Core.Syntax.SyntaxFactory>();
        services.AddSingleton<Endpoint.DomainDrivenDesign.Core.IDataContextProvider, Endpoint.DomainDrivenDesign.Core.FileSystenDataContextProvider>();
        services.AddSingleton<IDataContextProvider,FileSystemDataContextProvider>();
        services.AddSingleton<IDataContext,DataContext>();
        services.AddSingleton<IArtifactFactory,ArtifactFactory>();
        services.AddSyntaxGenerator(typeof(FileSystemDataContextProvider).Assembly);
    }
}
