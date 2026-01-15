// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Endpoint.Angular;
using Endpoint.DotNet;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

// Parse log level early if provided
var logLevel = LogEventLevel.Debug;
for (int i = 0; i < args.Length - 1; i++)
{
    if ((args[i] == "--log-level" || args[i] == "-l") && i + 1 < args.Length)
    {
        if (Enum.TryParse<LogEventLevel>(args[i + 1], true, out var parsedLevel))
        {
            logLevel = parsedLevel;
        }
        break;
    }
}

// Build configuration
var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

var config = configurationBuilder.Build();

var configuration = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", logLevel)
        .Enrich.FromLogContext()
        .WriteTo.Console(restrictedToMinimumLevel: logLevel);

Log.Logger = configuration.CreateBootstrapLogger();

Log.Information("Starting Endpoint");

var app = CodeGeneratorApplication.CreateBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(x => x.AddSerilog(Log.Logger));
        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<CodeGeneratorApplication>>();
        services.AddModernWebAppPatternCoreServices();
        services.AddAngularServices();
        services.AddRedisPubSubServices();
        services.AddEngineeringServices();
    })
    .Build();

var exitCode = await app.RunAsync();

return exitCode;
