/*// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Properties;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Endpoint.UnitTests.PlantUmlParserStrategy;
public class CreateShould
{
    [Fact]
    public void CreateSolution_GivenAPackageInPlantUml()
    {
        var services = new ServiceCollection();

        services.AddCoreServices();

        var container = services.BuildServiceProvider();

        var sut = container.GetRequiredService<IPlantUmlParserStrategyFactory>();

        var result = sut.CreateFor(@"@startuml

package DashboardService  {
'dotNetType:classlib

class Order {

}

}

'classlib
package DashboardService  {

class Order {

}

}
");

        Assert.Equal(typeof(SolutionModel), result.GetType());

    }

    [Fact]
    public void CreateClass_GivenAPackageInPlantUml()
    {
        var services = new ServiceCollection();

        services.AddCoreServices();

        var container = services.BuildServiceProvider();

        var sut = container.GetRequiredService<IPlantUmlParserStrategyFactory>();

        var result = sut.CreateFor(@"class User {
+Task ExecuteAsync(CancellationToken stoppingToken)
-string Username
-string Password
}");

        Assert.Equal(typeof(CodeFileModel<ClassModel>), result.GetType());

    }

    [Fact]
    public void CreateMethod_GivenAPackageInPlantUml()
    {
        var services = new ServiceCollection();

        services.AddCoreServices();

        var container = services.BuildServiceProvider();

        var sut = container.GetRequiredService<IPlantUmlParserStrategyFactory>();

        var result = sut.CreateFor(@"+Task ExecuteAsync(CancellationToken stoppingToken)");

        Assert.Equal(typeof(MethodModel), result.GetType());
    }

    [Fact]
    public void CreateProperty_GivenAPackageInPlantUml()
    {
        var services = new ServiceCollection();

        services.AddCoreServices();

        var container = services.BuildServiceProvider();

        var sut = container.GetRequiredService<IPlantUmlParserStrategyFactory>();

        var result = sut.CreateFor(@"-int Id");

        Assert.Equal(typeof(PropertyModel), result.GetType());
    }

    [Fact]
    public void CreateField_GivenAPackageInPlantUml()
    {
        var services = new ServiceCollection();

        services.AddCoreServices();

        var container = services.BuildServiceProvider();

        var sut = container.GetRequiredService<IPlantUmlParserStrategyFactory>();

        var result = sut.CreateFor(@"-Task ExecuteAsync(CancellationToken stoppingToken)");

        Assert.Equal(typeof(FieldModel), result.GetType());
    }
}

*/