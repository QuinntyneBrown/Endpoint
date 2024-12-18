// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Syntax.Entities;

namespace Endpoint.DotNet.Syntax.RouteHandlers;

public class RouteHandlerModel
{
    public RouteHandlerModel(string name, string pattern, string dbContextName, EntityModel entity, RouteType routeHandlerType)
    {
        Name = name;
        Pattern = pattern;
        DbContextName = dbContextName;
        Entity = entity;
        Type = routeHandlerType;
        Produces = new List<ProducesAttributeModel>();
    }

    public string Name { get; private set; }

    public string Pattern { get; private set; }

    public string DbContextName { get; private set; }

    public EntityModel Entity { get; private set; }

    public RouteType Type { get; private set; }

    public List<ProducesAttributeModel> Produces { get; private set; }
}
