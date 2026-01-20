// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Solutions;

namespace Endpoint.Engineering.Microservices;

/// <summary>
/// Factory interface for creating predefined microservice solutions.
/// </summary>
public interface IMicroserviceFactory
{
    /// <summary>
    /// Creates an Identity microservice that handles user authentication, authorization, and identity management.
    /// </summary>
    Task<SolutionModel> CreateIdentityMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Tenant microservice that manages multi-tenant architecture and tenant-specific configurations.
    /// </summary>
    Task<SolutionModel> CreateTenantMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Notification microservice that delivers notifications across multiple channels.
    /// </summary>
    Task<SolutionModel> CreateNotificationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Document Storage microservice that manages file uploads and document storage.
    /// </summary>
    Task<SolutionModel> CreateDocumentStorageMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Search microservice that provides full-text search and indexing capabilities.
    /// </summary>
    Task<SolutionModel> CreateSearchMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an Analytics microservice that collects and visualizes usage metrics and business intelligence.
    /// </summary>
    Task<SolutionModel> CreateAnalyticsMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Billing microservice that handles subscription management and payment processing.
    /// </summary>
    Task<SolutionModel> CreateBillingMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an OCR/Vision microservice that performs optical character recognition and image analysis.
    /// </summary>
    Task<SolutionModel> CreateOcrVisionMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Scheduling microservice that manages appointments and calendar synchronization.
    /// </summary>
    Task<SolutionModel> CreateSchedulingMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an Audit microservice that records domain events and user actions for compliance.
    /// </summary>
    Task<SolutionModel> CreateAuditMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an Export microservice that generates reports in various formats.
    /// </summary>
    Task<SolutionModel> CreateExportMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an Email microservice that sends transactional and marketing emails.
    /// </summary>
    Task<SolutionModel> CreateEmailMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an Integration microservice that manages third-party API integrations and webhooks.
    /// </summary>
    Task<SolutionModel> CreateIntegrationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Media microservice that processes images, videos, and audio.
    /// </summary>
    Task<SolutionModel> CreateMediaMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Geolocation microservice that provides mapping and geocoding services.
    /// </summary>
    Task<SolutionModel> CreateGeolocationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Tagging microservice that manages tags and hierarchical classification.
    /// </summary>
    Task<SolutionModel> CreateTaggingMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Collaboration microservice that enables sharing and real-time collaboration.
    /// </summary>
    Task<SolutionModel> CreateCollaborationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Calculation microservice that performs complex financial calculations.
    /// </summary>
    Task<SolutionModel> CreateCalculationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an Import microservice that handles bulk data import from various sources.
    /// </summary>
    Task<SolutionModel> CreateImportMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Cache microservice that provides distributed caching.
    /// </summary>
    Task<SolutionModel> CreateCacheMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Rate Limiting microservice that controls API usage and enforces quotas.
    /// </summary>
    Task<SolutionModel> CreateRateLimitingMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Localization microservice that manages translations and internationalization.
    /// </summary>
    Task<SolutionModel> CreateLocalizationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Workflow microservice that orchestrates multi-step business processes.
    /// </summary>
    Task<SolutionModel> CreateWorkflowMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Configuration Management microservice that acts as the single source of truth for definitions and configuration files.
    /// </summary>
    Task<SolutionModel> CreateConfigurationManagementMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Telemetry Streaming microservice that handles high-frequency, real-time data flow telemetry types.
    /// </summary>
    Task<SolutionModel> CreateTelemetryStreamingMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Historical Telemetry microservice that manages long-term storage and retrieval of telemetry data.
    /// </summary>
    Task<SolutionModel> CreateHistoricalTelemetryMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Git Analysis microservice that handles Git operations including branch isolation, diff generation, and .gitignore parsing.
    /// </summary>
    Task<SolutionModel> CreateGitAnalysisMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Realtime Notification microservice that provides WebSocket/SignalR based notifications with Redis Pub/Sub integration.
    /// </summary>
    Task<SolutionModel> CreateRealtimeNotificationMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Distribution microservice that provides subscription-based telemetry distribution via SignalR WebSockets.
    /// </summary>
    Task<SolutionModel> CreateDistributionMicroserviceAsync(string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a microservice by name.
    /// </summary>
    Task<SolutionModel> CreateByNameAsync(string name, string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of available predefined microservice names.
    /// </summary>
    IReadOnlyList<string> GetAvailableMicroservices();
}
