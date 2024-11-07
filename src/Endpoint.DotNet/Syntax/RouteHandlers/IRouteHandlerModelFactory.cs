// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.Syntax.RouteHandlers;

public interface IRouteHandlerFactory
{
    List<RouteHandlerModel> Create(string dbContextName, dynamic aggregateRoot);
}
