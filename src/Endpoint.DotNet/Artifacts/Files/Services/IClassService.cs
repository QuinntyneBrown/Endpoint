// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.Artifacts.Files.Services;

public interface IClassService
{
    Task CreateAsync(string name, List<KeyValuePair<string, string>> keyValuePairs, List<string> implements, string directory);

    Task UnitTestCreateAsync(string name, string methods, string directory);
}
