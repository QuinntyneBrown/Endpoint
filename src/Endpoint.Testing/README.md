# Endpoint.Testing

The testing utilities and test generation module for the Endpoint framework. This library provides factories and strategies for generating test projects, test classes, and testing infrastructure.

## Overview

Endpoint.Testing enables the generation of comprehensive test suites that accompany generated application code. It supports multiple testing frameworks and patterns.

## Project Structure

```
Endpoint.Testing/
├── AngularWorkspaceGenerationStrategy.cs  # Angular test workspace generation
├── ArtifactFactory.cs                     # Test artifact factory
├── IArtifactFactory.cs                    # Factory interface
├── ISyntaxFactory.cs                      # Syntax factory interface
├── SyntaxFactory.cs                       # Test syntax generation
└── ConfigureServices.cs                   # DI registration
```

## Key Features

### Artifact Factory

Create test artifacts programmatically:

```csharp
public interface IArtifactFactory
{
    Task<TestProject> CreateTestProjectAsync(string name, TestFramework framework);
    Task<TestClass> CreateTestClassAsync(string name, Type targetType);
    Task<TestMethod> CreateTestMethodAsync(string name, TestType type);
}
```

### Angular Workspace Generation

Generate Angular testing workspaces with Karma/Jasmine configuration:

```csharp
var strategy = new AngularWorkspaceGenerationStrategy(options);
await strategy.GenerateAsync(workspace);
```

### Service Registration

```csharp
using Endpoint.Testing;

services.AddTestingCoreServices();
```

## Usage

Generate tests through the CLI:

```bash
# Generate unit tests for an entity
endpoint generate tests --entity Customer --type unit

# Generate integration tests
endpoint generate tests --project Api --type integration

# Generate test project
endpoint generate test-project --name MyApp.Tests --framework xunit
```

## Supported Test Frameworks

| Framework | Description |
|-----------|-------------|
| xUnit | Modern testing framework |
| NUnit | Classic .NET testing |
| MSTest | Microsoft's testing framework |
| Jasmine | Angular unit testing |
| Jest | React testing |

## Generated Test Patterns

- **Unit Tests**: Isolated component testing
- **Integration Tests**: API and database testing
- **End-to-End Tests**: Full application flow testing
- **Snapshot Tests**: UI component testing

## Dependencies

- **Endpoint.Angular** - Angular test generation
- **Endpoint.DotNet** - .NET test generation

## Example Generated Test

```csharp
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _sut = new CustomerService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expected = new Customer { Id = customerId };
        _repositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync(customerId);

        // Assert
        Assert.Equal(expected, result);
    }
}
```

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
