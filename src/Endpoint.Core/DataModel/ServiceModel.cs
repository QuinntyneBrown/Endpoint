// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.Core.DataModel;

public class ServiceModel {
    public string Namespace { get; set; }
    public List<AggregateModel> Aggregates { get; set; } = [];
}
