# Cache Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Provides distributed caching for improved performance and reduced database load.

## 2. Microservice Structure

### Core Layer (Cache.Core)
- Entities: CacheEntry, CacheStatistics
- Interfaces: ICacheService, ICacheInvalidation
- Events: CacheUpdatedEvent, CacheInvalidatedEvent

### Infrastructure Layer
- SQL Server Express (for metadata): `Server=.\\SQLEXPRESS;Database=CacheDb;Trusted_Connection=True;TrustServerCertificate=True`
- Redis for actual caching
- DbContext: CacheDbContext (metadata only)

### API Layer
- Endpoints: GET /api/cache/{key}, PUT /api/cache/{key}, DELETE /api/cache/{key}
- Support TTL, sliding expiration

## 3. Platform Tiers
- Basic: In-memory caching per instance
- Standard: Redis distributed caching, cache invalidation, TTL management
- Enterprise: Multi-tier caching, cache warming, cache analytics, geographic distribution, cache-aside patterns

## 4. Database Schema
- CacheMetadata: Key (PK), Type, ExpiresAt, AccessCount, LastAccessedAt
- CacheStatistics: Timestamp (PK), HitCount, MissCount, EvictionCount

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
