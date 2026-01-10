// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class ControllerGetByIdExpressionModel : ExpressionModel
{
    public ControllerGetByIdExpressionModel(ClassModel @class, Query query)
        : base(string.Empty)
    {
        Class = @class;
        Query = query;
    }

    public ClassModel Class { get; set; }
    public Query Query { get; set; }
}
