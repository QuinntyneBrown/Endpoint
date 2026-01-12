# Notification Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: [Implementation Specification](../implementation.spec.md), [Message Design Specification](../messaging/message-design.spec.md)

## 1. Overview

The Notification Microservice delivers notifications across multiple channels including email, SMS, push notifications, and in-app messaging.

## 2. Microservice Structure

### 2.1 Core Layer (Notification.Core)

**Requirements**:
- REQ-NOTIF-CORE-001: MUST contain entities: Notification, NotificationTemplate, NotificationPreference, NotificationHistory
- REQ-NOTIF-CORE-002: MUST contain interfaces: INotificationRepository, INotificationService
- REQ-NOTIF-CORE-003: MUST contain domain events: NotificationSentEvent, NotificationDeliveredEvent, NotificationFailedEvent
- REQ-NOTIF-CORE-004: MUST support notification channels: Email, SMS, Push, InApp

**Acceptance Criteria**:
- AC-NOTIF-CORE-001: Notification entity MUST include: NotificationId, UserId, ChannelType, Subject, Body, Status, CreatedAt, SentAt

### 2.2 Infrastructure Layer (Notification.Infrastructure)

**Requirements**:
- REQ-NOTIF-INFRA-001: MUST use SQL Server Express: `Server=.\\SQLEXPRESS;Database=NotificationDb;Trusted_Connection=True;TrustServerCertificate=True`
- REQ-NOTIF-INFRA-002: MUST implement SMTP integration for email (Basic tier)
- REQ-NOTIF-INFRA-003: MUST implement SendGrid/Mailgun integration (Standard tier)
- REQ-NOTIF-INFRA-004: MUST implement SMS provider integration (Enterprise tier)
- REQ-NOTIF-INFRA-005: MUST implement push notification services (Firebase, APNS)
- REQ-NOTIF-INFRA-006: MUST publish domain events using MessagePack

**Acceptance Criteria**:
- AC-NOTIF-INFRA-001: DbContext MUST be named NotificationDbContext
- AC-NOTIF-INFRA-002: Email templates MUST support variable substitution
- AC-NOTIF-INFRA-003: Delivery tracking MUST record success/failure status

### 2.3 API Layer (Notification.Api)

**Requirements**:
- REQ-NOTIF-API-001: MUST expose: POST /api/notifications/send, GET /api/notifications/{id}, GET /api/notifications/user/{userId}
- REQ-NOTIF-API-002: MUST expose: POST /api/notifications/templates, GET /api/templates/{id}
- REQ-NOTIF-API-003: MUST expose: PUT /api/notifications/preferences (user preferences)
- REQ-NOTIF-API-004: MUST support batch sending

## 3. Platform Tiers

### Basic Tier
- Email notifications via SMTP only
- Simple text templates

### Standard Tier
- Email + push notifications
- HTML templates with variables
- Delivery tracking
- User notification preferences

### Enterprise Tier
- Multi-channel (Email, SMS, Push, Webhook)
- Template engine with conditional logic
- Scheduled delivery
- Retry logic with exponential backoff
- Notification analytics
- Priority queuing

## 4. Domain Events

- NotificationSentEvent: `notification.notification.sent.v1`
- NotificationDeliveredEvent: `notification.notification.delivered.v1`
- NotificationFailedEvent: `notification.notification.failed.v1`
- NotificationScheduledEvent: `notification.notification.scheduled.v1`

## 5. Database Schema

**Database Name**: NotificationDb

**Tables**:
- Notifications: NotificationId (PK), UserId, ChannelType, Subject, Body, Status, Priority, ScheduledFor, SentAt, CreatedAt
- NotificationTemplates: TemplateId (PK), Name, ChannelType, Subject, BodyTemplate, CreatedAt
- NotificationPreferences: UserId (PK), EnableEmail (BIT), EnableSMS (BIT), EnablePush (BIT), QuietHoursStart, QuietHoursEnd
- NotificationHistory: HistoryId (PK), NotificationId (FK), Status, ErrorMessage, AttemptedAt

## 6. Alignment with Implementation Spec

- Use C# 11+, three-layer architecture
- SQL Server Express for all storage
- MessagePack for event serialization
- Include copyright headers

## Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
