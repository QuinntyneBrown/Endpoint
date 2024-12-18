// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Artifacts.Services;

public interface IPlaywrightService
{
    void Create(string directory);

    void TestCreate(string description, string directory);
}