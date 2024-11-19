// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.Artifacts;

public interface IArtifactGenerationStrategy
{
    int GetPriority();

    Task<bool> GenerateAsync(IArtifactGenerator generator, object target);
}

public interface IArtifactGenerationStrategy<T>
{
    int GetPriority();

    Task<bool> GenerateAsync(T target);
}