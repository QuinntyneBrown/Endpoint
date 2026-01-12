// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Solutions;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Microservices;

/// <summary>
/// Factory for creating predefined microservice solutions.
/// </summary>
public class MicroserviceFactory : IMicroserviceFactory
{
    private readonly ILogger<MicroserviceFactory> logger;
    private readonly IProjectFactory projectFactory;

    private static readonly IReadOnlyList<string> AvailableMicroserviceNames = new[]
    {
        "Identity",
        "Tenant",
        "Notification",
        "DocumentStorage",
        "Search",
        "Analytics",
        "Billing",
        "OcrVision",
        "Scheduling",
        "Audit",
        "Export",
        "Email",
        "Integration",
        "Media",
        "Geolocation",
        "Tagging",
        "Collaboration",
        "Calculation",
        "Import",
        "Cache",
        "RateLimiting",
        "Localization",
        "Workflow",
        "Backup"
    };

    public MicroserviceFactory(
        ILogger<MicroserviceFactory> logger,
        IProjectFactory projectFactory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.projectFactory = projectFactory ?? throw new ArgumentNullException(nameof(projectFactory));
    }

    public IReadOnlyList<string> GetAvailableMicroservices() => AvailableMicroserviceNames;

    public async Task<SolutionModel> CreateByNameAsync(string name, string directory, CancellationToken cancellationToken = default)
    {
        return name.ToLowerInvariant() switch
        {
            "identity" => await CreateIdentityMicroserviceAsync(directory, cancellationToken),
            "tenant" => await CreateTenantMicroserviceAsync(directory, cancellationToken),
            "notification" => await CreateNotificationMicroserviceAsync(directory, cancellationToken),
            "documentstorage" => await CreateDocumentStorageMicroserviceAsync(directory, cancellationToken),
            "search" => await CreateSearchMicroserviceAsync(directory, cancellationToken),
            "analytics" => await CreateAnalyticsMicroserviceAsync(directory, cancellationToken),
            "billing" => await CreateBillingMicroserviceAsync(directory, cancellationToken),
            "ocrvision" => await CreateOcrVisionMicroserviceAsync(directory, cancellationToken),
            "scheduling" => await CreateSchedulingMicroserviceAsync(directory, cancellationToken),
            "audit" => await CreateAuditMicroserviceAsync(directory, cancellationToken),
            "export" => await CreateExportMicroserviceAsync(directory, cancellationToken),
            "email" => await CreateEmailMicroserviceAsync(directory, cancellationToken),
            "integration" => await CreateIntegrationMicroserviceAsync(directory, cancellationToken),
            "media" => await CreateMediaMicroserviceAsync(directory, cancellationToken),
            "geolocation" => await CreateGeolocationMicroserviceAsync(directory, cancellationToken),
            "tagging" => await CreateTaggingMicroserviceAsync(directory, cancellationToken),
            "collaboration" => await CreateCollaborationMicroserviceAsync(directory, cancellationToken),
            "calculation" => await CreateCalculationMicroserviceAsync(directory, cancellationToken),
            "import" => await CreateImportMicroserviceAsync(directory, cancellationToken),
            "cache" => await CreateCacheMicroserviceAsync(directory, cancellationToken),
            "ratelimiting" => await CreateRateLimitingMicroserviceAsync(directory, cancellationToken),
            "localization" => await CreateLocalizationMicroserviceAsync(directory, cancellationToken),
            "workflow" => await CreateWorkflowMicroserviceAsync(directory, cancellationToken),
            "backup" => await CreateBackupMicroserviceAsync(directory, cancellationToken),
            _ => throw new ArgumentException($"Unknown microservice: {name}. Available microservices: {string.Join(", ", AvailableMicroserviceNames)}", nameof(name))
        };
    }

