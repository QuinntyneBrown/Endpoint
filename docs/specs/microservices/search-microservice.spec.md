# Search Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: [Implementation Specification](../../implementation.spec.md), [Message Design Specification](../messaging/message-design.spec.md)

## 1. Overview

The Search Microservice provides full-text search, filtering, and indexing capabilities across application data.

## 2. Microservice Structure

### 2.1 Core Layer (Search.Core)

**Requirements**:
- REQ-SEARCH-CORE-001: MUST contain entities: SearchIndex, SearchQuery, SearchResult
- REQ-SEARCH-CORE-002: MUST contain interfaces: ISearchRepository, ISearchService, IIndexer
- REQ-SEARCH-CORE-003: MUST contain domain events: DocumentIndexedEvent, SearchPerformedEvent
- REQ-SEARCH-CORE-004: MUST support search operations: Match, Filter, Sort, Paginate, Facet

**Acceptance Criteria**:
- AC-SEARCH-CORE-001: SearchResult MUST include: DocumentId, Score, Highlights, MatchedFields

### 2.2 Infrastructure Layer (Search.Infrastructure)

**Requirements**:
- REQ-SEARCH-INFRA-001: MUST use SQL Server Express: `Server=.\\SQLEXPRESS;Database=SearchDb;Trusted_Connection=True;TrustServerCertificate=True`
- REQ-SEARCH-INFRA-002: MUST implement SQL full-text search (Basic tier)
- REQ-SEARCH-INFRA-003: MUST implement Elasticsearch integration (Standard/Enterprise tier)
- REQ-SEARCH-INFRA-004: MUST implement search result caching
- REQ-SEARCH-INFRA-005: MUST publish domain events using MessagePack

**Acceptance Criteria**:
- AC-SEARCH-INFRA-001: DbContext MUST be named SearchDbContext
- AC-SEARCH-INFRA-002: Index updates MUST be asynchronous
- AC-SEARCH-INFRA-003: Search queries MUST support pagination

### 2.3 API Layer (Search.Api)

**Requirements**:
- REQ-SEARCH-API-001: MUST expose: POST /api/search (query), POST /api/search/index (add document), DELETE /api/search/index/{id}
- REQ-SEARCH-API-002: MUST expose: GET /api/search/suggestions (autocomplete)
- REQ-SEARCH-API-003: MUST support query parameters: q (query), filters, sort, page, pageSize
- REQ-SEARCH-API-004: MUST return results with relevance scoring

## 3. Platform Tiers

### Basic Tier
- SQL-based LIKE queries
- Simple filtering

### Standard Tier
- Full-text search with Elasticsearch
- Faceted search
- Relevance scoring
- Auto-suggestions

### Enterprise Tier
- Advanced NLP
- Semantic search
- Multi-language support
- Search analytics
- Federated search across services

## 4. Domain Events

- DocumentIndexedEvent: `search.document.indexed.v1`
- DocumentRemovedEvent: `search.document.removed.v1`
- SearchPerformedEvent: `search.query.performed.v1`

## 5. Database Schema

**Database Name**: SearchDb

**Tables**:
- SearchIndexes: IndexId (PK), EntityType, EntityId, Content (TEXT), CreatedAt, UpdatedAt
- SearchQueries: QueryId (PK), Query, ResultCount, ExecutedAt, UserId

## 6. Alignment with Implementation Spec

- Use C# 11+, three-layer architecture
- SQL Server Express for query logging
- MessagePack for event serialization
- Include copyright headers

## Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
