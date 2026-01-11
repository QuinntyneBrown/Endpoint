// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using CommandLine;
using Endpoint.Angular;
using Endpoint.DotNet;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var parser = new Parser(with =>
{
    with.CaseSensitive = false;
    with.HelpWriter = Console.Out;
    with.IgnoreUnknownArguments = true;
});

var options = parser.ParseArguments<EndpointOptions>(args);

var configuration = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", options.Value.LogEventLevel)
        .Enrich.FromLogContext()
        .WriteTo.Console(restrictedToMinimumLevel: options.Value.LogEventLevel);

Log.Logger = configuration.CreateBootstrapLogger();

Log.Information("Starting Endpoint");

var app = CodeGeneratorApplication.CreateBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging(x => x.AddSerilog(Log.Logger));
        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<CodeGeneratorApplication>>();
        services.AddModernWebAppPatternCoreServices();
        services.AddAngularServices();
    })
    .Build();

await app.RunAsync();
