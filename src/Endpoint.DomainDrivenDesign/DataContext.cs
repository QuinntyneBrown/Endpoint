// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Models;

namespace Endpoint.DomainDrivenDesign;

public class DataContext : IDataContext
{
    public DataContext()
    {

    }

    public string ProductName { get; set; } = string.Empty;

    public List<BoundedContext> BoundedContexts { get; set; } = [];

    public List<Message> Messages { get; set; } = [];

}

