// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;

namespace Endpoint.DotNet.Syntax.Controllers;

public class ControllerMethodFactory : IControllerMedthodFactory
{
    private readonly INamingConventionConverter namingConventionConverter;

    public ControllerMethodFactory(INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter;
    }

    public MethodModel Create(ClassModel entity, RouteType routeType)
    {
        throw new NotImplementedException();
    }
}
