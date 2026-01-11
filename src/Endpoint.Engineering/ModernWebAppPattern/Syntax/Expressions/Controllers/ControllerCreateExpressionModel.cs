// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class ControllerCreateExpressionModel : ExpressionModel
{
    public ControllerCreateExpressionModel(ClassModel @class, Command command)
        : base(string.Empty)
    {
        Class = @class;
        Command = command;
    }

    public ClassModel Class { get; set; }
    public Command Command { get; set; }
}
