# Audit Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Records domain events, user actions, and system changes for compliance and debugging.

## 2. Microservice Structure

### Core Layer (Audit.Core)
- Entities: AuditEntry, ChangeLog
- Interfaces: IAuditRepository, IAuditService
- Events: AuditEntryCreatedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=AuditDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: AuditDbContext
- Immutable append-only storage

### API Layer
- Endpoints: GET /api/audit/entries, GET /api/audit/entries/{id}, GET /api/audit/entity/{entityId}
- Support filtering by: user, entity, date range, action type

## 3. Platform Tiers
- Basic: Simple logging to database
- Standard: Structured event logging, change tracking, configurable retention
- Enterprise: Event sourcing, immutable audit trail, compliance reporting (SOC2, HIPAA), forensic analysis

## 4. Database Schema
- AuditEntries: AuditId PK, UserId, EntityType, EntityId, Action, OldValue, NewValue, Timestamp
- Index on Timestamp, UserId, EntityId

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
