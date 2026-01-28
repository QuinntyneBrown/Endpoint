// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Engineering.CodeReview.Cli.Services;

public interface IGitService
{
    /// <summary>
    /// Gets the diff between a specified branch and the default branch of a repository.
    /// </summary>
    /// <param name="repositoryUrl">The URL of the git repository.</param>
    /// <param name="branchName">The name of the branch to compare.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The diff as a string.</returns>
    Task<string> GetDiffAsync(string repositoryUrl, string branchName, CancellationToken cancellationToken = default);
}
