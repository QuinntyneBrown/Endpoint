# Endpoint

A powerful template-based design-time code generator for .NET applications. Endpoint generates C# code, complete solutions, and frontend projects using a sophisticated template system that supports complex logic and transformations.

## Features

- **Multi-Framework Support**: Generate code for .NET 10+ applications
- **Domain-Driven Design**: Built-in support for DDD patterns including aggregates, entities, value objects, and bounded contexts
- **Physical Topologies**: Pre-configured templates for common application patterns
  - Modern Web App Pattern
  - Microservices Pattern
  - Worker Services
- **PlantUML Integration**: Generate complete solutions from PlantUML class diagrams and sequence diagrams
- **Frontend Integration**: Generate Angular projects and components
- **OpenAPI Support**: Generate OpenAPI specifications from existing .NET solutions
- **CLI-First Design**: Comprehensive command-line interface for all operations

## Give a Star!

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Requirements

- .NET 10 SDK or later
- Visual Studio 2025 (Version 18.0 or later) or VS Code
- Node.js (for frontend project generation)

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
├── Endpoint/                       # Core generation engine
├── Endpoint.Cli/                   # Main CLI application
├── Endpoint.DotNet/                # .NET-specific generators
├── Endpoint.Angular/               # Angular generators
├── Endpoint.DomainDrivenDesign/    # DDD patterns and templates
├── Endpoint.ModernWebAppPattern/   # Modern web app pattern implementation
└── Endpoint.Testing/               # Test generation utilities

tests/
├── Endpoint.UnitTests/
├── Endpoint.Cli.UnitTests/
├── Endpoint.DotNet.UnitTests/
├── Endpoint.Angular.UnitTests/
├── Endpoint.React.UnitTests/
├── Endpoint.DomainDrivenDesign.UnitTests/
├── Endpoint.MinimalApi.UnitTests/
├── Endpoint.ModernWebAppPattern.UnitTests/
├── Endpoint.ModernWebAppPlatform.Core.Tests/
└── Endpoint.Testing.UnitTests/

playground/
├── DddSolution/                  # DDD example workspace
├── EventDrivenMicroservices/     # Event-driven microservices example
├── FullStackSolution/            # Full-stack example workspace
├── Sample/                       # Sample project workspace
├── SolutionCreateFromSequence/   # Sequence diagram-based generation example
└── SolutionFromPlantuml/         # PlantUML-based generation example
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
endpoint class-from-plantuml-create -s <SourcePath>

# Validate PlantUML solution
endpoint solution-plantuml-validate -s <SourcePath>

# Create image from PlantUML
endpoint puml-image-create -s <SourceFile>
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

### Utility Commands

```bash
# Add copyright headers to files
endpoint copyright-add

# Create an .editorconfig file
endpoint editorconfig-create

# Create a README file
endpoint readme-create -n <Name>
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
A: Endpoint currently targets .NET 10 and later versions.

### Q: Can I generate solutions from PlantUML diagrams?
A: Yes! Endpoint supports both PlantUML class diagrams and sequence diagrams for solution generation using the `solution-create-from-plantuml` and `solution-create-from-sequence` commands.

### Q: Does Endpoint modify existing code?
A: Endpoint can both generate new code and update existing files. Always use version control when working with code generators.

### Q: What frontend frameworks are supported?
A: Currently supports Angular project and component generation.

### Q: Can I use Endpoint in CI/CD pipelines?
A: Yes, Endpoint is designed to work in automated environments and can be integrated into your CI/CD workflows.

### Q: How do I extend Endpoint with custom generators?
A: You can create custom generators by implementing the core generator abstractions in the `Endpoint` library and registering them with the CLI.
