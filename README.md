# Endpoint

A powerful template-based design-time code generator for .NET applications. Endpoint generates C# code, complete solutions, and frontend projects using a sophisticated template system that supports complex logic and transformations.

## Features

- **Multi-Framework Support**: Generate code for .NET 9.0+ applications
- **Domain-Driven Design**: Built-in support for DDD patterns including aggregates, entities, value objects, and bounded contexts
- **Physical Topologies**: Pre-configured templates for common application patterns
  - Modern Web App Pattern
  - Event-Driven Microservices Pattern
  - Worker Services
- **PlantUML Integration**: Generate complete solutions from PlantUML class diagrams and sequence diagrams
- **Frontend Integration**: Generate Angular, React, Lit, and TypeScript projects
- **Testing Support**: Built-in support for unit tests, SpecFlow, and Playwright E2E tests
- **Messaging Patterns**: Redis Pub/Sub, Service Bus, UDP messaging support
- **OpenAPI Support**: Generate OpenAPI specifications from existing .NET solutions
- **SignalR Support**: Generate SignalR hubs and clients for real-time applications
- **Static Analysis**: Built-in SonarQube-based static analysis with git diff comparison for C# and TypeScript
- **CLI-First Design**: Comprehensive command-line interface with 100+ commands

## Give a Star!

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Requirements

- .NET 9.0 SDK or later
- Visual Studio 2022+ or VS Code
- Node.js 18+ (for frontend project generation)
- Java Runtime (for PlantUML diagram generation)

## Get Started with Endpoint

### Installation

```bash
dotnet tool install -g Endpoint.Cli
```

### Quick Start

1. Create a DDD application with a bounded context:
```bash
endpoint ddd-app-create -n MyProduct -b Orders -a Order -p "Title:string,Quantity:int"
```

2. Or create a solution from a PlantUML sequence diagram:
```bash
endpoint solution-create-from-sequence -n MyMicroservices
```

3. Add entities to your project:
```bash
endpoint entity-create -n Customer -p "CustomerId:Guid,Name:string,Email:string"
```

4. Generate a controller for your entity:
```bash
endpoint controller-create -n CustomersController -e Customer
```

## How Does Endpoint Work?

Endpoint uses a template-based code generation system that:

1. **Analyzes** your project structure and configuration
2. **Processes** templates with your specified parameters
3. **Generates** code files using intelligent transformation logic
4. **Integrates** generated code into your existing solution

The generator supports:
- Complex conditional logic in templates
- Multi-file generation from single templates
- Incremental updates to existing code
- Custom template creation and extension

## Project Structure

```
src/
├── Endpoint/                         # Core generation engine and abstractions
├── Endpoint.Engineering.Cli/         # Main CLI application (100+ commands)
├── Endpoint.DotNet/                  # .NET-specific generators and services
├── Endpoint.Angular/                 # Angular project and component generators
├── Endpoint.DomainDrivenDesign/      # Domain-driven design patterns
├── Endpoint.Engineering/             # Engineering patterns and templates
├── Endpoint.ModernWebAppPattern/     # Modern web app pattern implementation
└── Endpoint.Testing/                 # Testing utilities and generators

tests/
├── Endpoint.UnitTests/
├── Endpoint.Cli.UnitTests/
├── Endpoint.Engineering.Cli.UnitTests/
├── Endpoint.DotNet.UnitTests/
├── Endpoint.Angular.UnitTests/
├── Endpoint.DomainDrivenDesign.UnitTests/
├── Endpoint.Engineering.UnitTests/
├── Endpoint.ModernWebAppPattern.UnitTests/
└── Endpoint.Testing.UnitTests/

playground/
├── DddSolution/                      # DDD example workspace
├── EventDrivenMicroservices/         # Event-driven microservices example
├── SolutionCreateFromSequence/       # Sequence diagram-based generation
├── SolutionFromPlantuml/             # PlantUML-based generation example
├── MessagingDemo/                    # Messaging patterns example
├── FullStackSolution/                # Full-stack application example
└── SharedLibraryDemo/                # Shared library generation examples
    ├── configs/                      # Example YAML configurations
    ├── SimpleLibrary/                # Minimal shared library example
    ├── IntermediateLibrary/          # Redis + contracts example
    └── CompleteLibrary/              # All protocols + CCSDS example
```

## CLI Commands

### Solution & Project Commands

```bash
# Create a new solution with a project
endpoint solution-create -n <SolutionName> -t <ProjectType>

# Create a DDD application with bounded contexts and aggregates
endpoint ddd-app-create -n <ProductName> -b <BoundedContext> -a <Aggregate> -p <Properties>

# Create a Modern Web App Pattern solution
endpoint mwa-create -n <Name> [-p <PathToJsonConfig>]

# Create solution from PlantUML class diagram
endpoint solution-create-from-plantuml -n <Name> -s <SourcePath>

# Create solution from PlantUML sequence diagram
endpoint solution-create-from-sequence -n <Name>

# Add a project to a solution
endpoint project-add -n <ProjectName> -t <ProjectType> [-f <FolderName>] [-r <References>]

# Remove a project from solution
endpoint project-remove -n <ProjectName>

# Move a project to a different folder
endpoint project-move -n <ProjectName> -f <FolderName>
```

