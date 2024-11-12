// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions;

public class ToDtoExpressionModel : ExpressionModel
{
    public ToDtoExpressionModel(Aggregate aggregate) : 
        base(string.Empty)
    {
        Aggregate = aggregate;
    }

    public Aggregate Aggregate { get; set; }
}
