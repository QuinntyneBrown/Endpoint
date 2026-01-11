// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions;

public class CommandRequestValidatorConstructorExpressionModel : ExpressionModel
{

    public CommandRequestValidatorConstructorExpressionModel(Command command)
        : base(string.Empty)
    {
        Command = command;
    }

    public Command Command { get; set; }
}
