// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Expressions;
using System;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class ControllerGetExpressionModel : ExpressionModel
{
    public ControllerGetExpressionModel(ClassModel @class, Query query)
        : base(string.Empty)
    {
        Class = @class;
        Query = query;
    }

    public ClassModel Class { get; set; }
    public Query Query { get; set; }
}
