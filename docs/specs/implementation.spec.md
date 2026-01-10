# Technical Implementation Specification

## Document Information

- **Version**: 1.0
- **Last Updated**: 2026-01-10
- **Status**: Active

## 1. Overview

This document defines the technical requirements, architectural principles, and acceptance criteria for the Endpoint code generation framework. It establishes standards for class naming, project organization, coding conventions, and the critical separation between artifact generation strategies and syntax generation strategies.

## 2. Core Architecture Principles

### 2.1 Separation of Concerns

The framework **MUST** maintain a clear separation between two distinct generation concerns:

1. **Syntax Generation** - Generates code text/syntax (string output)
2. **Artifact Generation** - Generates files, projects, solutions (filesystem operations)

**Rationale**: This separation ensures strategies are focused, composable, and testable. Syntax generation can be tested without file I/O, and artifact generation can leverage syntax generators without duplicating code generation logic.

### 2.2 Strategy Composition

Each generation strategy **MUST** generate exactly one artifact or one type of syntax element and **MUST** leverage other strategies via the appropriate generator interface to generate child syntax/artifacts.

**Prohibited Pattern**: Large monolithic generation strategies that build everything without reusing other strategies.

**Required Pattern**: Focused strategies that compose other strategies through dependency injection.

## 3. Project Organization

### 3.1 Project Structure

The solution **MUST** be organized into the following project categories:

#### Core Foundation
- **Endpoint** - Core abstractions, base classes, and services
  - Contains `ISyntaxGenerator`, `IArtifactGenerator`
  - Contains base strategy classes
  - Contains core model base classes (`SyntaxModel`, `ArtifactModel`)
  - Contains common services (file system, template engine)

#### Technology-Specific Implementations
- **Endpoint.DotNet** - .NET/C# code generation
- **Endpoint.Angular** - Angular code generation
- **Endpoint.React** - React code generation
- **Endpoint.Lit** - Lit web component generation
- Additional technology packages as needed

#### Application Layer
- **Endpoint.Cli** - Command-line interface application

### 3.2 Directory Structure

Each technology-specific project **MUST** follow this directory structure:

```
src/Endpoint.{Technology}/
├── Syntax/                          # Syntax generation strategies
│   ├── {Concept}/                   # Grouped by concept
│   │   ├── Strategies/              # Multiple strategies for same concept
│   │   │   └── {Concept}SyntaxGenerationStrategy.cs
│   │   └── {Concept}Model.cs       # Model at concept root
│   └── ...
├── Artifacts/                       # Artifact generation strategies
│   ├── {Concept}/
│   │   ├── Strategies/
│   │   │   └── {Concept}ArtifactGenerationStrategy.cs
│   │   └── {Concept}Model.cs
│   └── ...
├── Templates/                       # Template files (if using template engine)
│   └── {concept}.liquid
└── ConfigureServices.cs            # Dependency injection registration
```

**Requirements**:
- Strategies **MUST** be placed in `Strategies/` subdirectory when multiple strategies exist for a concept
- Models **SHOULD** be placed at the root of their concept folder
- Related strategies **MUST** be grouped under a common concept folder
- Template files **MUST** be placed in `Templates/` directory

### 3.3 Naming Structure Examples

**Good Examples**:
```
Syntax/Classes/Strategies/ClassSyntaxGenerationStrategy.cs
Syntax/Classes/ClassModel.cs
Syntax/Properties/Strategies/PropertySyntaxGenerationStrategy.cs
Syntax/Properties/Strategies/PropertiesSyntaxGenerationStrategy.cs
Artifacts/Files/Strategies/ClassCodeFileArtifactGenerationStrategy.cs
Artifacts/Projects/Strategies/ProjectGenerationStrategy.cs
```

## 4. Class Naming Conventions

### 4.1 Syntax Generation Strategy Classes

**Pattern**: `{Concept}SyntaxGenerationStrategy`

