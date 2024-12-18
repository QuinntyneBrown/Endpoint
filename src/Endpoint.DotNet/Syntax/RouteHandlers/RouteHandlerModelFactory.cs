// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.Syntax.RouteHandlers;

public class RouteHandlerFactory : IRouteHandlerFactory
{
    public List<RouteHandlerModel> Create(string dbContextName, dynamic aggregateRoot)
    {
        var resourceNameToken = (SyntaxToken)aggregateRoot.Name;

        var routes = new List<RouteHandlerModel>
        {
            new RouteHandlerModel(
            $"Create{resourceNameToken.PascalCase}",
            $"/{resourceNameToken.SnakeCasePlural}",
            dbContextName,
            aggregateRoot,
            RouteType.Create),

            new RouteHandlerModel(
            $"Get{resourceNameToken.PascalCasePlural}",
            $"/{resourceNameToken.SnakeCasePlural}",
            dbContextName,
            aggregateRoot,
            RouteType.Get),

            new RouteHandlerModel(
            $"Get{resourceNameToken.PascalCase}ById",
            $"/{resourceNameToken.SnakeCasePlural}",
            dbContextName,
            aggregateRoot,
            RouteType.GetById),

            new RouteHandlerModel(
            $"Update{resourceNameToken.PascalCase}",
            $"/{resourceNameToken.SnakeCasePlural}",
            dbContextName,
            aggregateRoot,
            RouteType.Update),

            new RouteHandlerModel(
            $"Delete{resourceNameToken.PascalCase}",
            $"/{resourceNameToken.SnakeCasePlural}",
            dbContextName,
            aggregateRoot,
            RouteType.Delete),
        };

        return routes;
    }
}
