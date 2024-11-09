// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Endpoint.DotNet.Artifacts.Projects.Services;

public interface IApiProjectService
{
    Task ControllerCreateAsync(string entityName, bool empty, string directory);

    Task ControllerMethodAdd(string name, string controller, string route, string directory);
}
