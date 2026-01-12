# Email Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Sends transactional and marketing emails with template management and delivery tracking.

## 2. Microservice Structure

### Core Layer (Email.Core)
- Entities: Email, EmailTemplate, EmailAttachment
- Interfaces: IEmailRepository, IEmailSender, ITemplateEngine
- Events: EmailSentEvent, EmailDeliveredEvent, EmailBouncedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=EmailDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: EmailDbContext
- SMTP/SendGrid/Mailgun integration

### API Layer
- Endpoints: POST /api/email/send, GET /api/email/{id}, GET /api/email/templates
- Support HTML templates with variable substitution

## 3. Platform Tiers
- Basic: SMTP sending with text templates
- Standard: HTML templates, SendGrid/Mailgun, delivery tracking, bounce handling
- Enterprise: A/B testing, personalization, analytics, dedicated IP, DKIM/SPF/DMARC

## 4. Database Schema
- Emails: EmailId PK, ToAddress, Subject, Body, Status, SentAt, DeliveredAt
- EmailTemplates: TemplateId PK, Name, Subject, BodyTemplate, CreatedAt

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
