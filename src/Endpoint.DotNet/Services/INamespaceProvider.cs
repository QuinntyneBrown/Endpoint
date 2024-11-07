// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Services;

public interface INamespaceProvider
{
    string Get(string directory, int depth = 0);
}
