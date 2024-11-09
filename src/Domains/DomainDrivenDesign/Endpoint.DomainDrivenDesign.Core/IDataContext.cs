// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;

namespace Endpoint.DomainDrivenDesign.Core;

public interface IDataContext
{
    public string ProductName { get; set; }

    public List<BoundedContext> BoundedContexts { get; set; }

    public List<Message> Messages { get; set; }

}

