# aggregate-create

Create a Domain-Driven Design (DDD) aggregate with entities, commands, and queries.

## Synopsis

```bash
endpoint aggregate-create [options]
```

## Description

The `aggregate-create` command generates a complete DDD aggregate structure, including the aggregate root, entities, commands, queries, and related infrastructure. This command launches an interactive JSON editor where you can customize the aggregate's structure before generation.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the aggregate to create | No | - |
| `--product-name` | `-p` | Name of the product/solution | No | - |
| `--directory` | `-d` | Target directory for the generated files | No | Current directory |

## Examples

### Create a basic aggregate

```bash
endpoint aggregate-create -n Order -p MyECommerce
```

This will open an interactive JSON editor with a template like:

```json
{
  "productName": "MyECommerce",
  "boundedContexts": [
    {
      "name": "Orders",
      "aggregates": [
        {
          "name": "Order",
          "entities": [],
          "commands": [
            { "name": "CreateOrder" },
            { "name": "UpdateOrder" },
            { "name": "DeleteOrder" }
          ],
          "queries": [
            { "name": "GetOrders" },
            { "name": "GetOrderById" }
          ]
        }
      ]
    }
  ]
}
```

### Specify output directory

```bash
endpoint aggregate-create -n Customer -p CRM -d ./src/Domain
```

## Generated Structure

For an aggregate named `Order`, the command generates:

```
Orders/
├── Order.cs                    # Aggregate root entity
├── Commands/
│   ├── CreateOrderCommand.cs
│   ├── UpdateOrderCommand.cs
│   └── DeleteOrderCommand.cs
├── Queries/
│   ├── GetOrdersQuery.cs
│   └── GetOrderByIdQuery.cs
├── Handlers/
│   ├── CreateOrderCommandHandler.cs
│   ├── UpdateOrderCommandHandler.cs
│   ├── DeleteOrderCommandHandler.cs
│   ├── GetOrdersQueryHandler.cs
│   └── GetOrderByIdQueryHandler.cs
└── DTOs/
    └── OrderDto.cs
```

## Interactive JSON Editor

When the command runs, it opens an interactive editor where you can:

1. Add or remove entities
2. Define custom commands
3. Add custom queries
4. Specify properties for each entity
5. Configure relationships

## Common Use Cases

1. **E-Commerce**: Orders, Products, Customers aggregates
2. **CRM Systems**: Contacts, Companies, Deals aggregates
3. **Project Management**: Projects, Tasks, Teams aggregates
4. **Inventory**: Products, Warehouses, Movements aggregates

## DDD Concepts

| Concept | Description |
|---------|-------------|
| **Aggregate** | A cluster of domain objects treated as a single unit |
| **Aggregate Root** | The entry point to the aggregate, ensures consistency |
| **Entity** | An object with identity that persists over time |
| **Command** | An intent to change the system state |
| **Query** | A request for data without side effects |

## Related Commands

- [ddd-app-create](../application-scaffolding/ddd-app-create.user-guide.md) - Create a complete DDD application
- [entity-create](./entity-create.user-guide.md) - Create simple entities
- [feature](./feature.user-guide.md) - Create CRUD features

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
