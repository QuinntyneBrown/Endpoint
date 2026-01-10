# PlantUML Solution Generator - Implementation Roadmap

## Executive Summary

This document outlines the implementation roadmap for the "Create Solution from PlantUML" feature in Endpoint.cli. The feature enables developers to generate complete n-tier .NET solutions, including microservices architecture, Angular frontends, and .NET Aspire orchestration, from PlantUML domain diagrams.

## Project Phases Overview

```
Phase 1: Foundation          Phase 2: N-Tier Gen        Phase 3: Frontend & Aspire    Phase 4: Production Ready
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────────┐    ┌─────────────────────┐
│ PlantUML Parser     │ ──► │ Solution Templates  │ ──► │ Angular Generation      │ ──► │ Polish & Testing    │
│ Semantic Analyzer   │     │ Layer Generation    │     │ Aspire Integration      │    │ Documentation       │
│ Basic Models        │     │ CQRS Scaffolding    │     │ Microservices Support   │    │ CI/CD               │
└─────────────────────┘     └─────────────────────┘     └─────────────────────────┘    └─────────────────────┘
```

---

## Phase 1: Core PlantUML Parser & Foundation

### Milestone 1.1: PlantUML Lexer & Tokenizer

**Objective**: Build the lexical analysis component to tokenize PlantUML syntax.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 1.1.1 | Define PlantUmlToken types (keywords, modifiers, relationships) | Medium | None |
| 1.1.2 | Implement PlantUmlLexer class | High | 1.1.1 |
| 1.1.3 | Handle PlantUML-specific syntax (stereotypes, notes, packages) | Medium | 1.1.2 |
| 1.1.4 | Unit tests for lexer | Medium | 1.1.2 |

**Deliverables**:
- `Endpoint.PlantUml.Parsing.PlantUmlLexer`
- `Endpoint.PlantUml.Parsing.PlantUmlToken`
- `Endpoint.PlantUml.Parsing.PlantUmlTokenType`

### Milestone 1.2: PlantUML Parser & AST

**Objective**: Build the syntactic analysis component to create an Abstract Syntax Tree.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 1.2.1 | Define AST model classes (PlantUmlDocument, PlantUmlClass, etc.) | Medium | None |
| 1.2.2 | Implement PlantUmlParser class | High | 1.1.4, 1.2.1 |
| 1.2.3 | Parse class definitions with fields, methods, properties | High | 1.2.2 |
| 1.2.4 | Parse relationships (inheritance, composition, etc.) | High | 1.2.2 |
| 1.2.5 | Parse stereotypes and metadata | Medium | 1.2.2 |
| 1.2.6 | Parse packages/namespaces | Medium | 1.2.2 |
| 1.2.7 | Unit tests for parser | High | 1.2.3-1.2.6 |

**Deliverables**:
- `Endpoint.PlantUml.Models.PlantUmlDocument`
- `Endpoint.PlantUml.Models.PlantUmlClass`
- `Endpoint.PlantUml.Models.PlantUmlRelationship`
- `Endpoint.PlantUml.Parsing.PlantUmlParser`

### Milestone 1.3: Semantic Analyzer

**Objective**: Build semantic analysis to validate and enrich the parsed model.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 1.3.1 | Implement TypeResolver for type reference resolution | Medium | 1.2.7 |
| 1.3.2 | Implement RelationshipAnalyzer | Medium | 1.2.7 |
| 1.3.3 | Implement DependencyGraphBuilder | High | 1.3.2 |
| 1.3.4 | Implement AggregateDetector (identify aggregates from stereotypes) | Medium | 1.2.7 |
| 1.3.5 | Implement BoundedContextAnalyzer | Medium | 1.2.7 |
| 1.3.6 | Implement ValidationRules engine | High | 1.3.1-1.3.5 |
| 1.3.7 | Unit tests for semantic analysis | High | 1.3.6 |

**Deliverables**:
- `Endpoint.PlantUml.Analysis.SemanticAnalyzer`
- `Endpoint.PlantUml.Analysis.SemanticModel`
- `Endpoint.PlantUml.Analysis.DependencyGraph`

---

## Phase 2: N-Tier Solution Generation

### Milestone 2.1: Solution Template System

