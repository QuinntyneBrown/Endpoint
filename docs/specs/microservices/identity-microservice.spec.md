# Identity Microservice Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active
- **Related Documents**: 
  - [Implementation Specification](../implementation.spec.md)
  - [Message Design Specification](../messaging/message-design.spec.md)
  - [Subscription Design Specification](../messaging/subscription-design.spec.md)

## 1. Overview

This specification defines the requirements for the Identity Microservice, which handles user authentication, authorization, and identity management across all applications in the platform. The microservice provides three platform tiers: Basic, Standard, and Enterprise.

## 2. Microservice Structure

The Identity Microservice MUST follow a three-layer architecture:

### 2.1 Core Layer (Identity.Core)

**Purpose**: Contains domain models, business logic, and abstractions.

**Requirements**:
- REQ-IDENT-CORE-001: MUST contain domain entities (User, Role, Permission, Claim)
- REQ-IDENT-CORE-002: MUST contain interfaces for repositories and services
- REQ-IDENT-CORE-003: MUST contain domain events (UserCreated, UserAuthenticated, etc.)
- REQ-IDENT-CORE-004: MUST NOT reference Infrastructure or Api layers
- REQ-IDENT-CORE-005: MUST use C# 11+ features (required keyword, file-scoped namespaces)

**Acceptance Criteria**:
- AC-IDENT-CORE-001.1: User entity MUST include: UserId (GUID), Username, Email, PasswordHash, IsActive, CreatedAt, LastLoginAt
- AC-IDENT-CORE-001.2: Role entity MUST include: RoleId (GUID), Name, Description, Permissions (collection)
- AC-IDENT-CORE-001.3: All entities MUST implement IAggregateRoot marker interface
- AC-IDENT-CORE-001.4: All domain events MUST implement IDomainEvent
- AC-IDENT-CORE-001.5: Repository interfaces MUST follow pattern: IUserRepository, IRoleRepository

### 2.2 Infrastructure Layer (Identity.Infrastructure)

**Purpose**: Contains data access, external service integrations, and infrastructure concerns.

**Requirements**:
- REQ-IDENT-INFRA-001: MUST use SQL Server Express for data persistence
- REQ-IDENT-INFRA-002: MUST use Entity Framework Core for data access
- REQ-IDENT-INFRA-003: MUST implement repository interfaces from Core layer
- REQ-IDENT-INFRA-004: MUST contain DbContext with entity configurations
- REQ-IDENT-INFRA-005: MUST implement messaging (publish domain events)
- REQ-IDENT-INFRA-006: MUST use MessagePack serialization for events

**Acceptance Criteria**:
- AC-IDENT-INFRA-001.1: Connection string MUST target SQL Server Express: `Server=.\\SQLEXPRESS;Database=IdentityDb;Trusted_Connection=True;TrustServerCertificate=True`
- AC-IDENT-INFRA-001.2: DbContext MUST be named `IdentityDbContext`
- AC-IDENT-INFRA-001.3: MUST include EF Core migrations in Migrations folder
- AC-IDENT-INFRA-001.4: Entity configurations MUST be in separate files (EntityTypeConfiguration pattern)
- AC-IDENT-INFRA-001.5: Repositories MUST use async methods (SaveChangesAsync, FindAsync)
- AC-IDENT-INFRA-002.1: MUST implement `IMessagePublisher` for publishing domain events
- AC-IDENT-INFRA-002.2: Domain events MUST be published after successful SaveChanges

### 2.3 API Layer (Identity.Api)

**Purpose**: Contains HTTP API endpoints, request/response models, and API infrastructure.

**Requirements**:
- REQ-IDENT-API-001: MUST use ASP.NET Core Minimal APIs or Controllers
- REQ-IDENT-API-002: MUST follow REST conventions for endpoints
- REQ-IDENT-API-003: MUST implement request validation using FluentValidation
- REQ-IDENT-API-004: MUST implement Swagger/OpenAPI documentation
- REQ-IDENT-API-005: MUST implement health checks
- REQ-IDENT-API-006: MUST implement proper error handling and status codes
- REQ-IDENT-API-007: MUST implement authentication middleware
- REQ-IDENT-API-008: MUST implement CORS configuration

