// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.DataModel;

public class AggregateModel {
    public string Name { get; set; }
    public List<PropertyModel> Properties { get; set; } = [];
}
