# Document Generator File Models - String-Based Code Generation

This document identifies all methods in the codebase where generators return `FileModel` objects with class, interface, or code syntax loosely defined by strings (string concatenation, interpolation, or raw string literals).

## Overview

The code generation system uses two primary approaches for generating C# code:
1. **Model-Based Generation**: Using syntax models (`ClassModel`, `InterfaceModel`, etc.) that are transformed into strings via `ISyntaxGenerationStrategy<T>` implementations
2. **Raw String Generation**: Directly embedding C# code as raw string literals in `FileModel.Body`

Both approaches ultimately result in string-based code that is written to files.

---

## Part 1: Syntax Generation Strategies

These classes implement `ISyntaxGenerationStrategy<T>` and return `Task<string>` with code constructed via `StringBuilder`.

### 1. ClassSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Classes/Strategies/ClassSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(ClassModel model, CancellationToken cancellationToken)` - Lines 31-106

**String Patterns**:
```csharp
builder.AppendLine($"using {directive.Alias} = {directive.Name};");  // Line 41
builder.Append(" static");                                           // Line 56
builder.Append($" class {model.Name}");                              // Line 59
builder.Append(": ");                                                // Line 63
builder.Append(" { }");                                              // Line 70
builder.AppendLine("{");                                             // Line 77
builder.AppendLine("}");                                             // Line 103
```

**Output Example**:
```csharp
public static class MyClassName: IInterface { }
```

---

### 2. InterfaceSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Interfaces/InterfaceSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(InterfaceModel model, CancellationToken cancellationToken)` - Lines 29-70

**String Patterns**:
```csharp
builder.Append($"public interface {model.Name}");  // Line 35
builder.Append(": ");                               // Line 39
builder.Append(" { }");                             // Line 46
builder.AppendLine("{");                            // Line 53
builder.AppendLine("}");                            // Line 67
```

**Output Example**:
```csharp
public interface IMyInterface: IBase { }
```

---

### 3. RecordSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Records/RecordSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(RecordModel model, CancellationToken cancellationToken)` - Lines 21-34

**String Patterns**:
```csharp
sb.AppendLine($"public record {model.Type switch { Struct => "struct", Class => "class" }} {model.Name}");  // Line 27
sb.AppendLine("{");   // Line 29
sb.AppendLine("}");   // Line 31
```

**Output Example**:
```csharp
public record class MyRecord
{
}
```

---

### 4. MethodSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Methods/Strategies/MethodSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(MethodModel model, CancellationToken cancellationToken)` - Lines 23-92

**String Patterns**:
```csharp
builder.Append(" override");           // Line 38
builder.Append(" async");              // Line 43
builder.Append(" static");             // Line 48
builder.Append(" implicit operator");  // Line 53
builder.Append(" explicit operator");  // Line 57
builder.Append($" {returnType}");      // Line 61
builder.Append($" {model.Name}");      // Line 64
builder.Append('(');                   // Line 66
builder.Append(')');                   // Line 70
builder.AppendLine("{");               // Line 75, 82
builder.AppendLine("}");               // Line 76
builder.Append('}');                   // Line 88
```

**Output Example**:
```csharp
public async static Task<string> MyMethod(string param)
{
    // body
}
```

---

### 5. ConstructorSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Constructors/ConstructorSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(ConstructorModel model, CancellationToken cancellationToken)` - Lines 30-63

**String Patterns**:
```csharp
builder.Append($" {model.Name}");                                           // Line 38
builder.Append('(');                                                        // Line 40
builder.Append(')');                                                        // Line 44
builder.AppendLine($": base({string.Join(',', model.BaseParams)})".Indent(1));  // Line 48
builder.AppendLine("{");                                                    // Line 51
builder.Append("}");                                                        // Line 60
```

**Output Example**:
```csharp
public MyClass(string param): base(param)
{
    // body
}
```

---

### 6. PropertySyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Properties/Strategies/PropertySyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(PropertyModel model, CancellationToken cancellationToken)` - Lines 27-60

**String Patterns**:
```csharp
builder.Append("required ");                                                 // Line 48
builder.Append($"{type} {model.Name} {accessors}");                         // Line 52
builder.Append($" = {model.DefaultValue};");                                // Line 56
```

**Output Example**:
```csharp
public required string Name { get; set; } = "default";
```

---

### 7. FieldsSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Fields/FieldsSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(List<FieldModel> model, CancellationToken cancellationToken)` - Lines 26-43

**Method**: `CreateAsync(ISyntaxGenerator, FieldModel)` - Lines 45-73

**String Patterns**:
```csharp
builder.Append(" static");                                        // Line 55
builder.Append(" readonly");                                      // Line 60
builder.Append($" {type} {model.Name} = {model.DefaultValue};");  // Line 65
builder.Append($" {type} {model.Name};");                         // Line 69
```

**Output Example**:
```csharp
private static readonly string _name = "value";
```

---