**Requirements**:
- **MUST** implement `ISyntaxGenerationStrategy<TModel>`
- **SHOULD** inherit from `SyntaxGenerationStrategyBase` (if using base functionality)
- **MUST** use noun representing the syntax element being generated
- **MUST** use singular form for single-element generators (e.g., `PropertySyntaxGenerationStrategy`)
- **MUST** use plural form for collection generators (e.g., `PropertiesSyntaxGenerationStrategy`)

**Examples**:
```csharp
// Good
ClassSyntaxGenerationStrategy
InterfaceSyntaxGenerationStrategy
MethodSyntaxGenerationStrategy
MethodsSyntaxGenerationStrategy
PropertySyntaxGenerationStrategy
PropertiesSyntaxGenerationStrategy
AttributeSyntaxGenerationStrategy
ConstructorSyntaxGenerationStrategy
TypeSyntaxGenerationStrategy

// Bad - Missing 'Syntax'
ClassGenerationStrategy          // Ambiguous - syntax or artifact?
PropertyStrategy                 // Too vague

// Bad - Not focused
ClassWithMethodsAndPropertiesSyntaxGenerationStrategy  // Too broad
```

### 4.2 Artifact Generation Strategy Classes

**Pattern**: `{Concept}ArtifactGenerationStrategy` or `{Concept}GenerationStrategy`

**Requirements**:
- **MUST** implement `IArtifactGenerationStrategy<TModel>`
- **SHOULD** inherit from `ArtifactGenerationStrategyBase` (if using base functionality)
- **MUST** use noun representing the artifact being generated
- **SHOULD** include `Artifact` in name for clarity, except for well-established concepts (File, Project, Solution)

**Examples**:
```csharp
// Good
ClassCodeFileArtifactGenerationStrategy
InterfaceCodeFileArtifactGenerationStrategy
FileGenerationStrategy              // Established concept
ProjectGenerationStrategy           // Established concept
SolutionGenerationStrategy          // Established concept
ControllerArtifactGenerationStrategy

// Bad - Missing artifact clarity
ClassFileStrategy                   // Too vague
CodeGenerator                       // Too generic
```

### 4.3 Model Classes

**Pattern**: `{Concept}Model`

**Requirements**:
- Syntax models **MUST** inherit from `SyntaxModel` or a derived class
- Artifact models **MUST** inherit from `ArtifactModel` or a derived class
- **MUST** use noun representing the concept being modeled
- **MUST** be immutable or use init-only properties where possible
- **MUST** include all data needed for generation

**Model Hierarchies**:
```csharp
// Syntax Model Hierarchy
SyntaxModel (base)
  └─> TypeDeclarationModel
      └─> InterfaceModel
          └─> ClassModel

// Artifact Model Hierarchy
ArtifactModel (base)
  └─> FileModel
      └─> CodeFileModel<T>
      └─> ContentFileModel
```

**Examples**:
```csharp
// Good
public class ClassModel : InterfaceModel
{
    public List<PropertyModel> Properties { get; init; }
    public List<MethodModel> Methods { get; init; }
    public List<FieldModel> Fields { get; init; }
    public List<ConstructorModel> Constructors { get; init; }
}

public class PropertyModel : SyntaxModel
{
    public string Name { get; init; }
    public string Type { get; init; }
    public AccessModifier AccessModifier { get; init; }
}

public class CodeFileModel<T> : FileModel where T : SyntaxModel
{
    public T Content { get; init; }
}
```

### 4.4 Interface Naming

**Patterns**:
- `ISyntaxGenerationStrategy<T>` - Syntax generation strategy interface
- `IArtifactGenerationStrategy<T>` - Artifact generation strategy interface
- `ISyntaxGenerator` - Orchestrator for syntax generation
- `IArtifactGenerator` - Orchestrator for artifact generation
- `I{ServiceName}` - Service interfaces

## 5. Coding Conventions

### 5.1 Strategy Implementation Requirements

#### Syntax Generation Strategy

