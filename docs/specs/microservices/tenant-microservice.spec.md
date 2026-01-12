# Tenant Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: [Implementation Specification](../../implementation.spec.md), [Message Design Specification](../messaging/message-design.spec.md)

## 1. Overview

The Tenant Microservice manages multi-tenant architecture, tenant isolation, and tenant-specific configurations across all applications in the platform.

## 2. Microservice Structure

### 2.1 Core Layer (Tenant.Core)

**Requirements**:
- REQ-TENANT-CORE-001: MUST contain entities: Tenant, TenantConfiguration, TenantFeature
- REQ-TENANT-CORE-002: MUST contain repository interfaces: ITenantRepository
- REQ-TENANT-CORE-003: MUST contain domain events: TenantCreatedEvent, TenantProvisionedEvent, TenantSuspendedEvent, TenantDeactivatedEvent
- REQ-TENANT-CORE-004: MUST follow implementation.spec.md conventions

**Acceptance Criteria**:
- AC-TENANT-CORE-001: Tenant entity MUST include: TenantId (GUID), Name, Slug (unique), Status, TenantTier (Basic/Standard/Enterprise), CreatedAt, SubscriptionExpiry

### 2.2 Infrastructure Layer (Tenant.Infrastructure)

**Requirements**:
- REQ-TENANT-INFRA-001: MUST use SQL Server Express: `Server=.\\SQLEXPRESS;Database=TenantDb;Trusted_Connection=True;TrustServerCertificate=True`
- REQ-TENANT-INFRA-002: MUST implement row-level security via EF Core global query filters (Standard tier)
- REQ-TENANT-INFRA-003: MUST support per-tenant database provisioning (Enterprise tier)
- REQ-TENANT-INFRA-004: MUST publish domain events using MessagePack serialization

**Acceptance Criteria**:
- AC-TENANT-INFRA-001: DbContext MUST be named TenantDbContext
- AC-TENANT-INFRA-002: Global query filter MUST apply TenantId filtering automatically
- AC-TENANT-INFRA-003: Enterprise tier MUST support dynamic connection strings per tenant

### 2.3 API Layer (Tenant.Api)

**Requirements**:
- REQ-TENANT-API-001: MUST expose: POST /api/tenants, GET /api/tenants/{id}, PUT /api/tenants/{id}, DELETE /api/tenants/{id}
- REQ-TENANT-API-002: MUST resolve tenant context from JWT claim or X-Tenant-Id header
- REQ-TENANT-API-003: MUST enforce tenant isolation (users can only access their tenant's data)
- REQ-TENANT-API-004: MUST validate tenant slug uniqueness

## 3. Platform Tiers

### Basic Tier
- Single-tenant mode (no isolation needed)

### Standard Tier  
- Multi-tenant with shared database
- Row-level security via TenantId column
- Automatic query filtering

### Enterprise Tier
- Multi-tenant with dedicated databases per tenant
- Tenant provisioning workflows
- Usage metering and reporting

## 4. Domain Events

- TenantCreatedEvent: `tenant.tenant.created.v1`
- TenantProvisionedEvent: `tenant.tenant.provisioned.v1`
- TenantSuspendedEvent: `tenant.tenant.suspended.v1`
- TenantDeactivatedEvent: `tenant.tenant.deactivated.v1`

## 5. Database Schema

**Database Name**: TenantDb

**Tables**:
- Tenants: TenantId (PK, GUID), Name (NVARCHAR(200)), Slug (NVARCHAR(100) UNIQUE), Status (INT), TenantTier (INT), CreatedAt (DATETIME2), SubscriptionExpiry (DATETIME2)
- TenantSettings: SettingId (PK, GUID), TenantId (FK), Key (NVARCHAR(100)), Value (NVARCHAR(MAX))

## 6. Alignment with Implementation Spec

- Use C# 11+ features
- Follow three-layer architecture (Core, Infrastructure, Api)
- Use SQL Server Express for all tiers
- Implement MessagePack serialization for events
- Include copyright headers

## Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
