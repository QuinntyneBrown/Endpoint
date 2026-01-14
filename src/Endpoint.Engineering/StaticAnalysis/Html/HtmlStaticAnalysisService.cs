// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.StaticAnalysis.Html;

/// <summary>
/// Service for performing static analysis on HTML files.
/// </summary>
public partial class HtmlStaticAnalysisService : IHtmlStaticAnalysisService
{
    private readonly ILogger<HtmlStaticAnalysisService> _logger;

    public HtmlStaticAnalysisService(ILogger<HtmlStaticAnalysisService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<HtmlStaticAnalysisResult> AnalyzeDirectoryAsync(
        string directoryPath,
        HtmlAnalysisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= HtmlAnalysisOptions.Default;

        _logger.LogInformation("Starting HTML static analysis for directory: {DirectoryPath}", directoryPath);

        var result = new HtmlStaticAnalysisResult
        {
            DirectoryPath = directoryPath
        };

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
            return result;
        }

        var searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = options.Extensions
            .SelectMany(ext => Directory.GetFiles(directoryPath, $"*{ext}", searchOption))
            .Distinct()
            .ToList();

        result.TotalFilesAnalyzed = files.Count;
        _logger.LogInformation("Found {FileCount} HTML files to analyze", files.Count);

        foreach (var filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var content = await File.ReadAllTextAsync(filePath, cancellationToken);
                var fileResult = AnalyzeFile(filePath, directoryPath, content, options);
                result.FileResults.Add(fileResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze file: {FilePath}", filePath);
            }
        }

        // Calculate summary
        result.Summary = CalculateSummary(result.FileResults);

        _logger.LogInformation("HTML analysis complete. Found {IssueCount} issues in {FileCount} files",
            result.Summary.TotalIssueCount, result.TotalFilesAnalyzed);

        return result;
    }

    private HtmlFileAnalysisResult AnalyzeFile(
        string filePath,
        string rootDirectory,
        string content,
        HtmlAnalysisOptions options)
    {
        var result = new HtmlFileAnalysisResult
        {
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(rootDirectory, filePath).Replace('\\', '/')
        };

        // Extract metadata
        result.Metadata = ExtractMetadata(content);

        // Run analysis checks
        if (options.CheckAccessibility)
        {
            result.Issues.AddRange(CheckAccessibility(content));
        }

        if (options.CheckSeo)
        {
            result.Issues.AddRange(CheckSeo(content, result.Metadata));
        }

        if (options.CheckSecurity)
        {
            result.Issues.AddRange(CheckSecurity(content));
        }

        if (options.CheckBestPractices)
        {
            result.Issues.AddRange(CheckBestPractices(content, result.Metadata));
        }

        return result;
    }

    private HtmlFileMetadata ExtractMetadata(string content)
    {
        var metadata = new HtmlFileMetadata();

        // Check for DOCTYPE
        metadata.HasDoctype = DoctypeRegex().IsMatch(content);

        // Extract title
        var titleMatch = TitleRegex().Match(content);
        if (titleMatch.Success)
        {
            metadata.Title = titleMatch.Groups[1].Value.Trim();
        }

        // Extract language
        var langMatch = LangRegex().Match(content);
        if (langMatch.Success)
        {
            metadata.Language = langMatch.Groups[1].Value;
        }

        // Extract charset
        var charsetMatch = CharsetRegex().Match(content);
        if (charsetMatch.Success)
        {
            metadata.Charset = charsetMatch.Groups[1].Value;
        }

        // Count elements
        metadata.ImageCount = ImgRegex().Matches(content).Count;
        metadata.LinkCount = AnchorRegex().Matches(content).Count;
        metadata.FormCount = FormRegex().Matches(content).Count;
        metadata.ScriptCount = ScriptRegex().Matches(content).Count;
        metadata.StylesheetCount = StylesheetRegex().Matches(content).Count;

        return metadata;
    }

