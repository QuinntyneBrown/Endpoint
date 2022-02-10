﻿using Endpoint.Core.Models;
using System.Collections.Generic;

namespace Endpoint.Core.Services
{
    public interface ISolutionFileService
    {
        public Settings Build(string name, string properties, string dbContextName, bool useShortIdProperty, bool useIntIdPropertyType, string resource, string directory, bool isMicroserviceArchitecture, List<string> plugins);

        public Settings Build(string name, string properties, string dbContextName, bool useShortIdProperty, bool useIntIdPropertyType, List<string> resources, string directory, bool isMicroserviceArchitecture, List<string> plugins);
    }
}
