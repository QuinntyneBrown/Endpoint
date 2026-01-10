// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Syntax;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.ModernWebAppPattern.Core.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.ModernWebAppPlatform.Core.Tests;

public class SyntaxFactoryGenerationTests
{
    [Fact]
    public async Task SyntaxGenerator_returns_syntax_given_controller_get_by_id_method_model()
    {
        // ARRANGE
        var expected = """
            [HttpGet("{fooId}", Name = "GetById")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> GetByIdAsync([FromRoute]GetFooByIdRequest request)
            {
                var response = await _mediator.Send(request);

                if(response.Foo == null)
                {
                    return NotFound(request.FooId);
                }

                return Ok(response);
            }
            """;

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDotNetServices();

        services.AddModernWebAppPatternCoreServices();

        var container = services.BuildServiceProvider();

        var syntaxFactory = container.GetRequiredService<ISyntaxFactory>();

        var syntaxGenerator = container.GetRequiredService<ISyntaxGenerator>();

        Query query = new ()
        {
            Name = "GetFooById",
            Aggregate = new ("Foo")
            {
                BoundedContext = new ("Foos"),
            },
            Kind = RequestKind.GetById
        };

        var syntaxModel = await syntaxFactory.ControllerGetByIdMethodCreateAsync(new (), query);

        // ACT
        string actual = await syntaxGenerator.GenerateAsync(syntaxModel);

        // ASSERT
        Assert.Equal(expected.RemoveTrivia(), actual.RemoveTrivia());
    }
}