**Required Interface**:
```csharp
public interface ISyntaxGenerationStrategy<T>
{
    Task<string> GenerateAsync(T model, CancellationToken cancellationToken = default);
    bool CanHandle(object target);
    int GetPriority();
}
```

**Implementation Requirements**:
1. **MUST** accept dependencies via constructor injection
2. **MUST** inject `ISyntaxGenerator` to compose other syntax strategies
3. **MUST** return generated code as a string
4. **MUST NOT** perform file I/O operations
5. **MUST** implement `CanHandle` to indicate which models it processes
6. **MUST** implement `GetPriority` for strategy selection (default: 1)
7. **SHOULD** be stateless and thread-safe

**Example**:
```csharp
public class ClassSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    private readonly ISyntaxGenerator _syntaxGenerator;
    private readonly INamingConventionConverter _namingConventionConverter;

    public ClassSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        INamingConventionConverter namingConventionConverter)
    {
        _syntaxGenerator = syntaxGenerator;
        _namingConventionConverter = namingConventionConverter;
    }

    public async Task<string> GenerateAsync(
        ClassModel model,
        CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();

        // Compose other syntax strategies
        builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.Namespace));
        builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.Usings));
        builder.Append(await _syntaxGenerator.GenerateAsync(model.AccessModifier));

        if (model.IsStatic)
            builder.Append("static ");

        builder.AppendLine($"class {model.Name}");
        builder.AppendLine("{");
        builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.Properties));
        builder.AppendLine(await _syntaxGenerator.GenerateAsync(model.Methods));
        builder.AppendLine("}");

        return builder.ToString();
    }

    public bool CanHandle(object target) => target is ClassModel;

    public int GetPriority() => 1; // Default priority
}
```

#### Artifact Generation Strategy

**Required Interface**:
```csharp
public interface IArtifactGenerationStrategy<T>
{
    Task GenerateAsync(T model, CancellationToken cancellationToken = default);
    bool CanHandle(object target);
    int GetPriority();
}
```

**Implementation Requirements**:
1. **MUST** accept dependencies via constructor injection
2. **MUST** inject `ISyntaxGenerator` when generating code text
3. **MUST** inject `IArtifactGenerator` when composing other artifact strategies
4. **MUST** perform file system operations (creating files, directories, projects)
5. **MUST NOT** duplicate syntax generation logic
6. **MUST** implement `CanHandle` to indicate which models it processes
7. **MUST** implement `GetPriority` for strategy selection (default: 1)
8. **SHOULD** be idempotent where possible

**Example**:
```csharp
public class ClassCodeFileArtifactGenerationStrategy
    : IArtifactGenerationStrategy<CodeFileModel<ClassModel>>
{
    private readonly ISyntaxGenerator _syntaxGenerator;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IFileSystem _fileSystem;

    public ClassCodeFileArtifactGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        IArtifactGenerator artifactGenerator,
        IFileSystem fileSystem)
    {
        _syntaxGenerator = syntaxGenerator;
        _artifactGenerator = artifactGenerator;
        _fileSystem = fileSystem;
    }

    public async Task GenerateAsync(
        CodeFileModel<ClassModel> model,
        CancellationToken cancellationToken = default)
    {
        // Use syntax generator to produce code text
        var classCode = await _syntaxGenerator.GenerateAsync(model.Content);

        // Create file model with generated code
        var fileModel = new FileModel
        {
            Directory = model.Directory,
            Name = $"{model.Content.Name}.cs",
            Content = classCode
        };

        // Delegate to file artifact generator
        await _artifactGenerator.GenerateAsync(fileModel, cancellationToken);
    }

    public bool CanHandle(object target)
        => target is CodeFileModel<ClassModel>;

    public int GetPriority() => 1;
}
```

### 5.2 Strategy Selection Mechanism

**Priority System**:
- **2 or higher**: Specialized strategies that should take precedence
- **1**: Default priority for standard strategies
- **0 or negative**: Fallback strategies when no better match exists

