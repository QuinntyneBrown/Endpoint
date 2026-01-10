// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Models;

public class Request
{

    public Request()
    {

    }

    public RequestKind Kind { get; set; }
    public AggregateModel Aggregate { get; set; }
    public string ProductName { get; set; }
}
