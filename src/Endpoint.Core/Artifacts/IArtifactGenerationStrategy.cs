// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.Artifacts;

/// <summary>
/// IArtifactGenerationStrategy.
/// </summary>
/// <typeparam name="T">Target.</typeparam>
public interface IArtifactGenerationStrategy<T>
{
    public bool CanHandle(object model) => model is T;

    public int GetPriority() => 1;

    Task GenerateAsync(T target);
}