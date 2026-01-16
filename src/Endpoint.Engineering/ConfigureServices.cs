// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Endpoint.Engineering.Api;
using Endpoint.Engineering.Microservices;
using Endpoint.Engineering.StaticAnalysis.CSharp;
using Endpoint.Engineering.StaticAnalysis.Scss;
using Endpoint.Engineering.StaticAnalysis;
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
using Endpoint.Engineering.Messaging.Artifacts;
using Endpoint.Engineering.StaticAnalysis.Angular;
using Endpoint.Engineering.CyclicRandomizr;
using Endpoint.Engineering.SolutionPruning;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddEngineeringServices(this IServiceCollection services)
    {
        services.AddApiGatewayServices();
        services.AddRedisPubSubServices();
        services.AddSingleton<ICodeParser, CodeParser>();
        services.AddSingleton<ICSharpStaticAnalysisService, CSharpStaticAnalysisService>();

        // Register HttpClient for HtmlParserService
        services.AddHttpClient("HtmlParser", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IHtmlParserService, HtmlParserService>();

        // Register SCSS static analysis service
        services.AddSingleton<IScssStaticAnalysisService, ScssStaticAnalysisService>();
        // Register Angular Static Analysis service
        services.AddSingleton<IAngularStaticAnalysisService, AngularStaticAnalysisService>();
        // Register Static Analysis Service
        services.AddSingleton<IStaticAnalysisService, StaticAnalysisService>();
        // Register Code Review Service
        services.AddSingleton<ICodeReviewService, CodeReviewService>();
        // Register Cyclic Randomizr Service
        services.AddSingleton<ICyclicRandomizrService, CyclicRandomizrService>();

        // Register Solution Prune Service
        services.AddSingleton<ISolutionPruneService, SolutionPruneService>();

        // Register artifact factories for all microservices
        services.AddSingleton<IIdentityArtifactFactory, IdentityArtifactFactory>();
        services.AddSingleton<ITenantArtifactFactory, TenantArtifactFactory>();
        services.AddSingleton<INotificationArtifactFactory, NotificationArtifactFactory>();
        services.AddSingleton<IDocumentStorageArtifactFactory, DocumentStorageArtifactFactory>();
        services.AddSingleton<ISearchArtifactFactory, SearchArtifactFactory>();
        services.AddSingleton<IAnalyticsArtifactFactory, AnalyticsArtifactFactory>();
        services.AddSingleton<IBillingArtifactFactory, BillingArtifactFactory>();
        services.AddSingleton<IOcrVisionArtifactFactory, OcrVisionArtifactFactory>();
        services.AddSingleton<ISchedulingArtifactFactory, SchedulingArtifactFactory>();
        services.AddSingleton<IAuditArtifactFactory, AuditArtifactFactory>();
        services.AddSingleton<IExportArtifactFactory, ExportArtifactFactory>();
        services.AddSingleton<IEmailArtifactFactory, EmailArtifactFactory>();
        services.AddSingleton<IIntegrationArtifactFactory, IntegrationArtifactFactory>();
        services.AddSingleton<IMediaArtifactFactory, MediaArtifactFactory>();
        services.AddSingleton<IGeolocationArtifactFactory, GeolocationArtifactFactory>();
        services.AddSingleton<ITaggingArtifactFactory, TaggingArtifactFactory>();
        services.AddSingleton<ICollaborationArtifactFactory, CollaborationArtifactFactory>();
        services.AddSingleton<ICalculationArtifactFactory, CalculationArtifactFactory>();
        services.AddSingleton<IImportArtifactFactory, ImportArtifactFactory>();
        services.AddSingleton<ICacheArtifactFactory, CacheArtifactFactory>();
        services.AddSingleton<IRateLimitingArtifactFactory, RateLimitingArtifactFactory>();
        services.AddSingleton<ILocalizationArtifactFactory, LocalizationArtifactFactory>();
        services.AddSingleton<IWorkflowArtifactFactory, WorkflowArtifactFactory>();
        services.AddSingleton<IConfigurationManagementArtifactFactory, ConfigurationManagementArtifactFactory>();
        services.AddSingleton<ITelemetryStreamingArtifactFactory, TelemetryStreamingArtifactFactory>();
        services.AddSingleton<IHistoricalTelemetryArtifactFactory, HistoricalTelemetryArtifactFactory>();
        services.AddSingleton<IGitAnalysisArtifactFactory, GitAnalysisArtifactFactory>();
        services.AddSingleton<IRealtimeNotificationArtifactFactory, RealtimeNotificationArtifactFactory>();

        // Register MicroserviceFactory for predefined microservice creation
        services.AddSingleton<IMicroserviceFactory, MicroserviceFactory>();

        // Register Messaging project generator services
        services.AddMessagingProjectGeneratorServices();
    }
}

