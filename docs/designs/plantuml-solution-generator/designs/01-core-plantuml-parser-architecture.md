# Design 1: Core PlantUML Parser & Solution Generator Architecture

## Overview

This design defines the core architecture for parsing PlantUML diagrams and transforming them into comprehensive .NET solution structures. The parser serves as the foundation for all subsequent generation capabilities.

## Goals

1. Parse PlantUML class diagrams into an Abstract Syntax Tree (AST)
2. Support all standard PlantUML class diagram elements
3. Provide extensible parsing strategies for different diagram types
4. Enable semantic validation of parsed models
5. Create a unified model that can drive multiple generation targets

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        PlantUML Input Layer                              │
├─────────────────────────────────────────────────────────────────────────┤
│  .puml file  │  Inline String  │  URL Reference  │  Multi-file Project  │
└──────────────┴─────────────────┴─────────────────┴──────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Lexical Analysis                                 │
├─────────────────────────────────────────────────────────────────────────┤
│  PlantUmlLexer  →  Token Stream  →  PlantUmlTokenizer                   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        Syntactic Analysis                                │
├─────────────────────────────────────────────────────────────────────────┤
│  PlantUmlParser  →  Parse Tree  →  PlantUmlSyntaxTree                   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                       Semantic Analysis                                  │
├─────────────────────────────────────────────────────────────────────────┤
│  TypeResolver  │  RelationshipAnalyzer  │  NamespaceResolver            │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    Solution Model Builder                                │
├─────────────────────────────────────────────────────────────────────────┤
│  SolutionModelFactory  →  ProjectModelFactory  →  EntityModelFactory    │
└─────────────────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. PlantUML Lexer

The lexer tokenizes raw PlantUML text into a stream of tokens.

```csharp
namespace Endpoint.PlantUml.Parsing
{
    public interface IPlantUmlLexer
    {
        IEnumerable<PlantUmlToken> Tokenize(string input);
        IEnumerable<PlantUmlToken> TokenizeFile(string filePath);
    }

    public class PlantUmlToken
    {
        public PlantUmlTokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Position { get; set; }
    }

    public enum PlantUmlTokenType
    {
        // Keywords
        StartUml,
        EndUml,
        Class,
        Interface,
        Abstract,
        Enum,
        Package,
        Namespace,

        // Modifiers
        Public,
        Private,
        Protected,
        Static,

        // Relationships
        Inheritance,      // <|--
        Composition,      // *--
        Aggregation,      // o--
        Association,      // --
        Dependency,       // ..>
        Realization,      // <|..

        // Structural
        OpenBrace,
        CloseBrace,
        Colon,
        Comma,

        // Literals
        Identifier,
        StringLiteral,
        Stereotype,       // <<stereotype>>
        Note,

        // Special
        Whitespace,
        NewLine,
        Comment,
        EndOfFile
    }
}
```

### 2. PlantUML Parser

The parser builds an Abstract Syntax Tree from the token stream.

```csharp
namespace Endpoint.PlantUml.Parsing
{
    public interface IPlantUmlParser
    {
        PlantUmlDocument Parse(IEnumerable<PlantUmlToken> tokens);
        PlantUmlDocument ParseFile(string filePath);
        PlantUmlDocument ParseString(string content);
    }

    public class PlantUmlDocument
    {
        public string Title { get; set; }
        public List<PlantUmlNamespace> Namespaces { get; set; } = new();
        public List<PlantUmlClass> Classes { get; set; } = new();
        public List<PlantUmlInterface> Interfaces { get; set; } = new();
        public List<PlantUmlEnum> Enums { get; set; } = new();
        public List<PlantUmlRelationship> Relationships { get; set; } = new();
        public List<PlantUmlNote> Notes { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
```

### 3. PlantUML AST Models

