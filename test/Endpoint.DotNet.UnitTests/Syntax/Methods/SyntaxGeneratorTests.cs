// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Endpoint.DotNet.UnitTests.Syntax.Methods;

public class SyntaxGeneratorTests
{

    [Fact]
    public async Task ShouldWork()
    {


        var services = new ServiceCollection();

        services.AddLogging();

        services.AddCoreServices(typeof(MethodModel).Assembly);

        services.AddModernWebAppPatternCoreServices();

        services.AddDotNetServices();

        var container = services.BuildServiceProvider();

        var syntaxGenerator = container.GetRequiredService<ISyntaxGenerator>();

        var handlers = container.GetService<IEnumerable<ISyntaxGenerationStrategy<MethodModel>>>();

        Assert.NotNull(handlers);
    }
}
