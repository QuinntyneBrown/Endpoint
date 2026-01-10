# PlantUML Microservices Design Documentation

This folder contains 5 comprehensive designs for implementing PlantUML-based microservices generation in Endpoint.cli.

## Design Overview

| Design | Focus | Key Features |
|--------|-------|--------------|
| [Design 1: Basic Architecture](./design-1-basic-architecture/) | Clean Architecture | Simple parsing, layered generation, extensible |
| [Design 2: Event-Driven](./design-2-event-driven/) | Message-Based | Event bus, sagas, integration events |
| [Design 3: Aspire Integration](./design-3-aspire-integration/) | Cloud-Native | .NET Aspire, observability, service discovery |
| [Design 4: DDD-Focused](./design-4-ddd-focused/) | Domain-Driven Design | Bounded contexts, aggregates, context mappings |
| [Design 5: Full Stack](./design-5-full-stack/) | End-to-End | Backend + Frontend, API clients, shared types |

## Quick Comparison

### Architecture Style

| Design | Microservice Pattern | Communication |
|--------|---------------------|---------------|
| Design 1 | Clean Architecture | HTTP/REST |
| Design 2 | Event-Driven | Message Bus (RabbitMQ) |
| Design 3 | Cloud-Native (Aspire) | Service Discovery |
| Design 4 | DDD Bounded Contexts | Events + ACL |
| Design 5 | Full Stack | REST + Real-time |

### Generated Artifacts

| Design | Backend | Frontend | Infrastructure |
|--------|---------|----------|----------------|
| Design 1 | Minimal APIs | - | Docker Compose |
| Design 2 | + Event Handlers | - | + Message Broker |
| Design 3 | + Aspire AppHost | - | + Azure Bicep |
| Design 4 | + ACL, Domain Events | - | + Context Mapping |
| Design 5 | + OpenAPI | Angular/React/Lit | + E2E Tests |

### Complexity vs Capability

```
Capability
    ▲
    │                              ┌─────────┐
    │                        ┌─────│Design 5 │
    │                  ┌─────│     │Full Stack│
    │            ┌─────│Design 4   └─────────┘
    │      ┌─────│     │DDD       │
    │┌─────│Design 3   └──────────┘
    ││     │Aspire    │
    ││Design 2        │
    ││Event-Driven    │
    │└────────────────┘
    │Design 1
    │Basic
    └─────────────────────────────────────────► Complexity
```

## Viewing Diagrams

### PlantUML Diagrams

PlantUML diagrams (`.puml` files) can be viewed using:

1. **VS Code Extension**: Install "PlantUML" extension
2. **Online Viewer**: Visit [PlantUML Server](http://www.plantuml.com/plantuml/uml/)
3. **GitHub**: GitHub renders PlantUML in Markdown when properly embedded

### Draw.io Diagrams

Draw.io diagrams (`.drawio` files) can be viewed using:

1. **VS Code Extension**: Install "Draw.io Integration" extension
2. **Online Editor**: Visit [draw.io](https://app.diagrams.net/) and open the file
3. **Desktop App**: Download from [diagrams.net](https://www.diagrams.net/)

## Rendering Diagrams Locally

### Prerequisites

```bash
# Install PlantUML (requires Java)
brew install plantuml  # macOS
apt-get install plantuml  # Ubuntu/Debian

# Or download directly
curl -L -o plantuml.jar https://github.com/plantuml/plantuml/releases/latest/download/plantuml.jar
```

### Generate Images

```bash
# Render all PlantUML files
for file in docs/designs/*/*.puml; do
    java -jar plantuml.jar -tpng "$file"
done

# Or use endpoint CLI (when implemented)
endpoint puml-image-create --file ./architecture.puml --output ./architecture.png
```

## Recommended Approach

Based on project needs:

| Scenario | Recommended Design |
|----------|-------------------|
| Simple CRUD microservices | Design 1: Basic Architecture |
| Complex workflows with events | Design 2: Event-Driven |
| Azure deployment with monitoring | Design 3: Aspire Integration |
| Complex domain with multiple teams | Design 4: DDD-Focused |
| Full application with UI | Design 5: Full Stack |

## Implementation Priority

For the Endpoint.cli `create-from-plantuml` command, recommended implementation order:

1. **Phase 1**: Design 1 - Basic parsing and generation
2. **Phase 2**: Design 4 - Add DDD patterns (extends Phase 1)
3. **Phase 3**: Design 3 - Add Aspire integration
4. **Phase 4**: Design 2 - Add event-driven features
5. **Phase 5**: Design 5 - Add frontend generation

## File Structure

```
docs/designs/
├── README.md                           # This file
├── design-1-basic-architecture/
│   ├── README.md                       # Design documentation
│   ├── architecture.puml               # Architecture diagram
│   ├── workflow.drawio                 # Workflow diagram
│   └── example-input.puml              # Example PlantUML input
├── design-2-event-driven/
│   ├── README.md
│   ├── architecture.puml
│   ├── message-flow.drawio
│   └── example-input.puml
├── design-3-aspire-integration/
│   ├── README.md
│   ├── architecture.puml
│   ├── service-topology.drawio
│   └── example-input.puml
├── design-4-ddd-focused/
│   ├── README.md
│   ├── context-map.puml
│   ├── tactical-design.drawio
│   └── example-input.puml
└── design-5-full-stack/
    ├── README.md
    ├── architecture.puml
    ├── component-hierarchy.drawio
    └── example-input.puml
```

## Contributing

When adding or modifying designs:

1. Follow the existing file naming conventions
2. Include both PlantUML and Draw.io diagrams
3. Provide example input files
4. Update this README with any new designs
