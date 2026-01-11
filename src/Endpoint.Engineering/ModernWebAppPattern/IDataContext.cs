// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ModernWebAppPattern.Models;

namespace Endpoint.Engineering.ModernWebAppPattern;

public interface IDataContext : Endpoint.Engineering.DomainDrivenDesign.IDataContext
{
    List<Microservice> Microservices { get; set; }

}

