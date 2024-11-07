// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Solutions;

namespace Endpoint.DotNet.Services;

public interface ICodeAnalysisService
{
    SyntaxModel SyntaxModel { get; set; }

    SolutionModel SolutionModel { get; set; }

    Task<bool> IsNpmPackageInstalledAsync(string name);

    Task<bool> IsPackageInstalledAsync(string name, string directory);
}
