﻿using Endpoint.Core.Models.Options;

namespace Endpoint.Core.Strategies.WorkspaceSettingss.Update
{
    public interface IWorkspaceSettingsUpdateStrategy
    {
        bool CanHandle(WorkspaceSettingsModel previous, WorkspaceSettingsModel next);
        int Order { get; }
        void Update(WorkspaceSettingsModel previous, WorkspaceSettingsModel next);
    }
}