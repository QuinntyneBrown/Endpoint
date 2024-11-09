// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;

namespace Endpoint.DotNet.Syntax.Controllers;

public interface IControllerMedthodFactory
{
    MethodModel Create(ClassModel entity, RouteType routeType);
}
