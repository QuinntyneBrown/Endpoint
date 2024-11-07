// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Units;

namespace Endpoint.DotNet.Syntax.Units.Factories;

public interface ISyntaxUnitFactory
{
    Task<AggregateModel> CreateAsync(string name, List<PropertyModel> properties);

    Task CreateAsync();
}
