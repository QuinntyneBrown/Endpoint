// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.Artifacts;

public interface IArtifactGenerationStrategy<T>
{
    public int GetPriority() => 1;

    Task GenerateAsync(T target);
}