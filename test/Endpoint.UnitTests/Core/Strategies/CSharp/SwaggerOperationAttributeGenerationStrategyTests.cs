// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Endpoint.Core.Syntax.Attributes;
using Endpoint.Core.Syntax.Attributes.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.UnitTests.Core.Strategies.CSharp;

public class SwaggerOperationAttributeGenerationStrategyTests
{
    [Fact]
    public async Task ShouldCreateAttribute()
    {
        var expected = new string[4]
        {
            "[SwaggerOperation(",
            "    Summary = \"Get Show By Id.\",",
            "    Description = \"Get Show By Id.\"",
            ")]",
        };
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddCoreServices();

        var mockLogger = new Mock<ILogger<AttributeSyntaxGenerationStrategy>>().Object;

        var sut = new AttributeSyntaxGenerationStrategy(mockLogger);

        var model = new AttributeModel(AttributeType.SwaggerOperation, "SwaggerOperation", new ()
        {
            { "Summary", "Get Show By Id." },
            { "Description", "Get Show By Id." },
        });

        var actual = await sut.GenerateAsync(default, model);

        Assert.Equal(string.Join(Environment.NewLine, expected), actual);
    }
}
