using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Properties.Strategies;

public class PropertyAccessorsSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<PropertyAccessorModel>>
{
    private readonly ILogger<PropertyAccessorsSyntaxGenerationStrategy> logger;

    public PropertyAccessorsSyntaxGenerationStrategy(

        ILogger<PropertyAccessorsSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(List<PropertyAccessorModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append("{ get; set; }");

        return builder.ToString();
    }
}