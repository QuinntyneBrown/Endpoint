# Sequence PlantUML Input Specification

**Version:** 1.0.0
**Date:** 2026-01-12
**Purpose:** Define the input format and transformation rules for converting simple PlantUML sequence diagrams into complete solution specifications

---

## Table of Contents

1. [Overview](#1-overview)
2. [Simple Sequence Syntax](#2-simple-sequence-syntax)
3. [Participant Type Mappings](#3-participant-type-mappings)
4. [Message Syntax and Interactions](#4-message-syntax-and-interactions)
5. [SignalR WebSocket Support](#5-signalr-websocket-support)
6. [ISequenceToSolutionPlantUmlService](#6-isequencetosolutionplantumlservice)
7. [Generated Output Structure](#7-generated-output-structure)
8. [Parsing Rules](#8-parsing-rules)
9. [Examples](#9-examples)
10. [Validation Rules](#10-validation-rules)

---

## 1. Overview

### 1.1 Purpose

This specification defines:
1. **Simple sequence diagram input format** for rapid solution scaffolding
2. **Participant type mappings** that determine project generation
3. **Message syntax rules** for defining interactions between components
4. **SignalR WebSocket support** for real-time communication patterns
5. **Transformation rules** for the `ISequenceToSolutionPlantUmlService`

### 1.2 Scope

The simple sequence input format enables developers to:
- Define solution architecture using minimal PlantUML syntax
- Automatically generate .NET microservices, Angular frontends, and worker services
- Configure real-time communication via SignalR with a single message declaration
- Generate complete solution PlantUML that conforms to `plantuml-scaffolding.spec`

### 1.3 Definitions

| Term | Definition |
|------|------------|
| Participant | An actor, service, or application in the sequence diagram |
| DotNetType | The .NET project type derived from participant comments |
| Message | An interaction arrow between two participants |
| Simple Sequence | A minimal PlantUML sequence diagram defining system architecture |

---

## 2. Simple Sequence Syntax

### 2.1 Document Structure

A simple sequence diagram MUST follow this structure:

```plantuml
@startuml
actor <actor-name> as <alias>
' <DotNetType>
participant "<Display Name>" as <alias>
' <DotNetType>
participant "<Display Name>" as <alias>

<source> -> <target> : <message-label>

@enduml
```

### 2.2 Header Format

The document MUST begin with `@startuml` and end with `@enduml`:

```plantuml
@startuml
' Content here
@enduml
```

### 2.3 Participant Declaration

Participants are declared using one of the following formats:

**Actor (for users/external systems):**
```plantuml
actor user as u
actor "System Admin" as admin
```

**Participant with DotNetType comment:**
```plantuml
' Microservice
participant "Customer Management" as crm

' Angular
participant "Admin Dashboard" as admin

' Worker
participant "Email Service" as email
```

**Participant without comment (defaults to Unknown):**
```plantuml
participant "Legacy System" as legacy
```

### 2.4 DotNetType Comment Convention

The comment line IMMEDIATELY preceding a participant declaration defines its `DotNetType`:

```plantuml
' <DotNetType>
participant "<Name>" as <alias>
```

Supported DotNetType values:

| Comment | DotNetType | Generated Projects |
|---------|------------|-------------------|
| `' Microservice` | Microservice | {Name}.Api, {Name}.Core, {Name}.Infrastructure |
| `' Angular` | Angular | Angular workspace with components and services |
| `' ts` | Angular | Angular workspace (alias for Angular) |
| `' Worker` | Worker | Background worker service project |

---

## 3. Participant Type Mappings

### 3.1 Microservice Mapping

When a participant has `DotNetType: Microservice`:

**Input:**
```plantuml
' Microservice
participant "Customer Management" as crm
```

**Generated Projects:**
```
{SolutionName}.CustomerManagement.Api/
{SolutionName}.CustomerManagement.Core/
{SolutionName}.CustomerManagement.Infrastructure/
```

**Generated PlantUML Domain Model:**
```plantuml
package "{SolutionName}.CustomerManagement.Aggregates.CustomerManagement" {
    class CustomerManagement <<aggregate>> {
        +CustomerManagementId : string
        +Name : string
        +Description : string
        +Status : string
        +CreatedAt : DateTime
        +ModifiedAt : DateTime
    }
}
```

### 3.2 Angular Mapping

When a participant has `DotNetType: Angular` or `DotNetType: ts`:

**Input:**
```plantuml
' Angular
participant "Admin Dashboard" as admin
```

**Generated Angular Workspace:**
```
{SolutionName}.AdminDashboard.App/
├── src/
│   ├── app/
│   │   ├── @core/
│   │   ├── pages/
│   │   └── models/
│   └── environments/
└── angular.json
```

### 3.3 Worker Mapping

When a participant has `DotNetType: Worker`:

**Input:**
```plantuml
' Worker
participant "Email Service" as email
```

**Generated Worker Project:**
```
{SolutionName}.EmailService.Worker/
├── Program.cs
├── Worker.cs
└── {SolutionName}.EmailService.Worker.csproj
```

---

## 4. Message Syntax and Interactions

### 4.1 Basic Message Format

Messages define interactions between participants:

```plantuml
<source-alias> -> <target-alias> : <message-label>
```

### 4.2 Message Types

| Syntax | Meaning | Generation Effect |
|--------|---------|-------------------|
| `->` | Synchronous call | HTTP/REST endpoint |
| `->>` | Asynchronous call | Async endpoint |
| `-->` | Return | Response type |
| `-->>` | Async return | Async response type |

### 4.3 Special Message Labels

Certain message labels trigger specific code generation:

| Message Label | Effect |
|---------------|--------|
| `HTTP/REST` | Standard REST API communication |
| `SignalR WebSocket` | SignalR Hub and client generation |
| `gRPC` | gRPC service generation (future) |
| `Message Queue` | Message queue integration (future) |

---

## 5. SignalR WebSocket Support

### 5.1 SignalR Declaration Syntax

To establish SignalR WebSocket communication between an Angular application and a Microservice:

```plantuml
<angular-alias> -> <microservice-alias> : SignalR WebSocket
```

**Example:**
```plantuml
@startuml
actor user as u
' Microservice
participant "Customer Management" as crm
' Angular
participant "Admin Dashboard" as admin

admin -> crm : SignalR WebSocket

@enduml
```

### 5.2 SignalR Detection Rules

The parser MUST detect SignalR WebSocket communication when:

1. A message exists between two participants
2. The message label contains `SignalR WebSocket` (case-insensitive)
3. The source participant is of type `Angular` or `ts`
4. The target participant is of type `Microservice`

**Regex Pattern:**
```regex
^(?<source>\w+)\s*->>?\s*(?<target>\w+)\s*:\s*SignalR\s+WebSocket\s*$
```

### 5.3 Backend SignalR Generation (Microservice)

When SignalR WebSocket is detected, the scaffolder MUST generate:

#### 5.3.1 EventHub Class

**Location:** `{SolutionName}.{BoundedContext}.Api/Hubs/EventHub.cs`

```csharp
// Hubs/EventHub.cs
using Microsoft.AspNetCore.SignalR;

namespace {SolutionName}.{BoundedContext}.Api.Hubs;

/// <summary>
/// SignalR hub for real-time event communication.
/// </summary>
public class EventHub : Hub
{
    private readonly ILogger<EventHub> _logger;

    public EventHub(ILogger<EventHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcasts a message to all connected clients.
    /// </summary>
    public async Task BroadcastMessage(string eventType, object payload)
    {
        await Clients.All.SendAsync("ReceiveMessage", eventType, payload);
    }

    /// <summary>
    /// Sends a message to a specific group.
    /// </summary>
    public async Task SendToGroup(string groupName, string eventType, object payload)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", eventType, payload);
    }

    /// <summary>
    /// Joins a client to a group.
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Removes a client from a group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}",
            Context.ConnectionId, groupName);
    }
}
```

#### 5.3.2 Hub Endpoint Mapping

**Location:** `{SolutionName}.{BoundedContext}.Api/Program.cs`

The `Program.cs` MUST include SignalR configuration:

```csharp
using {SolutionName}.{BoundedContext}.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ... existing services ...

// Add SignalR
builder.Services.AddSignalR();

// CORS for Angular with SignalR support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

// ... existing middleware ...

app.UseCors("AllowAngular");

// Map SignalR hub
app.MapHub<EventHub>("/events");

app.Run();
```

#### 5.3.3 Required NuGet Package

The Api project `.csproj` MUST include:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
</ItemGroup>
```

Note: For .NET 8.0+, SignalR is included in the framework, so explicit package reference may not be required.

### 5.4 Frontend SignalR Generation (Angular)

When SignalR WebSocket is detected with an Angular participant as the source:

#### 5.4.1 NPM Package Installation

The Angular workspace `package.json` MUST include:

```json
{
    "dependencies": {
        "@microsoft/signalr": "^8.0.0"
    }
}
```

**Installation Command:**
```bash
npm install @microsoft/signalr
```

#### 5.4.2 SignalR Service

**Location:** `{Project}.App/src/app/@core/signalr.service.ts`

```typescript
// @core/signalr.service.ts
import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SignalRMessage {
    eventType: string;
    payload: unknown;
}

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private hubConnection: signalR.HubConnection | null = null;
    private connectionState$ = new BehaviorSubject<signalR.HubConnectionState>(
        signalR.HubConnectionState.Disconnected
    );
    private messages$ = new Subject<SignalRMessage>();

    get connectionState(): Observable<signalR.HubConnectionState> {
        return this.connectionState$.asObservable();
    }

    get messages(): Observable<SignalRMessage> {
        return this.messages$.asObservable();
    }

    async connect(): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${environment.apiUrl}/events`)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.setupListeners();

        try {
            await this.hubConnection.start();
            this.connectionState$.next(signalR.HubConnectionState.Connected);
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error:', err);
            this.connectionState$.next(signalR.HubConnectionState.Disconnected);
            throw err;
        }
    }

    async disconnect(): Promise<void> {
        if (this.hubConnection) {
            await this.hubConnection.stop();
            this.connectionState$.next(signalR.HubConnectionState.Disconnected);
        }
    }

    private setupListeners(): void {
        if (!this.hubConnection) return;

        this.hubConnection.on('ReceiveMessage', (eventType: string, payload: unknown) => {
            this.messages$.next({ eventType, payload });
        });

        this.hubConnection.onreconnecting(() => {
            this.connectionState$.next(signalR.HubConnectionState.Reconnecting);
        });

        this.hubConnection.onreconnected(() => {
            this.connectionState$.next(signalR.HubConnectionState.Connected);
        });

        this.hubConnection.onclose(() => {
            this.connectionState$.next(signalR.HubConnectionState.Disconnected);
        });
    }

    async broadcastMessage(eventType: string, payload: unknown): Promise<void> {
        if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
            throw new Error('Not connected to SignalR hub');
        }
        await this.hubConnection.invoke('BroadcastMessage', eventType, payload);
    }

    async joinGroup(groupName: string): Promise<void> {
        if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
            throw new Error('Not connected to SignalR hub');
        }
        await this.hubConnection.invoke('JoinGroup', groupName);
    }

    async leaveGroup(groupName: string): Promise<void> {
        if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
            throw new Error('Not connected to SignalR hub');
        }
        await this.hubConnection.invoke('LeaveGroup', groupName);
    }

    async sendToGroup(groupName: string, eventType: string, payload: unknown): Promise<void> {
        if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
            throw new Error('Not connected to SignalR hub');
        }
        await this.hubConnection.invoke('SendToGroup', groupName, eventType, payload);
    }
}
```

#### 5.4.3 Environment Configuration

**Location:** `{Project}.App/src/environments/environment.ts`

```typescript
export const environment = {
    production: false,
    apiUrl: 'http://localhost:5000'  // URL of the microservice API
};
```

### 5.5 SignalR Hub Route

The SignalR hub MUST be mapped to the `/events` endpoint:

| Endpoint | Hub Class | Purpose |
|----------|-----------|---------|
| `/events` | `EventHub` | Real-time event communication |

---

## 6. ISequenceToSolutionPlantUmlService

### 6.1 Interface Definition

```csharp
public interface ISequenceToSolutionPlantUmlService
{
    /// <summary>
    /// Converts a PlantUML sequence diagram into a complete solution PlantUML specification
    /// that conforms to the plantuml-scaffolding.spec.
    /// </summary>
    /// <param name="sequenceDiagramContent">The PlantUML sequence diagram content</param>
    /// <param name="solutionName">The name of the solution to generate</param>
    /// <returns>A complete PlantUML specification for generating a solution</returns>
    string GenerateSolutionPlantUml(string sequenceDiagramContent, string solutionName);
}
```

### 6.2 Generation Algorithm

```
FUNCTION GenerateSolutionPlantUml(sequenceDiagram, solutionName) -> string:

    1. PARSE sequence diagram content
       - Extract all participant declarations
       - Extract DotNetType from comments
       - Extract all messages/interactions

    2. ANALYZE interactions
       - Identify SignalR WebSocket messages
       - Map source and target participants
       - Determine communication patterns

    3. GROUP participants by DotNetType
       - Microservice participants
       - Angular participants
       - Worker participants
       - Unknown participants

    4. GENERATE Domain Models PlantUML
       FOR EACH Microservice participant:
           - Create aggregate class with standard properties
           - Include in bounded context package

    5. GENERATE Solution Architecture PlantUML
       FOR EACH participant group:
           - Generate appropriate project components
           - Add relationships based on messages

    6. IF SignalR WebSocket detected:
       - Mark target Microservice for SignalR Hub generation
       - Mark source Angular for @microsoft/signalr package
       - Add SignalR relationship to architecture

    7. RETURN combined PlantUML documents
```

### 6.3 SignalR Detection in Generation

When generating solution PlantUML, the service MUST:

1. Parse all messages for `SignalR WebSocket` pattern
2. For each SignalR message:
   - Identify the Angular source participant
   - Identify the Microservice target participant
3. Add metadata to generated PlantUML indicating SignalR requirements:

```plantuml
note right of {microservice_alias}_api
    **SignalR Hub**
    Endpoint: /events
    Hub Class: EventHub
end note
```

---

## 7. Generated Output Structure

### 7.1 Complete Solution Structure with SignalR

When a simple sequence diagram includes SignalR WebSocket:

```
{Solution}/
├── src/
│   ├── {Solution}.{Microservice}.Api/
│   │   ├── Controllers/
│   │   ├── Hubs/
│   │   │   └── EventHub.cs          # SignalR Hub
│   │   ├── Program.cs               # With SignalR configuration
│   │   └── {Solution}.{Microservice}.Api.csproj
│   ├── {Solution}.{Microservice}.Core/
│   │   ├── Aggregates/
│   │   └── {Solution}.{Microservice}.Core.csproj
│   └── {Solution}.{Microservice}.Infrastructure/
│       └── {Solution}.{Microservice}.Infrastructure.csproj
├── {Solution}.{Angular}.App/
│   ├── src/
│   │   ├── app/
│   │   │   ├── @core/
│   │   │   │   └── signalr.service.ts  # SignalR client service
│   │   │   └── ...
│   │   └── environments/
│   ├── package.json                    # With @microsoft/signalr
│   └── angular.json
└── {Solution}.sln
```

### 7.2 Generated PlantUML Documents

The service generates two PlantUML documents:

**Domain Models Document:**
```plantuml
@startuml Domain Models
!pragma layout smetana
skinparam backgroundColor #FEFEFE
skinparam defaultFontSize 11

title {SolutionName} - Domain Models

package "{SolutionName}.{BoundedContext}.Aggregates.{Entity}" {
    class {Entity} <<aggregate>> {
        +{Entity}Id : string
        +Name : string
        +Description : string
        +Status : string
        +CreatedAt : DateTime
        +ModifiedAt : DateTime
    }
}

@enduml
```

**Solution Architecture Document:**
```plantuml
@startuml Solution Architecture
!pragma layout smetana
skinparam backgroundColor #FEFEFE
skinparam defaultFontSize 11

title {SolutionName} - Solution Architecture

package "Frontend" {
    component [{AngularName}] as angular <<Angular>>
}

package "Microservices" {
    package "{MicroserviceName}" {
        component [{MicroserviceName}.Api] as ms_api <<WebApi>>
        component [{MicroserviceName}.Core] as ms_core <<ClassLib>>
        component [{MicroserviceName}.Infrastructure] as ms_infra <<ClassLib>>
        ms_api --> ms_infra
        ms_infra --> ms_core
    }
}

angular --> ms_api : SignalR WebSocket

note right of ms_api
    **SignalR Hub**
    Endpoint: /events
    Hub Class: EventHub
end note

@enduml
```

---

## 8. Parsing Rules

### 8.1 Participant Extraction

**Regex for participant with alias:**
```regex
participant\s+"([^"]+)"\s+as\s+(\w+)
```

**Regex for actor:**
```regex
actor\s+(?:"([^"]+)"|(\w+))\s+as\s+(\w+)
```

### 8.2 DotNetType Comment Extraction

**Regex for DotNetType comment:**
```regex
'\s*(Microservice|Angular|ts|Worker)\s*$
```

**Parsing Rule:**
- Read the line immediately preceding each participant declaration
- If the line matches the DotNetType pattern, assign the type
- Otherwise, assign `Unknown`

### 8.3 Message Extraction

**Regex for message:**
```regex
(\w+)\s*(->>?|-->>?)\s*(\w+)\s*:\s*(.+)
```

**Capture Groups:**
1. Source alias
2. Arrow type
3. Target alias
4. Message label

### 8.4 SignalR Detection

**Regex for SignalR message:**
```regex
^(\w+)\s*->>?\s*(\w+)\s*:\s*SignalR\s+WebSocket\s*$
```

**Case Sensitivity:** Case-insensitive matching for `SignalR WebSocket`

---

## 9. Examples

### 9.1 Complete Example with SignalR

**Input Simple Sequence:**
```plantuml
@startuml
actor user as u
' Microservice
participant "Customer Management" as crm
' Angular
participant "Admin Dashboard" as admin

admin -> crm : SignalR WebSocket

@enduml
```

**Generated Domain Models:**
```plantuml
@startuml Domain Models
!pragma layout smetana
skinparam backgroundColor #FEFEFE
skinparam defaultFontSize 11

title Solution - Domain Models

package "Solution.CustomerManagement.Aggregates.CustomerManagement" {
    class CustomerManagement <<aggregate>> {
        +CustomerManagementId : string
        +Name : string
        +Description : string
        +Status : string
        +CreatedAt : DateTime
        +ModifiedAt : DateTime
    }
}

@enduml
```

**Generated Solution Architecture:**
```plantuml
@startuml Solution Architecture
!pragma layout smetana
skinparam backgroundColor #FEFEFE
skinparam defaultFontSize 11

title Solution - Solution Architecture

package "Frontend" {
    component [AdminDashboard] as admin <<Angular>>
}

package "Microservices" {
    package "CustomerManagement" {
        component [CustomerManagement.Api] as crm_api <<WebApi>>
        component [CustomerManagement.Core] as crm_core <<ClassLib>>
        component [CustomerManagement.Infrastructure] as crm_infra <<ClassLib>>
        crm_api --> crm_infra
        crm_infra --> crm_core
    }
}

admin --> crm_api : SignalR WebSocket

note right of crm_api
    **SignalR Hub**
    Endpoint: /events
    Hub Class: EventHub
end note

@enduml
```

### 9.2 Multiple Microservices Example

**Input:**
```plantuml
@startuml
actor user as u
' Microservice
participant "Order Service" as orders
' Microservice
participant "Inventory Service" as inventory
' Angular
participant "Store Frontend" as store
' Worker
participant "Notification Worker" as notify

store -> orders : HTTP/REST
store -> inventory : HTTP/REST
store -> orders : SignalR WebSocket
orders -> notify : Message Queue

@enduml
```

This generates:
- Two microservice project sets (Orders, Inventory)
- One Angular workspace with SignalR service
- One Worker project
- SignalR Hub in Orders Api project
- @microsoft/signalr package in Angular workspace

---

## 10. Validation Rules

### 10.1 Input Validation

Before processing, validate:

- [ ] `@startuml` and `@enduml` boundaries present
- [ ] At least one participant declared
- [ ] All message source/target aliases exist as participants
- [ ] SignalR messages have valid Angular source and Microservice target

### 10.2 SignalR Validation

When SignalR WebSocket is detected:

- [ ] Source participant MUST be of type `Angular` or `ts`
- [ ] Target participant MUST be of type `Microservice`
- [ ] Only one SignalR Hub per Microservice (EventHub)

### 10.3 Generation Validation

After generation, verify:

- [ ] All Microservice participants have Api, Core, Infrastructure projects
- [ ] All Angular participants have workspace structure
- [ ] SignalR Hub generated for target Microservices
- [ ] SignalR service generated for source Angular applications
- [ ] @microsoft/signalr package added to Angular package.json
- [ ] Hub endpoint mapped to /events in Program.cs

---

## Appendix A: Quick Reference

### A.1 Supported DotNetTypes

| Comment | Type | Generated |
|---------|------|-----------|
| `' Microservice` | Microservice | .Api, .Core, .Infrastructure |
| `' Angular` | Angular | Angular workspace |
| `' ts` | Angular | Angular workspace |
| `' Worker` | Worker | Worker service |

### A.2 Special Message Labels

| Label | Effect |
|-------|--------|
| `SignalR WebSocket` | EventHub + @microsoft/signalr |
| `HTTP/REST` | Standard REST communication |

### A.3 SignalR Artifacts

| Location | Artifact | Purpose |
|----------|----------|---------|
| Api/Hubs/EventHub.cs | SignalR Hub | Server-side hub |
| Api/Program.cs | MapHub | Route registration |
| Angular/@core/signalr.service.ts | Client service | Connection management |
| Angular/package.json | @microsoft/signalr | SignalR client library |

---

**End of Specification**
