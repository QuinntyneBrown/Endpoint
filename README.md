# Endpoint

A powerful template-based design-time code generator for .NET applications. Endpoint generates C# code, complete solutions, and frontend projects using a sophisticated template system that supports complex logic and transformations.

## Features

- **Multi-Framework Support**: Generate code for .NET 10+ applications
- **Domain-Driven Design**: Built-in support for DDD patterns and architectures
- **Physical Topologies**: Pre-configured templates for common application patterns
  - Modern Web App Pattern
  - Minimal API Pattern
- **Frontend Integration**: Generate Angular, React, and Lit components
- **Testing Support**: Automated test generation and scaffolding
- **Reactive Extensions**: Built-in Rx support for reactive programming patterns
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

1. Create a new project using a template:
```bash
endpoint create --template minimal-api --name MyProject
```

2. Generate domain entities:
```bash
endpoint generate entity --name Customer --domain Sales
```

3. Generate a complete CRUD API:
```bash
endpoint generate crud --entity Customer --endpoints all
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
├── Endpoint.Cli/              # Main CLI application
├── Endpoint.Core/             # Core generation engine
├── Endpoint.DotNet/           # .NET-specific generators
├── Endpoint.Angular/          # Angular generators
├── Endpoint.React/            # React generators
├── Endpoint.Lit/              # Lit component generators
├── Endpoint.Rx/               # Reactive extensions support
├── Domains/
│   └── DomainDrivenDesign/    # DDD patterns and templates
├── PhysicalTopologies/
│   ├── MinimalApi/            # Minimal API templates
│   └── ModernWebAppPattern/   # Modern web app templates
└── Testing/                   # Test generation utilities
```

## CLI Commands

### Code Generation Commands

```bash
# Generate a domain entity
endpoint generate entity --name <EntityName> --domain <DomainName>

# Generate a value object
endpoint generate valueobject --name <Name> --domain <DomainName>

# Generate an aggregate root
endpoint generate aggregate --name <Name> --domain <DomainName>

# Generate a repository
endpoint generate repository --entity <EntityName>

# Generate a service
endpoint generate service --name <ServiceName>
```

### File Generation Commands

```bash
# Generate a single file from template
endpoint file create --template <TemplateName> --output <FilePath>

# List available templates
endpoint file templates

# Generate configuration files
endpoint file config --type <ConfigType>
```

### ASP.NET Core Solution / Project Generation Commands

```bash
# Create a new solution
endpoint solution create --name <SolutionName> --pattern <PatternType>

# Add a new project to solution
endpoint project add --type <ProjectType> --name <ProjectName>

# Generate minimal API endpoints
endpoint api minimal --resource <ResourceName>

# Generate controllers
endpoint api controller --name <ControllerName>

# Generate middleware
endpoint generate middleware --name <MiddlewareName>
```

### Angular Workspace / Project Generation Commands

```bash
# Create Angular workspace
endpoint angular workspace --name <WorkspaceName>

# Generate Angular component
endpoint angular component --name <ComponentName>

# Generate Angular service
endpoint angular service --name <ServiceName>

# Generate Angular module
endpoint angular module --name <ModuleName>
```

### React Project Generation Commands

```bash
# Create React application
endpoint react create --name <AppName>

# Generate React component
endpoint react component --name <ComponentName>

# Generate React hook
endpoint react hook --name <HookName>
```

### Fullstack Generation Commands

```bash
# Generate fullstack feature (API + Frontend)
endpoint fullstack feature --name <FeatureName> --frontend <angular|react>

# Generate complete CRUD (Backend + Frontend)
endpoint fullstack crud --entity <EntityName> --frontend <angular|react>
```

### Git Commands

```bash
# Initialize repository with standard structure
endpoint git init

# Create feature branch with standard naming
endpoint git feature --name <FeatureName>

# Generate .gitignore for project type
endpoint git ignore --type <ProjectType>
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

### Q: Can I create custom templates?
A: Yes! Endpoint supports custom templates. Place your templates in the `templates` directory and reference them using the `--template` parameter.

### Q: Does Endpoint modify existing code?
A: Endpoint can both generate new code and update existing files when using incremental generation modes. Always use version control when working with code generators.

### Q: What frontend frameworks are supported?
A: Currently supports Angular, React, and Lit components.

### Q: Can I use Endpoint in CI/CD pipelines?
A: Yes, Endpoint is designed to work in automated environments and can be integrated into your CI/CD workflows.

### Q: How do I extend Endpoint with custom generators?
A: You can create custom generators by implementing the core generator abstractions in the `Endpoint.Core` library and registering them with the CLI.
