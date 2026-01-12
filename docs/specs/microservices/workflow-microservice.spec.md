# Workflow Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Orchestrates multi-step business processes, approvals, and state machines.

## 2. Microservice Structure

### Core Layer (Workflow.Core)
- Entities: Workflow, WorkflowInstance, WorkflowStep, WorkflowTransition, ApprovalRequest
- Interfaces: IWorkflowRepository, IWorkflowEngine, IStateManager
- Events: WorkflowStartedEvent, WorkflowCompletedEvent, ApprovalRequestedEvent, StateChangedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=WorkflowDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: WorkflowDbContext
- State machine implementation

### API Layer
- Endpoints: POST /api/workflows, GET /api/workflows/{id}, POST /api/workflows/{id}/transition
- Endpoints: POST /api/approvals/{id}/approve, POST /api/approvals/{id}/reject
- Support workflow definition in JSON/YAML

## 3. Platform Tiers
- Basic: Simple status fields on entities
- Standard: Configurable state machines, approval workflows, email notifications on transitions
- Enterprise: Visual workflow designer, parallel execution, escalation rules, SLA tracking, custom actions, audit trail

## 4. Database Schema
- Workflows: WorkflowId PK, Name, Definition (JSON), CreatedAt
- WorkflowInstances: InstanceId PK, WorkflowId FK, EntityType, EntityId, CurrentState, StartedAt, CompletedAt
- WorkflowHistory: HistoryId PK, InstanceId FK, FromState, ToState, TransitionedBy, TransitionedAt
- ApprovalRequests: RequestId PK, InstanceId FK, ApproverId, Status, RequestedAt, ResponseAt

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
