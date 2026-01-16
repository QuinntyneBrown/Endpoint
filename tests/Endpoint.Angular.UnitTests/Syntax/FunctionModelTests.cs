// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Syntax;

namespace Endpoint.Angular.UnitTests.Syntax;

public class FunctionModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var functionModel = new FunctionModel();

        // Assert
        Assert.NotNull(functionModel);
        Assert.NotNull(functionModel.Imports);
        Assert.Empty(functionModel.Imports);
        Assert.Equal(string.Empty, functionModel.Name);
        Assert.Equal(string.Empty, functionModel.Body);
    }

    [Fact]
    public void Name_ShouldBeSettableAndGettable()
    {
        // Arrange
        var functionModel = new FunctionModel();
        var name = "myFunction";

        // Act
        functionModel.Name = name;

        // Assert
        Assert.Equal(name, functionModel.Name);
    }

    [Fact]
    public void Body_ShouldBeSettableAndGettable()
    {
        // Arrange
        var functionModel = new FunctionModel();
        var body = "return true;";

        // Act
        functionModel.Body = body;

        // Assert
        Assert.Equal(body, functionModel.Body);
    }

    [Fact]
    public void Imports_ShouldBeSettableAndGettable()
    {
        // Arrange
        var functionModel = new FunctionModel();
        var imports = new List<ImportModel>
        {
            new ImportModel(),
            new ImportModel()
        };

        // Act
        functionModel.Imports = imports;

        // Assert
        Assert.Equal(imports, functionModel.Imports);
        Assert.Equal(2, functionModel.Imports.Count);
    }

    [Fact]
    public void Imports_CanAddItems()
    {
        // Arrange
        var functionModel = new FunctionModel();
        var import = new ImportModel();

        // Act
        functionModel.Imports.Add(import);

        // Assert
        Assert.Single(functionModel.Imports);
        Assert.Contains(import, functionModel.Imports);
    }

    [Fact]
    public void FunctionModel_ShouldInheritFromSyntaxModel()
    {
        // Arrange
        var functionModel = new FunctionModel();

        // Assert
        Assert.IsAssignableFrom<Endpoint.Syntax.SyntaxModel>(functionModel);
    }
}
