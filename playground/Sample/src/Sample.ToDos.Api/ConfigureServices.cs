// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Sample.ToDos.Api;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureApiServices
{
    public static void AddApiServices(this IServiceCollection services, Action<CorsPolicyBuilder> configureCorsPolicyBuilder, string connectionString)
    {
        services.AddControllers();

        services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                configureCorsPolicyBuilder.Invoke(builder);

                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(isOriginAllowed: _ => true)
                .AllowCredentials();
            }));

        services.AddTransient<IToDosDbContext, ToDosDbContext>();

        services.AddDbContextPool<ToDosDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                builder => builder.MigrationsAssembly("Sample.ToDos.Api")
                    .EnableRetryOnFailure())
            .EnableThreadSafetyChecks(false)
            .LogTo(Console.WriteLine)
            .EnableSensitiveDataLogging();
        });

        services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>());
    }
}