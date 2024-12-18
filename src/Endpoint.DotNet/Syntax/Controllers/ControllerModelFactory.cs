// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Controllers;

public class ControllerFactory : IControllerFactory
{
    private readonly INamingConventionConverter namingConventionConverter;

    public ControllerFactory(INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter;
    }

    public ClassModel Create(ClassModel entity)
    {
        throw new NotImplementedException();
    }
}
