// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Artifacts;

public interface IArtifactGenerationStrategy
{
    int GetPriority();

    Task<bool> GenerateAsync(IArtifactGenerator generator, object target);
}
