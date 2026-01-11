// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Expressions;
using System;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions.RequestHandlers;

public class UpdateRequestHandlerExpressionModel : ExpressionModel
{

    public UpdateRequestHandlerExpressionModel(Command command)
        : base(string.Empty)
    {
        Command = command;
    }
    public Command Command { get; set; }
}
