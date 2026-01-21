// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.StaticAnalysis.SonarQube;

/// <summary>
/// Implementation of the SonarQube git analyzer service.
/// </summary>
public class SonarQubeGitAnalyzer : ISonarQubeGitAnalyzer
{
    private readonly ILogger<SonarQubeGitAnalyzer> _logger;
    private readonly List<SonarQubeRule> _rules = new();

    public SonarQubeGitAnalyzer(ILogger<SonarQubeGitAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<SonarQubeAnalysisResult> AnalyzeAsync(
        string directory,
        string baseBranch = "master",
        string? rulesFilePath = null,
        CancellationToken cancellationToken = default)
    {
        // Find git repository root
        var repoRoot = FindGitRoot(directory);
        if (repoRoot == null)
        {
            throw new InvalidOperationException($"'{directory}' is not within a git repository.");
        }

        _logger.LogInformation("Found git repository at: {RepoRoot}", repoRoot);

        // Load rules
        var rulesPath = rulesFilePath ?? Path.Combine(repoRoot, "docs", "sonar-qube-rules.md");
        if (!File.Exists(rulesPath))
        {
            throw new FileNotFoundException($"SonarQube rules file not found at: {rulesPath}");
        }

        await LoadRulesAsync(rulesPath, cancellationToken);
        _logger.LogInformation("Loaded {Count} rules from {Path}", _rules.Count, rulesPath);

        // Get current branch and changed files
        using var repo = new Repository(repoRoot);
        var currentBranch = repo.Head.FriendlyName;

        var result = new SonarQubeAnalysisResult
        {
            CurrentBranch = currentBranch,
            BaseBranch = baseBranch,
            RootDirectory = repoRoot,
            RulesLoaded = _rules.Count
        };

        // Get changed files
        var changedFiles = GetChangedFiles(repo, repoRoot, baseBranch);
        result.ChangedFiles = changedFiles;
        _logger.LogInformation("Found {Count} changed files", changedFiles.Count);

        if (changedFiles.Count == 0)
        {
            return result;
        }

        // Analyze changed files
        foreach (var file in changedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(file.FullPath))
                continue;

            var content = await File.ReadAllTextAsync(file.FullPath, cancellationToken);
            var lines = content.Split('\n');

            // Get added lines from diff
            file.AddedLines = GetAddedLines(repo, baseBranch, file.Path);

            var issues = file.Language == "csharp"
                ? AnalyzeCSharpCode(file, lines)
                : AnalyzeTypeScriptCode(file, lines);

            result.Issues.AddRange(issues);
        }

        _logger.LogInformation("Found {Count} issues", result.Issues.Count);
        return result;
    }

    private string? FindGitRoot(string directory)
    {
        var current = directory;
        while (!string.IsNullOrEmpty(current))
        {
            var gitPath = Path.Combine(current, ".git");
            if (Directory.Exists(gitPath))
                return current;
            current = Path.GetDirectoryName(current);
        }
        return null;
    }

