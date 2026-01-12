// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Sample.ToDos.Api;
using Sample.Models.ToDo;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddValidation(typeof(ToDo));

builder.Services.AddApiServices(corsPolicyBuilder =>
{
    corsPolicyBuilder.WithOrigins(builder.Configuration["WithOrigins"]!.Split(','));
}, builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHttpsRedirection();

var services = (IServiceScopeFactory)app.Services.GetRequiredService(typeof(IServiceScopeFactory));

using (var scope = services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ToDosDbContext>();

    if (args.Contains("ci"))
        args = new string[4] { "dropdb", "migratedb", "seeddb", "stop" };

    if (args.Contains("dropdb"))
    {

    }

    if (args.Contains("migratedb"))
    {
        context.Database.Migrate();
    }

    if (args.Contains("seeddb"))
    {

    }

    if (args.Contains("stop"))
        Environment.Exit(0);
}

app.Run();