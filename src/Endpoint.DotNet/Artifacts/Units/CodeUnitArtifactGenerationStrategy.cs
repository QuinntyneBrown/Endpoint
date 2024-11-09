// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Documents;

namespace Endpoint.DotNet.Artifacts.Units;

public class Folder<T>
    where T : DocumentModel
{
}

public abstract class CodeUnitArtifactGenerationStrategy<T> : GenericArtifactGenerationStrategy<Folder<T>>
    where T : DocumentModel
{
    public override Task GenerateAsync(IArtifactGenerator generator, Folder<T> model)
    {
        throw new NotImplementedException();
    }
}