```csharp
namespace Endpoint.PlantUml.Models
{
    public abstract class PlantUmlElement
    {
        public string Name { get; set; }
        public List<string> Stereotypes { get; set; } = new();
        public string Namespace { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    public class PlantUmlClass : PlantUmlElement
    {
        public bool IsAbstract { get; set; }
        public List<PlantUmlField> Fields { get; set; } = new();
        public List<PlantUmlMethod> Methods { get; set; } = new();
        public List<PlantUmlProperty> Properties { get; set; } = new();
    }

    public class PlantUmlInterface : PlantUmlElement
    {
        public List<PlantUmlMethod> Methods { get; set; } = new();
        public List<PlantUmlProperty> Properties { get; set; } = new();
    }

    public class PlantUmlEnum : PlantUmlElement
    {
        public List<string> Values { get; set; } = new();
    }

    public class PlantUmlField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public AccessModifier Accessibility { get; set; }
        public bool IsStatic { get; set; }
        public string DefaultValue { get; set; }
    }

    public class PlantUmlMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public AccessModifier Accessibility { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public List<PlantUmlParameter> Parameters { get; set; } = new();
    }

    public class PlantUmlProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public AccessModifier Accessibility { get; set; }
        public bool HasGetter { get; set; } = true;
        public bool HasSetter { get; set; } = true;
    }

    public class PlantUmlRelationship
    {
        public string SourceClass { get; set; }
        public string TargetClass { get; set; }
        public RelationshipType Type { get; set; }
        public string SourceCardinality { get; set; }
        public string TargetCardinality { get; set; }
        public string Label { get; set; }
    }

    public enum RelationshipType
    {
        Inheritance,
        Implementation,
        Composition,
        Aggregation,
        Association,
        Dependency
    }

    public enum AccessModifier
    {
        Public,      // +
        Private,     // -
        Protected,   // #
        Package      // ~
    }
}
```

### 4. Semantic Analyzer

```csharp
namespace Endpoint.PlantUml.Analysis
{
    public interface ISemanticAnalyzer
    {
        SemanticModel Analyze(PlantUmlDocument document);
        IEnumerable<DiagnosticMessage> Validate(PlantUmlDocument document);
    }

    public class SemanticModel
    {
        public Dictionary<string, TypeInfo> Types { get; set; } = new();
        public List<InheritanceChain> InheritanceChains { get; set; } = new();
        public List<BoundedContext> BoundedContexts { get; set; } = new();
        public List<AggregateRoot> AggregateRoots { get; set; } = new();
        public DependencyGraph DependencyGraph { get; set; }
    }

    public class TypeInfo
    {
        public string FullyQualifiedName { get; set; }
        public PlantUmlElement Element { get; set; }
        public List<string> BaseTypes { get; set; } = new();
        public List<string> ImplementedInterfaces { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
    }
}
```

### 5. Solution Model Factory

Transforms semantic model into solution structure.

```csharp
namespace Endpoint.PlantUml.Generation
{
    public interface ISolutionModelFactory
    {
        SolutionModel CreateSolution(SemanticModel semanticModel, SolutionOptions options);
    }

    public class SolutionOptions
    {
        public string SolutionName { get; set; }
        public string OutputDirectory { get; set; }
        public SolutionTemplate Template { get; set; }
        public bool IncludeAngularFrontend { get; set; }
        public bool UseAspireOrchestration { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; }
        public AuthenticationScheme AuthenticationScheme { get; set; }
        public List<string> AdditionalPackages { get; set; } = new();
    }

    public enum SolutionTemplate
    {
        CleanArchitecture,
        VerticalSlice,
        Modular,
        Microservices,
        MinimalApi
    }

    public enum DatabaseProvider
    {
        SqlServer,
        PostgreSql,
        MySql,
        Sqlite,
        CosmosDb,
        MongoDB
    }
}
```

## PlantUML Syntax Support

### Supported Elements

| Element | PlantUML Syntax | Example |
|---------|----------------|---------|
| Class | `class ClassName` | `class Order` |
| Abstract Class | `abstract class ClassName` | `abstract class Entity` |
| Interface | `interface IName` | `interface IRepository` |
| Enum | `enum EnumName` | `enum OrderStatus` |
| Package/Namespace | `package Name { }` | `package Domain { }` |
| Stereotype | `<<stereotype>>` | `class Order <<aggregate>>` |

### Supported Relationships

| Relationship | PlantUML Syntax | Meaning |
|--------------|----------------|---------|
| Inheritance | `Child <\|-- Parent` | Class extends |
| Implementation | `Class ..\|> Interface` | Implements interface |
| Composition | `Whole *-- Part` | Strong ownership |
| Aggregation | `Container o-- Item` | Weak ownership |
| Association | `ClassA -- ClassB` | General association |
| Dependency | `ClassA ..> ClassB` | Uses/depends on |

### Member Syntax

```plantuml
class Example {
    - privateField: string        ' Private field
    + publicProperty: int         ' Public property
    # protectedMethod(): void     ' Protected method
    ~ packageMethod(param: T): R  ' Package-private method
    {static} staticField: bool    ' Static member
    {abstract} abstractMethod()   ' Abstract method
}
```

## Stereotype-Driven Generation

Stereotypes drive architectural decisions:

