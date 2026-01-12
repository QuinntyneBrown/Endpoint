# Specifications Index

## Overview

This directory contains formal specifications for the Endpoint code generation framework and its microservices platform architecture.

## Document Structure

### Core Specifications

- **[Implementation Specification](./implementation.spec.md)** - Core technical requirements, architectural principles, and coding conventions for the Endpoint framework. This is the foundational document that all other specifications must align with.

### Messaging Specifications

Located in `messaging/`:

- **[Message Design Specification](./messaging/message-design.spec.md)** - Defines requirements for high-performance message design using Redis Pub/Sub, including serialization with MessagePack, schema evolution, and message envelope structure.

- **[Subscription Design Specification](./messaging/subscription-design.spec.md)** - Defines requirements for selective message subscription and efficient routing in microservices, including idempotency, cycle detection, and causation tracking.

### Microservices Specifications

Located in `microservices/`:

All microservice specifications follow a consistent three-layer architecture:
1. **Core Layer** - Domain entities, business logic, and abstractions
2. **Infrastructure Layer** - Data access with SQL Server Express, external service integrations
3. **API Layer** - HTTP endpoints, request/response models, validation

#### Platform Microservices (24 Total)

1. **[Identity Microservice](./microservices/identity-microservice.spec.md)** - Authentication, authorization, and identity management (OAuth, RBAC, MFA, SSO)

2. **[Tenant Microservice](./microservices/tenant-microservice.spec.md)** - Multi-tenant architecture, tenant isolation, tenant-specific configurations

3. **[Notification Microservice](./microservices/notification-microservice.spec.md)** - Multi-channel notifications (email, SMS, push, in-app)

4. **[Document Storage Microservice](./microservices/document-storage-microservice.spec.md)** - File uploads, document storage, versioning, retrieval

5. **[Search Microservice](./microservices/search-microservice.spec.md)** - Full-text search, filtering, indexing with Elasticsearch

6. **[Analytics Microservice](./microservices/analytics-microservice.spec.md)** - Usage metrics, trends, business intelligence, dashboards

7. **[Billing Microservice](./microservices/billing-microservice.spec.md)** - Subscription management, payment processing, invoicing

8. **[OCR/Vision Microservice](./microservices/ocr-vision-microservice.spec.md)** - Optical character recognition, image analysis, document extraction

9. **[Scheduling Microservice](./microservices/scheduling-microservice.spec.md)** - Appointments, reminders, recurring events, calendar sync

10. **[Audit Microservice](./microservices/audit-microservice.spec.md)** - Event logging, change tracking, compliance reporting

11. **[Export Microservice](./microservices/export-microservice.spec.md)** - Report generation in PDF, CSV, Excel, JSON formats

12. **[Email Microservice](./microservices/email-microservice.spec.md)** - Transactional and marketing emails with template management

13. **[Integration Microservice](./microservices/integration-microservice.spec.md)** - Third-party API integrations, webhooks, data synchronization

14. **[Media Microservice](./microservices/media-microservice.spec.md)** - Image/video processing, compression, thumbnails, format conversion

15. **[Geolocation Microservice](./microservices/geolocation-microservice.spec.md)** - Mapping, geocoding, location tracking, spatial queries

16. **[Tagging Microservice](./microservices/tagging-microservice.spec.md)** - Tags, categories, labels, hierarchical classification

17. **[Collaboration Microservice](./microservices/collaboration-microservice.spec.md)** - Sharing, commenting, real-time collaboration

18. **[Calculation Microservice](./microservices/calculation-microservice.spec.md)** - Financial calculations, projections, simulations

19. **[Import Microservice](./microservices/import-microservice.spec.md)** - Bulk data import from CSV, Excel, JSON, external sources

20. **[Cache Microservice](./microservices/cache-microservice.spec.md)** - Distributed caching with Redis for performance optimization

21. **[Rate Limiting Microservice](./microservices/rate-limiting-microservice.spec.md)** - API usage control, abuse prevention, quota enforcement

22. **[Localization Microservice](./microservices/localization-microservice.spec.md)** - Translations, regional settings, internationalization

23. **[Workflow Microservice](./microservices/workflow-microservice.spec.md)** - Multi-step business processes, approvals, state machines

24. **[Backup Microservice](./microservices/backup-microservice.spec.md)** - Data backup, disaster recovery, point-in-time restoration

## Common Requirements Across All Microservices

### Technology Stack

- **Language**: C# 11+ with nullable reference types, file-scoped namespaces, required keyword
- **Framework**: ASP.NET Core for API layer
- **Database**: SQL Server Express (connection string pattern: `Server=.\\SQLEXPRESS;Database={ServiceName}Db;Trusted_Connection=True;TrustServerCertificate=True`)
- **ORM**: Entity Framework Core for data access
- **Messaging**: Redis Pub/Sub with MessagePack serialization
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI

### Architecture Principles

1. **Three-Layer Architecture**: Core → Infrastructure → API
2. **Domain Events**: All state changes publish events via messaging
3. **Dependency Injection**: All dependencies resolved via DI container
4. **Repository Pattern**: Data access abstracted behind interfaces
5. **CQRS**: Commands and queries separated where appropriate

### Coding Conventions

- Include copyright headers in all source files
- Follow PascalCase for public members, camelCase for private
- Use async/await for all I/O operations
- Implement proper error handling and status codes (400, 401, 403, 404, 500)
- Include comprehensive unit and integration tests

## Platform Tiers

Most microservices support three operational tiers:

- **Basic Tier**: Minimal functionality, suitable for development and small deployments
- **Standard Tier**: Full-featured for production use with common enterprise needs
- **Enterprise Tier**: Advanced capabilities including compliance, analytics, and scale

## Related Documentation

- Source design documents that inspired these specifications:
  - `docs/high-perf-message-design.md` - Original messaging design (now formalized)
  - `docs/sub-design.md` - Original subscription design (now formalized)
- External reference:
  - [Microservices Platform Architecture](https://raw.githubusercontent.com/QuinntyneBrown/Apps/refs/heads/main/docs/microservices-platform-architecture.md)

## Document Maintenance

All specifications are living documents and should be updated as the architecture evolves. Changes must:
1. Maintain backward compatibility or include migration guidance
2. Update the revision history table
3. Be reviewed by the architecture team

## Version

- **Current Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
