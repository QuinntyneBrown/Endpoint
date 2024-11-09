// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddDomainDrivenDesignCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDataContextProvider,DataContextProvider>();
        services.AddSingleton<IDataContext,DataContext>();
    }


}



