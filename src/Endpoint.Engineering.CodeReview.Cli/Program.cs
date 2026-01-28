// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using Endpoint.Engineering.CodeReview.Cli.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog(Log.Logger);
});

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Register services
builder.Services.AddServices();

var host = builder.Build();

var rootCommand = new RootCommand("Endpoint Code Review CLI - Git diff analysis tool");

var diffCommand = host.Services.GetRequiredService<DiffCommand>();
rootCommand.AddCommand(diffCommand.Create());

var result = await rootCommand.InvokeAsync(args);

await host.StopAsync();
Log.CloseAndFlush();

return result;