    private async Task LoadRulesAsync(string rulesPath, CancellationToken cancellationToken)
    {
        _rules.Clear();
        var content = await File.ReadAllTextAsync(rulesPath, cancellationToken);
        var lines = content.Split('\n');

        string? currentLanguage = null;
        string? currentCategory = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Detect language section
            if (trimmed.StartsWith("# C# Rules") || trimmed.Contains("## C# "))
                currentLanguage = "csharp";
            else if (trimmed.StartsWith("# TypeScript Rules") || trimmed.Contains("## TypeScript "))
                currentLanguage = "typescript";

            // Detect category
            if (trimmed.Contains("Vulnerabilities"))
                currentCategory = "Vulnerability";
            else if (trimmed.Contains("Bugs"))
                currentCategory = "Bug";
            else if (trimmed.Contains("Security Hotspots"))
                currentCategory = "Security Hotspot";
            else if (trimmed.Contains("Code Smells"))
                currentCategory = "Code Smell";

            // Parse table rows (| S#### | Title |)
            var match = Regex.Match(trimmed, @"^\|\s*(S\d+)\s*\|\s*(.+?)\s*\|$");
            if (match.Success && currentLanguage != null && currentCategory != null)
            {
                var ruleId = match.Groups[1].Value;
                var title = match.Groups[2].Value.Trim();

                if (!title.Contains("Rule ID")) // Skip header row
                {
                    _rules.Add(new SonarQubeRule
                    {
                        RuleId = ruleId,
                        Title = title,
                        Language = currentLanguage,
                        Category = currentCategory
                    });
                }
            }
        }
    }

    private List<ChangedFile> GetChangedFiles(Repository repo, string repoRoot, string baseBranch)
    {
        var changedFiles = new List<ChangedFile>();

        try
        {
            // Get the base branch
            var baseBranchRef = repo.Branches[baseBranch]
                ?? repo.Branches[$"origin/{baseBranch}"]
                ?? repo.Branches["main"]
                ?? repo.Branches["origin/main"];

            if (baseBranchRef == null)
            {
                _logger.LogWarning("Could not find base branch '{BaseBranch}' or 'main'", baseBranch);
                return changedFiles;
            }

            var currentCommit = repo.Head.Tip;
            var baseCommit = baseBranchRef.Tip;

            var changes = repo.Diff.Compare<TreeChanges>(baseCommit.Tree, currentCommit.Tree);

            foreach (var change in changes)
            {
                var ext = Path.GetExtension(change.Path).ToLowerInvariant();
                if (ext == ".cs" || ext == ".ts" || ext == ".tsx")
                {
                    changedFiles.Add(new ChangedFile
                    {
                        Path = change.Path,
                        FullPath = Path.Combine(repoRoot, change.Path),
                        Status = change.Status.ToString(),
                        Language = ext == ".cs" ? "csharp" : "typescript"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting changed files");
        }

        return changedFiles;
    }

    private HashSet<int> GetAddedLines(Repository repo, string baseBranch, string filePath)
    {
        var addedLines = new HashSet<int>();

        try
        {
            var baseBranchRef = repo.Branches[baseBranch]
                ?? repo.Branches[$"origin/{baseBranch}"]
                ?? repo.Branches["main"]
                ?? repo.Branches["origin/main"];

            if (baseBranchRef == null)
                return addedLines;

            var patch = repo.Diff.Compare<Patch>(baseBranchRef.Tip.Tree, repo.Head.Tip.Tree);
            var patchEntry = patch.FirstOrDefault(p => p.Path == filePath);

            if (patchEntry != null)
            {
                var patchText = patchEntry.Patch;
                var lineNumber = 0;
                var inHunk = false;

                foreach (var line in patchText.Split('\n'))
                {
                    if (line.StartsWith("@@"))
                    {
                        var match = Regex.Match(line, @"@@ -\d+(?:,\d+)? \+(\d+)(?:,\d+)? @@");
                        if (match.Success)
                        {
                            lineNumber = int.Parse(match.Groups[1].Value) - 1;
                            inHunk = true;
                        }
                    }
                    else if (inHunk)
                    {
                        if (line.StartsWith("+") && !line.StartsWith("+++"))
                        {
                            lineNumber++;
                            addedLines.Add(lineNumber);
                        }
                        else if (line.StartsWith("-") && !line.StartsWith("---"))
                        {
                            // Deleted line, don't increment
                        }
                        else
                        {
                            lineNumber++;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get added lines for {FilePath}", filePath);
        }

        return addedLines;
    }

    private List<SonarQubeIssue> AnalyzeCSharpCode(ChangedFile file, string[] lines)
    {
        var issues = new List<SonarQubeIssue>();

        for (int i = 0; i < lines.Length; i++)
        {
            var lineNum = i + 1;

            // Only analyze added/modified lines
            if (file.AddedLines.Count > 0 && !file.AddedLines.Contains(lineNum))
                continue;

            var line = lines[i];

            // S106: Console.WriteLine usage
            if (Regex.IsMatch(line, @"\bConsole\.(Write|WriteLine)\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S106", "Standard outputs should not be used directly to log anything", "Code Smell"));

            // S2068: Hard-coded credentials
            if (Regex.IsMatch(line, @"(password|pwd|secret|apikey|api_key)\s*=\s*""[^""]+""", RegexOptions.IgnoreCase))
                issues.Add(CreateIssue(file, lineNum, line, "S2068", "Hard-coded credentials are security-sensitive", "Security Hotspot"));

            // S1313: Hardcoded IP addresses
            if (Regex.IsMatch(line, @"""(\d{1,3}\.){3}\d{1,3}"""))
                issues.Add(CreateIssue(file, lineNum, line, "S1313", "Using hardcoded IP addresses is security-sensitive", "Security Hotspot"));

            // S112: General exceptions
            if (Regex.IsMatch(line, @"throw\s+new\s+(Exception|ApplicationException|SystemException)\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S112", "General or reserved exceptions should never be thrown", "Code Smell"));

            // S1135: TODO comments
            if (Regex.IsMatch(line, @"//\s*TODO", RegexOptions.IgnoreCase))
                issues.Add(CreateIssue(file, lineNum, line, "S1135", "Track uses of \"TODO\" tags", "Code Smell"));

            // S1134: FIXME comments
            if (Regex.IsMatch(line, @"//\s*FIXME", RegexOptions.IgnoreCase))
                issues.Add(CreateIssue(file, lineNum, line, "S1134", "Track uses of \"FIXME\" tags", "Code Smell"));

            // S125: Commented out code
            if (Regex.IsMatch(line, @"^\s*//.*;\s*$") && !Regex.IsMatch(line, @"//\s*(TODO|FIXME|NOTE|HACK|BUG)"))
                issues.Add(CreateIssue(file, lineNum, line, "S125", "Sections of code should not be commented out", "Code Smell"));

            // S2245: PRNG usage
            if (Regex.IsMatch(line, @"\bnew\s+Random\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S2245", "Using pseudorandom number generators (PRNGs) is security-sensitive", "Security Hotspot"));

            // S4790: Weak hashing
            if (Regex.IsMatch(line, @"\b(MD5|SHA1)\.Create\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S4790", "Using weak hashing algorithms is security-sensitive", "Security Hotspot"));

            // S2077: SQL formatting
            if (Regex.IsMatch(line, @"(SqlCommand|ExecuteSqlRaw|FromSqlRaw).*\$"""))
                issues.Add(CreateIssue(file, lineNum, line, "S2077", "Formatting SQL queries is security-sensitive", "Security Hotspot"));

            // S3168: async void methods
            if (Regex.IsMatch(line, @"\basync\s+void\b") && !Regex.IsMatch(line, @"event"))
                issues.Add(CreateIssue(file, lineNum, line, "S3168", "\"async\" methods should not return \"void\"", "Bug"));

            // S1075: Hardcoded URIs
            if (Regex.IsMatch(line, @"""https?://[^""]+""") && !Regex.IsMatch(line, @"localhost|127\.0\.0\.1|example\.com"))
                issues.Add(CreateIssue(file, lineNum, line, "S1075", "URIs should not be hardcoded", "Code Smell"));

            // S108: Empty nested block
            if (Regex.IsMatch(line, @"\{\s*\}") && !Regex.IsMatch(line, @"(=>|new\s+\w+\s*\{|\(\s*\))"))
                issues.Add(CreateIssue(file, lineNum, line, "S108", "Nested blocks of code should not be left empty", "Code Smell"));

            // S1116: Empty statement
            if (Regex.IsMatch(line, @"^\s*;\s*$"))
                issues.Add(CreateIssue(file, lineNum, line, "S1116", "Empty statements should be removed", "Code Smell"));

            // S2583: Unreachable code - simplified check for "if (false)" or "if (true)"
            if (Regex.IsMatch(line, @"\bif\s*\(\s*(true|false)\s*\)"))
                issues.Add(CreateIssue(file, lineNum, line, "S2583", "Conditionally executed code should be reachable", "Bug"));

            // S1481: Unused variable pattern (discards)
            if (Regex.IsMatch(line, @"^\s*var\s+_\s*="))
                issues.Add(CreateIssue(file, lineNum, line, "S1481", "Unused local variables should be removed", "Code Smell"));
        }

        return issues;
    }

    private List<SonarQubeIssue> AnalyzeTypeScriptCode(ChangedFile file, string[] lines)
    {
        var issues = new List<SonarQubeIssue>();

        for (int i = 0; i < lines.Length; i++)
        {
            var lineNum = i + 1;

            // Only analyze added/modified lines
            if (file.AddedLines.Count > 0 && !file.AddedLines.Contains(lineNum))
                continue;

            var line = lines[i];

            // S106: console.log usage
            if (Regex.IsMatch(line, @"\bconsole\.(log|warn|error|info|debug)\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S106", "Standard outputs should not be used directly to log anything", "Code Smell"));

            // S1525: Debugger statement
            if (Regex.IsMatch(line, @"\bdebugger\s*;?"))
                issues.Add(CreateIssue(file, lineNum, line, "S1525", "Debugger statements should not be used", "Vulnerability"));

            // S2068: Hard-coded credentials
            if (Regex.IsMatch(line, @"(password|pwd|secret|apiKey|api_key|token)\s*[=:]\s*['""][^'""]+['""]", RegexOptions.IgnoreCase))
                issues.Add(CreateIssue(file, lineNum, line, "S2068", "Hard-coded credentials are security-sensitive", "Security Hotspot"));

            // S1313: Hardcoded IP addresses
            if (Regex.IsMatch(line, @"['""](\d{1,3}\.){3}\d{1,3}['""]"))
                issues.Add(CreateIssue(file, lineNum, line, "S1313", "Using hardcoded IP addresses is security-sensitive", "Security Hotspot"));

            // S4204: any type usage
            if (Regex.IsMatch(line, @":\s*any\b"))
                issues.Add(CreateIssue(file, lineNum, line, "S4204", "The \"any\" type should not be used", "Code Smell"));

            // S1135: TODO comments
            if (Regex.IsMatch(line, @"//\s*TODO", RegexOptions.IgnoreCase))
                issues.Add(CreateIssue(file, lineNum, line, "S1135", "Track uses of \"TODO\" tags", "Code Smell"));

            // S1134: FIXME comments
            if (Regex.IsMatch(line, @"//\s*FIXME", RegexOptions.IgnoreCase))
                issues.Add(CreateIssue(file, lineNum, line, "S1134", "Track uses of \"FIXME\" tags", "Code Smell"));

            // S125: Commented out code
            if (Regex.IsMatch(line, @"^\s*//.*;\s*$") && !Regex.IsMatch(line, @"//\s*(TODO|FIXME|NOTE|HACK|BUG)"))
                issues.Add(CreateIssue(file, lineNum, line, "S125", "Sections of code should not be commented out", "Code Smell"));

            // S1440: == instead of ===
            if (Regex.IsMatch(line, @"[^!=]==[^=]") && !Regex.IsMatch(line, @"==="))
                issues.Add(CreateIssue(file, lineNum, line, "S1440", "\"===\" and \"!==\" should be used instead of \"==\" and \"!=\"", "Code Smell"));

            // S3504: var usage
            if (Regex.IsMatch(line, @"\bvar\s+\w+\s*="))
                issues.Add(CreateIssue(file, lineNum, line, "S3504", "Variables should be declared with \"let\" or \"const\"", "Code Smell"));

            // S2245: Math.random usage
            if (Regex.IsMatch(line, @"\bMath\.random\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S2245", "Using pseudorandom number generators (PRNGs) is security-sensitive", "Security Hotspot"));

            // S1523: eval usage
            if (Regex.IsMatch(line, @"\beval\s*\("))
                issues.Add(CreateIssue(file, lineNum, line, "S1523", "Dynamically executing code is security-sensitive", "Security Hotspot"));

            // S1116: Extra semicolons
            if (Regex.IsMatch(line, @";;\s*$"))
                issues.Add(CreateIssue(file, lineNum, line, "S1116", "Extra semicolons should be removed", "Code Smell"));

            // S108: Empty block
            if (Regex.IsMatch(line, @"\{\s*\}") && !Regex.IsMatch(line, @"(=>|\(\s*\))"))
                issues.Add(CreateIssue(file, lineNum, line, "S108", "Nested blocks of code should not be left empty", "Code Smell"));

            // S2138: undefined assignment
            if (Regex.IsMatch(line, @"=\s*undefined\s*;"))
                issues.Add(CreateIssue(file, lineNum, line, "S2138", "\"undefined\" should not be assigned", "Code Smell"));

            // S1075: Hardcoded URIs
            if (Regex.IsMatch(line, @"['""]https?://[^'""]+['""]") && !Regex.IsMatch(line, @"localhost|127\.0\.0\.1|example\.com"))
                issues.Add(CreateIssue(file, lineNum, line, "S1075", "URIs should not be hardcoded", "Code Smell"));

            // S2076: Command injection - simplified check
            if (Regex.IsMatch(line, @"(exec|spawn|execSync|spawnSync)\s*\(.*\$\{"))
                issues.Add(CreateIssue(file, lineNum, line, "S2076", "OS commands should not be vulnerable to command injection attacks", "Vulnerability"));
        }

        return issues;
    }

    private SonarQubeIssue CreateIssue(ChangedFile file, int lineNumber, string lineContent, string ruleId, string message, string category)
    {
        return new SonarQubeIssue
        {
            FilePath = file.Path,
            LineNumber = lineNumber,
            LineContent = lineContent.Trim(),
            RuleId = ruleId,
            Message = message,
            Category = category,
            Language = file.Language
        };
    }
}