**Objective**: Build the template configuration and registry system.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 2.1.1 | Define SolutionTemplateConfiguration model | Medium | None |
| 2.1.2 | Define LayerConfiguration model | Medium | 2.1.1 |
| 2.1.3 | Implement TemplateRegistry | Medium | 2.1.2 |
| 2.1.4 | Create CleanArchitecture template definition | High | 2.1.3 |
| 2.1.5 | Create VerticalSlice template definition | Medium | 2.1.3 |
| 2.1.6 | Create Modular template definition | Medium | 2.1.3 |
| 2.1.7 | Create Microservices template definition | High | 2.1.3 |

**Deliverables**:
- `Endpoint.PlantUml.Templates.SolutionTemplateConfiguration`
- `Endpoint.PlantUml.Templates.TemplateRegistry`
- Built-in template definitions

### Milestone 2.2: Domain Layer Generation

**Objective**: Generate domain layer artifacts from semantic model.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 2.2.1 | Implement AggregateRoot base class template | Medium | 1.3.7 |
| 2.2.2 | Implement Entity base class template | Medium | 2.2.1 |
| 2.2.3 | Implement ValueObject base class template | Medium | 2.2.1 |
| 2.2.4 | Implement DomainEvent generation | Medium | 2.2.1 |
| 2.2.5 | Implement Repository interface generation | Medium | 2.2.1 |
| 2.2.6 | Implement Enumeration generation | Low | 2.2.1 |
| 2.2.7 | Implement DomainException generation | Low | 2.2.1 |
| 2.2.8 | DomainLayerGenerator orchestration | High | 2.2.1-2.2.7 |

**Deliverables**:
- `Endpoint.PlantUml.Generation.DomainLayerGenerator`
- Domain layer Liquid templates

### Milestone 2.3: Application Layer Generation

**Objective**: Generate application layer with CQRS pattern.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 2.3.1 | Implement Command generation | High | 2.2.8 |
| 2.3.2 | Implement CommandHandler generation | High | 2.3.1 |
| 2.3.3 | Implement CommandValidator generation | Medium | 2.3.1 |
| 2.3.4 | Implement Query generation | Medium | 2.2.8 |
| 2.3.5 | Implement QueryHandler generation | High | 2.3.4 |
| 2.3.6 | Implement DTO generation | Medium | 2.2.8 |
| 2.3.7 | Implement AutoMapper profile generation | Medium | 2.3.6 |
| 2.3.8 | Implement MediatR pipeline behaviors | Medium | 2.3.2 |
| 2.3.9 | ApplicationLayerGenerator orchestration | High | 2.3.1-2.3.8 |

**Deliverables**:
- `Endpoint.PlantUml.Generation.ApplicationLayerGenerator`
- CQRS Liquid templates

### Milestone 2.4: Infrastructure Layer Generation

**Objective**: Generate infrastructure layer with EF Core and repository implementations.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 2.4.1 | Implement DbContext generation | High | 2.2.8 |
| 2.4.2 | Implement EntityTypeConfiguration generation | High | 2.4.1 |
| 2.4.3 | Implement Repository implementation generation | Medium | 2.2.5 |
| 2.4.4 | Implement UnitOfWork generation | Medium | 2.4.1 |
| 2.4.5 | Support multiple database providers | High | 2.4.1-2.4.4 |
| 2.4.6 | Implement external service scaffolding | Medium | 2.2.8 |
| 2.4.7 | InfrastructureLayerGenerator orchestration | High | 2.4.1-2.4.6 |

**Deliverables**:
- `Endpoint.PlantUml.Generation.InfrastructureLayerGenerator`
- Infrastructure Liquid templates

### Milestone 2.5: Presentation Layer Generation

**Objective**: Generate API controllers and configuration.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 2.5.1 | Implement Controller generation | High | 2.3.9 |
| 2.5.2 | Implement Program.cs generation | Medium | 2.5.1 |
| 2.5.3 | Implement middleware generation | Medium | 2.5.1 |
| 2.5.4 | Implement appsettings.json generation | Medium | 2.5.2 |
| 2.5.5 | Implement Swagger configuration | Low | 2.5.2 |
| 2.5.6 | PresentationLayerGenerator orchestration | High | 2.5.1-2.5.5 |

**Deliverables**:
- `Endpoint.PlantUml.Generation.PresentationLayerGenerator`
- API Liquid templates

---

## Phase 3: Angular Frontend & Aspire Integration

### Milestone 3.1: Angular Type Mapping

