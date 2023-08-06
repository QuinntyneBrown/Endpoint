﻿// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Endpoint.Core.Abstractions;

public interface IArtifactGenerator
{
    Task CreateAsync(object model, dynamic context = null);
    Task GenerateAsync<T>(T model, dynamic context = null);
}
