// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core.Models;

public class Request {

    public RequestKind Kind { get; set; }
    public Aggregate Aggregate { get; set; }
    public string ProductName { get; set; }
}
