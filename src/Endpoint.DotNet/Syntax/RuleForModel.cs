// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Properties;

namespace Endpoint.DotNet.Syntax;

public class RuleForModel
{
    public RuleForModel(PropertyModel property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    public PropertyModel Property { get; set; }
}
