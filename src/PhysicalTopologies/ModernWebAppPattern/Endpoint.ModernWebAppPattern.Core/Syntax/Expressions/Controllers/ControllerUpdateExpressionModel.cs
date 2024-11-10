// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class ControllerUpdateExpressionModel : ExpressionModel
{
    public ControllerUpdateExpressionModel(ClassModel @class, Aggregate aggregate)
        : base(string.Empty)
    {
        Class = @class;
        Aggregate = aggregate;
    }

    public ClassModel Class { get; set; }
    public Aggregate Aggregate { get; set; }
}
