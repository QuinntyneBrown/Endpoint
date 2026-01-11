// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax;

public interface ISyntaxFactory
{
    Task<MethodModel> ControllerCreateMethodCreateAsync(ClassModel @class, Command command);

    Task<MethodModel> ControllerGetMethodCreateAsync(ClassModel @class, Query query);

    Task<MethodModel> ControllerGetByIdMethodCreateAsync(ClassModel @class, Query query);

    Task<MethodModel> ControllerUpdateMethodCreateAsync(ClassModel @class, Command command);

    Task<MethodModel> ControllerDeleteMethodCreateAsync(ClassModel @class, Command command);

}