### 8. AttributeSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Attributes/Strategies/AttributeSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(AttributeModel target, CancellationToken cancellationToken)` - Lines 21-77

**String Patterns**:
```csharp
builder.Append('[');                                    // Line 27
builder.Append(target.Name);                            // Line 29
builder.Append($"({target.Template})");                 // Line 33
builder.Append($"{property.Key} = \"{property.Value}\"");  // Line 47, 61
builder.Append(']');                                    // Line 74
```

**Output Example**:
```csharp
[HttpGet("api/values")]
[Authorize(Roles = "Admin")]
```

---

### 9. TypeSyntaxGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Types/TypeSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(TypeModel model, CancellationToken cancellationToken)` - Lines 24-42

**String Patterns**:
```csharp
builder.Append(model.Name);   // Line 30
builder.Append('<');          // Line 34
builder.Append('>');          // Line 38
```

**Output Example**:
```csharp
IEnumerable<Task<string>>
```

---

### 10. NamespaceGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Namespaces/Strategies/NamespaceSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(NamespaceModel model, CancellationToken cancellationToken)` - Lines 19-28

**String Patterns**:
```csharp
sb.AppendLine($"namespace {model.Name};");  // Line 25
```

**Output Example**:
```csharp
namespace MyApp.Domain;
```

---

### 11. DocumentGenerationStrategy

**File**: `src/Endpoint.DotNet/Syntax/Documents/Strategies/DocumentSyntaxGenerationStrategy.cs`

**Method**: `GenerateAsync(DocumentModel model, CancellationToken cancellationToken)` - Lines 21-48

**String Patterns**:
```csharp
stringBuilder.AppendLine($"using {@using};");                              // Line 29
stringBuilder.AppendLine($"namespace {model.RootNamespace}.{model.Namespace};");  // Line 34
```

**Output Example**:
```csharp
using System;
using System.Collections.Generic;

namespace MyApp.Domain;
```

---

## Part 2: File Artifact Generation Strategies

### 12. CodeFileIArtifactGenerationStrategy<T>

**File**: `src/Endpoint.DotNet/Artifacts/Files/Strategies/CodeFileIArtifactGenerationStrategy.cs`

**Method**: `GenerateAsync(CodeFileModel<T> model)` - Lines 34-64

**String Patterns**:
```csharp
stringBuilder.AppendLine($"using {@using.Name};");     // Line 42
stringBuilder.AppendLine($"namespace {fileNamespace};");  // Line 54
```

This strategy assembles the final file by combining:
- Using directives
- Namespace declaration
- Generated syntax from the model object

---

## Part 3: FileFactory Methods

**File**: `src/Endpoint.DotNet/Artifacts/Files/Factories/FileFactory.cs`

### 13. CreateResponseBase

**Method**: `CreateResponseBase(string directory)` - Lines 85-103

**Description**: Creates a `ClassModel` with constructor body as string.

**String Pattern**:
```csharp
Body = new("Errors = new List<string>();")  // Line 91
```

---

### 14. CreateLinqExtensions

**Method**: `CreateLinqExtensions(string directory)` - Lines 105-156

**Description**: Creates extension method with expression body as string.

**String Pattern**:
```csharp
methodModel.Body = new Syntax.Expressions.ExpressionModel(
    "return queryable.Skip(pageSize * pageIndex).Take(pageSize);"
);  // Line 151
```

---

### 15. CreateCoreUsings

**Method**: `CreateCoreUsings(string directory)` - Lines 158-172

**Description**: Creates global usings file with string interpolation.

**String Pattern**:
```csharp
.Select(x => $"global using {x};")  // Line 166
```

**Output Example**:
```csharp
global using MediatR;
global using Microsoft.Extensions.Logging;
```

---

### 16. CreateUdpClientFactoryInterfaceAsync

**Method**: `CreateUdpClientFactoryInterfaceAsync(string directory)` - Lines 206-260

**Description**: Creates interface with method body as raw string literal.

**String Pattern (Lines 234-256)**:
```csharp
Body = new("""
UdpClient udpClient = null!;
int i = 1;
while (udpClient?.Client?.IsBound == null || udpClient.Client.IsBound == false)
{
    try
    {
        udpClient = new UdpClient();
        udpClient.Client.Bind(IPEndPoint.Parse($"127.0.0.{i}:{MulticastUrl.Split(':')[1]}"));
        udpClient.JoinMulticastGroup(IPAddress.Parse(MulticastUrl.Split(':')[0]), IPAddress.Parse($"127.0.0.{i}"));
    }
    catch (SocketException)
    {
        i++;
    }
}
return udpClient;
""")
```

---

### 17. CreateUdpServiceBusMessageAsync

**Method**: `CreateUdpServiceBusMessageAsync(string directory)` - Lines 295-324

**Description**: Creates class with constructor body as raw string.

**String Pattern (Lines 317-320)**:
```csharp
Body = new("""
    PayloadType = payloadType;
    Payload = payload;
    """)
```

