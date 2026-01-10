// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.ModernWebAppPattern.Core.Models;

namespace Endpoint.ModernWebAppPattern.Core;

public interface IDataContext : Endpoint.DomainDrivenDesign.Core.IDataContext
{
    List<Microservice> Microservices { get; set; }

}

