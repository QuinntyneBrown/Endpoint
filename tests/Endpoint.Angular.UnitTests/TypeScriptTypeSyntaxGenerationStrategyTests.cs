// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.UnitTests;

public class TypeScriptTypeSyntaxGenerationStrategyTests
{
    private readonly Mock<ILogger<TypeScriptTypeSyntaxGenerationStrategy>> _mockLogger;
    private readonly Mock<INamingConventionConverter> _mockNamingConverter;
    private readonly TypeScriptTypeSyntaxGenerationStrategy _sut;

    public TypeScriptTypeSyntaxGenerationStrategyTests()
    {
        _mockLogger = new Mock<ILogger<TypeScriptTypeSyntaxGenerationStrategy>>();
        _mockNamingConverter = new Mock<INamingConventionConverter>();

        // Setup default behavior for naming convention converter
        _mockNamingConverter
            .Setup(x => x.Convert(It.IsAny<NamingConvention>(), It.IsAny<string>()))
            .Returns((NamingConvention convention, string value) =>
                convention == NamingConvention.PascalCase ? ToPascalCase(value) : ToCamelCase(value));

        _sut = new TypeScriptTypeSyntaxGenerationStrategy(_mockLogger.Object, _mockNamingConverter.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TypeScriptTypeSyntaxGenerationStrategy(null!, _mockNamingConverter.Object));
    }

    [Fact]
    public void Constructor_WithNullNamingConventionConverter_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TypeScriptTypeSyntaxGenerationStrategy(_mockLogger.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            new TypeScriptTypeSyntaxGenerationStrategy(_mockLogger.Object, _mockNamingConverter.Object));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region GenerateAsync with Properties Tests

    [Fact]
    public async Task GenerateAsync_WithSingleProperty_ShouldGenerateCorrectType()
    {
        // Arrange
        var model = new TypeScriptTypeModel("User")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Id", Type = new TypeModel("number") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("export type User = {", result);
        Assert.Contains("id?: number;", result);
        Assert.Contains("};", result);
    }

    [Fact]
    public async Task GenerateAsync_WithMultipleProperties_ShouldGenerateAllProperties()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Product")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Id", Type = new TypeModel("number") },
                new() { Name = "Name", Type = new TypeModel("string") },
                new() { Name = "Price", Type = new TypeModel("number") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("export type Product = {", result);
        Assert.Contains("id?: number;", result);
        Assert.Contains("name?: string;", result);
        Assert.Contains("price?: number;", result);
    }

    [Fact]
    public async Task GenerateAsync_WithNoProperties_ShouldGenerateEmptyType()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Empty")
        {
            Properties = new List<PropertyModel>()
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("export type Empty = {", result);
        Assert.Contains("};", result);
    }

