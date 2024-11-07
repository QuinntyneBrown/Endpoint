﻿// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Artifacts;

public interface IArtifactParser
{
    Task<T> ParseAsync<T>(string valueOrDirectoryOrPath)
        where T : class;
}