    public Task<SolutionModel> CreateIdentityMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Identity microservice");
        return CreateMicroserviceAsync("Identity", directory, new[]
        {
            new PackageModel("Microsoft.AspNetCore.Identity.EntityFrameworkCore", "8.0.0"),
            new PackageModel("Microsoft.AspNetCore.Authentication.JwtBearer", "8.0.0"),
            new PackageModel("System.IdentityModel.Tokens.Jwt", "7.0.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateTenantMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Tenant microservice");
        return CreateMicroserviceAsync("Tenant", directory, new[]
        {
            new PackageModel("Finbuckle.MultiTenant", "6.12.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateNotificationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Notification microservice");
        return CreateMicroserviceAsync("Notification", directory, new[]
        {
            new PackageModel("FirebaseAdmin", "2.4.0"),
            new PackageModel("Twilio", "6.2.0"),
            new PackageModel("SendGrid", "9.28.1")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateDocumentStorageMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating DocumentStorage microservice");
        return CreateMicroserviceAsync("DocumentStorage", directory, new[]
        {
            new PackageModel("Azure.Storage.Blobs", "12.19.0"),
            new PackageModel("AWSSDK.S3", "3.7.0"),
            new PackageModel("MimeTypes", "2.4.1")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateSearchMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Search microservice");
        return CreateMicroserviceAsync("Search", directory, new[]
        {
            new PackageModel("Elastic.Clients.Elasticsearch", "8.11.0"),
            new PackageModel("Azure.Search.Documents", "11.5.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateAnalyticsMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Analytics microservice");
        return CreateMicroserviceAsync("Analytics", directory, new[]
        {
            new PackageModel("Microsoft.ApplicationInsights.AspNetCore", "2.22.0"),
            new PackageModel("Prometheus-net.AspNetCore", "8.0.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateBillingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Billing microservice");
        return CreateMicroserviceAsync("Billing", directory, new[]
        {
            new PackageModel("Stripe.net", "43.0.0"),
            new PackageModel("PayPalCheckoutSdk", "1.0.4")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateOcrVisionMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating OcrVision microservice");
        return CreateMicroserviceAsync("OcrVision", directory, new[]
        {
            new PackageModel("Azure.AI.FormRecognizer", "4.1.0"),
            new PackageModel("Tesseract", "5.2.0"),
            new PackageModel("Google.Cloud.Vision.V1", "3.4.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateSchedulingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Scheduling microservice");
        return CreateMicroserviceAsync("Scheduling", directory, new[]
        {
            new PackageModel("Quartz", "3.8.0"),
            new PackageModel("Hangfire.AspNetCore", "1.8.6"),
            new PackageModel("Ical.Net", "4.2.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateAuditMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Audit microservice");
        return CreateMicroserviceAsync("Audit", directory, new[]
        {
            new PackageModel("Audit.NET", "23.0.0"),
            new PackageModel("Audit.EntityFramework.Core", "23.0.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateExportMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Export microservice");
        return CreateMicroserviceAsync("Export", directory, new[]
        {
            new PackageModel("QuestPDF", "2023.12.0"),
            new PackageModel("ClosedXML", "0.102.1"),
            new PackageModel("CsvHelper", "30.0.1")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateEmailMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Email microservice");
        return CreateMicroserviceAsync("Email", directory, new[]
        {
            new PackageModel("SendGrid", "9.28.1"),
            new PackageModel("MailKit", "4.3.0"),
            new PackageModel("Fluid.Core", "2.5.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateIntegrationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Integration microservice");
        return CreateMicroserviceAsync("Integration", directory, new[]
        {
            new PackageModel("Polly", "8.2.0"),
            new PackageModel("Refit.HttpClientFactory", "7.0.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateMediaMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Media microservice");
        return CreateMicroserviceAsync("Media", directory, new[]
        {
            new PackageModel("SixLabors.ImageSharp", "3.1.0"),
            new PackageModel("FFMpegCore", "5.1.0"),
            new PackageModel("NAudio", "2.2.1")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateGeolocationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Geolocation microservice");
        return CreateMicroserviceAsync("Geolocation", directory, new[]
        {
            new PackageModel("NetTopologySuite", "2.5.0"),
            new PackageModel("GoogleMaps.LocationServices", "1.2.0.6"),
            new PackageModel("GeoCoordinate.NetCore", "1.0.0.1")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateTaggingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Tagging microservice");
        return CreateMicroserviceAsync("Tagging", directory, Array.Empty<PackageModel>(), cancellationToken);
    }

    public Task<SolutionModel> CreateCollaborationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Collaboration microservice");
        return CreateMicroserviceAsync("Collaboration", directory, new[]
        {
            new PackageModel("Microsoft.AspNetCore.SignalR", "1.1.0"),
            new PackageModel("Yarp.ReverseProxy", "2.1.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateCalculationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Calculation microservice");
        return CreateMicroserviceAsync("Calculation", directory, new[]
        {
            new PackageModel("MathNet.Numerics", "5.0.0"),
            new PackageModel("NCalc2", "2.1.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateImportMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Import microservice");
        return CreateMicroserviceAsync("Import", directory, new[]
        {
            new PackageModel("CsvHelper", "30.0.1"),
            new PackageModel("ExcelDataReader", "3.6.0"),
            new PackageModel("ExcelDataReader.DataSet", "3.6.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateCacheMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Cache microservice");
        return CreateMicroserviceAsync("Cache", directory, new[]
        {
            new PackageModel("Microsoft.Extensions.Caching.StackExchangeRedis", "8.0.0"),
            new PackageModel("StackExchange.Redis", "2.7.10")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateRateLimitingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating RateLimiting microservice");
        return CreateMicroserviceAsync("RateLimiting", directory, new[]
        {
            new PackageModel("AspNetCoreRateLimit", "5.0.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateLocalizationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Localization microservice");
        return CreateMicroserviceAsync("Localization", directory, new[]
        {
            new PackageModel("Microsoft.Extensions.Localization", "8.0.0"),
            new PackageModel("OrchardCore.Localization.Core", "1.8.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateWorkflowMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Workflow microservice");
        return CreateMicroserviceAsync("Workflow", directory, new[]
        {
            new PackageModel("Elsa", "2.14.1"),
            new PackageModel("WorkflowCore", "3.9.0")
        }, cancellationToken);
    }

    public Task<SolutionModel> CreateBackupMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Backup microservice");
        return CreateMicroserviceAsync("Backup", directory, new[]
        {
            new PackageModel("Azure.Storage.Blobs", "12.19.0"),
            new PackageModel("SharpZipLib", "1.4.2")
        }, cancellationToken);
    }

    private async Task<SolutionModel> CreateMicroserviceAsync(
        string name,
        string directory,
        PackageModel[] additionalCorePackages,
        CancellationToken cancellationToken)
    {
        var solutionModel = new SolutionModel(name, directory);

        var coreProject = await CreateCoreProjectAsync(name, solutionModel.SrcDirectory, additionalCorePackages);
        var infrastructureProject = await CreateInfrastructureProjectAsync(name, solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync(name, solutionModel.SrcDirectory);

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    private Task<ProjectModel> CreateCoreProjectAsync(string name, string directory, PackageModel[] additionalPackages)
    {
        var project = new ProjectModel
        {
            Name = $"{name}.Core",
            Directory = Path.Combine(directory, $"{name}.Core"),
            DotNetProjectType = DotNetProjectType.ClassLib
        };

        project.Packages.AddRange(new[]
        {
            new PackageModel("MediatR", "12.2.0"),
            new PackageModel("Microsoft.Extensions.Hosting.Abstractions", "8.0.0"),
            new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"),
            new PackageModel("Microsoft.Extensions.Logging.Abstractions", "8.0.0"),
            new PackageModel("FluentValidation", "11.9.0"),
            new PackageModel("FluentValidation.DependencyInjectionExtensions", "11.9.0"),
            new PackageModel("Newtonsoft.Json", "13.0.3")
        });

        if (additionalPackages?.Length > 0)
        {
            project.Packages.AddRange(additionalPackages);
        }

        return Task.FromResult(project);
    }

    private Task<ProjectModel> CreateInfrastructureProjectAsync(string name, string directory)
    {
        var project = new ProjectModel
        {
            Name = $"{name}.Infrastructure",
            Directory = Path.Combine(directory, $"{name}.Infrastructure"),
            DotNetProjectType = DotNetProjectType.ClassLib
        };

        project.Packages.AddRange(new[]
        {
            new PackageModel("Microsoft.EntityFrameworkCore.SqlServer", "8.0.0"),
            new PackageModel("Microsoft.EntityFrameworkCore.Design", "8.0.0"),
            new PackageModel("Microsoft.EntityFrameworkCore.Tools", "8.0.0")
        });

        project.References.Add($@"..\{name}.Core\{name}.Core.csproj");

        return Task.FromResult(project);
    }

    private Task<ProjectModel> CreateApiProjectAsync(string name, string directory)
    {
        var project = new ProjectModel
        {
            Name = $"{name}.Api",
            Directory = Path.Combine(directory, $"{name}.Api"),
            DotNetProjectType = DotNetProjectType.Web
        };

        project.Packages.AddRange(new[]
        {
            new PackageModel("Asp.Versioning.Mvc", "8.0.0"),
            new PackageModel("Microsoft.AspNetCore.OpenApi", "8.0.0"),
            new PackageModel("Serilog.AspNetCore", "8.0.0"),
            new PackageModel("Swashbuckle.AspNetCore", "6.5.0")
        });

        project.References.Add($@"..\{name}.Infrastructure\{name}.Infrastructure.csproj");

        return Task.FromResult(project);
    }
}
