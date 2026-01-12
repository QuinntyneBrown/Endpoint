// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Endpoint.Engineering.Microservices;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddEngineeringServices(this IServiceCollection services)
    {
        services.AddSingleton<ICodeParser, CodeParser>();

        // Register HttpClient for HtmlParserService
        services.AddHttpClient("HtmlParser", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IHtmlParserService, HtmlParserService>();

        // Register MicroserviceFactory for predefined microservice creation
        services.AddSingleton<IMicroserviceFactory, MicroserviceFactory>();
    }
}