    [Fact]
    public async Task GenerateAsync_WithBooleanProperty_ShouldGenerateCorrectly()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Settings")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "IsEnabled", Type = new TypeModel("boolean") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("isEnabled?: boolean;", result);
    }

    [Fact]
    public async Task GenerateAsync_WithArrayProperty_ShouldGenerateCorrectly()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Order")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Items", Type = new TypeModel("string[]") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("items?: string[];", result);
    }

    #endregion

    #region Name Convention Conversion Tests

    [Fact]
    public async Task GenerateAsync_ShouldConvertTypeNameToPascalCase()
    {
        // Arrange
        var model = new TypeScriptTypeModel("userProfile")
        {
            Properties = new List<PropertyModel>()
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        _mockNamingConverter.Verify(
            x => x.Convert(NamingConvention.PascalCase, "userProfile"),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_ShouldConvertPropertyNameToCamelCase()
    {
        // Arrange
        var model = new TypeScriptTypeModel("User")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "FirstName", Type = new TypeModel("string") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        _mockNamingConverter.Verify(
            x => x.Convert(NamingConvention.CamelCase, "FirstName"),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_ShouldConvertPropertyTypeToCamelCase()
    {
        // Arrange
        var model = new TypeScriptTypeModel("User")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Id", Type = new TypeModel("Number") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        _mockNamingConverter.Verify(
            x => x.Convert(NamingConvention.CamelCase, "Number"),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_WithMultipleProperties_ShouldConvertAllNames()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Customer")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "FirstName", Type = new TypeModel("string") },
                new() { Name = "LastName", Type = new TypeModel("string") },
                new() { Name = "EmailAddress", Type = new TypeModel("string") }
            }
        };

        // Act
        await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        _mockNamingConverter.Verify(
            x => x.Convert(NamingConvention.CamelCase, It.IsAny<string>()),
            Times.AtLeast(6)); // 3 property names + 3 type names
    }

    #endregion

    #region Output Format Tests

    [Fact]
    public async Task GenerateAsync_ShouldStartWithExportKeyword()
    {
        // Arrange
        var model = new TypeScriptTypeModel("TestType")
        {
            Properties = new List<PropertyModel>()
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.StartsWith("export type", result);
    }

    [Fact]
    public async Task GenerateAsync_ShouldContainEqualsSign()
    {
        // Arrange
        var model = new TypeScriptTypeModel("TestType")
        {
            Properties = new List<PropertyModel>()
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("= {", result);
    }

    [Fact]
    public async Task GenerateAsync_ShouldEndWithClosingBraceAndSemicolon()
    {
        // Arrange
        var model = new TypeScriptTypeModel("TestType")
        {
            Properties = new List<PropertyModel>()
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("};", result);
    }

    [Fact]
    public async Task GenerateAsync_PropertiesShouldBeOptional()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Optional")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Field", Type = new TypeModel("string") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("?:", result);
    }

    [Fact]
    public async Task GenerateAsync_PropertiesShouldEndWithSemicolon()
    {
        // Arrange
        var model = new TypeScriptTypeModel("TestType")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Field", Type = new TypeModel("string") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        var lines = result.Split(Environment.NewLine);
        var propertyLine = lines.FirstOrDefault(l => l.Contains("field"));
        Assert.NotNull(propertyLine);
        Assert.EndsWith(";", propertyLine.Trim());
    }

    #endregion

    #region Complex Type Tests

    [Fact]
    public async Task GenerateAsync_WithComplexModel_ShouldGenerateCorrectStructure()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Address")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "Street", Type = new TypeModel("string") },
                new() { Name = "City", Type = new TypeModel("string") },
                new() { Name = "State", Type = new TypeModel("string") },
                new() { Name = "ZipCode", Type = new TypeModel("string") },
                new() { Name = "Country", Type = new TypeModel("string") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("export type Address = {", result);
        Assert.Contains("street?:", result);
        Assert.Contains("city?:", result);
        Assert.Contains("state?:", result);
        Assert.Contains("zipCode?:", result);
        Assert.Contains("country?:", result);
    }

    [Fact]
    public async Task GenerateAsync_WithDateProperty_ShouldGenerateCorrectly()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Event")
        {
            Properties = new List<PropertyModel>
            {
                new() { Name = "StartDate", Type = new TypeModel("Date") },
                new() { Name = "EndDate", Type = new TypeModel("Date") }
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("startDate?:", result);
        Assert.Contains("endDate?:", result);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GenerateAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Test")
        {
            Properties = new List<PropertyModel>()
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _sut.GenerateAsync(model, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region TypeScriptTypeModel Tests

    [Fact]
    public void TypeScriptTypeModel_Constructor_ShouldSetName()
    {
        // Arrange & Act
        var model = new TypeScriptTypeModel("TestModel");

        // Assert
        Assert.Equal("TestModel", model.Name);
    }

    [Fact]
    public void TypeScriptTypeModel_Constructor_ShouldInitializeEmptyPropertiesList()
    {
        // Arrange & Act
        var model = new TypeScriptTypeModel("TestModel");

        // Assert
        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void TypeScriptTypeModel_Name_ShouldBeSettable()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Original");

        // Act
        model.Name = "NewName";

        // Assert
        Assert.Equal("NewName", model.Name);
    }

    [Fact]
    public void TypeScriptTypeModel_Properties_ShouldBeSettable()
    {
        // Arrange
        var model = new TypeScriptTypeModel("Test");
        var properties = new List<PropertyModel>
        {
            new() { Name = "Prop1", Type = new TypeModel("string") }
        };

        // Act
        model.Properties = properties;

        // Assert
        Assert.Single(model.Properties);
    }

    #endregion

    #region PropertyModel Tests

    [Fact]
    public void PropertyModel_Name_ShouldBeSettable()
    {
        // Arrange
        var property = new PropertyModel();

        // Act
        property.Name = "TestProperty";

        // Assert
        Assert.Equal("TestProperty", property.Name);
    }

    [Fact]
    public void PropertyModel_Type_ShouldBeSettable()
    {
        // Arrange
        var property = new PropertyModel();

        // Act
        property.Type = new TypeModel("string");

        // Assert
        Assert.Equal("string", property.Type.Name);
    }

    #endregion

    #region Helper Methods

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    #endregion
}
