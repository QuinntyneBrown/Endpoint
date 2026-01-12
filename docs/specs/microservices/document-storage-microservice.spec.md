# Document Storage Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: [Implementation Specification](../implementation.spec.md), [Message Design Specification](../messaging/message-design.spec.md)

## 1. Overview

The Document Storage Microservice manages file uploads, document storage, retrieval, and lifecycle management with support for various storage backends.

## 2. Microservice Structure

### 2.1 Core Layer (DocumentStorage.Core)

**Requirements**:
- REQ-DOCSTOR-CORE-001: MUST contain entities: Document, DocumentVersion, DocumentMetadata, DocumentTag
- REQ-DOCSTOR-CORE-002: MUST contain interfaces: IDocumentRepository, IStorageProvider
- REQ-DOCSTOR-CORE-003: MUST contain domain events: DocumentUploadedEvent, DocumentDeletedEvent, DocumentVersionedEvent
- REQ-DOCSTOR-CORE-004: MUST support storage backends: Local FileSystem, Azure Blob, AWS S3

**Acceptance Criteria**:
- AC-DOCSTOR-CORE-001: Document entity MUST include: DocumentId, FileName, ContentType, SizeBytes, StoragePath, UploadedBy, UploadedAt, Version

### 2.2 Infrastructure Layer (DocumentStorage.Infrastructure)

**Requirements**:
- REQ-DOCSTOR-INFRA-001: MUST use SQL Server Express: `Server=.\\SQLEXPRESS;Database=DocumentStorageDb;Trusted_Connection=True;TrustServerCertificate=True`
- REQ-DOCSTOR-INFRA-002: MUST implement local file system storage (Basic tier)
- REQ-DOCSTOR-INFRA-003: MUST implement cloud storage integration (Standard/Enterprise tiers)
- REQ-DOCSTOR-INFRA-004: MUST implement file versioning (Standard tier)
- REQ-DOCSTOR-INFRA-005: MUST implement encryption at rest (Enterprise tier)
- REQ-DOCSTOR-INFRA-006: MUST publish domain events using MessagePack

**Acceptance Criteria**:
- AC-DOCSTOR-INFRA-001: DbContext MUST be named DocumentStorageDbContext
- AC-DOCSTOR-INFRA-002: File paths MUST be stored as relative paths
- AC-DOCSTOR-INFRA-003: Virus scanning integration point MUST exist (Enterprise tier)

### 2.3 API Layer (DocumentStorage.Api)

**Requirements**:
- REQ-DOCSTOR-API-001: MUST expose: POST /api/documents (upload), GET /api/documents/{id} (metadata), GET /api/documents/{id}/download
- REQ-DOCSTOR-API-002: MUST expose: DELETE /api/documents/{id}, GET /api/documents/search
- REQ-DOCSTOR-API-003: MUST support multipart form upload
- REQ-DOCSTOR-API-004: MUST implement file size limits (configurable, default 50MB)
- REQ-DOCSTOR-API-005: MUST validate file types (whitelist approach)

## 3. Platform Tiers

### Basic Tier
- Local file system storage
- Simple upload/download
- File size limit: 10MB

### Standard Tier
- Cloud storage (Azure Blob or S3)
- File versioning
- Metadata tagging
- File size limit: 100MB

### Enterprise Tier
- Encryption at rest
- CDN integration
- Virus scanning
- Document lifecycle management
- Compliance retention policies
- File size limit: 500MB

## 4. Domain Events

- DocumentUploadedEvent: `documentstorage.document.uploaded.v1`
- DocumentDeletedEvent: `documentstorage.document.deleted.v1`
- DocumentVersionedEvent: `documentstorage.document.versioned.v1`
- DocumentScannedEvent: `documentstorage.document.scanned.v1`

## 5. Database Schema

**Database Name**: DocumentStorageDb

**Tables**:
- Documents: DocumentId (PK), FileName, ContentType, SizeBytes, StoragePath, UploadedBy, UploadedAt, IsDeleted
- DocumentVersions: VersionId (PK), DocumentId (FK), VersionNumber, StoragePath, CreatedAt, CreatedBy
- DocumentMetadata: DocumentId (FK), Key, Value
- DocumentTags: TagId (PK), DocumentId (FK), TagName

**Indexes**:
- IX_Documents_UploadedBy
- IX_Documents_UploadedAt
- IX_DocumentTags_TagName

## 6. Alignment with Implementation Spec

- Use C# 11+, three-layer architecture
- SQL Server Express for metadata storage
- MessagePack for event serialization
- Include copyright headers

## Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
