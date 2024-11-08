// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Endpoint.ModernWebAppPattern.Core.Models;

public class Microservice {

    public Microservice(string name, string boundedContextName, MicroseviceKind kind = MicroseviceKind.Api)
    {
        Name = name;
        BoundedContextName = boundedContextName;
        Kind = kind;
    }

    public string Name { get; set; }
    public string BoundedContextName { get; set; }

    public MicroseviceKind Kind { get; set; } = MicroseviceKind.Api;

}
