# Code Generation Commands

Commands for creating C# classes, interfaces, entities, enums, records, and other code artifacts.

## Available Commands

| Command | Description |
|---------|-------------|
| [class-create](./class-create.user-guide.md) | Create a C# class |
| [interface-create](./interface-create.user-guide.md) | Create a C# interface |
| [entity-create](./entity-create.user-guide.md) | Create a database entity with DTO |
| [enum-create](./enum-create.user-guide.md) | Create an enumeration |
| [record-create](./record-create.user-guide.md) | Create a C# record |
| [aggregate-create](./aggregate-create.user-guide.md) | Create a DDD aggregate |
| [controller-create](./controller-create.user-guide.md) | Create an API controller |
| [service-create](./service-create.user-guide.md) | Create a service class |
| [feature](./feature.user-guide.md) | Create a complete CRUD feature |

## Quick Examples

```bash
# Create a simple class
endpoint class-create -n Customer

# Create an entity with properties
endpoint entity-create -n Product -p "Name:string,Price:decimal,Description:string"

# Create a complete feature with CRUD operations
endpoint feature Product -d ./Features
```

[Back to Index](../index.md)