**Acceptance Criteria**:
- AC-IDENT-API-001.1: API MUST expose endpoints: POST /api/users/register, POST /api/users/login, GET /api/users/{id}, PUT /api/users/{id}, DELETE /api/users/{id}
- AC-IDENT-API-001.2: API MUST expose endpoints: GET /api/roles, POST /api/roles, PUT /api/roles/{id}, DELETE /api/roles/{id}
- AC-IDENT-API-001.3: Request DTOs MUST be validated before processing
- AC-IDENT-API-001.4: Validation errors MUST return 400 Bad Request with error details
- AC-IDENT-API-001.5: Unauthorized access MUST return 401 Unauthorized
- AC-IDENT-API-001.6: Forbidden access MUST return 403 Forbidden
- AC-IDENT-API-001.7: Not found resources MUST return 404 Not Found
- AC-IDENT-API-001.8: Successful creation MUST return 201 Created with Location header

## 3. Platform Tiers

### 3.1 Basic Tier

**Requirements**:
- REQ-IDENT-BASIC-001: MUST support username/password authentication
- REQ-IDENT-BASIC-002: MUST generate JWT tokens for authentication
- REQ-IDENT-BASIC-003: MUST hash passwords using BCrypt or PBKDF2
- REQ-IDENT-BASIC-004: MUST support user registration and login
- REQ-IDENT-BASIC-005: MUST support basic user profile management

**Acceptance Criteria**:
- AC-IDENT-BASIC-001.1: Registration MUST validate email format
- AC-IDENT-BASIC-001.2: Registration MUST validate password strength (min 8 characters, uppercase, lowercase, digit)
- AC-IDENT-BASIC-001.3: Login MUST verify username and password
- AC-IDENT-BASIC-001.4: Login MUST return JWT token with expiration (default: 24 hours)
- AC-IDENT-BASIC-001.5: JWT MUST include claims: UserId, Username, Email

### 3.2 Standard Tier

**Requirements**:
- REQ-IDENT-STD-001: MUST support OAuth 2.0/OpenID Connect
- REQ-IDENT-STD-002: MUST support social login (Google, Microsoft, Apple)
- REQ-IDENT-STD-003: MUST support refresh tokens
- REQ-IDENT-STD-004: MUST support email verification
- REQ-IDENT-STD-005: MUST support password reset flow

**Acceptance Criteria**:
- AC-IDENT-STD-001.1: OAuth flows MUST support authorization code with PKCE
- AC-IDENT-STD-001.2: Social login MUST link external identity to local user
- AC-IDENT-STD-001.3: Refresh tokens MUST expire after 30 days of inactivity
- AC-IDENT-STD-001.4: Email verification MUST send confirmation link valid for 24 hours
- AC-IDENT-STD-001.5: Password reset MUST send secure token valid for 1 hour

### 3.3 Enterprise Tier

**Requirements**:
- REQ-IDENT-ENT-001: MUST support role-based access control (RBAC)
- REQ-IDENT-ENT-002: MUST support attribute-based access control (ABAC)
- REQ-IDENT-ENT-003: MUST support multi-factor authentication (MFA)
- REQ-IDENT-ENT-004: MUST support single sign-on (SSO)
- REQ-IDENT-ENT-005: MUST support LDAP/Active Directory integration
- REQ-IDENT-ENT-006: MUST implement comprehensive audit logging

**Acceptance Criteria**:
- AC-IDENT-ENT-001.1: RBAC MUST support role hierarchies
- AC-IDENT-ENT-001.2: RBAC MUST support permission assignment at role level
- AC-IDENT-ENT-002.1: ABAC MUST support policy-based authorization
- AC-IDENT-ENT-003.1: MFA MUST support TOTP (Time-based One-Time Password)
- AC-IDENT-ENT-003.2: MFA MUST support SMS verification as fallback
- AC-IDENT-ENT-004.1: SSO MUST support SAML 2.0
- AC-IDENT-ENT-005.1: LDAP integration MUST sync users on schedule
- AC-IDENT-ENT-006.1: Audit log MUST record: login attempts, role changes, permission changes, profile updates