**Requirements**:
1. Strategies **MUST** implement `GetPriority()` method
2. When multiple strategies can handle the same target, highest priority wins
3. Priority **SHOULD** be used sparingly - prefer specific types over priority
4. `CanHandle` **MUST** return true only for types the strategy can process

**Examples**:
```csharp
// Specialized strategy for interface methods
public class InterfaceMethodSyntaxGenerationStrategy
    : ISyntaxGenerationStrategy<MethodModel>
{
    public int GetPriority() => 2; // Higher priority than default

    public bool CanHandle(object target)
        => target is MethodModel { Interface: true };
}

// Default method strategy
public class MethodSyntaxGenerationStrategy
    : ISyntaxGenerationStrategy<MethodModel>
{
    public int GetPriority() => 1; // Default

    public bool CanHandle(object target)
        => target is MethodModel;
}

// Fallback controller strategy
public class ControllerGenerationStrategy
    : IArtifactGenerationStrategy<ClassModel>
{
    public int GetPriority() => -1; // Fallback

    public bool CanHandle(object target)
        => target is ClassModel; // Handles any ClassModel
}
```

### 5.3 Dependency Injection

**Requirements**:
1. All strategies **MUST** be registered in `ConfigureServices.cs`
2. Registration **SHOULD** use reflection to auto-discover strategies
3. Strategies **MUST** be registered with appropriate lifetime (typically Transient or Singleton)
4. Generator interfaces **MUST** be registered as services

**Example Registration**:
```csharp
public static class ConfigureServices
{
    public static IServiceCollection AddEndpointDotNetServices(
        this IServiceCollection services)
    {
        // Register generators
        services.AddSingleton<ISyntaxGenerator, SyntaxGenerator>();
        services.AddSingleton<IArtifactGenerator, ArtifactGenerator>();

        // Auto-register all syntax generation strategies
        var syntaxStrategies = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ISyntaxGenerationStrategy<>)));

        foreach (var strategy in syntaxStrategies)
        {
            var interfaces = strategy.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ISyntaxGenerationStrategy<>));

            foreach (var @interface in interfaces)
            {
                services.AddTransient(@interface, strategy);
            }
        }

        // Similar for artifact strategies...

        return services;
    }
}
```

### 5.4 Code Style

**General Requirements**:
1. **MUST** follow C# coding conventions (PascalCase for public members, camelCase for private)
2. **MUST** use async/await for all generation operations
3. **MUST** accept `CancellationToken` parameters (with default value)
4. **MUST** include copyright header in all source files
5. **SHOULD** use nullable reference types
6. **SHOULD** use `StringBuilder` for complex string building
7. **SHOULD** use init-only properties for models
8. **SHOULD** prefer immutability

**Copyright Header** (REQUIRED):
```csharp
// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
```

## 6. Separation Between Artifact and Syntax Generation

### 6.1 Syntax Generation Strategies

**Characteristics**:
- **Purpose**: Generate code text/syntax
- **Return Type**: `Task<string>`
- **Side Effects**: None (pure functions)
- **Dependencies**: `ISyntaxGenerator` for composition
- **Testability**: Highly testable without I/O mocking

**Responsibilities**:
- ✅ Generate code syntax as strings
- ✅ Compose other syntax generators
- ✅ Apply naming conventions
- ✅ Format code structure
- ✅ Handle language-specific syntax rules

**Prohibited**:
- ❌ File I/O operations
- ❌ Directory creation
- ❌ Project scaffolding
- ❌ Running external commands

### 6.2 Artifact Generation Strategies

**Characteristics**:
- **Purpose**: Generate files, projects, solutions (filesystem artifacts)
- **Return Type**: `Task` (void)
- **Side Effects**: File/directory creation, process execution
- **Dependencies**: `ISyntaxGenerator`, `IArtifactGenerator`, `IFileSystem`
- **Testability**: Requires I/O mocking or filesystem abstraction

**Responsibilities**:
- ✅ Create files and directories
- ✅ Write content to disk
- ✅ Scaffold projects and solutions
- ✅ Execute commands (e.g., `dotnet new`)
- ✅ Compose syntax generators to get code text
- ✅ Compose other artifact generators

