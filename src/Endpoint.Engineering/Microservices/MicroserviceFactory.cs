// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.Engineering.Microservices.Analytics;
using Endpoint.Engineering.Microservices.Audit;
using Endpoint.Engineering.Microservices.Billing;
using Endpoint.Engineering.Microservices.Cache;
using Endpoint.Engineering.Microservices.Calculation;
using Endpoint.Engineering.Microservices.Collaboration;
using Endpoint.Engineering.Microservices.DocumentStorage;
using Endpoint.Engineering.Microservices.Email;
using Endpoint.Engineering.Microservices.Export;
using Endpoint.Engineering.Microservices.Geolocation;
using Endpoint.Engineering.Microservices.Identity;
using Endpoint.Engineering.Microservices.Import;
using Endpoint.Engineering.Microservices.Integration;
using Endpoint.Engineering.Microservices.Localization;
using Endpoint.Engineering.Microservices.Media;
using Endpoint.Engineering.Microservices.Notification;
using Endpoint.Engineering.Microservices.OcrVision;
using Endpoint.Engineering.Microservices.RateLimiting;
using Endpoint.Engineering.Microservices.Scheduling;
using Endpoint.Engineering.Microservices.Search;
using Endpoint.Engineering.Microservices.Tagging;
using Endpoint.Engineering.Microservices.Tenant;
using Endpoint.Engineering.Microservices.Workflow;
using Endpoint.Engineering.Microservices.ConfigurationManagement;
using Endpoint.Engineering.Microservices.TelemetryStreaming;
using Endpoint.Engineering.Microservices.HistoricalTelemetry;
using Endpoint.Engineering.Microservices.GitAnalysis;
using Endpoint.Engineering.Microservices.RealtimeNotification;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Microservices;

/// <summary>
/// Factory for creating predefined microservice solutions.
/// </summary>
public class MicroserviceFactory : IMicroserviceFactory
{
    private readonly ILogger<MicroserviceFactory> logger;
    private readonly IProjectFactory projectFactory;

