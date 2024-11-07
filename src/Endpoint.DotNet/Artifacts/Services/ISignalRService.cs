// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Endpoint.DotNet.Artifacts.Services;

public interface ISignalRService
{
    Task Add(string directory);

    Task AddHub(string name, string directory);
}
