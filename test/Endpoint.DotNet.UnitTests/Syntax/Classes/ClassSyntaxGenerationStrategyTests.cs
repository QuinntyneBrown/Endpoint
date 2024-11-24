// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Classes.Strategies;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.UnitTests.Classes;

public class ClassSyntaxGenerationStrategyTests
{
    [Fact]
    public async Task CreateShouldSerializeToExpectedString()
    {
        var expected = "public class Foo { }";

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDotNetServices();

        var container = services.BuildServiceProvider();

        var syntaxGenerator = container.GetRequiredService<ISyntaxGenerator>();

        var classModel = new ClassModel("Foo");

        classModel.Constructors =
        [
            new (classModel, classModel.Name),
        ];

        string result = await syntaxGenerator.GenerateAsync(classModel);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateConfigureServicesClass()
    {
        var expected = "public class Foo { }";

        var services = new ServiceCollection();

        services.AddLogging();


        var container = services.BuildServiceProvider();

        var syntaxGenerator = container.GetRequiredService<ISyntaxGenerator>();

        var sut = ActivatorUtilities.CreateInstance<ClassSyntaxGenerationStrategy>(container);

        var classModel = new ClassModel("ConfigureServices");

        var methodParam = new ParamModel()
        {
            Type = new Endpoint.DotNet.Syntax.Types.TypeModel("IServiceCollection"),
            Name = "services",
            ExtensionMethodParam = true,
        };

        var method = new MethodModel()
        {
            Name = "AddApplicationServices",
            ReturnType = new Endpoint.DotNet.Syntax.Types.TypeModel("void"),
            Static = true,
            Params = new List<ParamModel>() { methodParam },
        };

        classModel.Static = true;

        classModel.Methods.Add(method);

        var result = await sut.GenerateAsync(classModel, default);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateShouldSerializeToExpectedString_GivenAField()
    {
        var expected = "public class Foo { }";

        var services = new ServiceCollection();

        services.AddLogging();

        var container = services.BuildServiceProvider();

        var syntaxGenerator = container.GetRequiredService<ISyntaxGenerator>();

        var sut = ActivatorUtilities.CreateInstance<ClassSyntaxGenerationStrategy>(container);

        var classModel = new ClassModel("Foo");

        classModel.Fields = new List<FieldModel>()
        {
            new FieldModel()
            {
                Name = "_logger", Type = new Endpoint.DotNet.Syntax.Types.TypeModel()
            {
                Name = "ILogger",
                GenericTypeParameters = new List<Endpoint.DotNet.Syntax.Types.TypeModel>
                {
                    new Endpoint.DotNet.Syntax.Types.TypeModel() { Name = classModel.Name },
                },
            }, AccessModifier = AccessModifier.Private,
            },
        };

        classModel.Constructors = new List<ConstructorModel>()
        {
            new ConstructorModel(classModel, classModel.Name)
            {
                Params = new List<ParamModel>()
                {
                    new ParamModel
                    {
                        Type = new Endpoint.DotNet.Syntax.Types.TypeModel()
                        {
                            Name = "ILogger",
                            GenericTypeParameters = new List<Endpoint.DotNet.Syntax.Types.TypeModel>
                            {
                                new Endpoint.DotNet.Syntax.Types.TypeModel() { Name = classModel.Name },
                            },
                        },
                        Name = "logger",
                    },
                },
            },
        };

        var result = await sut.GenerateAsync(classModel,default);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ClassSyntaxGenerationStrategy_returns_valid_syntax_given_a_user_defined_type_model()
    {
        // ARRANGE
        var expected = $$"""
            public class Foo
            {
                private readonly bool _value;

                public Foo(bool value){
                    ArgumentNullException.ThrowIfNull(value);

                    _value = value;
                }

                public static implicit operator bool(Foo foo)
                {
                    return foo._value;
                }

                public static explicit operator Foo(bool value)
                {
                    return new Foo(value);
                }
            }
            """;

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDotNetServices();

        services.AddCoreServices(typeof(ClassSyntaxGenerationStrategy).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        var classFactory = serviceProvider.GetRequiredService<IClassFactory>();

        var userDefinedTypeModel = await classFactory.CreateUserDefinedTypeAsync("Foo", "bool");

        var sut = ActivatorUtilities.CreateInstance<ClassSyntaxGenerationStrategy>(serviceProvider);

        // ACT
        var actual = await sut.GenerateAsync(userDefinedTypeModel,default);

        // ASSERT
        Assert.Equal(expected.RemoveTrivia(), actual.RemoveTrivia());
    }
}
