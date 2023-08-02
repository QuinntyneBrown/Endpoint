
using Endpoint.Core.Strategies.CSharp.Attributes;
using Endpoint.Core.Syntax.Attributes;
using Endpoint.Core.Syntax.Attributes.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;


namespace Endpoint.UnitTests.Core.Strategies.CSharp;

public class SwaggerOperationAttributeGenerationStrategyTests
{
    [Fact]
    public void ShouldCreateAttribute()
    {
        var expected = new string[4] {
            "[SwaggerOperation(",
            "    Summary = \"Get Show By Id.\",",
            "    Description = \"Get Show By Id.\"",
            ")]"
        };
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddCoreServices();

        var mockLogger = new Mock<ILogger<AttributeSyntaxGenerationStrategy>>().Object;

        var sut = new AttributeSyntaxGenerationStrategy(services.BuildServiceProvider(), mockLogger);

        var model = new AttributeModel(AttributeType.SwaggerOperation, "SwaggerOperation", new()
        {
            { "Summary", "Get Show By Id." },
            { "Description", "Get Show By Id." },
        });

        var actual = sut.Create(model);

        Assert.Equal(string.Join(Environment.NewLine, expected), actual);
    }
}

