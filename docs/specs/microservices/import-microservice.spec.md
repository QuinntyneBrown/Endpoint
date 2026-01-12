# Import Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Handles bulk data import from various file formats and external sources.

## 2. Microservice Structure

### Core Layer (Import.Core)
- Entities: ImportJob, ImportMapping, ImportResult, ImportError
- Interfaces: IImportRepository, IImportService, IDataValidator
- Events: ImportStartedEvent, ImportCompletedEvent, ImportFailedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=ImportDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: ImportDbContext
- Libraries: CsvHelper, ClosedXML, System.Text.Json

### API Layer
- Endpoints: POST /api/import/upload, GET /api/import/jobs/{id}, POST /api/import/mappings
- Support file formats: CSV, Excel, JSON
- Support validation preview before import

## 3. Platform Tiers
- Basic: CSV upload with fixed column mapping
- Standard: Multi-format support, column mapping UI, validation preview
- Enterprise: Scheduled imports, API-based imports, data transformation rules, duplicate detection, incremental sync

## 4. Database Schema
- ImportJobs: JobId PK, FileName, Status, TotalRows, SuccessRows, ErrorRows, CreatedAt, CompletedAt
- ImportMappings: MappingId PK, Name, SourceFormat, ColumnMappings (JSON)
- ImportErrors: ErrorId PK, JobId FK, RowNumber, ErrorMessage

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