### DDD & Entity Commands

```bash
# Create a domain entity with properties
endpoint entity-create -n <EntityName> -p <Properties>

# Create a value object
endpoint value-object-create -n <Name> -p <Properties>

# Create an aggregate
endpoint aggregate-create -n <AggregateName> -p <ProductName>

# Create a DbContext from aggregate model
endpoint db-context-create-from-aggregate-model -n <Name>

# Create a DbContext
endpoint db-context-create -n <Name> [-e <Entities>]
```

### Code Generation Commands

```bash
# Create a class
endpoint class-create -n <ClassName> [-p <Properties>]

# Create an interface
endpoint interface-create -n <InterfaceName>

# Create a class and interface pair
endpoint class-and-interface-create -n <Name>

# Create a controller
endpoint controller-create -n <ControllerName> -e <EntityName>

# Create request handlers
endpoint request-handlers-create -n <Name> -e <EntityName>

# Create a worker service
endpoint worker-create -n <Name>

# Create an enum
endpoint enum-create -n <EnumName> -t <Type>

# Create a record
endpoint record-create -n <RecordName> -p <Properties>
```

### PlantUML Commands

```bash
# Create classes from PlantUML
endpoint class-from-plant-uml-create -s <SourcePath>

# Validate PlantUML solution
endpoint solution-plantuml-validate -s <SourcePath>

# Create image from PlantUML
endpoint puml-image-create -s <SourceFile>

# Create image from Draw.io
endpoint drawio-image-create -s <SourceFile>
```

### Frontend Commands

```bash
# Create React application
endpoint react-app-create -n <Name>

# Create Lit workspace
endpoint lit-workspace-create -n <Name>

# Create TypeScript standalone project
endpoint ts-project-create -n <Name>

# Create TypeScript file
endpoint ts-file-create -n <FileName>

# Create SignalR hub client
endpoint signalr-hub-client-create -n <Name>

# Add SignalR support
endpoint signalr-add
```

### Messaging Commands

```bash
# Add messaging infrastructure
endpoint messaging-add -n <Name>

# Create message
endpoint message-create -n <MessageName>

# Create MessagePack message
endpoint message-pack-message-create -n <Name>

# Create message handler
endpoint message-handler-create -n <Name>

# Create service bus message consumer
endpoint service-bus-message-consumer-create -n <Name>

# Add message producer project
endpoint message-producer-project-add -n <Name>

# Add UDP service bus project
endpoint udp-service-bus-project-add -n <Name>

# Create UDP client factory interface
endpoint udp-client-factory-interface-create -n <Name>
```

### Shared Library Commands

```bash
# Create a complete shared library from YAML configuration
endpoint shared-library-create -c <ConfigPath> [-o <OutputPath>] [--dry-run]

# Preview what would be generated
endpoint shared-library-create -c ./shared-library.yaml --dry-run

# Generate with specific protocols only
endpoint shared-library-create -c ./config.yaml --protocols redis,ccsds

# Generate with specific serializers
endpoint shared-library-create -c ./config.yaml --serializers messagepack,json
```

The shared-library-create command generates a complete shared library solution including:
- **Messaging Abstractions**: IEvent, IEventBus, IMessageSerializer interfaces
- **Domain Primitives**: Strongly-typed IDs, Value Objects, Result pattern
- **Contracts**: Service events and commands with MessagePack serialization
- **Protocol Implementations**: Redis, UDP Multicast, Azure Service Bus, CCSDS
- **CCSDS Support**: Bit-level packing/unpacking for space packet protocols

See the [Shared Library User Guide](docs/shared-library-guide.md) for detailed configuration options.

### Microservices Commands

```bash
# Create event-driven microservices solution
endpoint event-driven-microservices-create -n <Name>

# Add microservice
endpoint microservice-add -n <Name>

# Add predefined microservice
endpoint predefined-microservice-add -t <Type>
```

### Testing Commands

```bash
# Create unit test
endpoint unit-test-create -n <TestName>

# Create test
endpoint test-create -n <TestName>

# Create test header
endpoint test-header-create

# Create benchmark
endpoint benchmark-create -n <Name>

# Add SpecFlow project
endpoint spec-flow-project-add -n <Name>

# Create SpecFlow feature
endpoint spec-flow-feature-create -n <Name>

# Create SpecFlow hook
endpoint spec-flow-hook-create -n <Name>

# Add Playwright project
endpoint playwright-project-add -n <Name>

# Create Playwright test
endpoint playwright-create -n <Name>

# Remove test projects
endpoint test-projects-remove
```

