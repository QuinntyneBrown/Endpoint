// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Endpoint.DomainDrivenDesign.Core;

public interface IDataContext
{
    public string ProductName { get; set; }

}

