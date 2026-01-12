// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.AI.Services;

/// <summary>
/// Service for parsing HTML and extracting content optimized for LLM consumption.
/// </summary>
public interface IHtmlParserService
{
    /// <summary>
    /// Parses HTML content and extracts a token-efficient summary suitable for LLM consumption.
    /// </summary>
    /// <param name="htmlContent">The raw HTML content to parse.</param>
    /// <returns>A summarized version of the HTML body content.</returns>
    string ParseHtml(string htmlContent);

    /// <summary>
    /// Parses HTML from a file path and extracts a token-efficient summary suitable for LLM consumption.
    /// </summary>
    /// <param name="filePath">The path to the HTML file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A summarized version of the HTML body content.</returns>
    Task<string> ParseHtmlFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches HTML from a URL and extracts a token-efficient summary suitable for LLM consumption.
    /// </summary>
    /// <param name="url">The URL to fetch HTML from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A summarized version of the HTML body content.</returns>
    Task<string> ParseHtmlFromUrlAsync(string url, CancellationToken cancellationToken = default);
}