| Stereotype | Generated Artifacts |
|------------|-------------------|
| `<<aggregate>>` | AggregateRoot base class, Repository interface |
| `<<entity>>` | Entity base class, EF configuration |
| `<<value-object>>` | Value object with equality |
| `<<service>>` | Service interface and implementation |
| `<<repository>>` | Repository pattern implementation |
| `<<command>>` | MediatR command and handler |
| `<<query>>` | MediatR query and handler |
| `<<dto>>` | Data transfer object |
| `<<event>>` | Domain event class |
| `<<api>>` | API controller |

## Command Integration

```csharp
namespace Endpoint.Cli.Commands
{
    [Verb("solution-from-plantuml", HelpText = "Create a complete .NET solution from PlantUML diagram")]
    public class SolutionFromPlantUmlRequest : IRequest
    {
        [Option('f', "file", Required = true, HelpText = "Path to PlantUML file")]
        public string FilePath { get; set; }

        [Option('n', "name", Required = true, HelpText = "Solution name")]
        public string SolutionName { get; set; }

        [Option('o', "output", HelpText = "Output directory")]
        public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

        [Option('t', "template", Default = "CleanArchitecture", HelpText = "Solution template")]
        public string Template { get; set; }

        [Option("angular", HelpText = "Include Angular frontend")]
        public bool IncludeAngular { get; set; }

        [Option("aspire", HelpText = "Use .NET Aspire orchestration")]
        public bool UseAspire { get; set; }

        [Option("database", Default = "SqlServer", HelpText = "Database provider")]
        public string DatabaseProvider { get; set; }
    }

    public class SolutionFromPlantUmlRequestHandler : IRequestHandler<SolutionFromPlantUmlRequest>
    {
        private readonly IPlantUmlParser _parser;
        private readonly ISemanticAnalyzer _analyzer;
        private readonly ISolutionModelFactory _solutionFactory;
        private readonly IArtifactGenerator _generator;

        public async Task Handle(SolutionFromPlantUmlRequest request, CancellationToken ct)
        {
            // 1. Parse PlantUML file
            var document = _parser.ParseFile(request.FilePath);

            // 2. Analyze semantics
            var semanticModel = _analyzer.Analyze(document);

            // 3. Validate
            var diagnostics = _analyzer.Validate(document);
            if (diagnostics.Any(d => d.Severity == Severity.Error))
                throw new ValidationException(diagnostics);

            // 4. Create solution model
            var options = MapRequestToOptions(request);
            var solutionModel = _solutionFactory.CreateSolution(semanticModel, options);

            // 5. Generate artifacts
            await _generator.GenerateAsync(solutionModel);
        }
    }
}
```

## Error Handling & Diagnostics

```csharp
public class DiagnosticMessage
{
    public string Code { get; set; }
    public string Message { get; set; }
    public Severity Severity { get; set; }
    public Location Location { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

public enum Severity
{
    Info,
    Warning,
    Error
}

// Example diagnostics:
// PUML001: Unknown type reference '{0}'
// PUML002: Circular inheritance detected: {0}
// PUML003: Missing stereotype for domain entity '{0}'
// PUML004: Invalid relationship cardinality
// PUML005: Duplicate class definition '{0}'
```

## File Structure

```
src/
├── Endpoint.PlantUml/
│   ├── Parsing/
│   │   ├── IPlantUmlLexer.cs
│   │   ├── PlantUmlLexer.cs
│   │   ├── IPlantUmlParser.cs
│   │   ├── PlantUmlParser.cs
│   │   └── PlantUmlTokenizer.cs
│   ├── Models/
│   │   ├── PlantUmlDocument.cs
│   │   ├── PlantUmlClass.cs
│   │   ├── PlantUmlInterface.cs
│   │   ├── PlantUmlEnum.cs
│   │   ├── PlantUmlRelationship.cs
│   │   └── PlantUmlElements.cs
│   ├── Analysis/
│   │   ├── ISemanticAnalyzer.cs
│   │   ├── SemanticAnalyzer.cs
│   │   ├── TypeResolver.cs
│   │   └── DependencyAnalyzer.cs
│   ├── Generation/
│   │   ├── ISolutionModelFactory.cs
│   │   ├── SolutionModelFactory.cs
│   │   └── SolutionOptions.cs
│   └── ConfigureServices.cs
```

## Testing Strategy

1. **Unit Tests**: Parser, lexer, semantic analyzer
2. **Integration Tests**: End-to-end PlantUML to solution generation
3. **Snapshot Tests**: Generated code output verification
4. **Sample PlantUML Files**: Comprehensive test cases

## Dependencies

- `Sprache` or custom lexer for parsing
- Existing `Endpoint.DotNet` for code generation
- `FluentValidation` for semantic validation
