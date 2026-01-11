// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

namespace Endpoint.DotNet.UnitTests.Syntax.Types;

public class TypeModelTests
{
    [Fact]
    public void TypeModel_ShouldCreateInstance_WithName()
    {
        // Arrange & Act
        var typeModel = new TypeModel("string");

        // Assert
        Assert.NotNull(typeModel);
        Assert.Equal("string", typeModel.Name);
        Assert.NotNull(typeModel.GenericTypeParameters);
        Assert.Empty(typeModel.GenericTypeParameters);
    }

    [Fact]
    public void TypeModel_ShouldCreateInstance_WithoutName()
    {
        // Arrange & Act
        var typeModel = new TypeModel();

        // Assert
        Assert.NotNull(typeModel);
        Assert.Null(typeModel.Name);
        Assert.NotNull(typeModel.GenericTypeParameters);
    }

    [Fact]
    public void TaskOf_ShouldCreateGenericTaskType()
    {
        // Arrange & Act
        var taskType = TypeModel.TaskOf("string");

        // Assert
        Assert.NotNull(taskType);
        Assert.Equal("Task", taskType.Name);
        Assert.Single(taskType.GenericTypeParameters);
        Assert.Equal("string", taskType.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void DbSetOf_ShouldCreateGenericDbSetType()
    {
        // Arrange & Act
        var dbSetType = TypeModel.DbSetOf("User");

        // Assert
        Assert.NotNull(dbSetType);
        Assert.Equal("DbSet", dbSetType.Name);
        Assert.Single(dbSetType.GenericTypeParameters);
        Assert.Equal("User", dbSetType.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void LoggerOf_ShouldCreateGenericLoggerType()
    {
        // Arrange & Act
        var loggerType = TypeModel.LoggerOf("MyClass");

        // Assert
        Assert.NotNull(loggerType);
        Assert.Equal("ILogger", loggerType.Name);
        Assert.Single(loggerType.GenericTypeParameters);
        Assert.Equal("MyClass", loggerType.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void ListOf_ShouldCreateGenericListType()
    {
        // Arrange & Act
        var listType = TypeModel.ListOf("int");

        // Assert
        Assert.NotNull(listType);
        Assert.Equal("List", listType.Name);
        Assert.Single(listType.GenericTypeParameters);
        Assert.Equal("int", listType.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void CreateTaskOfActionResultOf_ShouldCreateNestedGenericType()
    {
        // Arrange & Act
        var taskActionResultType = TypeModel.CreateTaskOfActionResultOf("UserDto");

        // Assert
        Assert.NotNull(taskActionResultType);
        Assert.Equal("Task", taskActionResultType.Name);
        Assert.Single(taskActionResultType.GenericTypeParameters);
        
        var actionResultType = taskActionResultType.GenericTypeParameters[0];
        Assert.Equal("ActionResult", actionResultType.Name);
        Assert.Single(actionResultType.GenericTypeParameters);
        Assert.Equal("UserDto", actionResultType.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void TypeModel_ShouldAllowSettingNullableProperty()
    {
        // Arrange
        var typeModel = new TypeModel("string");

        // Act
        typeModel.Nullable = true;

        // Assert
        Assert.True(typeModel.Nullable);
    }

    [Fact]
    public void TypeModel_ShouldAllowSettingInterfaceProperty()
    {
        // Arrange
        var typeModel = new TypeModel("IService");

        // Act
        typeModel.Interface = true;

        // Assert
        Assert.True(typeModel.Interface);
    }

    [Fact]
    public void TypeModel_TaskStaticField_ShouldBeInitialized()
    {
        // Assert
        Assert.NotNull(TypeModel.Task);
        Assert.Equal("Task", TypeModel.Task.Name);
    }
}
