// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Entities;

namespace Endpoint.DotNet.Options;

public class UpdateCleanArchitectureMicroserviceOptions
{
    public string Directory { get; set; }

    public EntityModel Entity { get; set; }
}
