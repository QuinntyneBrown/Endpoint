// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Endpoint.UnitTests.MethodPlantUmlParsingStrategy;

using MethodPlantUmlParsingStrategy = Endpoint.DotNet.Syntax.Methods.Strategies.MethodPlantUmlParsingStrategy;

public class ParseAsyncShould
{
    [Fact]
    public async Task ReturnMethodModel_GivenPlantUmlString()
    {
        // ARRANGE
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton<IContext, Context>();

        var serviceProvider = services.BuildServiceProvider();

        var context = serviceProvider.GetService<IContext>();

        context.Set(new MethodModel() { Interface = true });

        var sut = ActivatorUtilities.CreateInstance<MethodPlantUmlParsingStrategy>(serviceProvider);

        // ACT
        var result = await sut.ParseAsync(default, "+void Lock()");

        // ASSERT
        Assert.NotNull(result);
    }
}
