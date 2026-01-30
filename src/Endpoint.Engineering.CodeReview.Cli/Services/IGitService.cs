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
    /// <param name="url">The URL to a git repository or branch. Supports GitHub, GitLab, and self-hosted Git URLs.
    /// If the URL points to a specific branch (e.g., /tree/branch-name), that branch will be compared.
    /// If no branch is specified, defaults to "main".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The diff as a string.</returns>
    Task<string> GetDiffAsync(string url, CancellationToken cancellationToken = default);
}
