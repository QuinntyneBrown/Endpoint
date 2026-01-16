# migration-add

Add Entity Framework Core database migrations.

## Synopsis

```bash
endpoint migration-add [options]
```

## Description

The `migration-add` command creates a new Entity Framework Core migration based on changes to your data model. This is a wrapper around the `dotnet ef migrations add` command integrated into the Endpoint CLI workflow.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the migration | No | - |
| `--directory` | `-d` | Directory containing the DbContext project | No | Current directory |

## Examples

### Create a migration

```bash
endpoint migration-add -n InitialCreate
```

### Create migration with descriptive name

```bash
endpoint migration-add -n "AddCustomerTable"
```

### Create migration in specific directory

```bash
endpoint migration-add -n "AddOrderRelationships" -d ./src/MyApp.Data
```

## Migration Naming Conventions

Use descriptive names that indicate what changed:

| Migration Name | Description |
|----------------|-------------|
| `InitialCreate` | First migration creating initial schema |
| `AddUserTable` | Adding a new table |
| `AddOrderStatusColumn` | Adding a column |
| `RemoveObsoleteFields` | Removing columns |
| `AddCustomerOrderRelationship` | Adding foreign key |
| `UpdateProductPriceType` | Changing column type |

## Generated Files

A migration creates two files:

```
Migrations/
├── 20240115120000_AddCustomerTable.cs           # Migration code
└── 20240115120000_AddCustomerTable.Designer.cs  # Snapshot
```

## Migration Structure

```csharp
public partial class AddCustomerTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Changes to apply
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback changes
        migrationBuilder.DropTable(name: "Customers");
    }
}
```

## Workflow

1. **Make Model Changes**: Update your entity classes
2. **Add Migration**: Run `endpoint migration-add -n "DescriptiveName"`
3. **Review Migration**: Check the generated Up/Down methods
4. **Apply Migration**: Run `dotnet ef database update`

## Common Use Cases

1. **Initial Setup**: Create initial database schema
2. **Schema Evolution**: Add tables, columns, relationships
3. **Data Type Changes**: Modify column types
4. **Index Management**: Add or remove indexes
5. **Refactoring**: Rename tables or columns

## Best Practices

- Use descriptive, timestamped migration names
- Review generated migrations before applying
- Test migrations in development first
- Keep migrations atomic (one logical change per migration)
- Never edit applied migrations

## Applying Migrations

After creating a migration:

```bash
# Apply all pending migrations
dotnet ef database update

# Apply to specific migration
dotnet ef database update AddCustomerTable

# Rollback to previous migration
dotnet ef database update PreviousMigrationName
```

## Related Commands

- [entity-create](../code-generation/entity-create.user-guide.md) - Create entities
- [db-context-add](./db-context-add.user-guide.md) - Add DbContext to project

[Back to Project Management](./index.md) | [Back to Index](../index.md)
