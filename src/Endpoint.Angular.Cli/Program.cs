// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular;
using Endpoint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddCoreServices(typeof(Program).Assembly);

builder.Services.AddAngularServices();

var app = builder.Build();

var hostApplicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

hostApplicationLifetime.ApplicationStarted.Register(async () =>
{

});

app.Run();