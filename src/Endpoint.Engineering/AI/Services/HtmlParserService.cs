// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.AI.Services;

/// <summary>
/// Service for parsing HTML and extracting content optimized for LLM consumption.
/// Focuses on extracting meaningful content while minimizing token usage.
/// </summary>
public partial class HtmlParserService : IHtmlParserService
{
    private readonly ILogger<HtmlParserService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public HtmlParserService(ILogger<HtmlParserService> logger, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public string ParseHtml(string htmlContent, int stripLevel = 0)
    {
        ArgumentNullException.ThrowIfNull(htmlContent);

        // Validate strip level
        if (stripLevel < 0 || stripLevel > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(stripLevel), "Strip level must be between 0 and 10.");
        }

        _logger.LogInformation("Parsing HTML content ({Length} characters) with strip level {StripLevel}", htmlContent.Length, stripLevel);

        try
        {
            string content;

            if (stripLevel >= 7)
            {
                // High stripping: extract basic HTML structure only
                content = StripToBasicHtml(htmlContent, stripLevel);
            }
            else
            {
                // Low to medium stripping: semantic extraction with optional attribute removal
                content = ExtractBodyContent(htmlContent);
                content = RemoveNonContentElements(content);
                
                if (stripLevel >= 1)
                {
                    content = StripAttributes(content, stripLevel);
                }
                
                content = ExtractSemanticContent(content);
                content = CleanupWhitespace(content);
            }

            _logger.LogInformation("Extracted content ({Length} characters)", content.Length);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing HTML content");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ParseHtmlFromFileAsync(string filePath, int stripLevel = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        _logger.LogInformation("Reading HTML from file: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"HTML file not found: {filePath}", filePath);
        }

