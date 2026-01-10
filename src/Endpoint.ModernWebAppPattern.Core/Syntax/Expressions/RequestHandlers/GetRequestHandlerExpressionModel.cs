// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;

public class GetRequestHandlerExpressionModel : ExpressionModel
{

    public GetRequestHandlerExpressionModel(Query query)
        : base(string.Empty)
    {
        Query = query;
    }
    public Query Query { get; set; }
}
