# Integration Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Manages third-party API integrations, webhooks, and data synchronization.

## 2. Microservice Structure

### Core Layer (Integration.Core)
- Entities: Integration, Webhook, SyncJob, ApiCredential
- Interfaces: IIntegrationRepository, IWebhookHandler, ISyncService
- Events: IntegrationConnectedEvent, WebhookReceivedEvent, SyncCompletedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=IntegrationDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: IntegrationDbContext
- OAuth token management, retry logic

### API Layer
- Endpoints: POST /api/integrations, GET /api/integrations/{id}, POST /api/webhooks/{integrationId}
- Support inbound/outbound webhooks

## 3. Platform Tiers
- Basic: Hardcoded integrations
- Standard: Webhook support, OAuth token management, retry logic
- Enterprise: Integration marketplace, custom connectors, data transformation, rate limiting, circuit breakers

## 4. Database Schema
- Integrations: IntegrationId PK, Provider, Status, Credentials (encrypted), CreatedAt
- Webhooks: WebhookId PK, IntegrationId FK, Url, Event, Payload, ProcessedAt

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