**Prohibited**:
- ❌ Duplicating syntax generation logic
- ❌ Inline code generation without using syntax generators

### 6.3 Composition Flow

**Required Pattern**:
```
Artifact Strategy
  └─> Uses ISyntaxGenerator to get code text
      └─> Syntax Strategy
          └─> Composes other Syntax Strategies via ISyntaxGenerator
  └─> Uses IArtifactGenerator to create child artifacts
      └─> Child Artifact Strategy
          └─> Uses ISyntaxGenerator...
```

**Example Flow**:
```
ProjectGenerationStrategy (artifact)
  ├─> Creates project directory (file I/O)
  ├─> Runs 'dotnet new' command
  └─> Generates files:
      └─> IArtifactGenerator.GenerateAsync(CodeFileModel<ClassModel>)
          └─> ClassCodeFileArtifactGenerationStrategy
              └─> ISyntaxGenerator.GenerateAsync(ClassModel)
                  └─> ClassSyntaxGenerationStrategy
                      ├─> ISyntaxGenerator.GenerateAsync(PropertyModel[])
                      │   └─> PropertiesSyntaxGenerationStrategy
                      │       └─> ISyntaxGenerator.GenerateAsync(PropertyModel)
                      │           └─> PropertySyntaxGenerationStrategy
                      └─> ISyntaxGenerator.GenerateAsync(MethodModel[])
                          └─> MethodsSyntaxGenerationStrategy
              └─> IArtifactGenerator.GenerateAsync(FileModel)
                  └─> FileGenerationStrategy (writes to disk)
```

## 7. Acceptance Criteria

### 7.1 Strategy Implementation

**AC1.1**: Each syntax generation strategy **MUST** generate exactly one type of syntax element
- ✅ Pass: `PropertySyntaxGenerationStrategy` generates a single property
- ❌ Fail: `ClassWithEverythingSyntaxGenerationStrategy` generates class, properties, methods in one strategy

**AC1.2**: Each artifact generation strategy **MUST** generate exactly one type of artifact
- ✅ Pass: `FileGenerationStrategy` creates a single file
- ❌ Fail: `ProjectWithAllFilesgenerationStrategy` creates project and all files without delegation

**AC1.3**: Strategies **MUST** compose other strategies through generator interfaces
- ✅ Pass: Strategy injects `ISyntaxGenerator` and calls `_syntaxGenerator.GenerateAsync(childModel)`
- ❌ Fail: Strategy directly instantiates `PropertySyntaxGenerationStrategy`

**AC1.4**: Strategies **MUST NOT** contain large monolithic generation logic
- ✅ Pass: Strategy delegates to 5+ other strategies for complex structures
- ❌ Fail: Strategy contains 500+ lines of generation code without composition

### 7.2 Separation of Concerns

**AC2.1**: Syntax generation strategies **MUST NOT** perform file I/O
- ✅ Pass: Strategy returns `Task<string>` and uses only `ISyntaxGenerator`
- ❌ Fail: Strategy uses `File.WriteAllText()` or `IFileSystem`

**AC2.2**: Artifact generation strategies **MUST** use syntax generators for code text
- ✅ Pass: Strategy calls `await _syntaxGenerator.GenerateAsync(model)` to get code
- ❌ Fail: Strategy contains inline code generation logic (e.g., `builder.Append("public class ")`)

**AC2.3**: Artifact generation strategies **MUST** delegate to other artifact strategies
- ✅ Pass: `ProjectGenerationStrategy` calls `_artifactGenerator.GenerateAsync(fileModel)`
- ❌ Fail: `ProjectGenerationStrategy` directly writes all files with inline logic

### 7.3 Naming Conventions

**AC3.1**: Syntax generation strategies **MUST** follow naming pattern `{Concept}SyntaxGenerationStrategy`
- ✅ Pass: `ClassSyntaxGenerationStrategy`, `MethodSyntaxGenerationStrategy`
- ❌ Fail: `ClassGenerator`, `MethodStrategy`, `GenerateClass`

