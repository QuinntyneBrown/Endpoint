# OCR/Vision Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: [Implementation Specification](../../implementation.spec.md), [Message Design Specification](../messaging/message-design.spec.md)

## 1. Overview

The OCR/Vision Microservice performs optical character recognition, image analysis, and document data extraction.

## 2. Microservice Structure

### 2.1 Core Layer (OcrVision.Core)

**Requirements**:
- REQ-OCR-CORE-001: MUST contain entities: OcrResult, ExtractedData, DocumentAnalysis
- REQ-OCR-CORE-002: MUST contain interfaces: IOcrService, IVisionService
- REQ-OCR-CORE-003: MUST contain domain events: DocumentAnalyzedEvent, TextExtractedEvent

**Acceptance Criteria**:
- AC-OCR-CORE-001: OcrResult MUST include: ResultId, DocumentId, ExtractedText, Confidence, Fields

### 2.2 Infrastructure Layer (OcrVision.Infrastructure)

**Requirements**:
- REQ-OCR-INFRA-001: MUST use SQL Server Express: `Server=.\\SQLEXPRESS;Database=OcrVisionDb;Trusted_Connection=True;TrustServerCertificate=True`
- REQ-OCR-INFRA-002: MUST implement Tesseract integration (Basic tier)
- REQ-OCR-INFRA-003: MUST implement Azure Computer Vision/Google Vision API (Standard tier)
- REQ-OCR-INFRA-004: MUST support receipt scanning and field extraction
- REQ-OCR-INFRA-005: MUST publish domain events using MessagePack

**Acceptance Criteria**:
- AC-OCR-INFRA-001: DbContext MUST be named OcrVisionDbContext

### 2.3 API Layer (OcrVision.Api)

**Requirements**:
- REQ-OCR-API-001: MUST expose: POST /api/ocr/analyze (upload image), GET /api/ocr/results/{id}
- REQ-OCR-API-002: MUST support image formats: JPEG, PNG, PDF
- REQ-OCR-API-003: MUST return confidence scores

## 3. Platform Tiers

### Basic Tier: Tesseract OCR
### Standard Tier: Azure/Google Vision API, receipt scanning
### Enterprise Tier: Custom ML models, batch processing, human-in-the-loop

## 4. Domain Events

- DocumentAnalyzedEvent: `ocrvision.document.analyzed.v1`
- TextExtractedEvent: `ocrvision.text.extracted.v1`

## 5. Database Schema

**Database Name**: OcrVisionDb
**Tables**: OcrResults (ResultId PK, DocumentId, ExtractedText, Confidence, ProcessedAt), ExtractedFields (FieldId PK, ResultId FK, FieldName, FieldValue, Confidence)

## Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