---

## Part 4: Microservice Artifact Factories

These factories use **raw string literals** (triple-quoted strings) to embed complete C# files directly in `FileModel.Body`.

### 18. ConfigurationManagementArtifactFactory

**File**: `src/Endpoint.Engineering/Microservices/ConfigurationManagement/ConfigurationManagementArtifactFactory.cs`

#### Method: AddCoreFiles (Lines 24-193)

Creates FileModels with full class/interface definitions as raw strings:

| Line | File Created | Type |
|------|-------------|------|
| 33-55 | ConfigurationFile | Class (Entity) |
| 57-81 | ConfigurationFileItem | Class + Enum |
| 84-104 | IConfigurationFileRepository | Interface |
| 106-125 | IConfigurationFileItemRepository | Interface |
| 128-138 | ConfigurationFileCreatedEvent | Record |
| 140-150 | ConfigurationFileUpdatedEvent | Record |
| 153-173 | ConfigurationFileDto | Class (DTO) |
| 175-193 | ConfigurationFileItemDto | Class (DTO) |

#### Method: AddInfrastructureFiles (Lines 196-450)

| Line | File Created | Type |
|------|-------------|------|
| 205-233 | ConfigurationManagementDbContext | Class |
| 236-263 | ConfigurationFileConfiguration | Class |
| 265-287 | ConfigurationFileItemConfiguration | Class |
| 290-357 | ConfigurationFileRepository | Class |
| 359-417 | ConfigurationFileItemRepository | Class |
| 420-449 | ConfigureServices | Static Class |

#### Method: AddApiFiles (Lines 452-617)

| Line | File Created | Type |
|------|-------------|------|
| 459-554 | FileController | Class (Controller) |
| 557-574 | appsettings | JSON |
| 577-616 | Program | Class |

#### Method: AddInfrastructureSeederFiles (Lines 619-797)

| Line | File Created | Type |
|------|-------------|------|
| 626-796 | DatabaseSeeder | Static Class |

---

### 19. Other Microservice Artifact Factories

*All factories in this section have been refactored to use `CodeFileModel<ClassModel>` and `CodeFileModel<InterfaceModel>` with strongly typed syntax models instead of raw string literals.*

---

## Summary Table

| Category | File Count | Pattern Type |
|----------|-----------|--------------|
| Syntax Generation Strategies | 11 | StringBuilder + String Interpolation |
| File Artifact Strategies | 1 | StringBuilder + String Interpolation |
| FileFactory Methods | 5 | Model Bodies as Strings / Raw Strings |
| Microservice Factories | 1 | Raw String Literals (ConfigurationManagement only - others refactored) |
| **Total** | **18** | |

---

## String Building Patterns Used

| Pattern | Example | Usage Location |
|---------|---------|----------------|
| String Interpolation | `$"public class {model.Name}"` | All Syntax Strategies |
| StringBuilder.Append | `builder.Append(" static")` | All Syntax Strategies |
| StringBuilder.AppendLine | `builder.AppendLine("{")` | All Syntax Strategies |
| StringBuilder.AppendJoin | `builder.AppendJoin(',', items)` | TypeSyntaxGenerationStrategy |
| String.Join | `string.Join(',', collection)` | Class/Interface inheritance |
| Raw String Literals | `"""public class X { }"""` | Microservice Factories |
| StringBuilderCache | `StringBuilderCache.Acquire()` / `.GetStringAndRelease()` | All Strategies |

---

## Code Generation Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Entry Points                                 │
├─────────────────────────────────────────────────────────────────────┤
│  FileFactory Methods          │  Microservice Artifact Factories    │
│  (Creates model objects)      │  (Creates FileModel with raw Body)  │
└─────────────┬─────────────────┴──────────────────┬──────────────────┘
              │                                     │
              ▼                                     │
┌─────────────────────────────────────────────────┐│
│              SyntaxGenerator                     ││
│  Routes to ISyntaxGenerationStrategy<T>          ││
└─────────────┬───────────────────────────────────┘│
              │                                     │
              ▼                                     │
┌─────────────────────────────────────────────────┐│
│         Syntax Generation Strategies             ││
│  StringBuilder + String Interpolation            ││
│  Returns: Task<string>                           ││
└─────────────┬───────────────────────────────────┘│
              │                                     │
              ▼                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    CodeFileModel<T> / FileModel                      │
│                         Body: string                                 │
└─────────────────────────────────┬───────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      File Generation Strategy                        │
│                    Writes Body to Disk via IFileSystem               │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Recommendations

1. **Consistency**: Consider standardizing on one approach (model-based or raw strings) for similar use cases
2. **Type Safety**: Model-based generation provides better type safety and validation
3. **Maintainability**: Raw strings are easier to read but harder to maintain at scale
4. **Testing**: Syntax strategies are more testable in isolation
5. **Refactoring**: Consider extracting common patterns into shared utilities
