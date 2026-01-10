// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Attributes.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.UnitTests.Syntax.Attributes;

public class AttributeSyntaxGenerationStrategyTests
{
    [Fact]
    public async Task AttributeSyntaxGenerationStrategy_returns_syntax_given_model()
    {
        // ARRANGE
        var expected = string.Empty;

        var model = new AttributeModel();

        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var sut = ActivatorUtilities.CreateInstance<AttributeSyntaxGenerationStrategy>(serviceProvider);

        // ACT
        var actual = await sut.GenerateAsync(model, default);

        // ASSERT
        Assert.Equal(expected, actual);
    }
}
