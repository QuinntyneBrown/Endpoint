// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.ModernWebAppPattern.Models;

namespace Endpoint.ModernWebAppPattern;

public interface IDataContext : Endpoint.DomainDrivenDesign.IDataContext
{
    List<Microservice> Microservices { get; set; }

}