        var htmlContent = await File.ReadAllTextAsync(filePath, cancellationToken);
        return ParseHtml(htmlContent, stripLevel);
    }

    /// <inheritdoc/>
    public async Task<string> ParseHtmlFromUrlAsync(string url, int stripLevel = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        _logger.LogInformation("Fetching HTML from URL: {Url}", url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"URL must use HTTP or HTTPS scheme: {url}", nameof(url));
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient("HtmlParser");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; HtmlParserService/1.0)");

            var response = await httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Fetched {Length} characters from {Url}", htmlContent.Length, url);

            return ParseHtml(htmlContent, stripLevel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch HTML from URL: {Url}", url);
            throw new InvalidOperationException($"Failed to fetch HTML from URL: {url}", ex);
        }
    }

    private static string ExtractBodyContent(string html)
    {
        // Try to extract body content, fall back to full content if no body tag
        var bodyMatch = BodyRegex().Match(html);
        return bodyMatch.Success ? bodyMatch.Groups[1].Value : html;
    }

    private static string RemoveNonContentElements(string html)
    {
        // Remove script tags and content
        html = ScriptRegex().Replace(html, string.Empty);

        // Remove style tags and content
        html = StyleRegex().Replace(html, string.Empty);

        // Remove noscript tags and content
        html = NoscriptRegex().Replace(html, string.Empty);

        // Remove HTML comments
        html = CommentRegex().Replace(html, string.Empty);

        // Remove SVG elements
        html = SvgRegex().Replace(html, string.Empty);

        // Remove iframe elements
        html = IframeRegex().Replace(html, string.Empty);

        // Remove link tags (CSS/icons)
        html = LinkRegex().Replace(html, string.Empty);

        // Remove meta tags
        html = MetaRegex().Replace(html, string.Empty);

        return html;
    }

    private static string ExtractSemanticContent(string html)
    {
        var result = new StringBuilder();

        // Extract title if present
        var titleMatch = TitleRegex().Match(html);
        if (titleMatch.Success)
        {
            var title = CleanText(titleMatch.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(title))
            {
                result.AppendLine($"# {title}");
                result.AppendLine();
            }
        }

        // Process headings (h1-h6)
        html = ProcessHeadings(html, result);

        // Extract alt text from images
        foreach (Match match in ImgAltRegex().Matches(html))
        {
            var altText = CleanText(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(altText))
            {
                result.AppendLine($"[Image: {altText}]");
            }
        }

        // Extract link text with context
        html = ProcessLinks(html, result);

        // Extract button text
        foreach (Match match in ButtonRegex().Matches(html))
        {
            var buttonText = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(buttonText))
            {
                result.AppendLine($"[Button: {buttonText}]");
            }
        }

        // Extract form elements (labels, placeholders)
        ProcessFormElements(html, result);

        // Extract list items
        ProcessLists(html, result);

        // Extract table content
        ProcessTables(html, result);

        // Extract remaining paragraph and div text content
        ProcessTextContent(html, result);

        return result.ToString();
    }

    private static string ProcessHeadings(string html, StringBuilder result)
    {
        for (int i = 1; i <= 6; i++)
        {
            var headingRegex = new Regex($@"<h{i}[^>]*>(.*?)</h{i}>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in headingRegex.Matches(html))
            {
                var headingText = CleanText(StripTags(match.Groups[1].Value));
                if (!string.IsNullOrWhiteSpace(headingText))
                {
                    var prefix = new string('#', i);
                    result.AppendLine($"{prefix} {headingText}");
                    result.AppendLine();
                }
            }
        }

        return html;
    }

    private static string ProcessLinks(string html, StringBuilder result)
    {
        foreach (Match match in AnchorRegex().Matches(html))
        {
            var linkText = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(linkText) && linkText.Length > 2)
            {
                // Only include meaningful link text (not just icons or single chars)
                result.AppendLine($"- {linkText}");
            }
        }

        return html;
    }

    private static void ProcessFormElements(string html, StringBuilder result)
    {
        // Extract labels
        foreach (Match match in LabelRegex().Matches(html))
        {
            var labelText = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(labelText))
            {
                result.AppendLine($"[Field: {labelText}]");
            }
        }

        // Extract input placeholders
        foreach (Match match in PlaceholderRegex().Matches(html))
        {
            var placeholder = CleanText(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(placeholder))
            {
                result.AppendLine($"[Input: {placeholder}]");
            }
        }
    }

    private static void ProcessLists(string html, StringBuilder result)
    {
        // Extract list items
        foreach (Match match in ListItemRegex().Matches(html))
        {
            var itemText = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(itemText))
            {
                result.AppendLine($"- {itemText}");
            }
        }
    }

    private static void ProcessTables(string html, StringBuilder result)
    {
        // Extract table headers
        foreach (Match match in TableHeaderRegex().Matches(html))
        {
            var headerText = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(headerText))
            {
                result.AppendLine($"| {headerText} |");
            }
        }

        // Extract table cells
        foreach (Match match in TableCellRegex().Matches(html))
        {
            var cellText = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(cellText))
            {
                result.AppendLine($"| {cellText} |");
            }
        }
    }

    private static void ProcessTextContent(string html, StringBuilder result)
    {
        // Extract paragraph content
        foreach (Match match in ParagraphRegex().Matches(html))
        {
            var text = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(text) && text.Length > 10)
            {
                result.AppendLine(text);
                result.AppendLine();
            }
        }

        // Extract span/div content that might be meaningful
        foreach (Match match in SpanDivRegex().Matches(html))
        {
            var text = CleanText(StripTags(match.Groups[1].Value));
            if (!string.IsNullOrWhiteSpace(text) && text.Length > 20)
            {
                result.AppendLine(text);
            }
        }
    }

    private static string StripTags(string html)
    {
        return TagRegex().Replace(html, string.Empty);
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Decode common HTML entities
        text = text.Replace("&nbsp;", " ")
                   .Replace("&amp;", "&")
                   .Replace("&lt;", "<")
                   .Replace("&gt;", ">")
                   .Replace("&quot;", "\"")
                   .Replace("&#39;", "'")
                   .Replace("&apos;", "'");

        // Decode numeric entities
        text = NumericEntityRegex().Replace(text, m =>
        {
            if (int.TryParse(m.Groups[1].Value, out int code))
            {
                return ((char)code).ToString();
            }

            return m.Value;
        });

        return text.Trim();
    }

    private static string CleanupWhitespace(string content)
    {
        // Normalize line endings
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        // Remove excessive blank lines (more than 2 consecutive)
        content = MultipleNewlinesRegex().Replace(content, "\n\n");

        // Remove excessive spaces
        content = MultipleSpacesRegex().Replace(content, " ");

        // Trim each line
        var lines = content.Split('\n')
                          .Select(line => line.Trim())
                          .Where(line => !string.IsNullOrWhiteSpace(line) || line == string.Empty);

        return string.Join("\n", lines).Trim();
    }

    private static string StripAttributes(string html, int stripLevel)
    {
        // Strip level 1-3: Remove inline styles and event handlers
        if (stripLevel <= 3)
        {
            // Remove style attributes
            html = StyleAttributeRegex().Replace(html, string.Empty);
            
            // Remove event handler attributes (onclick, onload, etc.)
            html = EventHandlerRegex().Replace(html, string.Empty);
        }
        // Strip level 4-6: Remove most attributes except core semantic ones
        else if (stripLevel <= 6)
        {
            // Keep only href, src, alt, title attributes
            html = OpeningTagWithAttributesRegex().Replace(html, match =>
            {
                var tag = match.Value;
                
                // Extract and preserve semantic attributes
                var href = AttributeRegex("href").Match(tag);
                var src = AttributeRegex("src").Match(tag);
                var alt = AttributeRegex("alt").Match(tag);
                var title = AttributeRegex("title").Match(tag);
                
                // Get tag name
                var tagName = TagNameRegex().Match(tag).Groups[1].Value;
                
                var preservedAttrs = new StringBuilder();
                if (href.Success) preservedAttrs.Append($" {href.Value}");
                if (src.Success) preservedAttrs.Append($" {src.Value}");
                if (alt.Success) preservedAttrs.Append($" {alt.Value}");
                if (title.Success) preservedAttrs.Append($" {title.Value}");
                
                return $"<{tagName}{preservedAttrs}>";
            });
        }
        
        return html;
    }

    private static string StripToBasicHtml(string html, int stripLevel)
    {
        // Extract body content
        html = ExtractBodyContent(html);
        
        // Remove non-content elements (scripts, styles, etc.)
        html = RemoveNonContentElements(html);
        
        // Strip level 7-9: Remove all attributes but keep all tags
        if (stripLevel <= 9)
        {
            html = OpeningTagWithAttributesRegex().Replace(html, match =>
            {
                var tagName = TagNameRegex().Match(match.Value).Groups[1].Value;
                return $"<{tagName}>";
            });
        }
        // Strip level 10: Only basic HTML tags
        else
        {
            // Define basic allowed tags
            var allowedTags = new HashSet<string>
            {
                "p", "div", "span", "br", "hr",
                "h1", "h2", "h3", "h4", "h5", "h6",
                "ul", "ol", "li",
                "table", "tr", "td", "th", "thead", "tbody",
                "a", "strong", "em", "b", "i", "u",
                "blockquote", "pre", "code"
            };
            
            // First, remove all attributes
            html = OpeningTagWithAttributesRegex().Replace(html, match =>
            {
                var tagName = TagNameRegex().Match(match.Value).Groups[1].Value;
                return $"<{tagName}>";
            });
            
            // Then, remove non-allowed tags (but keep their content)
            html = AllTagsRegex().Replace(html, match =>
            {
                var tagName = TagNameRegex().Match(match.Value).Groups[1].Value.ToLower();
                
                // If it's a closing tag
                if (match.Value.StartsWith("</"))
                {
                    return allowedTags.Contains(tagName) ? match.Value : string.Empty;
                }
                
                // If it's a self-closing or opening tag
                return allowedTags.Contains(tagName) ? match.Value : string.Empty;
            });
        }
        
        return CleanupWhitespace(html);
    }

    private static Regex AttributeRegex(string attrName)
    {
        return new Regex($@"{attrName}=[""'][^""']*[""']", RegexOptions.IgnoreCase);
    }

    // Generated regex patterns for performance
    [GeneratedRegex(@"<body[^>]*>(.*)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BodyRegex();

    [GeneratedRegex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptRegex();

    [GeneratedRegex(@"<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StyleRegex();

    [GeneratedRegex(@"<noscript[^>]*>.*?</noscript>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex NoscriptRegex();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"<svg[^>]*>.*?</svg>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SvgRegex();

    [GeneratedRegex(@"<iframe[^>]*>.*?</iframe>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex IframeRegex();

    [GeneratedRegex(@"<link[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"<meta[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex MetaRegex();

    [GeneratedRegex(@"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"<img[^>]*alt=[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ImgAltRegex();

    [GeneratedRegex(@"<a[^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex(@"<button[^>]*>(.*?)</button>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ButtonRegex();

    [GeneratedRegex(@"<label[^>]*>(.*?)</label>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex LabelRegex();

    [GeneratedRegex(@"placeholder=[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex PlaceholderRegex();

    [GeneratedRegex(@"<li[^>]*>(.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ListItemRegex();

    [GeneratedRegex(@"<th[^>]*>(.*?)</th>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TableHeaderRegex();

    [GeneratedRegex(@"<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TableCellRegex();

    [GeneratedRegex(@"<p[^>]*>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ParagraphRegex();

    [GeneratedRegex(@"<(?:span|div)[^>]*>(.*?)</(?:span|div)>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SpanDivRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"&#(\d+);")]
    private static partial Regex NumericEntityRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MultipleNewlinesRegex();

    [GeneratedRegex(@"[ \t]{2,}")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex(@"\s+style=[""'][^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex StyleAttributeRegex();

    [GeneratedRegex(@"\s+on\w+=[""'][^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerRegex();

    [GeneratedRegex(@"<(\w+)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex OpeningTagWithAttributesRegex();

    [GeneratedRegex(@"</?(\w+)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex AllTagsRegex();

    [GeneratedRegex(@"</?(\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex TagNameRegex();
}
