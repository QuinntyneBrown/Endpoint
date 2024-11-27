// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core.Models;

/// <summary>
/// BoundedContext.
/// </summary>
public class BoundedContext
{
    public BoundedContext(string name)
    {
        Name = name;
    }

    public string Name { get; set; } = string.Empty;

    public List<AggregateModel> Aggregates { get; set; } = [];

    public List<Message> Handles { get; set; } = [];

    public string ProductName { get; set; }

}