**AC3.2**: Artifact generation strategies **MUST** follow naming pattern `{Concept}ArtifactGenerationStrategy` or `{Concept}GenerationStrategy` (for established concepts)
- ✅ Pass: `ClassCodeFileArtifactGenerationStrategy`, `FileGenerationStrategy`, `ProjectGenerationStrategy`
- ❌ Fail: `ClassArtifact`, `FileCreator`, `MakeProject`

**AC3.3**: Models **MUST** follow naming pattern `{Concept}Model`
- ✅ Pass: `ClassModel`, `PropertyModel`, `FileModel`
- ❌ Fail: `Class`, `Property`, `FileInfo`

**AC3.4**: Models **MUST** inherit from appropriate base class
- ✅ Pass: `ClassModel : InterfaceModel`, `FileModel : ArtifactModel`
- ❌ Fail: `ClassModel` inherits from `object` only

### 7.4 Project Organization

**AC4.1**: Technology-specific strategies **MUST** be in separate projects
- ✅ Pass: `ClassSyntaxGenerationStrategy` in `Endpoint.DotNet` project
- ❌ Fail: `ClassSyntaxGenerationStrategy` in `Endpoint` core project

**AC4.2**: Syntax strategies **MUST** be in `Syntax/{Concept}/Strategies/` directory
- ✅ Pass: `Syntax/Classes/Strategies/ClassSyntaxGenerationStrategy.cs`
- ❌ Fail: `Strategies/ClassSyntaxGenerationStrategy.cs` or `Classes/ClassSyntaxGenerationStrategy.cs`

**AC4.3**: Artifact strategies **MUST** be in `Artifacts/{Concept}/Strategies/` directory
- ✅ Pass: `Artifacts/Files/Strategies/FileGenerationStrategy.cs`
- ❌ Fail: `Strategies/FileGenerationStrategy.cs`

**AC4.4**: Models **SHOULD** be at the root of their concept folder
- ✅ Pass: `Syntax/Classes/ClassModel.cs`
- ❌ Fail: `Syntax/Classes/Models/ClassModel.cs`

### 7.5 Code Quality

**AC5.1**: All source files **MUST** include copyright header
- ✅ Pass: First two lines are copyright and license
- ❌ Fail: Missing header or incorrect format

**AC5.2**: Strategies **MUST** use dependency injection for dependencies
- ✅ Pass: Dependencies injected via constructor
- ❌ Fail: Using `new SyntaxGenerator()` or service locator pattern

**AC5.3**: Strategies **MUST** implement `CanHandle` and `GetPriority`
- ✅ Pass: Both methods implemented with appropriate logic
- ❌ Fail: Missing methods or always returning true/1

**AC5.4**: Generation methods **MUST** accept `CancellationToken`
- ✅ Pass: `Task<string> GenerateAsync(T model, CancellationToken cancellationToken = default)`
- ❌ Fail: No cancellation token parameter

**AC5.5**: Strategies **SHOULD** be stateless and thread-safe
- ✅ Pass: No mutable instance fields, all state passed via parameters
- ❌ Fail: Mutable instance fields modified during generation

### 7.6 Testing Requirements

**AC6.1**: Syntax strategies **MUST** be testable without file system mocking
- ✅ Pass: Unit test instantiates strategy, calls `GenerateAsync`, asserts string output
- ❌ Fail: Test requires mocking `IFileSystem` or file I/O

**AC6.2**: Artifact strategies **MUST** use file system abstraction for testability
- ✅ Pass: Strategy uses `IFileSystem` interface, tests mock it
- ❌ Fail: Strategy uses `File.WriteAllText` directly

**AC6.3**: Each strategy **SHOULD** have corresponding unit tests
- ✅ Pass: `ClassSyntaxGenerationStrategyTests.cs` exists with comprehensive tests
- ❌ Fail: No tests for strategy

