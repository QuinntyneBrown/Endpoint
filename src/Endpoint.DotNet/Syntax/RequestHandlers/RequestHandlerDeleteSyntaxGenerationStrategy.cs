// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.RequestHandlers;

public class RequestHandlerDeleteSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RequestHandlerModel>
{
    private readonly ILogger<RequestHandlerDeleteSyntaxGenerationStrategy> logger;

    public RequestHandlerDeleteSyntaxGenerationStrategy(

        ILogger<RequestHandlerDeleteSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
        => model is RequestHandlerModel requestHandlerModel && requestHandlerModel.RouteType == RouteType.Delete;

    public async Task<string> GenerateAsync(RequestHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0} and type {1}.", model);

        var builder = StringBuilderCache.Acquire();

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
