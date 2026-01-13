// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Artifacts;
using Endpoint.Engineering.Api.Models;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Api;

/// <summary>
/// Factory for creating API Gateway artifacts.
/// </summary>
public class ApiArtifactFactory : IApiArtifactFactory
{
    private readonly ILogger<ApiArtifactFactory> _logger;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiArtifactFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileSystem">The file system abstraction.</param>
    public ApiArtifactFactory(ILogger<ApiArtifactFactory> logger, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public async Task<ApiGatewayModel> CreateApiGatewayProjectAsync(ApiGatewayInputModel model, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating API Gateway project for solution: {SolutionName}", model.SolutionName);

        var projectModel = new ApiGatewayModel(model.SolutionName, model.Directory);

        // Add Program.cs file
        projectModel.Files.Add(CreateProgramFile(projectModel.Namespace, projectModel.Directory));

        // Add appsettings.json file
        projectModel.Files.Add(CreateAppSettingsFile(projectModel.Directory));

        // Add appsettings.Development.json file
        projectModel.Files.Add(CreateAppSettingsDevelopmentFile(projectModel.Directory));

        return projectModel;
    }

    private static FileModel CreateProgramFile(string @namespace, string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = $$"""
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            using Microsoft.AspNetCore.Authentication.JwtBearer;
            using Microsoft.IdentityModel.Tokens;
            using System.Text;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            // Configure JWT authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            builder.Services.AddAuthorization();

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Configure rate limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(context =>
                {
                    return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapReverseProxy();

            app.Run();
            """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning",
                  "Yarp": "Information"
                }
              },
              "AllowedHosts": "*",
              "AllowedOrigins": [
                "http://localhost:4200",
                "https://localhost:4200"
              ],
              "JwtSettings": {
                "SecretKey": "your-secret-key-min-32-chars-long-CHANGE-THIS-IN-PRODUCTION",
                "Issuer": "YourIssuer",
                "Audience": "YourAudience",
                "ExpirationMinutes": 60
              },
              "ReverseProxy": {
                "Routes": {
                  "api-route": {
                    "ClusterId": "api-cluster",
                    "Match": {
                      "Path": "/api/{**catch-all}"
                    },
                    "Transforms": [
                      { "PathPattern": "/{**catch-all}" }
                    ]
                  },
                  "websocket-route": {
                    "ClusterId": "websocket-cluster",
                    "Match": {
                      "Path": "/ws/{**catch-all}"
                    }
                  }
                },
                "Clusters": {
                  "api-cluster": {
                    "Destinations": {
                      "destination1": {
                        "Address": "https://localhost:5001"
                      }
                    }
                  },
                  "websocket-cluster": {
                    "Destinations": {
                      "destination1": {
                        "Address": "https://localhost:5002"
                      }
                    }
                  }
                }
              }
            }
            """
        };
    }

    private static FileModel CreateAppSettingsDevelopmentFile(string directory)
    {
        return new FileModel("appsettings.Development", directory, ".json")
        {
            Body = """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Debug",
                  "Microsoft.AspNetCore": "Information",
                  "Yarp": "Debug"
                }
              }
            }
            """
        };
    }
}