## 8. Common Patterns and Examples

### 8.1 Creating a New Syntax Strategy

**Checklist**:
1. ✅ Create model class inheriting from `SyntaxModel` or derived class
2. ✅ Create strategy class with pattern `{Concept}SyntaxGenerationStrategy`
3. ✅ Implement `ISyntaxGenerationStrategy<TModel>`
4. ✅ Inject `ISyntaxGenerator` for composition
5. ✅ Implement `GenerateAsync` to return generated code
6. ✅ Compose other syntax strategies for child elements
7. ✅ Implement `CanHandle` and `GetPriority`
8. ✅ Add copyright header
9. ✅ Register in DI container (if not auto-registered)
10. ✅ Write unit tests

### 8.2 Creating a New Artifact Strategy

**Checklist**:
1. ✅ Create model class inheriting from `ArtifactModel` or derived class
2. ✅ Create strategy class with pattern `{Concept}ArtifactGenerationStrategy`
3. ✅ Implement `IArtifactGenerationStrategy<TModel>`
4. ✅ Inject `ISyntaxGenerator` for code generation
5. ✅ Inject `IArtifactGenerator` for child artifacts
6. ✅ Inject `IFileSystem` or other required services
7. ✅ Implement `GenerateAsync` to perform file operations
8. ✅ Use syntax generators for code text (don't generate inline)
9. ✅ Delegate to artifact generators for child artifacts
10. ✅ Implement `CanHandle` and `GetPriority`
11. ✅ Add copyright header
12. ✅ Register in DI container (if not auto-registered)
13. ✅ Write unit tests with appropriate mocks

### 8.3 Anti-Patterns to Avoid

**Anti-Pattern 1: Monolithic Strategy**
```csharp
// ❌ BAD - Everything in one strategy
public class CompleteProjectGenerationStrategy
{
    public async Task GenerateAsync(ProjectModel model)
    {
        // Creates directory
        Directory.CreateDirectory(model.Path);

        // Generates all class code inline
        var classCode = "public class " + model.ClassName + " { ";
        foreach (var prop in model.Properties)
        {
            classCode += "public " + prop.Type + " " + prop.Name + " { get; set; } ";
        }
        classCode += "}";

        // Writes file directly
        File.WriteAllText(Path.Combine(model.Path, model.ClassName + ".cs"), classCode);

        // Generates interface code inline
        var interfaceCode = "public interface I" + model.ClassName + " { ... }";
        File.WriteAllText(...);

        // ... continues for all files
    }
}
```

**Correct Pattern**:
```csharp
// ✅ GOOD - Composed strategies
public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IFileSystem _fileSystem;

    public async Task GenerateAsync(ProjectModel model, CancellationToken cancellationToken)
    {
        // Create directory
        _fileSystem.CreateDirectory(model.Path);

        // Delegate class file generation
        await _artifactGenerator.GenerateAsync(
            new CodeFileModel<ClassModel> { Content = model.ClassModel, Directory = model.Path },
            cancellationToken);

        // Delegate interface file generation
        await _artifactGenerator.GenerateAsync(
            new CodeFileModel<InterfaceModel> { Content = model.InterfaceModel, Directory = model.Path },
            cancellationToken);
    }
}
```

**Anti-Pattern 2: Syntax Strategy Doing File I/O**
```csharp
// ❌ BAD - Syntax strategy writing files
public class ClassSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    public async Task<string> GenerateAsync(ClassModel model)
    {
        var code = GenerateClassCode(model);

        // This is wrong! Syntax strategies should not do file I/O
        File.WriteAllText($"{model.Name}.cs", code);

        return code;
    }
}
```

**Correct Pattern**:
```csharp
// ✅ GOOD - Syntax returns string, artifact writes file
public class ClassSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    public async Task<string> GenerateAsync(ClassModel model)
    {
        var builder = new StringBuilder();
        // ... generate code ...
        return builder.ToString(); // Just return the string
    }
}

public class ClassCodeFileArtifactGenerationStrategy
    : IArtifactGenerationStrategy<CodeFileModel<ClassModel>>
{
    public async Task GenerateAsync(CodeFileModel<ClassModel> model)
    {
        // Get code from syntax generator
        var code = await _syntaxGenerator.GenerateAsync(model.Content);

        // Create file model
        var fileModel = new FileModel { Content = code, ... };

        // Delegate to file generator for I/O
        await _artifactGenerator.GenerateAsync(fileModel);
    }
}
```

**Anti-Pattern 3: Artifact Strategy with Inline Code Generation**
```csharp
// ❌ BAD - Artifact generating code inline
public class FileGenerationStrategy : IArtifactGenerationStrategy<FileModel>
{
    public async Task GenerateAsync(FileModel model)
    {
        var content = model.Content;

        // This is wrong! Should use syntax generator
        if (model.Type == "class")
        {
            content = "public class " + model.Name + " { }";
        }

        File.WriteAllText(model.Path, content);
    }
}
```

**Correct Pattern**:
```csharp
// ✅ GOOD - Artifact uses syntax generator for code
public class CodeFileArtifactGenerationStrategy
    : IArtifactGenerationStrategy<CodeFileModel<ClassModel>>
{
    private readonly ISyntaxGenerator _syntaxGenerator;
    private readonly IArtifactGenerator _artifactGenerator;

    public async Task GenerateAsync(CodeFileModel<ClassModel> model)
    {
        // Use syntax generator to get code
        var content = await _syntaxGenerator.GenerateAsync(model.Content);

        // Create file model
        var fileModel = new FileModel { Content = content, ... };

        // Delegate to file generator
        await _artifactGenerator.GenerateAsync(fileModel);
    }
}
```

## 9. Extension Points

### 9.1 Adding New Technology Support

To add support for a new technology (e.g., Python, Java):

1. Create new project: `Endpoint.{Technology}`
2. Implement technology-specific syntax strategies
3. Implement technology-specific artifact strategies
4. Create appropriate models
5. Register strategies in `ConfigureServices`
6. Follow all naming and organizational conventions from this spec

### 9.2 Priority-Based Specialization

Use priority system for specialized behavior:

```csharp
// Generic strategy - priority 1 (default)
public class MethodSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    public int GetPriority() => 1;
    public bool CanHandle(object target) => target is MethodModel;
}

// Specialized strategy - priority 2 (higher)
public class InterfaceMethodSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    public int GetPriority() => 2; // Takes precedence
    public bool CanHandle(object target) => target is MethodModel { Interface: true };
}
```

## 10. References

### 10.1 Key Files

- [ISyntaxGenerationStrategy.cs](../src/Endpoint/Syntax/ISyntaxGenerationStrategy.cs)
- [IArtifactGenerationStrategy.cs](../src/Endpoint/Artifacts/IArtifactGenerationStrategy.cs)
- [SyntaxGenerator.cs](../src/Endpoint/Syntax/SyntaxGenerator.cs)
- [ArtifactGenerator.cs](../src/Endpoint/Artifacts/ArtifactGenerator.cs)

### 10.2 Example Implementations

- [ClassSyntaxGenerationStrategy.cs](../src/Endpoint.DotNet/Syntax/Classes/Strategies/ClassSyntaxGenerationStrategy.cs)
- [ClassCodeFileArtifactGenerationStrategy.cs](../src/Endpoint.DotNet/Artifacts/Files/Strategies/ClassCodeFileArtifactGenerationStrategy.cs)
- [ProjectGenerationStrategy.cs](../src/Endpoint.DotNet/Artifacts/Projects/Strategies/ProjectGenerationStrategy.cs)

## 11. Revision History

| Version | Date       | Author | Changes                        |
|---------|------------|--------|--------------------------------|
| 1.0     | 2026-01-10 | System | Initial specification document |

---

**Document Status**: This is a living document and should be updated as the architecture evolves. All changes must maintain backward compatibility or include migration guidance.