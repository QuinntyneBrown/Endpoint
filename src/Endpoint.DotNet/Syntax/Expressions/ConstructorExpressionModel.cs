﻿// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;

namespace Endpoint.DotNet.Syntax.Expressions;

public class ConstructorExpressionModel : ExpressionModel
{
    public ConstructorExpressionModel(ClassModel @class, ConstructorModel constructor)
        : base(string.Empty)
    {
        Class = @class;
        Constructor = constructor;
    }

    public ClassModel Class { get; set; }

    public ConstructorModel Constructor { get; set; }
}