**Objective**: Map C# types to TypeScript types.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.1.1 | Implement TypeScriptTypeMapper | Medium | 2.2.8 |
| 3.1.2 | Handle nullable types | Low | 3.1.1 |
| 3.1.3 | Handle collection types | Medium | 3.1.1 |
| 3.1.4 | Handle dictionary types | Medium | 3.1.1 |
| 3.1.5 | Handle complex/custom types | Medium | 3.1.1 |

**Deliverables**:
- `Endpoint.Angular.TypeMapping.TypeScriptTypeMapper`

### Milestone 3.2: Angular Models & API Client

**Objective**: Generate TypeScript interfaces and API services.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.2.1 | Implement TypeScript interface generation | Medium | 3.1.5 |
| 3.2.2 | Implement request/response model generation | Medium | 3.2.1 |
| 3.2.3 | Implement API service generation | High | 3.2.2 |
| 3.2.4 | Implement HTTP interceptors generation | Medium | 3.2.3 |
| 3.2.5 | Generate API client library structure | Medium | 3.2.4 |

**Deliverables**:
- Angular models library
- Angular API client library

### Milestone 3.3: Angular NgRx State

**Objective**: Generate NgRx state management code.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.3.1 | Implement NgRx actions generation | Medium | 3.2.5 |
| 3.3.2 | Implement NgRx reducer generation | High | 3.3.1 |
| 3.3.3 | Implement NgRx effects generation | High | 3.3.2 |
| 3.3.4 | Implement NgRx selectors generation | Medium | 3.3.2 |
| 3.3.5 | Implement NgRx facade generation | Medium | 3.3.4 |
| 3.3.6 | Generate state library structure | Medium | 3.3.5 |

**Deliverables**:
- Angular state library with NgRx

### Milestone 3.4: Angular Components

**Objective**: Generate Angular feature modules and components.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.4.1 | Implement list component generation | High | 3.3.6 |
| 3.4.2 | Implement detail component generation | Medium | 3.3.6 |
| 3.4.3 | Implement form component generation | High | 3.3.6 |
| 3.4.4 | Implement card component generation | Medium | 3.3.6 |
| 3.4.5 | Implement feature module routing | Medium | 3.4.1-3.4.4 |
| 3.4.6 | Generate feature module structure | High | 3.4.5 |

**Deliverables**:
- Angular feature modules
- Reusable component templates

### Milestone 3.5: Aspire AppHost Generation

**Objective**: Generate .NET Aspire orchestration project.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.5.1 | Implement Aspire resource model | Medium | 2.4.7 |
| 3.5.2 | Implement AppHost Program.cs generation | High | 3.5.1 |
| 3.5.3 | Implement resource provisioning code | High | 3.5.2 |
| 3.5.4 | Implement service references | Medium | 3.5.3 |
| 3.5.5 | Support multiple resource types (SQL, Redis, RabbitMQ) | High | 3.5.3 |
| 3.5.6 | AppHostGenerator orchestration | High | 3.5.1-3.5.5 |

**Deliverables**:
- `Endpoint.Aspire.Generation.AppHostGenerator`

### Milestone 3.6: Aspire ServiceDefaults

**Objective**: Generate ServiceDefaults project.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.6.1 | Implement Extensions.cs generation | Medium | 3.5.6 |
| 3.6.2 | Implement OpenTelemetry configuration | Medium | 3.6.1 |
| 3.6.3 | Implement health checks configuration | Medium | 3.6.1 |
| 3.6.4 | Implement service discovery configuration | Medium | 3.6.1 |
| 3.6.5 | ServiceDefaultsGenerator orchestration | Medium | 3.6.1-3.6.4 |

**Deliverables**:
- `Endpoint.Aspire.Generation.ServiceDefaultsGenerator`

### Milestone 3.7: Microservices Architecture

**Objective**: Support microservices solution generation.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 3.7.1 | Implement bounded context to microservice mapping | High | 1.3.5 |
| 3.7.2 | Implement shared kernel generation | Medium | 3.7.1 |
| 3.7.3 | Implement message contracts generation | High | 3.7.2 |
| 3.7.4 | Implement message publisher generation | High | 3.7.3 |
| 3.7.5 | Implement message consumer generation | High | 3.7.3 |
| 3.7.6 | Implement API gateway configuration | Medium | 3.7.1 |
| 3.7.7 | MicroservicesSolutionGenerator orchestration | High | 3.7.1-3.7.6 |

