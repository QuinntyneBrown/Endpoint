# Collaboration Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Enables sharing, commenting, and real-time collaboration between users.

## 2. Microservice Structure

### Core Layer (Collaboration.Core)
- Entities: Share, Comment, Mention, Activity, Presence
- Interfaces: IShareRepository, ICommentRepository, ICollaborationService
- Events: EntitySharedEvent, CommentAddedEvent, MentionCreatedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=CollaborationDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: CollaborationDbContext
- SignalR for real-time updates

### API Layer
- Endpoints: POST /api/shares, GET /api/shares/{entityId}, POST /api/comments, GET /api/comments/{entityId}
- Support real-time notifications via SignalR

## 3. Platform Tiers
- Basic: Simple sharing via links
- Standard: User/group sharing, permission levels (view/edit), comments and mentions
- Enterprise: Real-time collaborative editing, activity feeds, @mentions, presence indicators, version history

## 4. Database Schema
- Shares: ShareId PK, EntityType, EntityId, SharedWith (UserId/GroupId), Permission, CreatedAt
- Comments: CommentId PK, EntityType, EntityId, AuthorId, Content, CreatedAt
- Mentions: MentionId PK, CommentId FK, MentionedUserId, NotifiedAt

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
