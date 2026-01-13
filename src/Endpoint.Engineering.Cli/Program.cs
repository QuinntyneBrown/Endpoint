// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using CommandLine;
using Endpoint.Angular;
using Endpoint.DotNet;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

// Handle help flags early before other initialization
if (args.Length == 0 ||
    (args.Length == 1 && (args[0] == "--help" || args[0] == "-h" || args[0] == "-?" || args[0] == "help")))
{
    HelpService.DisplayHelp();
    return;
}

if (args.Length == 2 && (args[1] == "--help" || args[1] == "-h" || args[1] == "-?"))
{
    HelpService.DisplayCommandHelp(args[0]);
    return;
}

if (args.Length == 2 && args[0] == "help")
{
    HelpService.DisplayCommandHelp(args[1]);
    return;
}

var parser = new Parser(with =>
{
    with.CaseSensitive = false;
    with.HelpWriter = null; // Disable default help writer since we have custom help
    with.IgnoreUnknownArguments = true;
});

var options = parser.ParseArguments<EndpointOptions>(args);

var logLevel = options.Value?.LogEventLevel ?? LogEventLevel.Debug;

var configuration = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", logLevel)
        .Enrich.FromLogContext()
        .WriteTo.Console(restrictedToMinimumLevel: logLevel);

Log.Logger = configuration.CreateBootstrapLogger();

Log.Information("Starting Endpoint");

var app = CodeGeneratorApplication.CreateBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging(x => x.AddSerilog(Log.Logger));
        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<CodeGeneratorApplication>>();
        services.AddModernWebAppPatternCoreServices();
        services.AddAngularServices();
        services.AddRedisPubSubServices();
        services.AddEngineeringServices();
    })
    .Build();

await app.RunAsync();
