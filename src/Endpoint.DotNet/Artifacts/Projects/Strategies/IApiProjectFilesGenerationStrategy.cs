// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Options;

namespace Endpoint.DotNet.Artifacts.Projects.Strategies;

public interface IApiProjectFilesGenerationStrategy
{
    void Build(dynamic settings);

    void BuildAdditionalResource(string additionalResource, dynamic settings);

    void AddGenerateDocumentationFile(string csProjPath);
}
