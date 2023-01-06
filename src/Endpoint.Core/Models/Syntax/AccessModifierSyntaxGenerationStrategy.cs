using Endpoint.Core.Abstractions;
using Endpoint.Core.Enums;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.Core.Models.Syntax;

public class AccessModifierSyntaxGenerationStrategy : SyntaxGenerationStrategyBase<AccessModifier>
{
    private readonly ILogger<AccessModifierSyntaxGenerationStrategy> _logger;
    public AccessModifierSyntaxGenerationStrategy(
        IServiceProvider serviceProvider,
        ILogger<AccessModifierSyntaxGenerationStrategy> logger) 
        : base(serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override string Create(ISyntaxGenerationStrategyFactory syntaxGenerationStrategyFactory, AccessModifier model, dynamic configuration = null)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append(model switch
        {
            AccessModifier.Public => "public",
            AccessModifier.Protected => "protected",
            _ => "public"
        });

        return builder.ToString();
    }
}