/*// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Attributes.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Endpoint.UnitTests;

public class AttributeSyntaxGenerationStrategyTests
{
    [Fact]
    public void CreateShould_ReturnAppropiateAttribute()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDotNetServices();

        var container = services.BuildServiceProvider();

        var sut = ActivatorUtilities.CreateInstance<AttributeSyntaxGenerationStrategy>(container);

        Assert.NotNull(sut);
    }

    [Fact]
    public async Task CreateShould_ReturnApiControllerAttribute()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDotNetServices();

        var container = services.BuildServiceProvider();

        var sut = ActivatorUtilities.CreateInstance<AttributeSyntaxGenerationStrategy>(container);

        var model = new AttributeModel()
        {
            Name = "ApiController",
        };

        var result = await sut.GenerateAsync(default, model);

        Assert.Equal("[ApiController]", result);
    }
}
*/