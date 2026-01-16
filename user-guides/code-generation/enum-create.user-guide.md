# enum-create

Create a C# enumeration with specified values.

## Synopsis

```bash
endpoint enum-create [options]
```

## Description

The `enum-create` command generates a C# enumeration (enum) file with the specified name, base type, and enum values. This is useful for creating strongly-typed constants that represent a fixed set of values.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the enum to create | No | - |
| `--type` | `-t` | Base type of the enum | No | `int` |
| `--enums` | `-e` | Enum values (format: `Name:Value,Name:Value`) | No | Empty |
| `--directory` | `-d` | Target directory for the generated file | No | Current directory |

## Supported Base Types

- `int` (default)
- `string`

## Enum Values Format

Enum values are specified as comma-separated key-value pairs:

```
EnumName:EnumValue
```

## Examples

### Create a simple enum

```bash
endpoint enum-create -n OrderStatus
```

**Output: `OrderStatus.cs`**
```csharp
public enum OrderStatus
{
}
```

### Create an enum with values

```bash
endpoint enum-create -n OrderStatus -e "Pending:0,Processing:1,Shipped:2,Delivered:3,Cancelled:4"
```

**Output: `OrderStatus.cs`**
```csharp
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
```

### Create an enum with string base type

```bash
endpoint enum-create -n PaymentType -t string -e "CreditCard:CC,DebitCard:DC,BankTransfer:BT,Cash:CASH"
```

### Specify output directory

```bash
endpoint enum-create -n UserRole -e "Admin:1,User:2,Guest:3" -d ./src/Enums
```

## Common Use Cases

1. **Status Values**: Order status, user status, task status
2. **Type Classifications**: Payment types, user roles, product categories
3. **Configuration Options**: Settings, preferences, modes
4. **Domain Constants**: Business-specific constant values

## Best Practices

- Use PascalCase for enum names and values
- Start enum values at 0 or 1 for explicit clarity
- Group related constants in the same enum
- Use meaningful names that describe the value's purpose

## Related Commands

- [class-create](./class-create.user-guide.md) - Create classes that use enums
- [entity-create](./entity-create.user-guide.md) - Create entities with enum properties

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
