# controller-create

Create an ASP.NET Core API controller with optional CRUD endpoints.

## Synopsis

```bash
endpoint controller-create [options]
```

## Description

The `controller-create` command generates an ASP.NET Core API controller. It can create either an empty controller or a full-featured controller with CRUD operations for a specified entity. When a product name is provided, it generates a controller following DDD patterns with MediatR integration.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the entity for the controller | No | - |
| `--empty` | `-e` | Create an empty controller without CRUD methods | No | `false` |
| `--product-name` | `-p` | Product name for DDD-style generation | No | - |
| `--bounded-context-name` | `-b` | Bounded context name | No | Pluralized entity name |
| `--directory` | `-d` | Target directory for the generated file | No | Current directory |

## Examples

### Create a simple controller

```bash
endpoint controller-create -n Product
```

**Output: `ProductsController.cs`**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get()
    {
        return Ok(await _mediator.Send(new GetProductsQuery()));
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDto>> GetById([FromRoute] Guid productId)
    {
        return Ok(await _mediator.Send(new GetProductByIdQuery { ProductId = productId }));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    [HttpPut]
    public async Task<ActionResult> Update([FromBody] UpdateProductCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{productId}")]
    public async Task<ActionResult> Delete([FromRoute] Guid productId)
    {
        await _mediator.Send(new DeleteProductCommand { ProductId = productId });
        return NoContent();
    }
}
```

### Create an empty controller

```bash
endpoint controller-create -n Health -e
```

**Output: `HealthController.cs`**
```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
}
```

### Create a DDD-style controller

```bash
endpoint controller-create -n Order -p MyECommerce -b Orders
```

### Specify output directory

```bash
endpoint controller-create -n Customer -d ./src/Api/Controllers
```

## Generated Endpoints

When not using the `-e` (empty) flag, the following endpoints are generated:

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/{entity}` | Get all entities |
| GET | `/api/{entity}/{id}` | Get entity by ID |
| POST | `/api/{entity}` | Create new entity |
| PUT | `/api/{entity}` | Update existing entity |
| DELETE | `/api/{entity}/{id}` | Delete entity by ID |

## Common Use Cases

1. **CRUD APIs**: Full Create, Read, Update, Delete operations
2. **Empty Controllers**: Starting point for custom endpoints
3. **DDD APIs**: Controllers following Domain-Driven Design patterns
4. **Microservices**: API controllers for microservice endpoints

## Best Practices

- Use plural names for controllers (e.g., `ProductsController`)
- Follow REST conventions for endpoint design
- Use MediatR for decoupling controllers from business logic
- Add proper validation and error handling

## Related Commands

- [feature](./feature.user-guide.md) - Create complete CRUD features
- [aggregate-create](./aggregate-create.user-guide.md) - Create DDD aggregates
- [ddd-app-create](../application-scaffolding/ddd-app-create.user-guide.md) - Create DDD applications

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