**Deliverables**:
- `Endpoint.PlantUml.Generation.MicroservicesSolutionGenerator`

---

## Phase 4: Production Readiness

### Milestone 4.1: CLI Command Integration

**Objective**: Integrate all generators into Endpoint.cli command.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 4.1.1 | Implement CreateSolutionFromPlantUmlRequest | Medium | 3.7.7 |
| 4.1.2 | Implement RequestHandler with full pipeline | High | 4.1.1 |
| 4.1.3 | Implement configuration file loading | Medium | 4.1.2 |
| 4.1.4 | Implement dry-run mode | Medium | 4.1.2 |
| 4.1.5 | Implement verbose logging | Low | 4.1.2 |

**Deliverables**:
- Complete CLI command implementation

### Milestone 4.2: Testing

**Objective**: Comprehensive test coverage.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 4.2.1 | Unit tests for all generators | High | All milestones |
| 4.2.2 | Integration tests for end-to-end generation | High | 4.1.5 |
| 4.2.3 | Snapshot tests for generated code | High | 4.2.2 |
| 4.2.4 | Sample PlantUML test files | Medium | 4.2.1 |
| 4.2.5 | Build verification tests | Medium | 4.2.2 |

**Deliverables**:
- Comprehensive test suite

### Milestone 4.3: Documentation

**Objective**: User and developer documentation.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 4.3.1 | User guide with examples | Medium | 4.2.5 |
| 4.3.2 | PlantUML syntax reference | Medium | 1.2.7 |
| 4.3.3 | Configuration reference | Medium | 4.1.3 |
| 4.3.4 | Template customization guide | Medium | 2.1.7 |
| 4.3.5 | API documentation | Medium | 4.1.5 |

**Deliverables**:
- Complete documentation set

### Milestone 4.4: Polish & Optimization

**Objective**: Final polish and performance optimization.

**Tasks**:
| Task | Description | Complexity | Dependencies |
|------|-------------|------------|--------------|
| 4.4.1 | Error messages and diagnostics improvement | Medium | 4.2.5 |
| 4.4.2 | Generation performance optimization | Medium | 4.2.5 |
| 4.4.3 | Code formatting integration | Low | 4.2.5 |
| 4.4.4 | Progress reporting | Low | 4.4.1 |
| 4.4.5 | Final review and bug fixes | Medium | 4.4.1-4.4.4 |

**Deliverables**:
- Production-ready feature

---

## Dependency Graph

```
1.1 Lexer ─────────────┐
                       ├──► 1.2 Parser ─────► 1.3 Semantic ─────┐
                       │                                         │
                       │                                         ▼
2.1 Templates ◄────────┼──────────────────────────────────► 2.2 Domain
                       │                                         │
                       │                                         ▼
                       │                                    2.3 Application
                       │                                         │
                       │                                         ▼
                       │                                    2.4 Infrastructure
                       │                                         │
                       │                                         ▼
                       │                                    2.5 Presentation
                       │                                         │
                       ▼                                         ▼
3.1 Type Mapping ──► 3.2 Angular ──► 3.3 NgRx ──► 3.4 Components
                                                         │
3.5 AppHost ◄───────────────────────────────────────────┼
       │                                                 │
       ▼                                                 ▼
3.6 ServiceDefaults ◄──────────────────────────── 3.7 Microservices
                                                         │
                                                         ▼
                                                   4.1 CLI Integration
                                                         │
                                                         ▼
                                              4.2 Testing ──► 4.3 Docs ──► 4.4 Polish
```

---

## Risk Assessment

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| PlantUML syntax complexity | High | Medium | Start with subset, expand iteratively |
| Angular version changes | Medium | Low | Use stable APIs, abstract version specifics |
| Aspire API changes | Medium | Medium | Follow Aspire releases, maintain compatibility |
| Template maintenance | Medium | Medium | Well-structured, documented templates |
| Performance with large diagrams | Medium | Low | Implement streaming, caching strategies |

---

## Success Criteria

1. **Functional**: Generate complete, buildable solutions from PlantUML
2. **Quality**: Generated code follows best practices and patterns
3. **Performance**: Handle diagrams with 100+ classes efficiently
4. **Usability**: Clear error messages and helpful diagnostics
5. **Documentation**: Comprehensive user and developer documentation
6. **Testing**: >80% code coverage with unit and integration tests
