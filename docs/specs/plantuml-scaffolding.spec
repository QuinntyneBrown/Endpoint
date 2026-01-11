# PlantUML Scaffolding Specification

**Version:** 1.0.0
**Date:** 2026-01-10
**Purpose:** Enable bidirectional code generation between PlantUML architecture documents and .NET/Angular codebases

---

## Table of Contents

1. [Overview](#1-overview)
2. [PlantUML Document Structure](#2-plantuml-document-structure)
3. [PlantUML Syntax Reference](#3-plantuml-syntax-reference)
4. [Code-to-PlantUML Generation](#4-code-to-plantuml-generation)
5. [PlantUML-to-Code Scaffolding](#5-plantuml-to-code-scaffolding)
6. [CRUD Operations Specification](#6-crud-operations-specification)
7. [Entity Definition Schema](#7-entity-definition-schema)
8. [API Endpoint Schema](#8-api-endpoint-schema)
9. [Angular Component Schema](#9-angular-component-schema)
10. [Parsing Rules](#10-parsing-rules)
11. [Generation Templates](#11-generation-templates)
12. [Validation Rules](#12-validation-rules)
13. [Testing Requirements](#13-testing-requirements)

---

## 1. Overview

### 1.1 Purpose

This specification defines:
1. **Standard PlantUML document formats** for representing .NET and Angular architecture
2. **Parsing rules** for extracting code structure from PlantUML diagrams
3. **Generation rules** for producing PlantUML from existing codebases
4. **Scaffolding templates** for creating operational code from PlantUML specifications
5. **CRUD operation requirements** ensuring scaffolded solutions are fully functional

### 1.2 Scope

A coding agent implementing this specification SHALL be able to:
- **Parse** PlantUML architecture documents and extract all entities, relationships, endpoints, and components
- **Generate** complete .NET solutions with working CRUD operations
- **Generate** Angular frontends with working CRUD operations
- **Reverse-engineer** existing codebases into standardized PlantUML documentation
- **Produce** fully testable and operational scaffolded solutions

### 1.3 Definitions

| Term | Definition |
|------|------------|
| Entity | A domain model class representing a business concept |
| Aggregate | A cluster of entities with a root entity |
| CRUD | Create, Read, Update, Delete operations |
| Scaffold | Generated code structure with placeholder implementation |
| Operational | Code that compiles, runs, and passes tests |

---

## 2. PlantUML Document Structure

### 2.1 Required Documents

A complete PlantUML architecture specification MUST include:

| Document | Purpose | Contains |
|----------|---------|----------|
| `solution-architecture.puml` | Solution structure | Projects, dependencies, layers |
| `domain-models.puml` | Entity definitions | Classes, properties, relationships, enums |
| `cqrs-pattern.puml` | CQRS implementation | Requests, responses, handlers, validators |
| `api-endpoints.puml` | REST API specification | Controllers, routes, HTTP methods |
| `database-architecture.puml` | Data layer | Contexts, repositories, schemas |
| `authentication-flow.puml` | Security | Auth flows, JWT, roles |
| `angular-architecture.puml` | Frontend structure | Services, components, modules |
| `angular-routing.puml` | Frontend routing | Routes, guards, components |
| `angular-http.puml` | API communication | HTTP services, interceptors |

### 2.2 Document Header Format

Every PlantUML document MUST begin with:

```plantuml
@startuml Document-Title
!pragma layout smetana
skinparam backgroundColor #FEFEFE
skinparam defaultFontSize 11
skinparam wrapWidth 250

title Document Title - Version X.X

' Document content here

@enduml
```

### 2.3 Metadata Block

Each document SHOULD include a metadata note:

```plantuml
note "Document Metadata" as metadata
    **Generated:** YYYY-MM-DD
    **Version:** X.X.X
    **Source:** manual | generated
    **Target Framework:** .NET 8.0
    **Angular Version:** 19
end note
```

---

## 3. PlantUML Syntax Reference

### 3.1 Styling Configuration

```plantuml
' Standard color palette
skinparam component {
    BackgroundColor #E3F2FD
    BorderColor #1976D2
}

skinparam class {
    BackgroundColor #E1F5FE
    BorderColor #01579B
}

skinparam database {
    BackgroundColor #C8E6C9
    BorderColor #388E3C
}

skinparam package {
    BackgroundColor #F3E5F5
    BorderColor #8E24AA
}
```

### 3.2 Component Definition (VALID)

```plantuml
' CORRECT - Simple bracket notation
component [ComponentName] as alias

note right of alias
    **Title**
    Description text

    - Detail 1
    - Detail 2
end note
```

```plantuml
' INVALID - Multi-line braces (DO NOT USE)
component "ComponentName" as alias {
    Content here  ' WILL CAUSE PARSE ERROR
}
```

### 3.3 Class Definition

```plantuml
class ClassName {
    +PublicProperty : Type
    -PrivateProperty : Type
    #ProtectedProperty : Type
    ~PackageProperty : Type
    __
    +PublicMethod(param: Type) : ReturnType
    -PrivateMethod() : void
}
```

**Visibility Modifiers:**
- `+` Public
- `-` Private
- `#` Protected
- `~` Package/Internal

**Type Annotations:**
- `Type?` - Nullable
- `List<Type>` - Collection
- `Dictionary<K,V>` - Map/Dictionary
- `Type[]` - Array

### 3.4 Relationship Notation

```plantuml
' Composition (strong ownership)
Parent "1" *-- "0..*" Child : contains

' Aggregation (weak ownership)
Container "0..1" o-- "0..*" Item : has

' Association (reference)
ClassA --> ClassB : uses

' Dependency (dashed)
ClassA ..> ClassB : depends on

' Inheritance
Child --|> Parent : extends

' Implementation
Concrete ..|> Interface : implements
```

**Cardinality Notation:**
- `1` - Exactly one
- `0..1` - Zero or one
- `*` or `0..*` - Zero or many
- `1..*` - One or many
- `n..m` - Range

### 3.5 Sequence Diagram Elements

```plantuml
actor "User" as user
participant "Component" as comp
database "Database" as db
entity "Entity" as ent
boundary "Boundary" as bound
control "Controller" as ctrl

' Messages
user -> comp : sync call
user ->> comp : async call
comp --> user : return
comp -->> user : async return

' Activation
activate comp
deactivate comp

' Conditionals
alt condition
    comp -> db : action1
else otherwise
    comp -> db : action2
end

' Loops
loop for each item
    comp -> db : process
end

' Notes (VALID positions)
note over comp
    Note text
end note

note right of comp
    Note text
end note
```

### 3.6 Width Constraint (1200px Maximum)

```plantuml
' If diagram exceeds 1200px:
scale 0.95

' Or reduce font:
skinparam defaultFontSize 10

' Or reduce wrap width:
skinparam wrapWidth 200
skinparam maxMessageSize 180
```

---

## 4. Code-to-PlantUML Generation

### 4.1 C# Source Analysis

A generator implementing this specification MUST analyze:

**Solution Level:**
- `.sln` file for project references
- Project dependencies from `.csproj` files
- NuGet package references

**Domain Models:**
- Classes inheriting from entity base classes
- Properties with `[Key]`, `[Required]` attributes
- Navigation properties for relationships
- Enum definitions

**CQRS Pattern:**
- Classes implementing `IRequest<T>`
- Classes implementing `IRequestHandler<TRequest, TResponse>`
- Classes inheriting from `AbstractValidator<T>`

**API Endpoints:**
- Controllers with `[ApiController]` attribute
- Methods with `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Route templates from `[Route]` attributes
- Parameter bindings (`[FromBody]`, `[FromQuery]`, `[FromRoute]`)

**Database:**
- Classes inheriting from `DbContext`
- `DbSet<T>` properties
- `OnModelCreating` configurations

### 4.2 Angular Source Analysis

A generator MUST analyze:

**Services:**
- Classes with `@Injectable()` decorator
- HTTP method calls (`get`, `post`, `put`, `delete`)
- Return type observables

**Components:**
- Classes with `@Component()` decorator
- Template references
- Input/Output properties

**Routing:**
- Route configurations in `app.routes.ts`
- Guard implementations
- Lazy-loaded modules

### 4.3 Generation Algorithm

```
FUNCTION GeneratePlantUML(codebase: Codebase) -> PlantUMLDocuments:

    1. SCAN solution file
       - Extract project names and types
       - Map project dependencies
       - Identify layer classification (Api, Core, Infrastructure, Test)

    2. FOR EACH domain model file:
       - Parse class declaration
       - Extract properties with types
       - Identify relationships via navigation properties
       - Detect inheritance hierarchy
       - OUTPUT to domain-models.puml

    3. FOR EACH request/handler pair:
       - Parse request class properties
       - Parse response class properties
       - Identify handler method logic
       - OUTPUT to cqrs-pattern.puml

    4. FOR EACH controller:
       - Extract route base
       - FOR EACH action method:
         - Determine HTTP verb
         - Extract route template
         - Identify parameters
         - Map to request/response types
       - OUTPUT to api-endpoints.puml

    5. FOR EACH Angular service:
       - Extract injectable configuration
       - Parse HTTP method calls
       - Map endpoints to backend
       - OUTPUT to angular-http.puml

    6. FOR EACH Angular component:
       - Extract selector, template, styles
       - Identify service dependencies
       - Map to routes
       - OUTPUT to angular-architecture.puml

    RETURN all PlantUML documents
```

### 4.4 Class-to-PlantUML Mapping

**C# Class:**
```csharp
namespace Tseten.Aggregates.SoftwareRequirement;

public class SoftwareRequirement
{
    public string SoftwareRequirementId { get; set; }
    public string? ParentSoftwareRequirementId { get; set; }
    public string Description { get; set; }
    public bool CanImplement { get; set; }
    public bool CanTest { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public List<AcceptanceCriteria> AcceptanceCriteria { get; set; } = new();
}
```

**PlantUML Output:**
```plantuml
package "Tseten.Aggregates.SoftwareRequirement" {
    class SoftwareRequirement {
        +SoftwareRequirementId : string
        +ParentSoftwareRequirementId : string?
        +Description : string
        +CanImplement : bool
        +CanTest : bool
        +Priority : Priority
        +Status : Status
        +Comments : List<Comment>
        +AcceptanceCriteria : List<AcceptanceCriteria>
    }
}

SoftwareRequirement "1" *-- "0..*" Comment : contains
SoftwareRequirement "1" *-- "0..*" AcceptanceCriteria : contains
SoftwareRequirement --> Priority
SoftwareRequirement --> Status
```

---

## 5. PlantUML-to-Code Scaffolding

### 5.1 Parsing Algorithm

```
FUNCTION ParsePlantUML(document: PlantUMLDocument) -> CodeModel:

    1. TOKENIZE document into PlantUML elements
       - @startuml / @enduml boundaries
       - class / enum / component definitions
       - relationship declarations
       - note blocks

    2. BUILD Abstract Syntax Tree (AST)
       - Package hierarchy
       - Class definitions with members
       - Relationship graph

    3. EXTRACT code model:
       - Namespaces from packages
       - Classes with properties and methods
       - Enums with values
       - Relationships with cardinality

    4. VALIDATE model completeness:
       - All referenced types exist
       - Relationships are bidirectional
       - Required properties identified

    RETURN CodeModel
```

### 5.2 Entity Extraction Regex Patterns

```regex
# Class definition
class\s+(\w+)\s*(?:<<(\w+)>>)?\s*\{([^}]*)\}

# Property line
([+\-#~])?(\w+)\s*:\s*(\w+(?:<[\w,\s]+>)?)\??

# Enum definition
enum\s+(\w+)\s*\{([^}]*)\}

# Relationship
(\w+)\s*"([^"]+)"\s*([*o\-]+)\s*"([^"]+)"\s*(\w+)\s*:\s*(.+)

# Package
package\s+"([^"]+)"\s*(?:<<(\w+)>>)?\s*\{
```

### 5.3 Scaffolding Output Structure

```
Solution/
├── src/
│   ├── {Project}.Api/
│   │   ├── Controllers/
│   │   │   └── {Entity}Controller.cs
│   │   ├── Program.cs
│   │   └── {Project}.Api.csproj
│   ├── {Project}.Core/
│   │   ├── Aggregates/
│   │   │   └── {Entity}/
│   │   │       ├── {Entity}.cs
│   │   │       ├── {Entity}Dto.cs
│   │   │       └── {Entity}Extensions.cs
│   │   ├── {Entity}/
│   │   │   ├── Create{Entity}Request.cs
│   │   │   ├── Create{Entity}Response.cs
│   │   │   ├── Create{Entity}Handler.cs
│   │   │   ├── Create{Entity}Validator.cs
│   │   │   ├── Get{Entity}ByIdRequest.cs
│   │   │   ├── Get{Entity}ByIdResponse.cs
│   │   │   ├── Get{Entity}ByIdHandler.cs
│   │   │   ├── Get{Entity}sRequest.cs
│   │   │   ├── Get{Entity}sResponse.cs
│   │   │   ├── Get{Entity}sHandler.cs
│   │   │   ├── Update{Entity}Request.cs
│   │   │   ├── Update{Entity}Response.cs
│   │   │   ├── Update{Entity}Handler.cs
│   │   │   ├── Update{Entity}Validator.cs
│   │   │   ├── Delete{Entity}Request.cs
│   │   │   ├── Delete{Entity}Response.cs
│   │   │   └── Delete{Entity}Handler.cs
│   │   └── {Project}.Core.csproj
│   └── {Project}.Infrastructure/
│       ├── Data/
│       │   └── {Project}Context.cs
│       └── {Project}.Infrastructure.csproj
├── test/
│   ├── {Project}.Api.Tests/
│   │   └── Controllers/
│   │       └── {Entity}ControllerTests.cs
│   ├── {Project}.Core.Tests/
│   │   └── {Entity}/
│   │       └── {Entity}HandlerTests.cs
│   └── {Project}.Infrastructure.Tests/
│       └── Data/
│           └── {Project}ContextTests.cs
├── {Project}.App/                    # Angular frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── @core/
│   │   │   │   └── {entity}.service.ts
│   │   │   ├── pages/
│   │   │   │   └── {entity}/
│   │   │   │       ├── {entity}-list/
│   │   │   │       ├── {entity}-detail/
│   │   │   │       └── {entity}-form/
│   │   │   └── models/
│   │   │       └── {entity}.model.ts
│   │   └── environments/
│   └── angular.json
└── {Project}.sln
```

---

## 6. CRUD Operations Specification

### 6.1 Required Operations Per Entity

For each entity defined in `domain-models.puml`, the scaffolder MUST generate:

| Operation | Backend Components | Frontend Components |
|-----------|-------------------|---------------------|
| **Create** | Request, Response, Handler, Validator, Endpoint | Form component, Service method |
| **Read (Single)** | Request, Response, Handler, Endpoint | Detail component, Service method |
| **Read (List)** | Request, Response, Handler, Endpoint | List component, Service method |
| **Update** | Request, Response, Handler, Validator, Endpoint | Form component (edit mode), Service method |
| **Delete** | Request, Response, Handler, Endpoint | Delete action, Service method |

### 6.2 Backend CRUD Implementation

#### 6.2.1 Create Operation

**Request Class:**
```csharp
// {Entity}/Create{Entity}Request.cs
namespace {Project}.Core.{Entity};

public class Create{Entity}Request : IRequest<Create{Entity}Response>
{
    // All required properties from entity (excluding Id and audit fields)
    public string Property1 { get; set; }
    public int Property2 { get; set; }
    // ... mapped from PlantUML class definition
}
```

**Response Class:**
```csharp
// {Entity}/Create{Entity}Response.cs
namespace {Project}.Core.{Entity};

public class Create{Entity}Response
{
    public {Entity}Dto {Entity} { get; set; }
}
```

**Handler Class:**
```csharp
// {Entity}/Create{Entity}Handler.cs
namespace {Project}.Core.{Entity};

public class Create{Entity}Handler : IRequestHandler<Create{Entity}Request, Create{Entity}Response>
{
    private readonly I{Project}Context _context;

    public Create{Entity}Handler(I{Project}Context context)
    {
        _context = context;
    }

    public async Task<Create{Entity}Response> Handle(
        Create{Entity}Request request,
        CancellationToken cancellationToken)
    {
        var entity = new Aggregates.{Entity}.{Entity}
        {
            {Entity}Id = Guid.NewGuid().ToString(),
            Property1 = request.Property1,
            Property2 = request.Property2,
            // ... map all properties
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        await _context.{Entity}s.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new Create{Entity}Response
        {
            {Entity} = entity.ToDto()
        };
    }
}
```

**Validator Class:**
```csharp
// {Entity}/Create{Entity}Validator.cs
namespace {Project}.Core.{Entity};

public class Create{Entity}Validator : AbstractValidator<Create{Entity}Request>
{
    public Create{Entity}Validator()
    {
        // For each required string property:
        RuleFor(x => x.Property1)
            .NotEmpty()
            .MaximumLength(500);

        // For each numeric property with constraints:
        RuleFor(x => x.Property2)
            .GreaterThan(0);

        // ... validation rules derived from property types and notes
    }
}
```

#### 6.2.2 Read (Single) Operation

**Request Class:**
```csharp
// {Entity}/Get{Entity}ByIdRequest.cs
namespace {Project}.Core.{Entity};

public class Get{Entity}ByIdRequest : IRequest<Get{Entity}ByIdResponse>
{
    public string {Entity}Id { get; set; }
}
```

**Response Class:**
```csharp
// {Entity}/Get{Entity}ByIdResponse.cs
namespace {Project}.Core.{Entity};

public class Get{Entity}ByIdResponse
{
    public {Entity}Dto {Entity} { get; set; }
}
```

**Handler Class:**
```csharp
// {Entity}/Get{Entity}ByIdHandler.cs
namespace {Project}.Core.{Entity};

public class Get{Entity}ByIdHandler : IRequestHandler<Get{Entity}ByIdRequest, Get{Entity}ByIdResponse>
{
    private readonly I{Project}Context _context;

    public Get{Entity}ByIdHandler(I{Project}Context context)
    {
        _context = context;
    }

    public async Task<Get{Entity}ByIdResponse> Handle(
        Get{Entity}ByIdRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.{Entity}s
            .Include(e => e.ChildCollection1)  // Include related entities
            .Include(e => e.ChildCollection2)
            .FirstOrDefaultAsync(
                e => e.{Entity}Id == request.{Entity}Id,
                cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"{Entity} with ID {request.{Entity}Id} not found");
        }

        return new Get{Entity}ByIdResponse
        {
            {Entity} = entity.ToDto()
        };
    }
}
```

#### 6.2.3 Read (List) Operation

**Request Class:**
```csharp
// {Entity}/Get{Entity}sRequest.cs
namespace {Project}.Core.{Entity};

public class Get{Entity}sRequest : IRequest<Get{Entity}sResponse>
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
```

**Response Class:**
```csharp
// {Entity}/Get{Entity}sResponse.cs
namespace {Project}.Core.{Entity};

public class Get{Entity}sResponse
{
    public List<{Entity}Dto> {Entity}s { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

**Handler Class:**
```csharp
// {Entity}/Get{Entity}sHandler.cs
namespace {Project}.Core.{Entity};

public class Get{Entity}sHandler : IRequestHandler<Get{Entity}sRequest, Get{Entity}sResponse>
{
    private readonly I{Project}Context _context;

    public Get{Entity}sHandler(I{Project}Context context)
    {
        _context = context;
    }

    public async Task<Get{Entity}sResponse> Handle(
        Get{Entity}sRequest request,
        CancellationToken cancellationToken)
    {
        var query = _context.{Entity}s.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(e =>
                e.Property1.Contains(request.SearchTerm) ||
                e.Property2.Contains(request.SearchTerm));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "property1" => request.SortDescending
                ? query.OrderByDescending(e => e.Property1)
                : query.OrderBy(e => e.Property1),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.ModifiedAt)
        };

        // Apply pagination
        var entities = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new Get{Entity}sResponse
        {
            {Entity}s = entities.Select(e => e.ToDto()).ToList(),
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
```

#### 6.2.4 Update Operation

**Request Class:**
```csharp
// {Entity}/Update{Entity}Request.cs
namespace {Project}.Core.{Entity};

public class Update{Entity}Request : IRequest<Update{Entity}Response>
{
    public string {Entity}Id { get; set; }
    public string Property1 { get; set; }
    public int Property2 { get; set; }
    // ... all updateable properties
}
```

**Response Class:**
```csharp
// {Entity}/Update{Entity}Response.cs
namespace {Project}.Core.{Entity};

public class Update{Entity}Response
{
    public {Entity}Dto {Entity} { get; set; }
}
```

**Handler Class:**
```csharp
// {Entity}/Update{Entity}Handler.cs
namespace {Project}.Core.{Entity};

public class Update{Entity}Handler : IRequestHandler<Update{Entity}Request, Update{Entity}Response>
{
    private readonly I{Project}Context _context;

    public Update{Entity}Handler(I{Project}Context context)
    {
        _context = context;
    }

    public async Task<Update{Entity}Response> Handle(
        Update{Entity}Request request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.{Entity}s
            .FirstOrDefaultAsync(
                e => e.{Entity}Id == request.{Entity}Id,
                cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"{Entity} with ID {request.{Entity}Id} not found");
        }

        // Update properties
        entity.Property1 = request.Property1;
        entity.Property2 = request.Property2;
        entity.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new Update{Entity}Response
        {
            {Entity} = entity.ToDto()
        };
    }
}
```

**Validator Class:**
```csharp
// {Entity}/Update{Entity}Validator.cs
namespace {Project}.Core.{Entity};

public class Update{Entity}Validator : AbstractValidator<Update{Entity}Request>
{
    public Update{Entity}Validator()
    {
        RuleFor(x => x.{Entity}Id)
            .NotEmpty();

        // Same rules as Create validator
        RuleFor(x => x.Property1)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Property2)
            .GreaterThan(0);
    }
}
```

#### 6.2.5 Delete Operation

**Request Class:**
```csharp
// {Entity}/Delete{Entity}Request.cs
namespace {Project}.Core.{Entity};

public class Delete{Entity}Request : IRequest<Delete{Entity}Response>
{
    public string {Entity}Id { get; set; }
}
```

**Response Class:**
```csharp
// {Entity}/Delete{Entity}Response.cs
namespace {Project}.Core.{Entity};

public class Delete{Entity}Response
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

**Handler Class:**
```csharp
// {Entity}/Delete{Entity}Handler.cs
namespace {Project}.Core.{Entity};

public class Delete{Entity}Handler : IRequestHandler<Delete{Entity}Request, Delete{Entity}Response>
{
    private readonly I{Project}Context _context;

    public Delete{Entity}Handler(I{Project}Context context)
    {
        _context = context;
    }

    public async Task<Delete{Entity}Response> Handle(
        Delete{Entity}Request request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.{Entity}s
            .FirstOrDefaultAsync(
                e => e.{Entity}Id == request.{Entity}Id,
                cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"{Entity} with ID {request.{Entity}Id} not found");
        }

        _context.{Entity}s.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new Delete{Entity}Response
        {
            Success = true,
            Message = $"{Entity} with ID {request.{Entity}Id} deleted successfully"
        };
    }
}
```

### 6.3 API Controller Implementation

```csharp
// Controllers/{Entity}Controller.cs
namespace {Project}.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class {Entity}Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public {Entity}Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all {entity}s with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Get{Entity}sResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var request = new Get{Entity}sRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var response = await _mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Get {entity} by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Get{Entity}ByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var request = new Get{Entity}ByIdRequest { {Entity}Id = id };
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Create new {entity}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Create{Entity}Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Create{Entity}Request request)
    {
        var response = await _mediator.Send(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.{Entity}.{Entity}Id },
            response);
    }

    /// <summary>
    /// Update existing {entity}
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Update{Entity}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] Update{Entity}Request request)
    {
        request.{Entity}Id = id;
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Delete {entity}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Delete{Entity}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var request = new Delete{Entity}Request { {Entity}Id = id };
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
```

### 6.4 Frontend CRUD Implementation

#### 6.4.1 Angular Service

```typescript
// @core/{entity}.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
    {Entity},
    Create{Entity}Request,
    Update{Entity}Request,
    {Entity}ListResponse
} from '../models/{entity}.model';

@Injectable({
    providedIn: 'root'
})
export class {Entity}Service {
    private http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/api/{entity}`;

    getAll(
        pageIndex = 0,
        pageSize = 20,
        searchTerm?: string,
        sortBy?: string,
        sortDescending = false
    ): Observable<{Entity}ListResponse> {
        let params = new HttpParams()
            .set('pageIndex', pageIndex.toString())
            .set('pageSize', pageSize.toString())
            .set('sortDescending', sortDescending.toString());

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }
        if (sortBy) {
            params = params.set('sortBy', sortBy);
        }

        return this.http.get<{Entity}ListResponse>(this.baseUrl, { params });
    }

    getById(id: string): Observable<{ {entity}: {Entity} }> {
        return this.http.get<{ {entity}: {Entity} }>(`${this.baseUrl}/${id}`);
    }

    create(request: Create{Entity}Request): Observable<{ {entity}: {Entity} }> {
        return this.http.post<{ {entity}: {Entity} }>(this.baseUrl, request);
    }

    update(id: string, request: Update{Entity}Request): Observable<{ {entity}: {Entity} }> {
        return this.http.put<{ {entity}: {Entity} }>(`${this.baseUrl}/${id}`, request);
    }

    delete(id: string): Observable<{ success: boolean; message: string }> {
        return this.http.delete<{ success: boolean; message: string }>(`${this.baseUrl}/${id}`);
    }
}
```

#### 6.4.2 Angular Model

```typescript
// models/{entity}.model.ts
export interface {Entity} {
    {entity}Id: string;
    property1: string;
    property2: number;
    // ... all properties from PlantUML
    createdAt: Date;
    modifiedAt: Date;
}

export interface Create{Entity}Request {
    property1: string;
    property2: number;
    // ... all createable properties
}

export interface Update{Entity}Request {
    {entity}Id: string;
    property1: string;
    property2: number;
    // ... all updateable properties
}

export interface {Entity}ListResponse {
    {entity}s: {Entity}[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}
```

#### 6.4.3 List Component

```typescript
// pages/{entity}/{entity}-list/{entity}-list.component.ts
import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { {Entity}Service } from '../../../@core/{entity}.service';
import { {Entity} } from '../../../models/{entity}.model';

@Component({
    selector: 'app-{entity}-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        MatTableModule,
        MatPaginatorModule,
        MatButtonModule,
        MatIconModule,
        MatInputModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatSnackBarModule
    ],
    template: `
        <div class="container">
            <div class="header">
                <h1>{Entity}s</h1>
                <button mat-raised-button color="primary" routerLink="new">
                    <mat-icon>add</mat-icon> New {Entity}
                </button>
            </div>

            <mat-form-field appearance="outline" class="search-field">
                <mat-label>Search</mat-label>
                <input matInput [(ngModel)]="searchTerm" (keyup.enter)="search()">
                <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>

            @if (loading()) {
                <mat-spinner></mat-spinner>
            } @else {
                <table mat-table [dataSource]="{entity}s()">
                    <ng-container matColumnDef="property1">
                        <th mat-header-cell *matHeaderCellDef>Property 1</th>
                        <td mat-cell *matCellDef="let item">{{ item.property1 }}</td>
                    </ng-container>

                    <ng-container matColumnDef="property2">
                        <th mat-header-cell *matHeaderCellDef>Property 2</th>
                        <td mat-cell *matCellDef="let item">{{ item.property2 }}</td>
                    </ng-container>

                    <ng-container matColumnDef="actions">
                        <th mat-header-cell *matHeaderCellDef>Actions</th>
                        <td mat-cell *matCellDef="let item">
                            <button mat-icon-button [routerLink]="[item.{entity}Id]">
                                <mat-icon>visibility</mat-icon>
                            </button>
                            <button mat-icon-button [routerLink]="[item.{entity}Id, 'edit']">
                                <mat-icon>edit</mat-icon>
                            </button>
                            <button mat-icon-button color="warn" (click)="delete(item)">
                                <mat-icon>delete</mat-icon>
                            </button>
                        </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
                </table>

                <mat-paginator
                    [length]="totalCount()"
                    [pageIndex]="pageIndex()"
                    [pageSize]="pageSize()"
                    [pageSizeOptions]="[10, 20, 50]"
                    (page)="onPageChange($event)">
                </mat-paginator>
            }
        </div>
    `,
    styles: [`
        .container { padding: 20px; }
        .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
        .search-field { width: 100%; max-width: 400px; margin-bottom: 20px; }
        table { width: 100%; }
    `]
})
export class {Entity}ListComponent {
    private service = inject({Entity}Service);
    private snackBar = inject(MatSnackBar);

    {entity}s = signal<{Entity}[]>([]);
    loading = signal(false);
    totalCount = signal(0);
    pageIndex = signal(0);
    pageSize = signal(20);
    searchTerm = '';

    displayedColumns = ['property1', 'property2', 'actions'];

    constructor() {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.service.getAll(
            this.pageIndex(),
            this.pageSize(),
            this.searchTerm || undefined
        ).subscribe({
            next: (response) => {
                this.{entity}s.set(response.{entity}s);
                this.totalCount.set(response.totalCount);
                this.loading.set(false);
            },
            error: (err) => {
                this.snackBar.open('Error loading {entity}s', 'Close', { duration: 3000 });
                this.loading.set(false);
            }
        });
    }

    search(): void {
        this.pageIndex.set(0);
        this.load();
    }

    onPageChange(event: PageEvent): void {
        this.pageIndex.set(event.pageIndex);
        this.pageSize.set(event.pageSize);
        this.load();
    }

    delete(item: {Entity}): void {
        if (confirm('Are you sure you want to delete this {entity}?')) {
            this.service.delete(item.{entity}Id).subscribe({
                next: () => {
                    this.snackBar.open('{Entity} deleted', 'Close', { duration: 3000 });
                    this.load();
                },
                error: () => {
                    this.snackBar.open('Error deleting {entity}', 'Close', { duration: 3000 });
                }
            });
        }
    }
}
```

#### 6.4.4 Form Component

```typescript
// pages/{entity}/{entity}-form/{entity}-form.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { {Entity}Service } from '../../../@core/{entity}.service';

@Component({
    selector: 'app-{entity}-form',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatSnackBarModule
    ],
    template: `
        <div class="container">
            <h1>{{ isEditMode() ? 'Edit' : 'Create' }} {Entity}</h1>

            @if (loading()) {
                <mat-spinner></mat-spinner>
            } @else {
                <form [formGroup]="form" (ngSubmit)="submit()">
                    <mat-form-field appearance="outline" class="full-width">
                        <mat-label>Property 1</mat-label>
                        <input matInput formControlName="property1">
                        @if (form.get('property1')?.hasError('required')) {
                            <mat-error>Property 1 is required</mat-error>
                        }
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="full-width">
                        <mat-label>Property 2</mat-label>
                        <input matInput type="number" formControlName="property2">
                        @if (form.get('property2')?.hasError('required')) {
                            <mat-error>Property 2 is required</mat-error>
                        }
                    </mat-form-field>

                    <!-- Additional form fields generated from PlantUML properties -->

                    <div class="actions">
                        <button mat-button type="button" routerLink="..">Cancel</button>
                        <button mat-raised-button color="primary" type="submit"
                            [disabled]="form.invalid || submitting()">
                            {{ submitting() ? 'Saving...' : 'Save' }}
                        </button>
                    </div>
                </form>
            }
        </div>
    `,
    styles: [`
        .container { padding: 20px; max-width: 600px; }
        .full-width { width: 100%; margin-bottom: 16px; }
        .actions { display: flex; gap: 16px; justify-content: flex-end; margin-top: 20px; }
    `]
})
export class {Entity}FormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private service = inject({Entity}Service);
    private snackBar = inject(MatSnackBar);

    form: FormGroup;
    loading = signal(false);
    submitting = signal(false);
    isEditMode = signal(false);
    private {entity}Id: string | null = null;

    constructor() {
        this.form = this.fb.group({
            property1: ['', [Validators.required, Validators.maxLength(500)]],
            property2: [0, [Validators.required, Validators.min(1)]]
            // Additional controls generated from PlantUML properties
        });
    }

    ngOnInit(): void {
        this.{entity}Id = this.route.snapshot.paramMap.get('id');

        if (this.{entity}Id && this.{entity}Id !== 'new') {
            this.isEditMode.set(true);
            this.loadEntity();
        }
    }

    private loadEntity(): void {
        this.loading.set(true);
        this.service.getById(this.{entity}Id!).subscribe({
            next: (response) => {
                this.form.patchValue({
                    property1: response.{entity}.property1,
                    property2: response.{entity}.property2
                });
                this.loading.set(false);
            },
            error: () => {
                this.snackBar.open('Error loading {entity}', 'Close', { duration: 3000 });
                this.router.navigate(['/{entity}s']);
            }
        });
    }

    submit(): void {
        if (this.form.invalid) return;

        this.submitting.set(true);
        const data = this.form.value;

        const request$ = this.isEditMode()
            ? this.service.update(this.{entity}Id!, data)
            : this.service.create(data);

        request$.subscribe({
            next: () => {
                this.snackBar.open(
                    `{Entity} ${this.isEditMode() ? 'updated' : 'created'} successfully`,
                    'Close',
                    { duration: 3000 }
                );
                this.router.navigate(['/{entity}s']);
            },
            error: () => {
                this.snackBar.open('Error saving {entity}', 'Close', { duration: 3000 });
                this.submitting.set(false);
            }
        });
    }
}
```

#### 6.4.5 Detail Component

```typescript
// pages/{entity}/{entity}-detail/{entity}-detail.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { {Entity}Service } from '../../../@core/{entity}.service';
import { {Entity} } from '../../../models/{entity}.model';

@Component({
    selector: 'app-{entity}-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatSnackBarModule
    ],
    template: `
        <div class="container">
            @if (loading()) {
                <mat-spinner></mat-spinner>
            } @else if ({entity}()) {
                <mat-card>
                    <mat-card-header>
                        <mat-card-title>{{ {entity}()?.property1 }}</mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                        <div class="detail-row">
                            <strong>ID:</strong> {{ {entity}()?.{entity}Id }}
                        </div>
                        <div class="detail-row">
                            <strong>Property 1:</strong> {{ {entity}()?.property1 }}
                        </div>
                        <div class="detail-row">
                            <strong>Property 2:</strong> {{ {entity}()?.property2 }}
                        </div>
                        <!-- Additional detail rows generated from PlantUML properties -->
                        <div class="detail-row">
                            <strong>Created:</strong> {{ {entity}()?.createdAt | date:'medium' }}
                        </div>
                        <div class="detail-row">
                            <strong>Modified:</strong> {{ {entity}()?.modifiedAt | date:'medium' }}
                        </div>
                    </mat-card-content>
                    <mat-card-actions align="end">
                        <button mat-button routerLink="/{entity}s">Back to List</button>
                        <button mat-raised-button color="primary" [routerLink]="['edit']">
                            <mat-icon>edit</mat-icon> Edit
                        </button>
                        <button mat-raised-button color="warn" (click)="delete()">
                            <mat-icon>delete</mat-icon> Delete
                        </button>
                    </mat-card-actions>
                </mat-card>
            }
        </div>
    `,
    styles: [`
        .container { padding: 20px; max-width: 800px; }
        .detail-row { margin: 12px 0; }
        mat-card-content { padding: 16px; }
    `]
})
export class {Entity}DetailComponent implements OnInit {
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private service = inject({Entity}Service);
    private snackBar = inject(MatSnackBar);

    {entity} = signal<{Entity} | null>(null);
    loading = signal(false);

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.load(id);
        }
    }

    private load(id: string): void {
        this.loading.set(true);
        this.service.getById(id).subscribe({
            next: (response) => {
                this.{entity}.set(response.{entity});
                this.loading.set(false);
            },
            error: () => {
                this.snackBar.open('Error loading {entity}', 'Close', { duration: 3000 });
                this.router.navigate(['/{entity}s']);
            }
        });
    }

    delete(): void {
        const entity = this.{entity}();
        if (!entity) return;

        if (confirm('Are you sure you want to delete this {entity}?')) {
            this.service.delete(entity.{entity}Id).subscribe({
                next: () => {
                    this.snackBar.open('{Entity} deleted', 'Close', { duration: 3000 });
                    this.router.navigate(['/{entity}s']);
                },
                error: () => {
                    this.snackBar.open('Error deleting {entity}', 'Close', { duration: 3000 });
                }
            });
        }
    }
}
```

#### 6.4.6 Routing Configuration

```typescript
// app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './@core/auth.guard';

export const routes: Routes = [
    { path: '', redirectTo: '/{entity}s', pathMatch: 'full' },
    {
        path: '{entity}s',
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                loadComponent: () => import('./pages/{entity}/{entity}-list/{entity}-list.component')
                    .then(m => m.{Entity}ListComponent)
            },
            {
                path: 'new',
                loadComponent: () => import('./pages/{entity}/{entity}-form/{entity}-form.component')
                    .then(m => m.{Entity}FormComponent)
            },
            {
                path: ':id',
                loadComponent: () => import('./pages/{entity}/{entity}-detail/{entity}-detail.component')
                    .then(m => m.{Entity}DetailComponent)
            },
            {
                path: ':id/edit',
                loadComponent: () => import('./pages/{entity}/{entity}-form/{entity}-form.component')
                    .then(m => m.{Entity}FormComponent)
            }
        ]
    }
    // ... routes for other entities
];
```

---

## 7. Entity Definition Schema

### 7.1 PlantUML Entity Format

```plantuml
package "Namespace.Aggregates.EntityName" {
    class EntityName <<aggregate>> {
        ' Primary key - REQUIRED
        +EntityNameId : string

        ' Foreign keys
        +ParentEntityId : string?

        ' Required properties (no ? suffix)
        +RequiredProperty : Type

        ' Optional properties (? suffix)
        +OptionalProperty : Type?

        ' Collections (child entities)
        +Children : List<ChildType>

        ' Enumerations
        +Status : StatusEnum

        ' Audit fields - AUTOMATICALLY GENERATED
        +CreatedAt : DateTime
        +ModifiedAt : DateTime
        +CreatedBy : string?
        +ModifiedBy : string?
    }
}
```

### 7.2 Supported Property Types

| PlantUML Type | C# Type | TypeScript Type |
|---------------|---------|-----------------|
| `string` | `string` | `string` |
| `int` | `int` | `number` |
| `long` | `long` | `number` |
| `decimal` | `decimal` | `number` |
| `double` | `double` | `number` |
| `bool` | `bool` | `boolean` |
| `DateTime` | `DateTime` | `Date` |
| `DateTimeOffset` | `DateTimeOffset` | `Date` |
| `Guid` | `Guid` | `string` |
| `List<T>` | `List<T>` | `T[]` |
| `Dictionary<K,V>` | `Dictionary<K,V>` | `Record<K,V>` |
| `Type?` | `Type?` | `Type \| null` |

### 7.3 Relationship Markers

| Relationship | PlantUML | C# Implementation |
|--------------|----------|-------------------|
| One-to-Many | `Parent "1" *-- "0..*" Child` | `List<Child>` in Parent |
| Many-to-One | `Child "*" --> "1" Parent` | `ParentId` + navigation property |
| One-to-One | `A "1" -- "1" B` | Navigation property + foreign key |
| Many-to-Many | `A "*" -- "*" B` | Junction table or collection |

### 7.4 Stereotypes

| Stereotype | Meaning | Scaffolding Effect |
|------------|---------|-------------------|
| `<<aggregate>>` | Aggregate root | Full CRUD, own controller, placed in Aggregates folder |
| `<<entity>>` | Child entity | CRUD via parent, placed in Aggregates folder with parent |
| `<<valueobject>>` | Value object | No ID, embedded |
| `<<enum>>` | Enumeration | Enum file generation |
| `<<service>>` | Service class | Service generation |

---

## 8. API Endpoint Schema

### 8.1 PlantUML Endpoint Format

```plantuml
component [EntityController] as entityCtrl
note right of entityCtrl
    **Route:** /api/entity
    **Authentication:** Required

    **Endpoints:**

    GET / - List all
    Query: pageIndex, pageSize, searchTerm, sortBy, sortDescending
    Returns: GetEntitysResponse

    GET /{id} - Get by ID
    Route: {id}
    Returns: GetEntityByIdResponse

    POST / - Create
    Body: CreateEntityRequest
    Returns: CreateEntityResponse (201)

    PUT /{id} - Update
    Route: {id}
    Body: UpdateEntityRequest
    Returns: UpdateEntityResponse

    DELETE /{id} - Delete
    Route: {id}
    Returns: DeleteEntityResponse
end note
```

### 8.2 Endpoint Parsing Rules

The scaffolder MUST parse endpoint notes and extract:
- HTTP verb (GET, POST, PUT, DELETE, PATCH)
- Route template (relative path)
- Query parameters (name and type)
- Route parameters (name and type)
- Request body type
- Response type
- Status codes

---

## 9. Angular Component Schema

### 9.1 PlantUML Component Format

```plantuml
package "Angular Application" {
    package "@core" {
        component [EntityService] as entitySvc
        note right of entitySvc
            **Injectable Service**

            Methods:
            - getAll(page, size, search): Observable<EntityListResponse>
            - getById(id): Observable<EntityResponse>
            - create(request): Observable<EntityResponse>
            - update(id, request): Observable<EntityResponse>
            - delete(id): Observable<DeleteResponse>
        end note
    }

    package "pages/entity" {
        component [EntityList] as list
        component [EntityDetail] as detail
        component [EntityForm] as form

        note right of list
            **List Component**
            - Paginated table
            - Search functionality
            - Sort columns
            - Delete action
            - Navigate to detail/edit
        end note

        note right of form
            **Form Component**
            - Reactive form
            - Validation
            - Create/Edit modes
        end note
    }
}
```

### 9.2 Route Definition Format

```plantuml
component [AppRoutes] as routes
note right of routes
    **Routes:**

    '' => /entitys (redirect)
    '/entitys' => EntityList [AuthGuard]
    '/entitys/new' => EntityForm [AuthGuard]
    '/entitys/:id' => EntityDetail [AuthGuard]
    '/entitys/:id/edit' => EntityForm [AuthGuard]
end note
```

---

## 10. Parsing Rules

### 10.1 Entity Extraction

```
INPUT: PlantUML class definition
OUTPUT: EntityModel object

RULES:
1. Class name from `class ClassName`
2. Stereotype from `<<stereotype>>`
3. Properties from lines with `: Type` pattern
4. Visibility from prefix (+, -, #, ~)
5. Nullable from ? suffix on type
6. Collections from List<T>, ICollection<T> patterns
7. Relationships from composition/aggregation arrows
```

### 10.2 Property Extraction Regex

```regex
^(?<visibility>[+\-#~])?\s*(?<name>\w+)\s*:\s*(?<type>\w+(?:<[\w,\s]+>)?)\s*(?<nullable>\?)?(?:\s*=\s*(?<default>.+))?$
```

### 10.3 Relationship Extraction Regex

```regex
^(?<source>\w+)\s*"(?<sourceCard>[^"]+)"\s*(?<relType>[*o\-]+)\s*"(?<targetCard>[^"]+)"\s*(?<target>\w+)\s*:\s*(?<label>.+)$
```

### 10.4 Endpoint Extraction Rules

```
FROM note blocks on controller components:

1. Parse "**Route:** {path}" for base route
2. Parse "{HTTP_VERB} {path} - {description}" for endpoints
3. Parse "Query: {params}" for query parameters
4. Parse "Route: {params}" for route parameters
5. Parse "Body: {type}" for request body
6. Parse "Returns: {type}" for response type
7. Parse status codes from parentheses
```

---

## 11. Generation Templates

### 11.1 Solution File Template

```xml
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{Project}.Api", "src\{Project}.Api\{Project}.Api.csproj", "{GUID1}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{Project}.Core", "src\{Project}.Core\{Project}.Core.csproj", "{GUID2}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{Project}.Infrastructure", "src\{Project}.Infrastructure\{Project}.Infrastructure.csproj", "{GUID3}"
EndProject
' ... test projects
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
EndGlobal
```

### 11.2 API Project Template

```xml
<!-- {Project}.Api.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="MediatR" Version="12.2.0" />
        <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
        <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\{Project}.Core\{Project}.Core.csproj" />
        <ProjectReference Include="..\{Project}.Infrastructure\{Project}.Infrastructure.csproj" />
    </ItemGroup>
</Project>
```

### 11.3 Program.cs Template

```csharp
using {Project}.Core;
using {Project}.Infrastructure;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof({Project}Core).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof({Project}Core).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 11.4 Angular Project Template

```json
// angular.json (partial)
{
    "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
    "version": 1,
    "newProjectRoot": "projects",
    "projects": {
        "{project}-app": {
            "projectType": "application",
            "schematics": {
                "@schematics/angular:component": {
                    "style": "scss",
                    "standalone": true
                }
            },
            "root": "",
            "sourceRoot": "src",
            "prefix": "app",
            "architect": {
                "build": {
                    "builder": "@angular-devkit/build-angular:application",
                    "options": {
                        "outputPath": "dist/{project}-app",
                        "index": "src/index.html",
                        "browser": "src/main.ts",
                        "polyfills": ["zone.js"],
                        "tsConfig": "tsconfig.app.json",
                        "styles": [
                            "@angular/material/prebuilt-themes/azure-blue.css",
                            "src/styles.scss"
                        ],
                        "scripts": []
                    }
                }
            }
        }
    }
}
```

---

## 12. Validation Rules

### 12.1 PlantUML Document Validation

Before processing, validate:

- [ ] `@startuml` and `@enduml` boundaries present
- [ ] All class blocks properly closed
- [ ] All note blocks properly closed with `end note`
- [ ] No multi-line content in component `{ }` blocks
- [ ] All referenced types exist in document set
- [ ] No duplicate class/component definitions
- [ ] All relationships reference existing elements

### 12.2 Generated Code Validation

After generation, verify:

- [ ] Solution builds without errors
- [ ] All projects have correct references
- [ ] All CRUD operations compile
- [ ] Controllers have all endpoints
- [ ] Validators have appropriate rules
- [ ] Angular project compiles
- [ ] All services match backend endpoints
- [ ] Routes configured correctly

### 12.3 Runtime Validation

Scaffolded solution MUST:

- [ ] Start without runtime errors
- [ ] Accept API requests
- [ ] Perform database operations
- [ ] Return appropriate status codes
- [ ] Handle validation errors
- [ ] Angular app loads in browser
- [ ] CRUD operations work end-to-end

---

## 13. Testing Requirements

### 13.1 Required Test Coverage

For each entity, generate tests covering:

#### Backend Tests

```csharp
// {Entity}HandlerTests.cs
public class {Entity}HandlerTests
{
    [Fact]
    public async Task Create{Entity}_ValidRequest_ReturnsCreatedEntity()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var handler = new Create{Entity}Handler(context);
        var request = new Create{Entity}Request { /* valid data */ };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response.{Entity});
        Assert.NotEmpty(response.{Entity}.{Entity}Id);
    }

    [Fact]
    public async Task Get{Entity}ById_ExistingId_ReturnsEntity()
    {
        // ...
    }

    [Fact]
    public async Task Get{Entity}ById_NonExistingId_ThrowsNotFoundException()
    {
        // ...
    }

    [Fact]
    public async Task Get{Entity}s_WithPagination_ReturnsPagedResults()
    {
        // ...
    }

    [Fact]
    public async Task Update{Entity}_ValidRequest_ReturnsUpdatedEntity()
    {
        // ...
    }

    [Fact]
    public async Task Delete{Entity}_ExistingId_RemovesEntity()
    {
        // ...
    }
}
```

#### Validator Tests

```csharp
// {Entity}ValidatorTests.cs
public class Create{Entity}ValidatorTests
{
    [Fact]
    public void Validate_EmptyRequiredField_ReturnsError()
    {
        var validator = new Create{Entity}Validator();
        var request = new Create{Entity}Request { Property1 = "" };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Property1");
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        // ...
    }
}
```

#### Integration Tests

```csharp
// {Entity}ControllerIntegrationTests.cs
public class {Entity}ControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateAndRetrieve{Entity}_FullCycle_Works()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/{entity}", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Retrieve
        var created = await createResponse.Content.ReadFromJsonAsync<Create{Entity}Response>();
        var getResponse = await _client.GetAsync($"/api/{entity}/{created.{Entity}.{Entity}Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/api/{entity}/{created.{Entity}.{Entity}Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/{entity}/{created.{Entity}.{Entity}Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Verify deleted
        var verifyResponse = await _client.GetAsync($"/api/{entity}/{created.{Entity}.{Entity}Id}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }
}
```

### 13.2 Test Execution Requirements

The scaffolded solution MUST:

1. **Build successfully**: `dotnet build` completes without errors
2. **Pass all unit tests**: `dotnet test` passes all generated tests
3. **Run integration tests**: With test database, all CRUD cycles complete
4. **Angular tests pass**: `ng test` passes all generated tests
5. **E2E verification**: Manual verification of full frontend-backend cycle

---

## Appendix A: Complete PlantUML Example

```plantuml
@startuml Domain Models
!pragma layout smetana
skinparam backgroundColor #FEFEFE
skinparam defaultFontSize 11

title Domain Models - SoftwareRequirement Aggregate

package "Tseten.Aggregates.SoftwareRequirement" {
    class SoftwareRequirement <<aggregate>> {
        +SoftwareRequirementId : string
        +ParentSoftwareRequirementId : string?
        +Title : string
        +Description : string
        +CanImplement : bool
        +CanTest : bool
        +Priority : Priority
        +Status : Status
        +Comments : List<Comment>
        +AcceptanceCriteria : List<AcceptanceCriteria>
        +CreatedAt : DateTime
        +ModifiedAt : DateTime
    }

    class Comment <<entity>> {
        +CommentId : string
        +SoftwareRequirementId : string
        +Text : string
        +Author : string
        +CreatedAt : DateTime
    }

    class AcceptanceCriteria <<entity>> {
        +AcceptanceCriteriaId : string
        +SoftwareRequirementId : string
        +Description : string
        +IsMet : bool
    }

    enum Priority {
        Low
        Medium
        High
        Critical
    }

    enum Status {
        Draft
        InReview
        Approved
        Implemented
        Verified
        Closed
    }
}

SoftwareRequirement "1" *-- "0..*" Comment : contains
SoftwareRequirement "1" *-- "0..*" AcceptanceCriteria : contains
SoftwareRequirement --> Priority
SoftwareRequirement --> Status

note right of SoftwareRequirement
    **Aggregate Root**

    Represents a software requirement
    that can be tracked through the
    development lifecycle.

    CRUD Operations: Full
    Authentication: Required
end note

@enduml
```

---

## Appendix B: Scaffolder Implementation Checklist

When implementing a scaffolder based on this specification:

### Phase 1: Parser
- [ ] PlantUML tokenizer
- [ ] AST builder for classes, enums, relationships
- [ ] Note block content extractor
- [ ] Endpoint specification parser
- [ ] Validation rule extractor

### Phase 2: Code Model
- [ ] Entity model representation
- [ ] Relationship graph
- [ ] CQRS operation model
- [ ] Angular component model
- [ ] Route configuration model

### Phase 3: .NET Generator
- [ ] Solution file generator
- [ ] Project file generator
- [ ] Entity class generator
- [ ] DTO generator
- [ ] Request/Response generator
- [ ] Handler generator
- [ ] Validator generator
- [ ] Controller generator
- [ ] DbContext generator
- [ ] Program.cs generator

### Phase 4: Angular Generator
- [ ] angular.json generator
- [ ] Service generator
- [ ] Model interface generator
- [ ] List component generator
- [ ] Detail component generator
- [ ] Form component generator
- [ ] Route configuration generator
- [ ] Environment configuration generator

### Phase 5: Test Generator
- [ ] Handler unit tests
- [ ] Validator unit tests
- [ ] Controller integration tests
- [ ] Angular service tests
- [ ] Component tests

### Phase 6: Verification
- [ ] Build verification
- [ ] Test execution
- [ ] Runtime verification
- [ ] E2E verification

---

**End of Specification**
