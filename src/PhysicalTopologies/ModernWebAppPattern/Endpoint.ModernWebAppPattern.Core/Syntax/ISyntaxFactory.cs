// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;

namespace Endpoint.ModernWebAppPattern.Core.Syntax;

public interface ISyntaxFactory
{
    Task<MethodModel> ControllerCreateMethodCreateAsync(ClassModel @class, Aggregate aggregate);

    Task<MethodModel> ControllerGetMethodCreateAsync(ClassModel @class, Aggregate aggregate);

    Task<MethodModel> ControllerGetByIdMethodCreateAsync(ClassModel @class, Aggregate aggregate);

    Task<MethodModel> ControllerUpdateMethodCreateAsync(ClassModel @class, Aggregate aggregate);

    Task<MethodModel> ControllerDeleteMethodCreateAsync(ClassModel @class, Aggregate aggregate);

}