    private List<HtmlAnalysisIssue> CheckAccessibility(string content)
    {
        var issues = new List<HtmlAnalysisIssue>();

        // Check for images without alt attribute
        var imgMatches = ImgRegex().Matches(content);
        foreach (Match match in imgMatches)
        {
            if (!AltAttributeRegex().IsMatch(match.Value))
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "Accessibility",
                    Severity = HtmlIssueSeverity.Error,
                    RuleId = "A11Y-IMG-ALT",
                    Message = "Image missing alt attribute",
                    LineNumber = GetLineNumber(content, match.Index),
                    Element = TruncateElement(match.Value),
                    Suggestion = "Add an alt attribute to describe the image content"
                });
            }
        }

        // Check for form inputs without labels
        var inputMatches = InputRegex().Matches(content);
        foreach (Match match in inputMatches)
        {
            var inputHtml = match.Value;
            var idMatch = IdAttributeRegex().Match(inputHtml);

            // Skip hidden, submit, button, and image inputs
            if (InputTypeHiddenRegex().IsMatch(inputHtml) ||
                InputTypeSubmitRegex().IsMatch(inputHtml) ||
                InputTypeButtonRegex().IsMatch(inputHtml) ||
                InputTypeImageRegex().IsMatch(inputHtml))
            {
                continue;
            }

            if (idMatch.Success)
            {
                var inputId = idMatch.Groups[1].Value;
                var labelPattern = $@"<label[^>]*for\s*=\s*[""']{Regex.Escape(inputId)}[""']";
                if (!Regex.IsMatch(content, labelPattern, RegexOptions.IgnoreCase))
                {
                    issues.Add(new HtmlAnalysisIssue
                    {
                        Category = "Accessibility",
                        Severity = HtmlIssueSeverity.Warning,
                        RuleId = "A11Y-INPUT-LABEL",
                        Message = $"Input with id '{inputId}' has no associated label",
                        LineNumber = GetLineNumber(content, match.Index),
                        Element = TruncateElement(inputHtml),
                        Suggestion = "Add a <label for=\"id\"> element or use aria-label attribute"
                    });
                }
            }
            else if (!AriaLabelRegex().IsMatch(inputHtml))
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "Accessibility",
                    Severity = HtmlIssueSeverity.Warning,
                    RuleId = "A11Y-INPUT-LABEL",
                    Message = "Input element has no id or aria-label",
                    LineNumber = GetLineNumber(content, match.Index),
                    Element = TruncateElement(inputHtml),
                    Suggestion = "Add an id attribute with a corresponding label, or use aria-label"
                });
            }
        }

        // Check for missing language attribute on html element
        if (!LangRegex().IsMatch(content))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "Accessibility",
                Severity = HtmlIssueSeverity.Error,
                RuleId = "A11Y-HTML-LANG",
                Message = "HTML element missing lang attribute",
                Suggestion = "Add lang attribute to <html> element (e.g., <html lang=\"en\">)"
            });
        }

        // Check for empty links
        var anchorMatches = AnchorWithContentRegex().Matches(content);
        foreach (Match match in anchorMatches)
        {
            var linkContent = match.Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(linkContent) ||
                linkContent.Equals("&nbsp;", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "Accessibility",
                    Severity = HtmlIssueSeverity.Warning,
                    RuleId = "A11Y-EMPTY-LINK",
                    Message = "Link has no text content",
                    LineNumber = GetLineNumber(content, match.Index),
                    Element = TruncateElement(match.Value),
                    Suggestion = "Add descriptive text to the link or use aria-label"
                });
            }
        }

        // Check for missing skip navigation link
        if (!SkipNavRegex().IsMatch(content) && MainContentRegex().IsMatch(content))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "Accessibility",
                Severity = HtmlIssueSeverity.Info,
                RuleId = "A11Y-SKIP-NAV",
                Message = "Consider adding a skip navigation link",
                Suggestion = "Add a 'Skip to main content' link at the beginning of the page"
            });
        }

        // Check for tables without headers
        var tableMatches = TableRegex().Matches(content);
        foreach (Match match in tableMatches)
        {
            var tableContent = match.Value;
            if (!ThRegex().IsMatch(tableContent) && !ScopeAttributeRegex().IsMatch(tableContent))
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "Accessibility",
                    Severity = HtmlIssueSeverity.Warning,
                    RuleId = "A11Y-TABLE-HEADERS",
                    Message = "Table may be missing header cells",
                    LineNumber = GetLineNumber(content, match.Index),
                    Suggestion = "Use <th> elements for header cells or add scope attributes"
                });
            }
        }

        return issues;
    }

    private List<HtmlAnalysisIssue> CheckSeo(string content, HtmlFileMetadata metadata)
    {
        var issues = new List<HtmlAnalysisIssue>();

        // Check for missing title
        if (string.IsNullOrWhiteSpace(metadata.Title))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Error,
                RuleId = "SEO-TITLE",
                Message = "Page is missing a title element",
                Suggestion = "Add a descriptive <title> element in the <head> section"
            });
        }
        else if (metadata.Title.Length < 10)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEO-TITLE-SHORT",
                Message = "Page title is too short",
                Suggestion = "Use a more descriptive title (recommended: 50-60 characters)"
            });
        }
        else if (metadata.Title.Length > 60)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEO-TITLE-LONG",
                Message = "Page title may be too long for search results",
                Suggestion = "Keep title under 60 characters for optimal display in search results"
            });
        }

        // Check for missing meta description
        if (!MetaDescriptionRegex().IsMatch(content))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEO-META-DESC",
                Message = "Page is missing meta description",
                Suggestion = "Add <meta name=\"description\" content=\"...\"> with a 150-160 character description"
            });
        }

        // Check for multiple H1 tags
        var h1Matches = H1Regex().Matches(content);
        if (h1Matches.Count == 0)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEO-H1-MISSING",
                Message = "Page is missing an H1 heading",
                Suggestion = "Add exactly one H1 heading that describes the page content"
            });
        }
        else if (h1Matches.Count > 1)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEO-H1-MULTIPLE",
                Message = $"Page has {h1Matches.Count} H1 headings",
                Suggestion = "Use only one H1 heading per page"
            });
        }

        // Check for missing canonical URL
        if (!CanonicalRegex().IsMatch(content))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Info,
                RuleId = "SEO-CANONICAL",
                Message = "Page is missing canonical URL",
                Suggestion = "Add <link rel=\"canonical\" href=\"...\"> to specify the preferred URL"
            });
        }

        // Check for images without descriptive alt text (SEO perspective)
        var imgMatches = ImgRegex().Matches(content);
        foreach (Match match in imgMatches)
        {
            var altMatch = AltValueRegex().Match(match.Value);
            if (altMatch.Success)
            {
                var altValue = altMatch.Groups[1].Value;
                if (altValue.Length < 5 && !string.IsNullOrEmpty(altValue))
                {
                    issues.Add(new HtmlAnalysisIssue
                    {
                        Category = "SEO",
                        Severity = HtmlIssueSeverity.Info,
                        RuleId = "SEO-IMG-ALT-SHORT",
                        Message = "Image alt text may be too brief for SEO",
                        LineNumber = GetLineNumber(content, match.Index),
                        Element = TruncateElement(match.Value),
                        Suggestion = "Use more descriptive alt text for better image SEO"
                    });
                }
            }
        }

        // Check for missing viewport meta tag
        if (!ViewportRegex().IsMatch(content))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "SEO",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEO-VIEWPORT",
                Message = "Page is missing viewport meta tag",
                Suggestion = "Add <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
            });
        }

        return issues;
    }

    private List<HtmlAnalysisIssue> CheckSecurity(string content)
    {
        var issues = new List<HtmlAnalysisIssue>();

        // Check for inline JavaScript
        var inlineScriptMatches = InlineScriptRegex().Matches(content);
        foreach (Match match in inlineScriptMatches)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "Security",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEC-INLINE-JS",
                Message = "Inline JavaScript detected",
                LineNumber = GetLineNumber(content, match.Index),
                Suggestion = "Move JavaScript to external files for better CSP compliance"
            });
        }

        // Check for inline event handlers
        var eventHandlerMatches = EventHandlerRegex().Matches(content);
        foreach (Match match in eventHandlerMatches)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "Security",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEC-INLINE-HANDLER",
                Message = $"Inline event handler '{match.Groups[1].Value}' detected",
                LineNumber = GetLineNumber(content, match.Index),
                Element = TruncateElement(match.Value),
                Suggestion = "Use addEventListener in external JavaScript files instead"
            });
        }

        // Check for external scripts without integrity attribute
        var externalScriptMatches = ExternalScriptRegex().Matches(content);
        foreach (Match match in externalScriptMatches)
        {
            var scriptTag = match.Value;
            var srcMatch = SrcAttributeRegex().Match(scriptTag);
            if (srcMatch.Success)
            {
                var src = srcMatch.Groups[1].Value;
                // Only check external CDN scripts
                if (src.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                    src.StartsWith("//"))
                {
                    if (!IntegrityAttributeRegex().IsMatch(scriptTag))
                    {
                        issues.Add(new HtmlAnalysisIssue
                        {
                            Category = "Security",
                            Severity = HtmlIssueSeverity.Warning,
                            RuleId = "SEC-SRI",
                            Message = "External script missing integrity attribute",
                            LineNumber = GetLineNumber(content, match.Index),
                            Element = TruncateElement(scriptTag),
                            Suggestion = "Add integrity and crossorigin attributes for subresource integrity"
                        });
                    }
                }
            }
        }

        // Check for forms without CSRF protection (action to external or POST)
        var formMatches = FormRegex().Matches(content);
        foreach (Match match in formMatches)
        {
            var formTag = match.Value;
            var methodMatch = MethodAttributeRegex().Match(formTag);
            if (methodMatch.Success &&
                methodMatch.Groups[1].Value.Equals("post", StringComparison.OrdinalIgnoreCase))
            {
                // Look for hidden CSRF token field
                var formContent = match.Groups[1].Value;
                if (!CsrfTokenRegex().IsMatch(formContent))
                {
                    issues.Add(new HtmlAnalysisIssue
                    {
                        Category = "Security",
                        Severity = HtmlIssueSeverity.Info,
                        RuleId = "SEC-CSRF",
                        Message = "POST form may be missing CSRF token",
                        LineNumber = GetLineNumber(content, match.Index),
                        Suggestion = "Ensure CSRF protection is implemented server-side"
                    });
                }
            }
        }

        // Check for target="_blank" without rel="noopener"
        var blankTargetMatches = TargetBlankRegex().Matches(content);
        foreach (Match match in blankTargetMatches)
        {
            if (!RelNoopenerRegex().IsMatch(match.Value))
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "Security",
                    Severity = HtmlIssueSeverity.Warning,
                    RuleId = "SEC-NOOPENER",
                    Message = "Link with target=\"_blank\" missing rel=\"noopener\"",
                    LineNumber = GetLineNumber(content, match.Index),
                    Element = TruncateElement(match.Value),
                    Suggestion = "Add rel=\"noopener noreferrer\" to prevent reverse tabnabbing"
                });
            }
        }

        // Check for HTTP links in HTTPS context
        var httpLinkMatches = HttpLinkRegex().Matches(content);
        foreach (Match match in httpLinkMatches)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "Security",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "SEC-MIXED-CONTENT",
                Message = "HTTP link may cause mixed content issues",
                LineNumber = GetLineNumber(content, match.Index),
                Element = TruncateElement(match.Value),
                Suggestion = "Use HTTPS or protocol-relative URLs"
            });
        }

        return issues;
    }

    private List<HtmlAnalysisIssue> CheckBestPractices(string content, HtmlFileMetadata metadata)
    {
        var issues = new List<HtmlAnalysisIssue>();

        // Check for missing DOCTYPE
        if (!metadata.HasDoctype)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "BestPractices",
                Severity = HtmlIssueSeverity.Error,
                RuleId = "BP-DOCTYPE",
                Message = "Document is missing DOCTYPE declaration",
                Suggestion = "Add <!DOCTYPE html> at the beginning of the document"
            });
        }

        // Check for missing charset
        if (string.IsNullOrEmpty(metadata.Charset))
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "BestPractices",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "BP-CHARSET",
                Message = "Document is missing charset declaration",
                Suggestion = "Add <meta charset=\"UTF-8\"> in the <head> section"
            });
        }

        // Check for deprecated HTML elements
        var deprecatedElements = new Dictionary<string, string>
        {
            { "center", "Use CSS text-align instead" },
            { "font", "Use CSS font properties instead" },
            { "marquee", "Use CSS animations instead" },
            { "blink", "Avoid blinking text entirely" },
            { "frame", "Use iframes or modern layouts instead" },
            { "frameset", "Use modern layouts instead" },
            { "strike", "Use <del> or CSS text-decoration instead" },
            { "big", "Use CSS font-size instead" },
            { "tt", "Use <code> or CSS font-family instead" }
        };

        foreach (var (element, suggestion) in deprecatedElements)
        {
            var pattern = $@"<{element}[^>]*>";
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "BestPractices",
                    Severity = HtmlIssueSeverity.Warning,
                    RuleId = "BP-DEPRECATED",
                    Message = $"Deprecated element <{element}> found",
                    LineNumber = GetLineNumber(content, match.Index),
                    Element = TruncateElement(match.Value),
                    Suggestion = suggestion
                });
            }
        }

        // Check for deprecated attributes
        var deprecatedAttrs = new[] { "align", "bgcolor", "border", "cellpadding", "cellspacing", "valign", "width", "height" };
        foreach (var attr in deprecatedAttrs)
        {
            var pattern = $@"\s{attr}\s*=";
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 3)
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "BestPractices",
                    Severity = HtmlIssueSeverity.Info,
                    RuleId = "BP-DEPRECATED-ATTR",
                    Message = $"Deprecated attribute '{attr}' used {matches.Count} times",
                    Suggestion = $"Use CSS instead of the {attr} attribute"
                });
            }
        }

        // Check for empty elements that shouldn't be empty
        var emptyElementPatterns = new[] { "div", "span", "p", "section", "article" };
        foreach (var element in emptyElementPatterns)
        {
            var pattern = $@"<{element}[^>]*>\s*</{element}>";
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "BestPractices",
                    Severity = HtmlIssueSeverity.Info,
                    RuleId = "BP-EMPTY-ELEMENT",
                    Message = $"Empty <{element}> element found",
                    LineNumber = GetLineNumber(content, match.Index),
                    Element = TruncateElement(match.Value),
                    Suggestion = "Remove empty elements or add content"
                });
            }
        }

        // Check for inline styles
        var inlineStyleMatches = InlineStyleRegex().Matches(content);
        if (inlineStyleMatches.Count > 5)
        {
            issues.Add(new HtmlAnalysisIssue
            {
                Category = "BestPractices",
                Severity = HtmlIssueSeverity.Warning,
                RuleId = "BP-INLINE-STYLE",
                Message = $"Excessive inline styles detected ({inlineStyleMatches.Count} occurrences)",
                Suggestion = "Move styles to external CSS files for better maintainability"
            });
        }

        // Check for very long lines
        var lines = content.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > 500)
            {
                issues.Add(new HtmlAnalysisIssue
                {
                    Category = "BestPractices",
                    Severity = HtmlIssueSeverity.Info,
                    RuleId = "BP-LONG-LINE",
                    Message = $"Very long line detected ({lines[i].Length} characters)",
                    LineNumber = i + 1,
                    Suggestion = "Consider formatting the HTML for better readability"
                });
                break; // Only report once
            }
        }

        return issues;
    }

    private static HtmlAnalysisSummary CalculateSummary(List<HtmlFileAnalysisResult> fileResults)
    {
        var summary = new HtmlAnalysisSummary();

        var allIssues = fileResults.SelectMany(f => f.Issues).ToList();
        summary.TotalIssueCount = allIssues.Count;
        summary.ErrorCount = allIssues.Count(i => i.Severity == HtmlIssueSeverity.Error);
        summary.WarningCount = allIssues.Count(i => i.Severity == HtmlIssueSeverity.Warning);
        summary.InfoCount = allIssues.Count(i => i.Severity == HtmlIssueSeverity.Info);

        summary.IssuesByCategory = allIssues
            .GroupBy(i => i.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        return summary;
    }

    private static int GetLineNumber(string content, int charIndex)
    {
        if (charIndex < 0 || charIndex >= content.Length)
            return 0;

        var line = 1;
        for (var i = 0; i < charIndex; i++)
        {
            if (content[i] == '\n')
                line++;
        }
        return line;
    }

    private static string TruncateElement(string element)
    {
        const int maxLength = 100;
        if (element.Length <= maxLength)
            return element;
        return element[..maxLength] + "...";
    }

    // Regex patterns
    [GeneratedRegex(@"<!DOCTYPE[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex DoctypeRegex();

    [GeneratedRegex(@"<title[^>]*>([^<]*)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"<html[^>]*\slang\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex LangRegex();

    [GeneratedRegex(@"<meta[^>]*charset\s*=\s*[""']?([^""'\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex CharsetRegex();

    [GeneratedRegex(@"<img[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ImgRegex();

    [GeneratedRegex(@"<a[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex(@"<form[^>]*>(.*?)</form>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FormRegex();

    [GeneratedRegex(@"<script[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptRegex();

    [GeneratedRegex(@"<link[^>]*rel\s*=\s*[""']stylesheet[""'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex StylesheetRegex();

    [GeneratedRegex(@"\salt\s*=", RegexOptions.IgnoreCase)]
    private static partial Regex AltAttributeRegex();

    [GeneratedRegex(@"\salt\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex AltValueRegex();

    [GeneratedRegex(@"<input[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex InputRegex();

    [GeneratedRegex(@"\sid\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex IdAttributeRegex();

    [GeneratedRegex(@"\stype\s*=\s*[""']hidden[""']", RegexOptions.IgnoreCase)]
    private static partial Regex InputTypeHiddenRegex();

    [GeneratedRegex(@"\stype\s*=\s*[""']submit[""']", RegexOptions.IgnoreCase)]
    private static partial Regex InputTypeSubmitRegex();

    [GeneratedRegex(@"\stype\s*=\s*[""']button[""']", RegexOptions.IgnoreCase)]
    private static partial Regex InputTypeButtonRegex();

    [GeneratedRegex(@"\stype\s*=\s*[""']image[""']", RegexOptions.IgnoreCase)]
    private static partial Regex InputTypeImageRegex();

    [GeneratedRegex(@"\saria-label\s*=", RegexOptions.IgnoreCase)]
    private static partial Regex AriaLabelRegex();

    [GeneratedRegex(@"<a[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex AnchorWithContentRegex();

    [GeneratedRegex(@"skip\s*(to\s*)?(main|content|nav)", RegexOptions.IgnoreCase)]
    private static partial Regex SkipNavRegex();

    [GeneratedRegex(@"<main|id\s*=\s*[""']main", RegexOptions.IgnoreCase)]
    private static partial Regex MainContentRegex();

    [GeneratedRegex(@"<table[^>]*>.*?</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TableRegex();

    [GeneratedRegex(@"<th[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ThRegex();

    [GeneratedRegex(@"\sscope\s*=", RegexOptions.IgnoreCase)]
    private static partial Regex ScopeAttributeRegex();

    [GeneratedRegex(@"<meta[^>]*name\s*=\s*[""']description[""'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex MetaDescriptionRegex();

    [GeneratedRegex(@"<h1[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex H1Regex();

    [GeneratedRegex(@"<link[^>]*rel\s*=\s*[""']canonical[""'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex CanonicalRegex();

    [GeneratedRegex(@"<meta[^>]*name\s*=\s*[""']viewport[""'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ViewportRegex();

    [GeneratedRegex(@"<script[^>]*>[^<]+</script>", RegexOptions.IgnoreCase)]
    private static partial Regex InlineScriptRegex();

    [GeneratedRegex(@"\s(on\w+)\s*=\s*[""'][^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerRegex();

    [GeneratedRegex(@"<script[^>]*src\s*=[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ExternalScriptRegex();

    [GeneratedRegex(@"\ssrc\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex SrcAttributeRegex();

    [GeneratedRegex(@"\sintegrity\s*=", RegexOptions.IgnoreCase)]
    private static partial Regex IntegrityAttributeRegex();

    [GeneratedRegex(@"\smethod\s*=\s*[""'](\w+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex MethodAttributeRegex();

    [GeneratedRegex(@"(csrf|_token|authenticity_token|__RequestVerificationToken)", RegexOptions.IgnoreCase)]
    private static partial Regex CsrfTokenRegex();

    [GeneratedRegex(@"<a[^>]*target\s*=\s*[""']_blank[""'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex TargetBlankRegex();

    [GeneratedRegex(@"\srel\s*=\s*[""'][^""']*noopener[^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex RelNoopenerRegex();

    [GeneratedRegex(@"(href|src)\s*=\s*[""']http://", RegexOptions.IgnoreCase)]
    private static partial Regex HttpLinkRegex();

    [GeneratedRegex(@"\sstyle\s*=\s*[""'][^""']+[""']", RegexOptions.IgnoreCase)]
    private static partial Regex InlineStyleRegex();
}
