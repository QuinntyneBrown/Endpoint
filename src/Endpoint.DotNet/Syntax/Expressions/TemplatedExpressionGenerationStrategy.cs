// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Expressions;

public class TemplatedExpressionGenerationStrategy : ISyntaxGenerationStrategy<TemplateExpressionModel>
{
    private readonly ILogger<TemplatedExpressionGenerationStrategy> logger;
    private readonly ITemplateLocator templateLocator;

    public TemplatedExpressionGenerationStrategy(
        ILogger<TemplatedExpressionGenerationStrategy> logger,
        ITemplateLocator templateLocator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
    }

    public async Task<string> GenerateAsync(TemplateExpressionModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Templated Expression Body");

        StringBuilder sb = new StringBuilder();

        var result = string.Join(Environment.NewLine, templateLocator.Get(model.Template));

        sb.AppendLine(result);

        return sb.ToString();
    }
}