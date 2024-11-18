// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.UnitTests.AttributeSyntaxGenerationStrategy;

using AttributeSyntaxGenerationStrategy = Endpoint.DotNet.Syntax.Attributes.Strategies.AttributeSyntaxGenerationStrategy;

public class CreateShould
{
    [Fact]
    public async Task CreateExpectedSyntax_GivenValidModel()
    {
        // ARRANGE
        var services = new ServiceCollection();

        services.AddDotNetServices();

        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        var expected = "[Fact]";

        // ACT
        var sut = serviceProvider.GetRequiredService<ISyntaxGenerator>();

        // ASSERT
        var result = await sut.GenerateAsync(new AttributeModel() { Name = "Fact" });

        Assert.Equal(expected, result);
    }
}