    // Artifact factories for all microservices
    private readonly IIdentityArtifactFactory identityArtifactFactory;
    private readonly ITenantArtifactFactory tenantArtifactFactory;
    private readonly INotificationArtifactFactory notificationArtifactFactory;
    private readonly IDocumentStorageArtifactFactory documentStorageArtifactFactory;
    private readonly ISearchArtifactFactory searchArtifactFactory;
    private readonly IAnalyticsArtifactFactory analyticsArtifactFactory;
    private readonly IBillingArtifactFactory billingArtifactFactory;
    private readonly IOcrVisionArtifactFactory ocrVisionArtifactFactory;
    private readonly ISchedulingArtifactFactory schedulingArtifactFactory;
    private readonly IAuditArtifactFactory auditArtifactFactory;
    private readonly IExportArtifactFactory exportArtifactFactory;
    private readonly IEmailArtifactFactory emailArtifactFactory;
    private readonly IIntegrationArtifactFactory integrationArtifactFactory;
    private readonly IMediaArtifactFactory mediaArtifactFactory;
    private readonly IGeolocationArtifactFactory geolocationArtifactFactory;
    private readonly ITaggingArtifactFactory taggingArtifactFactory;
    private readonly ICollaborationArtifactFactory collaborationArtifactFactory;
    private readonly ICalculationArtifactFactory calculationArtifactFactory;
    private readonly IImportArtifactFactory importArtifactFactory;
    private readonly ICacheArtifactFactory cacheArtifactFactory;
    private readonly IRateLimitingArtifactFactory rateLimitingArtifactFactory;
    private readonly ILocalizationArtifactFactory localizationArtifactFactory;
    private readonly IWorkflowArtifactFactory workflowArtifactFactory;
    private readonly IConfigurationManagementArtifactFactory configurationManagementArtifactFactory;
    private readonly ITelemetryStreamingArtifactFactory telemetryStreamingArtifactFactory;
    private readonly IHistoricalTelemetryArtifactFactory historicalTelemetryArtifactFactory;
    private readonly IGitAnalysisArtifactFactory gitAnalysisArtifactFactory;
    private readonly IRealtimeNotificationArtifactFactory realtimeNotificationArtifactFactory;

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
        "ConfigurationManagement",
        "TelemetryStreaming",
        "HistoricalTelemetry",
        "GitAnalysis",
        "RealtimeNotification",
        // Friendly aliases
        "Real Time Notification Microservice",
        "Git Processing Microservice"
    };

    public MicroserviceFactory(
        ILogger<MicroserviceFactory> logger,
        IProjectFactory projectFactory,
        IIdentityArtifactFactory identityArtifactFactory,
        ITenantArtifactFactory tenantArtifactFactory,
        INotificationArtifactFactory notificationArtifactFactory,
        IDocumentStorageArtifactFactory documentStorageArtifactFactory,
        ISearchArtifactFactory searchArtifactFactory,
        IAnalyticsArtifactFactory analyticsArtifactFactory,
        IBillingArtifactFactory billingArtifactFactory,
        IOcrVisionArtifactFactory ocrVisionArtifactFactory,
        ISchedulingArtifactFactory schedulingArtifactFactory,
        IAuditArtifactFactory auditArtifactFactory,
        IExportArtifactFactory exportArtifactFactory,
        IEmailArtifactFactory emailArtifactFactory,
        IIntegrationArtifactFactory integrationArtifactFactory,
        IMediaArtifactFactory mediaArtifactFactory,
        IGeolocationArtifactFactory geolocationArtifactFactory,
        ITaggingArtifactFactory taggingArtifactFactory,
        ICollaborationArtifactFactory collaborationArtifactFactory,
        ICalculationArtifactFactory calculationArtifactFactory,
        IImportArtifactFactory importArtifactFactory,
        ICacheArtifactFactory cacheArtifactFactory,
        IRateLimitingArtifactFactory rateLimitingArtifactFactory,
        ILocalizationArtifactFactory localizationArtifactFactory,
        IWorkflowArtifactFactory workflowArtifactFactory,
        IConfigurationManagementArtifactFactory configurationManagementArtifactFactory,
        ITelemetryStreamingArtifactFactory telemetryStreamingArtifactFactory,
        IHistoricalTelemetryArtifactFactory historicalTelemetryArtifactFactory,
        IGitAnalysisArtifactFactory gitAnalysisArtifactFactory,
        IRealtimeNotificationArtifactFactory realtimeNotificationArtifactFactory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.projectFactory = projectFactory ?? throw new ArgumentNullException(nameof(projectFactory));
        this.identityArtifactFactory = identityArtifactFactory ?? throw new ArgumentNullException(nameof(identityArtifactFactory));
        this.tenantArtifactFactory = tenantArtifactFactory ?? throw new ArgumentNullException(nameof(tenantArtifactFactory));
        this.notificationArtifactFactory = notificationArtifactFactory ?? throw new ArgumentNullException(nameof(notificationArtifactFactory));
        this.documentStorageArtifactFactory = documentStorageArtifactFactory ?? throw new ArgumentNullException(nameof(documentStorageArtifactFactory));
        this.searchArtifactFactory = searchArtifactFactory ?? throw new ArgumentNullException(nameof(searchArtifactFactory));
        this.analyticsArtifactFactory = analyticsArtifactFactory ?? throw new ArgumentNullException(nameof(analyticsArtifactFactory));
        this.billingArtifactFactory = billingArtifactFactory ?? throw new ArgumentNullException(nameof(billingArtifactFactory));
        this.ocrVisionArtifactFactory = ocrVisionArtifactFactory ?? throw new ArgumentNullException(nameof(ocrVisionArtifactFactory));
        this.schedulingArtifactFactory = schedulingArtifactFactory ?? throw new ArgumentNullException(nameof(schedulingArtifactFactory));
        this.auditArtifactFactory = auditArtifactFactory ?? throw new ArgumentNullException(nameof(auditArtifactFactory));
        this.exportArtifactFactory = exportArtifactFactory ?? throw new ArgumentNullException(nameof(exportArtifactFactory));
        this.emailArtifactFactory = emailArtifactFactory ?? throw new ArgumentNullException(nameof(emailArtifactFactory));
        this.integrationArtifactFactory = integrationArtifactFactory ?? throw new ArgumentNullException(nameof(integrationArtifactFactory));
        this.mediaArtifactFactory = mediaArtifactFactory ?? throw new ArgumentNullException(nameof(mediaArtifactFactory));
        this.geolocationArtifactFactory = geolocationArtifactFactory ?? throw new ArgumentNullException(nameof(geolocationArtifactFactory));
        this.taggingArtifactFactory = taggingArtifactFactory ?? throw new ArgumentNullException(nameof(taggingArtifactFactory));
        this.collaborationArtifactFactory = collaborationArtifactFactory ?? throw new ArgumentNullException(nameof(collaborationArtifactFactory));
        this.calculationArtifactFactory = calculationArtifactFactory ?? throw new ArgumentNullException(nameof(calculationArtifactFactory));
        this.importArtifactFactory = importArtifactFactory ?? throw new ArgumentNullException(nameof(importArtifactFactory));
        this.cacheArtifactFactory = cacheArtifactFactory ?? throw new ArgumentNullException(nameof(cacheArtifactFactory));
        this.rateLimitingArtifactFactory = rateLimitingArtifactFactory ?? throw new ArgumentNullException(nameof(rateLimitingArtifactFactory));
        this.localizationArtifactFactory = localizationArtifactFactory ?? throw new ArgumentNullException(nameof(localizationArtifactFactory));
        this.workflowArtifactFactory = workflowArtifactFactory ?? throw new ArgumentNullException(nameof(workflowArtifactFactory));
        this.configurationManagementArtifactFactory = configurationManagementArtifactFactory ?? throw new ArgumentNullException(nameof(configurationManagementArtifactFactory));
        this.telemetryStreamingArtifactFactory = telemetryStreamingArtifactFactory ?? throw new ArgumentNullException(nameof(telemetryStreamingArtifactFactory));
        this.historicalTelemetryArtifactFactory = historicalTelemetryArtifactFactory ?? throw new ArgumentNullException(nameof(historicalTelemetryArtifactFactory));
        this.gitAnalysisArtifactFactory = gitAnalysisArtifactFactory ?? throw new ArgumentNullException(nameof(gitAnalysisArtifactFactory));
        this.realtimeNotificationArtifactFactory = realtimeNotificationArtifactFactory ?? throw new ArgumentNullException(nameof(realtimeNotificationArtifactFactory));
    }

    public IReadOnlyList<string> GetAvailableMicroservices() => AvailableMicroserviceNames;

    public async Task<SolutionModel> CreateByNameAsync(string name, string directory, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeMicroserviceName(name);

        return normalized switch
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
            "configurationmanagement" => await CreateConfigurationManagementMicroserviceAsync(directory, cancellationToken),
            "telemetrystreaming" => await CreateTelemetryStreamingMicroserviceAsync(directory, cancellationToken),
            "historicaltelemetry" => await CreateHistoricalTelemetryMicroserviceAsync(directory, cancellationToken),
            "gitanalysis" => await CreateGitAnalysisMicroserviceAsync(directory, cancellationToken),
            // Alias: "Git Processing Microservice"
            "gitprocessing" => await CreateGitAnalysisMicroserviceAsync(directory, cancellationToken),
            "realtimenotification" => await CreateRealtimeNotificationMicroserviceAsync(directory, cancellationToken),
            _ => throw new ArgumentException($"Unknown microservice: {name}. Available microservices: {string.Join(", ", AvailableMicroserviceNames)}", nameof(name))
        };
    }

    private static string NormalizeMicroserviceName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        // Strip spaces/punctuation by keeping only letters and digits.
        // Then remove an optional "microservice" suffix.
        var compact = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        return compact.Replace("microservice", string.Empty);
    }

    public async Task<SolutionModel> CreateIdentityMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Identity microservice with full implementation");

        var solutionModel = new SolutionModel("Identity", directory);

        var coreProject = await CreateIdentityCoreProjectAsync(solutionModel.SrcDirectory);
        var infrastructureProject = await CreateIdentityInfrastructureProjectAsync(solutionModel.SrcDirectory);
        var apiProject = await CreateIdentityApiProjectAsync(solutionModel.SrcDirectory);

        identityArtifactFactory.AddCoreFiles(coreProject);
        identityArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Identity");
        identityArtifactFactory.AddApiFiles(apiProject, "Identity");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateTenantMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Tenant microservice with full implementation");

        var solutionModel = new SolutionModel("Tenant", directory);

        var coreProject = await CreateCoreProjectAsync("Tenant", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Finbuckle.MultiTenant", "6.12.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Tenant", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Tenant", solutionModel.SrcDirectory);

        tenantArtifactFactory.AddCoreFiles(coreProject);
        tenantArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Tenant");
        tenantArtifactFactory.AddApiFiles(apiProject, "Tenant");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateNotificationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Notification microservice with full implementation");

        var solutionModel = new SolutionModel("Notification", directory);

        var coreProject = await CreateCoreProjectAsync("Notification", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("FirebaseAdmin", "2.4.0"),
            new PackageModel("Twilio", "6.2.0"),
            new PackageModel("SendGrid", "9.28.1")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Notification", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Notification", solutionModel.SrcDirectory);

        notificationArtifactFactory.AddCoreFiles(coreProject);
        notificationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Notification");
        notificationArtifactFactory.AddApiFiles(apiProject, "Notification");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateDocumentStorageMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating DocumentStorage microservice with full implementation");

        var solutionModel = new SolutionModel("DocumentStorage", directory);

        var coreProject = await CreateCoreProjectAsync("DocumentStorage", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Azure.Storage.Blobs", "12.19.0"),
            new PackageModel("AWSSDK.S3", "3.7.0"),
            new PackageModel("MimeTypes", "2.4.1")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("DocumentStorage", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("DocumentStorage", solutionModel.SrcDirectory);

        documentStorageArtifactFactory.AddCoreFiles(coreProject);
        documentStorageArtifactFactory.AddInfrastructureFiles(infrastructureProject, "DocumentStorage");
        documentStorageArtifactFactory.AddApiFiles(apiProject, "DocumentStorage");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateSearchMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Search microservice with full implementation");

        var solutionModel = new SolutionModel("Search", directory);

        var coreProject = await CreateCoreProjectAsync("Search", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Elastic.Clients.Elasticsearch", "8.11.0"),
            new PackageModel("Azure.Search.Documents", "11.5.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Search", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Search", solutionModel.SrcDirectory);

        searchArtifactFactory.AddCoreFiles(coreProject);
        searchArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Search");
        searchArtifactFactory.AddApiFiles(apiProject, "Search");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateAnalyticsMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Analytics microservice with full implementation");

        var solutionModel = new SolutionModel("Analytics", directory);

        var coreProject = await CreateCoreProjectAsync("Analytics", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Microsoft.ApplicationInsights.AspNetCore", "2.22.0"),
            new PackageModel("Prometheus-net.AspNetCore", "8.0.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Analytics", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Analytics", solutionModel.SrcDirectory);

        analyticsArtifactFactory.AddCoreFiles(coreProject);
        analyticsArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Analytics");
        analyticsArtifactFactory.AddApiFiles(apiProject, "Analytics");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateBillingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Billing microservice with full implementation");

        var solutionModel = new SolutionModel("Billing", directory);

        var coreProject = await CreateCoreProjectAsync("Billing", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Stripe.net", "43.0.0"),
            new PackageModel("PayPalCheckoutSdk", "1.0.4")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Billing", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Billing", solutionModel.SrcDirectory);

        billingArtifactFactory.AddCoreFiles(coreProject);
        billingArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Billing");
        billingArtifactFactory.AddApiFiles(apiProject, "Billing");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateOcrVisionMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating OcrVision microservice with full implementation");

        var solutionModel = new SolutionModel("OcrVision", directory);

        var coreProject = await CreateCoreProjectAsync("OcrVision", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Azure.AI.FormRecognizer", "4.1.0"),
            new PackageModel("Tesseract", "5.2.0"),
            new PackageModel("Google.Cloud.Vision.V1", "3.4.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("OcrVision", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("OcrVision", solutionModel.SrcDirectory);

        ocrVisionArtifactFactory.AddCoreFiles(coreProject);
        ocrVisionArtifactFactory.AddInfrastructureFiles(infrastructureProject, "OcrVision");
        ocrVisionArtifactFactory.AddApiFiles(apiProject, "OcrVision");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateSchedulingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Scheduling microservice with full implementation");

        var solutionModel = new SolutionModel("Scheduling", directory);

        var coreProject = await CreateCoreProjectAsync("Scheduling", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Quartz", "3.8.0"),
            new PackageModel("Hangfire.AspNetCore", "1.8.6"),
            new PackageModel("Ical.Net", "4.2.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Scheduling", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Scheduling", solutionModel.SrcDirectory);

        schedulingArtifactFactory.AddCoreFiles(coreProject);
        schedulingArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Scheduling");
        schedulingArtifactFactory.AddApiFiles(apiProject, "Scheduling");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateAuditMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Audit microservice with full implementation");

        var solutionModel = new SolutionModel("Audit", directory);

        var coreProject = await CreateCoreProjectAsync("Audit", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Audit.NET", "23.0.0"),
            new PackageModel("Audit.EntityFramework.Core", "23.0.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Audit", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Audit", solutionModel.SrcDirectory);

        auditArtifactFactory.AddCoreFiles(coreProject);
        auditArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Audit");
        auditArtifactFactory.AddApiFiles(apiProject, "Audit");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateExportMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Export microservice with full implementation");

        var solutionModel = new SolutionModel("Export", directory);

        var coreProject = await CreateCoreProjectAsync("Export", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("QuestPDF", "2023.12.0"),
            new PackageModel("ClosedXML", "0.102.1"),
            new PackageModel("CsvHelper", "30.0.1")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Export", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Export", solutionModel.SrcDirectory);

        exportArtifactFactory.AddCoreFiles(coreProject);
        exportArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Export");
        exportArtifactFactory.AddApiFiles(apiProject, "Export");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateEmailMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Email microservice with full implementation");

        var solutionModel = new SolutionModel("Email", directory);

        var coreProject = await CreateCoreProjectAsync("Email", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("SendGrid", "9.28.1"),
            new PackageModel("MailKit", "4.3.0"),
            new PackageModel("Fluid.Core", "2.5.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Email", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Email", solutionModel.SrcDirectory);

        emailArtifactFactory.AddCoreFiles(coreProject);
        emailArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Email");
        emailArtifactFactory.AddApiFiles(apiProject, "Email");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateIntegrationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Integration microservice with full implementation");

        var solutionModel = new SolutionModel("Integration", directory);

        var coreProject = await CreateCoreProjectAsync("Integration", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Polly", "8.2.0"),
            new PackageModel("Refit.HttpClientFactory", "7.0.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Integration", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Integration", solutionModel.SrcDirectory);

        integrationArtifactFactory.AddCoreFiles(coreProject);
        integrationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Integration");
        integrationArtifactFactory.AddApiFiles(apiProject, "Integration");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateMediaMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Media microservice with full implementation");

        var solutionModel = new SolutionModel("Media", directory);

        var coreProject = await CreateCoreProjectAsync("Media", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("SixLabors.ImageSharp", "3.1.0"),
            new PackageModel("FFMpegCore", "5.1.0"),
            new PackageModel("NAudio", "2.2.1")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Media", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Media", solutionModel.SrcDirectory);

        mediaArtifactFactory.AddCoreFiles(coreProject);
        mediaArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Media");
        mediaArtifactFactory.AddApiFiles(apiProject, "Media");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateGeolocationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Geolocation microservice with full implementation");

        var solutionModel = new SolutionModel("Geolocation", directory);

        var coreProject = await CreateCoreProjectAsync("Geolocation", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("NetTopologySuite", "2.5.0"),
            new PackageModel("GoogleMaps.LocationServices", "1.2.0.6"),
            new PackageModel("GeoCoordinate.NetCore", "1.0.0.1")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Geolocation", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Geolocation", solutionModel.SrcDirectory);

        geolocationArtifactFactory.AddCoreFiles(coreProject);
        geolocationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Geolocation");
        geolocationArtifactFactory.AddApiFiles(apiProject, "Geolocation");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateTaggingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Tagging microservice with full implementation");

        var solutionModel = new SolutionModel("Tagging", directory);

        var coreProject = await CreateCoreProjectAsync("Tagging", solutionModel.SrcDirectory, Array.Empty<PackageModel>());
        var infrastructureProject = await CreateInfrastructureProjectAsync("Tagging", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Tagging", solutionModel.SrcDirectory);

        taggingArtifactFactory.AddCoreFiles(coreProject);
        taggingArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Tagging");
        taggingArtifactFactory.AddApiFiles(apiProject, "Tagging");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateCollaborationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Collaboration microservice with full implementation");

        var solutionModel = new SolutionModel("Collaboration", directory);

        var coreProject = await CreateCoreProjectAsync("Collaboration", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Microsoft.AspNetCore.SignalR", "1.1.0"),
            new PackageModel("Yarp.ReverseProxy", "2.1.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Collaboration", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Collaboration", solutionModel.SrcDirectory);

        collaborationArtifactFactory.AddCoreFiles(coreProject);
        collaborationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Collaboration");
        collaborationArtifactFactory.AddApiFiles(apiProject, "Collaboration");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateCalculationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Calculation microservice with full implementation");

        var solutionModel = new SolutionModel("Calculation", directory);

        var coreProject = await CreateCoreProjectAsync("Calculation", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("MathNet.Numerics", "5.0.0"),
            new PackageModel("NCalc2", "2.1.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Calculation", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Calculation", solutionModel.SrcDirectory);

        calculationArtifactFactory.AddCoreFiles(coreProject);
        calculationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Calculation");
        calculationArtifactFactory.AddApiFiles(apiProject, "Calculation");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateImportMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Import microservice with full implementation");

        var solutionModel = new SolutionModel("Import", directory);

        var coreProject = await CreateCoreProjectAsync("Import", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("CsvHelper", "30.0.1"),
            new PackageModel("ExcelDataReader", "3.6.0"),
            new PackageModel("ExcelDataReader.DataSet", "3.6.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Import", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Import", solutionModel.SrcDirectory);

        importArtifactFactory.AddCoreFiles(coreProject);
        importArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Import");
        importArtifactFactory.AddApiFiles(apiProject, "Import");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateCacheMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Cache microservice with full implementation");

        var solutionModel = new SolutionModel("Cache", directory);

        var coreProject = await CreateCoreProjectAsync("Cache", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Microsoft.Extensions.Caching.StackExchangeRedis", "8.0.0"),
            new PackageModel("StackExchange.Redis", "2.7.10")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Cache", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Cache", solutionModel.SrcDirectory);

        cacheArtifactFactory.AddCoreFiles(coreProject);
        cacheArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Cache");
        cacheArtifactFactory.AddApiFiles(apiProject, "Cache");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateRateLimitingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating RateLimiting microservice with full implementation");

        var solutionModel = new SolutionModel("RateLimiting", directory);

        var coreProject = await CreateCoreProjectAsync("RateLimiting", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("AspNetCoreRateLimit", "5.0.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("RateLimiting", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("RateLimiting", solutionModel.SrcDirectory);

        rateLimitingArtifactFactory.AddCoreFiles(coreProject);
        rateLimitingArtifactFactory.AddInfrastructureFiles(infrastructureProject, "RateLimiting");
        rateLimitingArtifactFactory.AddApiFiles(apiProject, "RateLimiting");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateLocalizationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Localization microservice with full implementation");

        var solutionModel = new SolutionModel("Localization", directory);

        var coreProject = await CreateCoreProjectAsync("Localization", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Microsoft.Extensions.Localization", "8.0.0"),
            new PackageModel("OrchardCore.Localization.Core", "1.8.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Localization", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Localization", solutionModel.SrcDirectory);

        localizationArtifactFactory.AddCoreFiles(coreProject);
        localizationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Localization");
        localizationArtifactFactory.AddApiFiles(apiProject, "Localization");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateWorkflowMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Workflow microservice with full implementation");

        var solutionModel = new SolutionModel("Workflow", directory);

        var coreProject = await CreateCoreProjectAsync("Workflow", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Elsa", "2.14.1"),
            new PackageModel("WorkflowCore", "3.9.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("Workflow", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("Workflow", solutionModel.SrcDirectory);

        workflowArtifactFactory.AddCoreFiles(coreProject);
        workflowArtifactFactory.AddInfrastructureFiles(infrastructureProject, "Workflow");
        workflowArtifactFactory.AddApiFiles(apiProject, "Workflow");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateConfigurationManagementMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating ConfigurationManagement microservice with full implementation");

        var solutionModel = new SolutionModel("EventMonitoring.ConfigurationManagement", directory);

        var coreProject = await CreateCoreProjectAsync("EventMonitoring.ConfigurationManagement", solutionModel.SrcDirectory, Array.Empty<PackageModel>());
        var infrastructureProject = await CreateInfrastructureProjectAsync("EventMonitoring.ConfigurationManagement", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("EventMonitoring.ConfigurationManagement", solutionModel.SrcDirectory);

        configurationManagementArtifactFactory.AddCoreFiles(coreProject);
        configurationManagementArtifactFactory.AddInfrastructureFiles(infrastructureProject, "EventMonitoring.ConfigurationManagement");
        configurationManagementArtifactFactory.AddInfrastructureSeederFiles(infrastructureProject);
        configurationManagementArtifactFactory.AddApiFiles(apiProject, "EventMonitoring.ConfigurationManagement");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateTelemetryStreamingMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating TelemetryStreaming microservice with full implementation");

        var solutionModel = new SolutionModel("EventMonitoring.TelemetryStreaming", directory);

        var coreProject = await CreateCoreProjectAsync("EventMonitoring.TelemetryStreaming", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("Microsoft.AspNetCore.SignalR", "1.1.0")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("EventMonitoring.TelemetryStreaming", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("EventMonitoring.TelemetryStreaming", solutionModel.SrcDirectory);

        telemetryStreamingArtifactFactory.AddCoreFiles(coreProject);
        telemetryStreamingArtifactFactory.AddInfrastructureFiles(infrastructureProject, "EventMonitoring.TelemetryStreaming");
        telemetryStreamingArtifactFactory.AddApiFiles(apiProject, "EventMonitoring.TelemetryStreaming");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateHistoricalTelemetryMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating HistoricalTelemetry microservice with full implementation");

        var solutionModel = new SolutionModel("EventMonitoring.HistoricalTelemetry", directory);

        var coreProject = await CreateCoreProjectAsync("EventMonitoring.HistoricalTelemetry", solutionModel.SrcDirectory, new[]
        {
            new PackageModel("StackExchange.Redis", "2.7.10")
        });
        var infrastructureProject = await CreateInfrastructureProjectAsync("EventMonitoring.HistoricalTelemetry", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("EventMonitoring.HistoricalTelemetry", solutionModel.SrcDirectory);

        historicalTelemetryArtifactFactory.AddCoreFiles(coreProject);
        historicalTelemetryArtifactFactory.AddInfrastructureFiles(infrastructureProject, "EventMonitoring.HistoricalTelemetry");
        historicalTelemetryArtifactFactory.AddApiFiles(apiProject, "EventMonitoring.HistoricalTelemetry");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateGitAnalysisMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating GitAnalysis microservice with full implementation");

        var solutionModel = new SolutionModel("GitAnalysis", directory);

        var coreProject = await CreateCoreProjectAsync("GitAnalysis", solutionModel.SrcDirectory, Array.Empty<PackageModel>());
        var infrastructureProject = await CreateInfrastructureProjectAsync("GitAnalysis", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("GitAnalysis", solutionModel.SrcDirectory);

        // Add LibGit2Sharp to infrastructure for Git operations
        infrastructureProject.Packages.Add(new PackageModel("LibGit2Sharp", "0.27.2"));

        gitAnalysisArtifactFactory.AddCoreFiles(coreProject);
        gitAnalysisArtifactFactory.AddInfrastructureFiles(infrastructureProject, "GitAnalysis");
        gitAnalysisArtifactFactory.AddApiFiles(apiProject, "GitAnalysis");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    public async Task<SolutionModel> CreateRealtimeNotificationMicroserviceAsync(string directory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating RealtimeNotification microservice with full implementation");

        var solutionModel = new SolutionModel("RealtimeNotification", directory);

        var coreProject = await CreateCoreProjectAsync("RealtimeNotification", solutionModel.SrcDirectory, Array.Empty<PackageModel>());
        var infrastructureProject = await CreateInfrastructureProjectAsync("RealtimeNotification", solutionModel.SrcDirectory);
        var apiProject = await CreateApiProjectAsync("RealtimeNotification", solutionModel.SrcDirectory);

        // Add SignalR and Redis packages
        coreProject.Packages.Add(new PackageModel("Microsoft.AspNetCore.SignalR.Common", "8.0.0"));
        infrastructureProject.Packages.Add(new PackageModel("StackExchange.Redis", "2.7.10"));
        apiProject.Packages.AddRange(new[]
        {
            new PackageModel("Microsoft.AspNetCore.SignalR.StackExchangeRedis", "8.0.0"),
            new PackageModel("Microsoft.AspNetCore.Authentication.JwtBearer", "8.0.0")
        });

        realtimeNotificationArtifactFactory.AddCoreFiles(coreProject);
        realtimeNotificationArtifactFactory.AddInfrastructureFiles(infrastructureProject, "RealtimeNotification");
        realtimeNotificationArtifactFactory.AddApiFiles(apiProject, "RealtimeNotification");

        solutionModel.Projects.Add(coreProject);
        solutionModel.Projects.Add(infrastructureProject);
        solutionModel.Projects.Add(apiProject);

        solutionModel.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        solutionModel.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));

        return solutionModel;
    }

    private Task<ProjectModel> CreateIdentityCoreProjectAsync(string directory)
    {
        var project = new ProjectModel
        {
            Name = "Identity.Core",
            Directory = Path.Combine(directory, "Identity.Core"),
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
            new PackageModel("Newtonsoft.Json", "13.0.3"),
            new PackageModel("Microsoft.AspNetCore.Identity.EntityFrameworkCore", "8.0.0"),
            new PackageModel("Microsoft.AspNetCore.Authentication.JwtBearer", "8.0.0"),
            new PackageModel("System.IdentityModel.Tokens.Jwt", "7.0.3")
        });

        return Task.FromResult(project);
    }

    private Task<ProjectModel> CreateIdentityInfrastructureProjectAsync(string directory)
    {
        var project = new ProjectModel
        {
            Name = "Identity.Infrastructure",
            Directory = Path.Combine(directory, "Identity.Infrastructure"),
            DotNetProjectType = DotNetProjectType.ClassLib
        };

        project.Packages.AddRange(new[]
        {
            new PackageModel("Microsoft.EntityFrameworkCore.SqlServer", "8.0.0"),
            new PackageModel("Microsoft.EntityFrameworkCore.Design", "8.0.0"),
            new PackageModel("Microsoft.EntityFrameworkCore.Tools", "8.0.0"),
            new PackageModel("Microsoft.Extensions.Configuration.Abstractions", "8.0.0")
        });

        project.References.Add(@"..\Identity.Core\Identity.Core.csproj");

        return Task.FromResult(project);
    }

    private Task<ProjectModel> CreateIdentityApiProjectAsync(string directory)
    {
        var project = new ProjectModel
        {
            Name = "Identity.Api",
            Directory = Path.Combine(directory, "Identity.Api"),
            DotNetProjectType = DotNetProjectType.Web
        };

        project.Packages.AddRange(new[]
        {
            new PackageModel("Asp.Versioning.Mvc", "8.0.0"),
            new PackageModel("Microsoft.AspNetCore.OpenApi", "8.0.0"),
            new PackageModel("Serilog.AspNetCore", "8.0.0"),
            new PackageModel("Swashbuckle.AspNetCore", "6.5.0"),
            new PackageModel("Microsoft.AspNetCore.Authentication.JwtBearer", "8.0.0")
        });

        project.References.Add(@"..\Identity.Infrastructure\Identity.Infrastructure.csproj");

        return Task.FromResult(project);
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
            new PackageModel("Microsoft.EntityFrameworkCore.Tools", "8.0.0"),
            new PackageModel("Microsoft.Extensions.Options.ConfigurationExtensions", "8.0.0")
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