### OpenAPI Commands

```bash
# Generate OpenAPI specification from .NET solution
endpoint open-api -s <SolutionPath> [-o <OutputPath>]
```

### Git Commands

```bash
# Create a git repository
endpoint git-create

# Create a .gitignore file
endpoint gitignore-create
```

### Static Analysis Commands

```bash
# Run static analysis on the current codebase
endpoint static-analysis

# Run static analysis comparing current branch against master (only analyze changed code)
endpoint static-analysis --git-compare

# Compare against a specific base branch
endpoint static-analysis --git-compare --base-branch main

# Generate a unique markdown report file
endpoint static-analysis --git-compare --report

# Specify output directory for the report
endpoint static-analysis --git-compare --output ./reports

# Use custom SonarQube rules file
endpoint static-analysis --git-compare --sonar-rules ./custom-rules.md

# Output results in JSON format
endpoint static-analysis --git-compare --json

# Show all issues including code smells (verbose mode)
endpoint static-analysis --git-compare --verbose

# Fail on warnings (useful for CI/CD)
endpoint static-analysis --git-compare --fail-on-warning
```

The static-analysis command performs code quality checks based on SonarQube rules defined in `docs/sonar-qube-rules.md`. When used with `--git-compare`, it:
- Compares the current branch against the base branch (master/main)
- Only analyzes lines that were added or modified
- Checks for vulnerabilities, bugs, security hotspots, and code smells
- Generates a unique, human-readable report with timestamp and GUID

Supported languages: C# (.cs) and TypeScript (.ts, .tsx)

### Utility Commands

```bash
# Add copyright headers to files
endpoint copyright-add

# Create an .editorconfig file
endpoint editor-config-create

# Create a README file
endpoint readme-create -n <Name>

# Add package reference
endpoint package-add -n <PackageName>

# Add project reference
endpoint reference-add -n <ProjectName>

# Replace text in files
endpoint replace -o <OldText> -n <NewText>

# Create namespace
endpoint namespace-create -n <Namespace>

# Reset namespace
endpoint namespace-reset

# Unnest namespace
endpoint namespace-unnest

# Create usings file
endpoint usings-create

# Move file
endpoint file-move -s <Source> -d <Destination>

# Rename file
endpoint file-rename -o <OldName> -n <NewName>

# Get full path
endpoint get-full-path -p <Path>
```

### Advanced Commands

```bash
# Create WebSocket application
endpoint ws-app-create -n <Name>

# Create syntax generation strategy
endpoint syntax-generation-strategy-create -n <Name>

# Create public API
endpoint public-api-create

# Create building block
endpoint building-block-create -n <Name>

# Create user-defined type
endpoint udt-create -n <Name>

# Create value type
endpoint value-type-create -n <Name>

# Create query
endpoint query-create -n <Name>

# Create response
endpoint response-create -n <Name>

# Create event
endpoint event-add -n <EventName>

# Create service
endpoint service-create -n <ServiceName>

# Add migration
endpoint migration-add -n <MigrationName>

# Add DbContext
endpoint db-context-add -n <Name>

# Create configure services
endpoint configure-services-create

# Create specification
endpoint spec-create -n <Name>

# Unnest class
endpoint class-unnest -n <ClassName>

# Nest component
endpoint component-nest -n <ComponentName>

# Embed project
endpoint project-embed -n <ProjectName>

# Parse code
endpoint code-parse -s <SourcePath>

# Parse HTML
endpoint html-parse -s <SourcePath>

# Create HTTP project
endpoint http-project -n <Name>

# Remove Mediator
endpoint remove-mediator

# Add generate documentation file
endpoint generate-documentation-file-add

# Create pull request
endpoint pr -t <Title> -d <Description>
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## FAQ

### Q: What versions of .NET does Endpoint support?
A: Endpoint currently targets .NET 9.0 and later versions.

### Q: Can I generate solutions from PlantUML diagrams?
A: Yes! Endpoint supports both PlantUML class diagrams and sequence diagrams for solution generation using the `solution-create-from-plant-uml` and `solution-create-from-sequence` commands.

### Q: Does Endpoint modify existing code?
A: Endpoint can both generate new code and update existing files. Always use version control when working with code generators.

### Q: What frontend frameworks are supported?
A: Endpoint supports Angular, React, Lit, and standalone TypeScript projects, including component generation and project scaffolding.

### Q: Can I use Endpoint in CI/CD pipelines?
A: Yes, Endpoint is designed to work in automated environments and can be integrated into your CI/CD workflows.

### Q: How do I extend Endpoint with custom generators?
A: You can create custom generators by implementing the core generator abstractions in the `Endpoint` library and registering them with the CLI.
