// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Properties;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Entities;

public class IdPropertySyntaxGenerationStrategy : ISyntaxGenerationStrategy<PropertyModel>
{
    private readonly ILogger<IdPropertySyntaxGenerationStrategy> logger;
    private readonly ICodeAnalysisService codeAnalysisService;

    public IdPropertySyntaxGenerationStrategy(

        ICodeAnalysisService codeAnalysisService,
        ILogger<IdPropertySyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeAnalysisService = codeAnalysisService ?? throw new ArgumentNullException(nameof(codeAnalysisService));
    }

    public int GetPriority => 1;

    public bool CanHandle(object model)
        => model is PropertyModel propertyModel && propertyModel.Id;

    public async Task<string> GenerateAsync(PropertyModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        return builder.ToString();
    }
}
