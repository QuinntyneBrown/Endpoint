// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddEngineeringServices(this IServiceCollection services)
    {
        services.AddSingleton<ICodeParser,CodeParser>();
    }


}


