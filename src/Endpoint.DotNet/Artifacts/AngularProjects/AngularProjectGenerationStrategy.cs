// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.AngularProjects;

public class AngularProjectGenerationStrategy : IArtifactGenerationStrategy<AngularProjectModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<AngularProjectGenerationStrategy> logger;

    public AngularProjectGenerationStrategy(
        ICommandService commandService,
        ILogger<AngularProjectGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => throw new NotImplementedException();

    public bool CanHandle(AngularProjectModel model) => model is AngularProjectModel;

    public async Task GenerateAsync(AngularProjectModel model)
    {
        logger.LogInformation("Create Angular Project. {name}", model.Name);

        commandService.Start($"ng new {model.Name}", model.Directory);
    }
}
