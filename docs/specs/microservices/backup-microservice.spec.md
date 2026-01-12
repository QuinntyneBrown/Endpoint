# Backup Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Manages data backup, disaster recovery, and point-in-time restoration.

## 2. Microservice Structure

### Core Layer (Backup.Core)
- Entities: BackupJob, BackupSet, RestorePoint, BackupSchedule
- Interfaces: IBackupRepository, IBackupService, IRestoreService
- Events: BackupStartedEvent, BackupCompletedEvent, RestoreCompletedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=BackupDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: BackupDbContext
- Integration with SQL Server backup/restore commands
- Storage: Local file system, Azure Blob Storage

### API Layer
- Endpoints: POST /api/backups/create, GET /api/backups, POST /api/backups/{id}/restore
- Endpoints: POST /api/backups/schedules, GET /api/backups/schedules
- Support full, differential, and transaction log backups

## 3. Platform Tiers
- Basic: Manual database dumps
- Standard: Scheduled backups, retention policies, backup verification
- Enterprise: Continuous backup, point-in-time recovery, geo-redundant storage, backup encryption, compliance reporting

## 4. Database Schema
- BackupJobs: JobId PK, Type (Full/Differential/Log), Status, SizeBytes, FilePath, StartedAt, CompletedAt
- BackupSchedules: ScheduleId PK, DatabaseName, Type, CronExpression, RetentionDays, IsEnabled
- RestorePoints: PointId PK, JobId FK, Timestamp, IsVerified

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
