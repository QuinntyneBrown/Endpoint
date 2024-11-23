// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Endpoint.Core.Syntax;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Strategies;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Types;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Endpoint.UnitTests;

public class ClassSyntaxGenerationStrategyTests
{
    [Fact]
    public async Task CreateShouldSerializeToExpectedString()
    {
        var expected = "public class Foo { }";

        var services = new ServiceCollection();

        services.AddLogging();

        //services.AddCliServices();

        var container = services.BuildServiceProvider();

        var syntaxGenerator = container.GetRequiredService<ISyntaxGenerator>();

        var sut = ActivatorUtilities.CreateInstance<ClassSyntaxGenerationStrategy>(container);

        var classModel = new ClassModel("Foo");

        classModel.Constructors = new List<ConstructorModel>()
        {
            new ConstructorModel(classModel, classModel.Name),
        };

        var result = await sut.GenerateAsync(classModel, default);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateConfigureServicesClass()
    {
        var expected = "public class Foo { }";

        var services = new ServiceCollection();

        services.AddLogging();

        //services.AddCliServices();

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

        //services.AddCliServices();

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
}
