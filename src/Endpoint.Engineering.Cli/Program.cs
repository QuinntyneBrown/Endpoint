// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using CommandLine;
using Endpoint.Angular;
using Endpoint.DotNet;
using Endpoint.DotNet.Cli;
using Endpoint.Services;
using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

var parser = new Parser(with =>
{
    with.CaseSensitive = false;
    with.HelpWriter = Console.Out; // Enable default help/version handling
    with.IgnoreUnknownArguments = true;
});

var parseResult = parser.ParseArguments<EndpointOptions>(args);

EndpointOptions? options = null;

parseResult
    .WithParsed(opts => options = opts)
    .WithNotParsed(_ => Environment.Exit(0));

if (options == null)
{
    return 0;
}

var logLevel = options.LogEventLevel;

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

// Configure database path in user's home directory to persist across CLI installations
var userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var appDataDirectory = Path.Combine(userHomeDirectory, ".endpoint-alacarte");
Directory.CreateDirectory(appDataDirectory);
var databasePath = Path.Combine(appDataDirectory, "ALaCarte.db");

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

        // Register ALaCarte database context
        services.AddDbContext<ALaCarteContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));
        services.AddScoped<IALaCarteContext>(provider => provider.GetRequiredService<ALaCarteContext>());
    })
    .Build();

await app.RunCliAsync();

return 0;
