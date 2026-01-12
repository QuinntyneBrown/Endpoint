# Rate Limiting Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Controls API usage, prevents abuse, and enforces usage quotas.

## 2. Microservice Structure

### Core Layer (RateLimiting.Core)
- Entities: RateLimit, QuotaUsage, RateLimitRule
- Interfaces: IRateLimitRepository, IRateLimitService
- Events: RateLimitExceededEvent, QuotaResetEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=RateLimitingDb;Trusted_Connection=True;TrustServerCertificate=True`
- Redis for rate limit counters (high performance)
- DbContext: RateLimitingDbContext (rules and history)

### API Layer
- Middleware: Rate limiting middleware for all API endpoints
- Endpoints: GET /api/ratelimits/status, GET /api/ratelimits/quotas
- Support custom headers: X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset

## 3. Platform Tiers
- Basic: Simple request counting per endpoint
- Standard: Token bucket/sliding window algorithms, per-user limits, configurable thresholds
- Enterprise: Tiered rate limits by subscription, API key management, quota management, real-time analytics, adaptive limiting

## 4. Database Schema
- RateLimitRules: RuleId PK, Endpoint, Limit, Window (seconds), TierId, CreatedAt
- QuotaUsage: UserId (PK), ApiKey, CurrentUsage, QuotaLimit, ResetAt

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
