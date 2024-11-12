// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Expressions;
using System;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;

public class CreateRequestHandlerExpressionModel : ExpressionModel {

    public CreateRequestHandlerExpressionModel(Command command)
        :base(string.Empty)
    {
        Command = command;
    }
    public Command Command { get; set; }
}
