// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions;

public class ToDtoExpressionModel : ExpressionModel
{
    public ToDtoExpressionModel(AggregateModel aggregate) :
        base(string.Empty)
    {
        Aggregate = aggregate;
    }

    public AggregateModel Aggregate { get; set; }
}
