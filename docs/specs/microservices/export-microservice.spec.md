# Export Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Generates reports and exports data in various formats (PDF, CSV, Excel, JSON).

## 2. Microservice Structure

### Core Layer (Export.Core)
- Entities: ExportJob, ExportTemplate, ExportResult
- Interfaces: IExportRepository, IExportService, IReportGenerator
- Events: ExportCompletedEvent, ExportFailedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=ExportDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: ExportDbContext
- Libraries: QuestPDF (PDF), ClosedXML (Excel), CsvHelper (CSV)

### API Layer
- Endpoints: POST /api/export/generate, GET /api/export/jobs/{id}, GET /api/export/download/{id}
- Support async processing for large datasets

## 3. Platform Tiers
- Basic: CSV export only
- Standard: PDF generation, Excel export, customizable templates
- Enterprise: Scheduled reports, branded templates, bulk export, async processing, email delivery

## 4. Database Schema
- ExportJobs: JobId PK, TemplateId, Format, Status, FilePath, CreatedAt, CompletedAt
- ExportTemplates: TemplateId PK, Name, Format, Configuration (JSON)

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
