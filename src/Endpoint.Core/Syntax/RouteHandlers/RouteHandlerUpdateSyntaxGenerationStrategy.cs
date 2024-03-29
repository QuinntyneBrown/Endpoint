// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Extensions.Logging;

namespace Endpoint.Core.Syntax.RouteHandlers;

public class RouteHandlerUpdateSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<RouteHandlerModel>
{
    private readonly ILogger<RouteHandlerSyntaxGenerationStrategy> logger;

    public RouteHandlerUpdateSyntaxGenerationStrategy(

        ILogger<RouteHandlerSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
    {
        if (model is RouteHandlerModel routeHandlerModel)
        {
            return routeHandlerModel.Type == RouteType.Update;
        }

        return false;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, RouteHandlerModel model)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        return builder.ToString();
    }
}
