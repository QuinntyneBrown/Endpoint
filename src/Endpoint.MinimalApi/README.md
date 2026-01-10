# Endpoint.MinimalApi

The ASP.NET Core Minimal APIs code generation module for the Endpoint framework. This library provides generators for creating lightweight, high-performance API endpoints.

## Overview

Endpoint.MinimalApi enables the generation of ASP.NET Core Minimal API endpoints, which offer a streamlined approach to building HTTP APIs with minimal ceremony and maximum performance.

## Key Features

- **Endpoint Generation**: Type-safe endpoint definitions
- **Route Handlers**: Lambda-based and method group handlers
- **Request/Response**: Strongly-typed request and response models
- **Validation**: Integrated input validation
- **OpenAPI**: Automatic OpenAPI documentation

## Usage

Generate Minimal API endpoints through the CLI:

```bash
# Generate a minimal API endpoint
endpoint api minimal --resource Customer

# Generate CRUD endpoints
endpoint api minimal --resource Product --crud

# Generate with specific HTTP methods
endpoint api minimal --resource Order --methods "get,post,put,delete"
```

## Generated Artifacts

Example generated endpoint:

```csharp
public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers");

        group.MapGet("/", GetCustomers);
        group.MapGet("/{id}", GetCustomer);
        group.MapPost("/", CreateCustomer);
        group.MapPut("/{id}", UpdateCustomer);
        group.MapDelete("/{id}", DeleteCustomer);
    }

    private static async Task<IResult> GetCustomers(
        ICustomerService service)
    {
        var customers = await service.GetAllAsync();
        return Results.Ok(customers);
    }

    // Additional handlers...
}
```

## Benefits of Minimal APIs

- **Performance**: Lower overhead than controller-based APIs
- **Simplicity**: Less boilerplate code
- **AOT-Friendly**: Better support for Native AOT compilation
- **Flexibility**: Easy to customize and extend

## Integration

Minimal API endpoints integrate with:

- **Endpoint.DomainDrivenDesign**: Domain-driven endpoint patterns
- **Endpoint.ModernWebAppPattern**: Modern web app architecture

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
