# record-create

Create a C# record type for immutable data objects.

## Synopsis

```bash
endpoint record-create [options]
```

## Description

The `record-create` command generates a C# record type. Records are reference types that provide built-in functionality for encapsulating immutable data, including value equality, deconstruction, and with-expressions.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the record to create | No | - |
| `--directory` | `-d` | Target directory for the generated file | No | Current directory |

## Examples

### Create a simple record

```bash
endpoint record-create -n CustomerRecord
```

**Output: `CustomerRecord.cs`**
```csharp
public record CustomerRecord
{
}
```

### Specify output directory

```bash
endpoint record-create -n OrderSummary -d ./src/Records
```

## When to Use Records vs Classes

| Use Records When | Use Classes When |
|-----------------|------------------|
| Data is immutable | Data needs to be mutable |
| Value equality is important | Reference equality is needed |
| Creating DTOs | Creating entities with behavior |
| Working with patterns | Complex inheritance is required |

## Record Features (C# 9+)

Records automatically provide:

- **Value Equality**: Two records with the same values are equal
- **Immutability**: Properties are init-only by default
- **Deconstruction**: Built-in deconstruction support
- **With-Expressions**: Easy creation of modified copies
- **ToString()**: Automatic meaningful string representation

## Example Usage of Generated Record

```csharp
// Create instance
var customer = new CustomerRecord
{
    Name = "John Doe",
    Email = "john@example.com"
};

// With-expression (creates a copy with modified values)
var updatedCustomer = customer with { Email = "newemail@example.com" };

// Value equality
var customer2 = new CustomerRecord { Name = "John Doe", Email = "john@example.com" };
bool areEqual = customer == customer2; // true
```

## Common Use Cases

1. **DTOs**: Data Transfer Objects for APIs
2. **Event Data**: Event sourcing and message data
3. **Configuration**: Immutable configuration objects
4. **Value Objects**: DDD value objects
5. **Pattern Matching**: Records work great with pattern matching

## Related Commands

- [class-create](./class-create.user-guide.md) - Create mutable classes
- [entity-create](./entity-create.user-guide.md) - Create entities with DTOs

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
