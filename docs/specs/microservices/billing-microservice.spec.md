# Billing Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: [Implementation Specification](../../implementation.spec.md), [Message Design Specification](../messaging/message-design.spec.md)

## 1. Overview

The Billing Microservice handles subscription management, payment processing, invoicing, and revenue tracking.

## 2. Microservice Structure

### 2.1 Core Layer (Billing.Core)

**Requirements**:
- REQ-BILLING-CORE-001: MUST contain entities: Subscription, Invoice, Payment, Plan, Usage
- REQ-BILLING-CORE-002: MUST contain interfaces: ISubscriptionRepository, IPaymentGateway, IInvoiceGenerator
- REQ-BILLING-CORE-003: MUST contain domain events: SubscriptionCreatedEvent, PaymentProcessedEvent, InvoiceGeneratedEvent
- REQ-BILLING-CORE-004: MUST support subscription models: Flat-rate, Usage-based, Tiered

**Acceptance Criteria**:
- AC-BILLING-CORE-001: Subscription entity MUST include: SubscriptionId, CustomerId, PlanId, Status, BillingCycle, NextBillingDate, Amount

### 2.2 Infrastructure Layer (Billing.Infrastructure)

**Requirements**:
- REQ-BILLING-INFRA-001: MUST use SQL Server Express: `Server=.\\SQLEXPRESS;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True`
- REQ-BILLING-INFRA-002: MUST implement payment gateway integration (Stripe/PayPal)
- REQ-BILLING-INFRA-003: MUST implement invoice generation
- REQ-BILLING-INFRA-004: MUST implement subscription lifecycle management
- REQ-BILLING-INFRA-005: MUST publish domain events using MessagePack

**Acceptance Criteria**:
- AC-BILLING-INFRA-001: DbContext MUST be named BillingDbContext
- AC-BILLING-INFRA-002: Payment transactions MUST be idempotent
- AC-BILLING-INFRA-003: Failed payments MUST trigger retry logic

### 2.3 API Layer (Billing.Api)

**Requirements**:
- REQ-BILLING-API-001: MUST expose: POST /api/subscriptions, GET /api/subscriptions/{id}, PUT /api/subscriptions/{id}/cancel
- REQ-BILLING-API-002: MUST expose: POST /api/payments, GET /api/invoices/{id}, GET /api/invoices/customer/{customerId}
- REQ-BILLING-API-003: MUST support webhook endpoints for payment provider notifications
- REQ-BILLING-API-004: MUST implement payment security (PCI compliance considerations)

## 3. Platform Tiers

### Basic Tier
- Manual billing
- Simple invoice generation

### Standard Tier
- Stripe/PayPal integration
- Subscription plans
- Recurring payments
- Usage-based billing

### Enterprise Tier
- Multi-currency support
- Tax compliance
- Dunning management
- Revenue recognition
- Enterprise contracts
- Payment retry logic

## 4. Domain Events

- SubscriptionCreatedEvent: `billing.subscription.created.v1`
- PaymentProcessedEvent: `billing.payment.processed.v1`
- PaymentFailedEvent: `billing.payment.failed.v1`
- InvoiceGeneratedEvent: `billing.invoice.generated.v1`
- SubscriptionCancelledEvent: `billing.subscription.cancelled.v1`

## 5. Database Schema

**Database Name**: BillingDb

**Tables**:
- Subscriptions: SubscriptionId (PK), CustomerId, PlanId, Status, BillingCycle, Amount, NextBillingDate, CreatedAt
- Invoices: InvoiceId (PK), SubscriptionId (FK), Amount, Status, IssuedDate, DueDate, PaidDate
- Payments: PaymentId (PK), InvoiceId (FK), Amount, Status, PaymentMethod, TransactionId, ProcessedAt
- Plans: PlanId (PK), Name, Description, Price, BillingInterval, Features (JSON)

## 6. Alignment with Implementation Spec

- Use C# 11+, three-layer architecture
- SQL Server Express for all data
- MessagePack for event serialization
- Include copyright headers

## Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
