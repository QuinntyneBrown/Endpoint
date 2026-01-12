# Tagging Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Manages tags, categories, labels, and hierarchical classification across entities.

## 2. Microservice Structure

### Core Layer (Tagging.Core)
- Entities: Tag, Category, EntityTag, Taxonomy
- Interfaces: ITagRepository, ITaggingService, ITaxonomyService
- Events: TagCreatedEvent, EntityTaggedEvent, TagMergedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=TaggingDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: TaggingDbContext

### API Layer
- Endpoints: POST /api/tags, GET /api/tags, POST /api/tags/{entityType}/{entityId}
- Support auto-suggestions, tag merging, usage statistics

## 3. Platform Tiers
- Basic: Simple string tags
- Standard: Tag management UI, auto-suggestions, tag merging, usage statistics
- Enterprise: Hierarchical taxonomies, ML-powered auto-tagging, cross-entity tagging, synonym support

## 4. Database Schema
- Tags: TagId PK, Name, Description, UsageCount, CreatedAt
- EntityTags: EntityType, EntityId, TagId (composite PK), TaggedAt
- Taxonomies: TaxonomyId PK, Name, ParentId (FK self-reference)

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
