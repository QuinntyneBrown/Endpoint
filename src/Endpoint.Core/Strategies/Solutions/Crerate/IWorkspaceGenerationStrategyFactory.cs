﻿using Endpoint.Core.Models;

namespace Endpoint.Core.Strategies.Solutions.Crerate
{
    public interface IWorkspaceGenerationStrategyFactory
    {
        void CreateFor(WorkspaceSettingsModel model);
    }

}
