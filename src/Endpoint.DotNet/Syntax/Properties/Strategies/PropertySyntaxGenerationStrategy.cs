using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Properties.Strategies;

public class PropertySyntaxGenerationStrategy : ISyntaxGenerationStrategy<PropertyModel>
{
    private readonly ILogger<PropertySyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public PropertySyntaxGenerationStrategy(

        ILogger<PropertySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(PropertyModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var attribute in model.Attributes)
        {
            string attributeSyntax = await syntaxGenerator.GenerateAsync(attribute);

            builder.AppendLine(attributeSyntax);
        }

        if (model.IsClassProperty || model.ForceAccessModifier)
        {
            builder.Append(await syntaxGenerator.GenerateAsync(model.AccessModifier));

            builder.Append(" ");

            if (model.Required)
            {
                builder.Append("required ");
            }
        }

        builder.Append($"{await syntaxGenerator.GenerateAsync(model.Type)} {model.Name} {await syntaxGenerator.GenerateAsync(model.Accessors)}");

        if (model.IsClassProperty && !string.IsNullOrEmpty(model.DefaultValue))
        {
            builder.Append($" = {model.DefaultValue};");
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