## 4. Domain Events

**Requirements**:
- REQ-IDENT-EVENTS-001: MUST publish UserCreatedEvent when user registers
- REQ-IDENT-EVENTS-002: MUST publish UserAuthenticatedEvent on successful login
- REQ-IDENT-EVENTS-003: MUST publish UserAuthenticationFailedEvent on failed login
- REQ-IDENT-EVENTS-004: MUST publish UserUpdatedEvent when profile changes
- REQ-IDENT-EVENTS-005: MUST publish UserDeactivatedEvent when user deactivated
- REQ-IDENT-EVENTS-006: MUST publish RoleAssignedEvent when role assigned to user

**Acceptance Criteria**:
- AC-IDENT-EVENTS-001.1: Events MUST follow channel naming: `identity.user.{action}.v1`
- AC-IDENT-EVENTS-001.2: Events MUST include AggregateId (UserId)
- AC-IDENT-EVENTS-001.3: Events MUST be serialized using MessagePack
- AC-IDENT-EVENTS-001.4: Events MUST include correlation tracking

## 5. Database Schema

**Requirements**:
- REQ-IDENT-DB-001: MUST use SQL Server Express as database engine
- REQ-IDENT-DB-002: Database MUST be named `IdentityDb`
- REQ-IDENT-DB-003: MUST include table: Users (UserId PK, Username, Email, PasswordHash, IsActive, CreatedAt, UpdatedAt)
- REQ-IDENT-DB-004: MUST include table: Roles (RoleId PK, Name, Description, CreatedAt)
- REQ-IDENT-DB-005: MUST include table: UserRoles (UserId FK, RoleId FK, AssignedAt)
- REQ-IDENT-DB-006: MUST include table: Permissions (PermissionId PK, Name, Resource, Action)
- REQ-IDENT-DB-007: MUST include table: RolePermissions (RoleId FK, PermissionId FK)
- REQ-IDENT-DB-008: MUST include indexes on frequently queried columns (Username, Email)

**Acceptance Criteria**:
- AC-IDENT-DB-001.1: UserId MUST be UNIQUEIDENTIFIER (GUID)
- AC-IDENT-DB-001.2: Username MUST be NVARCHAR(100), unique, not null
- AC-IDENT-DB-001.3: Email MUST be NVARCHAR(255), unique, not null
- AC-IDENT-DB-001.4: PasswordHash MUST be NVARCHAR(255), not null
- AC-IDENT-DB-002.1: Unique index MUST exist on Users.Username
- AC-IDENT-DB-002.2: Unique index MUST exist on Users.Email
- AC-IDENT-DB-003.1: Foreign keys MUST have ON DELETE CASCADE for UserRoles
- AC-IDENT-DB-003.2: CreatedAt/UpdatedAt MUST be DATETIME2, not null

## 6. API Endpoints

### 6.1 Authentication Endpoints

**POST /api/auth/register**
- Request: `{ username, email, password }`
- Response: `201 Created { userId, username, email }`
- Validation: Email format, password strength

**POST /api/auth/login**
- Request: `{ username, password }`
- Response: `200 OK { token, refreshToken, expiresIn }`
- Response (Failure): `401 Unauthorized { error }`

**POST /api/auth/refresh**
- Request: `{ refreshToken }`
- Response: `200 OK { token, expiresIn }`

**POST /api/auth/logout**
- Request: `{ refreshToken }`
- Response: `204 No Content`

### 6.2 User Management Endpoints

**GET /api/users/{id}**
- Response: `200 OK { userId, username, email, roles, createdAt }`
- Response (Not Found): `404 Not Found`

**PUT /api/users/{id}**
- Request: `{ email, ... }`
- Response: `200 OK { userId, ... }`
- Authorization: User must be owner or admin

**DELETE /api/users/{id}**
- Response: `204 No Content`
- Authorization: Admin only

