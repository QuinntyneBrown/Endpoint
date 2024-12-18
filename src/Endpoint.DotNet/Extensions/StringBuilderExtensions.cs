// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.DotNet.Extensions;

public static class StringBuilderExtensions { 

    public static StringBuilder AppendDoubleLine(this StringBuilder builder, string value)
        => builder.AppendLine(value)
            .AppendLine();

    public static ExpressionModel ToExpression(this StringBuilder builder)
    {
        return new ExpressionModel(StringBuilderCache.GetStringAndRelease(builder));
    }

}
