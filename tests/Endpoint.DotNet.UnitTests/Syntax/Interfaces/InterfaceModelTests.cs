// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;
using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

namespace Endpoint.DotNet.UnitTests.Syntax.Interfaces;

public class InterfaceModelTests
{
    [Fact]
    public void InterfaceModel_ShouldCreateInstance_WithEmptyConstructor()
    {
        // Arrange & Act
        var interfaceModel = new InterfaceModel();

        // Assert
        Assert.NotNull(interfaceModel);
        Assert.NotNull(interfaceModel.Implements);
        Assert.NotNull(interfaceModel.Methods);
        Assert.Empty(interfaceModel.Implements);
        Assert.Empty(interfaceModel.Methods);
    }

    [Fact]
    public void InterfaceModel_ShouldCreateInstance_WithName()
    {
        // Arrange & Act
        var interfaceModel = new InterfaceModel("IUserService");

        // Assert
        Assert.NotNull(interfaceModel);
        Assert.Equal("IUserService", interfaceModel.Name);
        Assert.NotNull(interfaceModel.Methods);
    }

    [Fact]
    public void AddMethod_ShouldAddMethodWithInterfaceFlag()
    {
        // Arrange
        var interfaceModel = new InterfaceModel("IService");
        var method = new MethodModel
        {
            Name = "GetData",
            ReturnType = new TypeModel("Task"),
        };

        // Act
        interfaceModel.AddMethod(method);

        // Assert
        Assert.Single(interfaceModel.Methods);
        Assert.True(method.Interface);
        Assert.Equal("GetData", interfaceModel.Methods[0].Name);
    }

    [Fact]
    public void InterfaceModel_ShouldAllowAddingImplements()
    {
        // Arrange
        var interfaceModel = new InterfaceModel("IService");
        var baseInterface = new TypeModel("IBaseService");

        // Act
        interfaceModel.Implements.Add(baseInterface);

        // Assert
        Assert.Single(interfaceModel.Implements);
        Assert.Equal("IBaseService", interfaceModel.Implements[0].Name);
    }
}