### 6.3 Role Management Endpoints

**GET /api/roles**
- Response: `200 OK [ { roleId, name, description } ]`

**POST /api/roles**
- Request: `{ name, description, permissions }`
- Response: `201 Created { roleId, name }`
- Authorization: Admin only

**PUT /api/users/{userId}/roles**
- Request: `{ roleIds: [...] }`
- Response: `200 OK`
- Authorization: Admin only

## 7. Configuration

**Requirements**:
- REQ-IDENT-CONFIG-001: JWT signing key MUST be configurable via environment variable
- REQ-IDENT-CONFIG-002: Token expiration MUST be configurable (default: 24h)
- REQ-IDENT-CONFIG-003: Password policy MUST be configurable
- REQ-IDENT-CONFIG-004: Database connection string MUST be configurable
- REQ-IDENT-CONFIG-005: Redis connection string MUST be configurable for event publishing

**Acceptance Criteria**:
- AC-IDENT-CONFIG-001.1: Configuration MUST use appsettings.json and environment variables
- AC-IDENT-CONFIG-001.2: Sensitive values MUST use user secrets in development
- AC-IDENT-CONFIG-001.3: Production MUST use Azure Key Vault or environment variables

## 8. Non-Functional Requirements

### 8.1 Performance
- Login endpoint MUST respond within 200ms at 95th percentile
- Registration MUST complete within 500ms at 95th percentile
- Database queries MUST use indexes for O(log n) lookups

### 8.2 Security
- Passwords MUST be hashed using BCrypt with work factor ≥ 12
- JWT tokens MUST be signed using RS256 or HS256
- HTTPS MUST be enforced in production
- SQL injection MUST be prevented via parameterized queries

### 8.3 Scalability
- MUST support horizontal scaling (stateless API)
- MUST support connection pooling for database
- MUST support Redis caching for frequently accessed data

## 9. Testing Requirements

**Requirements**:
- REQ-IDENT-TEST-001: Unit tests MUST cover business logic in Core layer
- REQ-IDENT-TEST-002: Integration tests MUST cover API endpoints
- REQ-IDENT-TEST-003: Integration tests MUST use SQL Server Express test database
- REQ-IDENT-TEST-004: Test coverage MUST be ≥ 80% for Core layer

**Acceptance Criteria**:
- AC-IDENT-TEST-001.1: MUST test password hashing and verification
- AC-IDENT-TEST-001.2: MUST test JWT token generation and validation
- AC-IDENT-TEST-001.3: MUST test role and permission checking
- AC-IDENT-TEST-002.1: MUST test registration with valid and invalid input
- AC-IDENT-TEST-002.2: MUST test login with correct and incorrect credentials
- AC-IDENT-TEST-002.3: MUST test authorization rules

## 10. Deployment

**Requirements**:
- REQ-IDENT-DEPLOY-001: MUST support Docker containerization
- REQ-IDENT-DEPLOY-002: MUST include health check endpoint at /health
- REQ-IDENT-DEPLOY-003: MUST include readiness check for database connectivity
- REQ-IDENT-DEPLOY-004: MUST support graceful shutdown

**Acceptance Criteria**:
- AC-IDENT-DEPLOY-001.1: Dockerfile MUST use multi-stage build
- AC-IDENT-DEPLOY-001.2: Dockerfile MUST use ASP.NET Core runtime image
- AC-IDENT-DEPLOY-002.1: Health check MUST return 200 OK when healthy
- AC-IDENT-DEPLOY-002.2: Health check MUST return 503 Service Unavailable when unhealthy

## 11. Alignment with Implementation Spec

All code MUST follow the conventions in [implementation.spec.md](../implementation.spec.md):

- Use C# 11+ features (file-scoped namespaces, required keyword)
- Use nullable reference types
- Include copyright headers in all source files
- Follow naming conventions (PascalCase for public members)
- Use dependency injection for all dependencies
- Separate concerns: Core (domain) → Infrastructure (data access) → API (HTTP)

## 12. Revision History

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification for Identity Microservice |
