# Scheduling Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Manages appointments, reminders, recurring events, and calendar synchronization.

## 2. Microservice Structure

### Core Layer (Scheduling.Core)
- Entities: Appointment, Reminder, RecurringEvent, Calendar
- Interfaces: IAppointmentRepository, ISchedulingService
- Events: AppointmentCreatedEvent, AppointmentCancelledEvent, ReminderSentEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=SchedulingDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: SchedulingDbContext
- MessagePack event serialization

### API Layer
- Endpoints: POST /api/appointments, GET /api/appointments/{id}, DELETE /api/appointments/{id}
- Support recurring events (RRULE), timezone handling

## 3. Platform Tiers
- Basic: Simple date/time storage
- Standard: Recurring events, timezone handling, reminders
- Enterprise: Calendar sync (Google, Outlook), conflict detection, resource scheduling

## 4. Database Schema
- Appointments: AppointmentId PK, Title, StartTime, EndTime, RecurrenceRule, CreatedAt
- Reminders: ReminderId PK, AppointmentId FK, ReminderTime, Status, SentAt

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
