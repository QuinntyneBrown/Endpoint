﻿using System.Collections.Generic;
using System.Linq;

namespace Endpoint.Core.Abstractions;

public class ArtifactGenerationStrategyFactory : IArtifactGenerationStrategyFactory
{
    private readonly IEnumerable<IArtifactGenerationStrategy> _strategies;
    public ArtifactGenerationStrategyFactory(IEnumerable<IArtifactGenerationStrategy> strategies)
    {
        _strategies = strategies;
    }
    public void CreateFor(object model, dynamic configuration = null)
    {
        var strategy = _strategies.Where(x => x.CanHandle(model, configuration))
            .OrderBy(x => x.Priority)
            .FirstOrDefault();

        strategy.Create(model);
    }
}