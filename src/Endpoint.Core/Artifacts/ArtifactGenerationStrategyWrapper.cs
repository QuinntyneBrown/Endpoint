// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.Artifacts;

public abstract class ArtifactGenerationStrategyWrapper<T> : ArtifactGenerationStrategyBase {

    public abstract Task GenerateAsync(T target, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
