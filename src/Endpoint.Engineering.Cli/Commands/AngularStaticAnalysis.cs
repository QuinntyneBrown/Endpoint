// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.StaticAnalysis.Angular;
using Endpoint.Engineering.StaticAnalysis.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("angular-static-analysis", HelpText = "Perform static analysis on an Angular workspace")]
public class AngularStaticAnalysisRequest : IRequest
{
    [Option('d', "directory", Required = false,
        HelpText = "Directory to analyze (defaults to current directory). The workspace root will be automatically detected.")]
    public string? Directory { get; set; }

    [Option('o', "output", Required = false,
        HelpText = "Output file path (optional, prints to console if not specified)")]
    public string? Output { get; set; }

    [Option("no-templates", Required = false, Default = false,
        HelpText = "Skip template analysis")]
    public bool NoTemplates { get; set; }

    [Option("no-styles", Required = false, Default = false,
        HelpText = "Skip style analysis")]
    public bool NoStyles { get; set; }

    [Option("no-issues", Required = false, Default = false,
        HelpText = "Skip issue checking")]
    public bool NoIssues { get; set; }

    [Option("no-routing", Required = false, Default = false,
        HelpText = "Skip routing analysis")]
    public bool NoRouting { get; set; }

    [Option('v', "verbose", Required = false, Default = false,
        HelpText = "Show verbose output with full details")]
    public bool Verbose { get; set; }

    [Option("json", Required = false, Default = false,
        HelpText = "Output results as JSON")]
    public bool Json { get; set; }
}

public class AngularStaticAnalysisRequestHandler : IRequestHandler<AngularStaticAnalysisRequest>
{
    private readonly ILogger<AngularStaticAnalysisRequestHandler> _logger;
    private readonly IAngularStaticAnalysisService _analysisService;

    public AngularStaticAnalysisRequestHandler(
        ILogger<AngularStaticAnalysisRequestHandler> logger,
        IAngularStaticAnalysisService analysisService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
    }

    public async Task Handle(AngularStaticAnalysisRequest request, CancellationToken cancellationToken)
    {
        var startDirectory = string.IsNullOrWhiteSpace(request.Directory)
            ? Environment.CurrentDirectory
            : Path.GetFullPath(request.Directory);

        _logger.LogInformation("Starting Angular static analysis from: {Directory}", startDirectory);

        // Find the Angular workspace root
        var workspaceRoot = _analysisService.FindWorkspaceRoot(startDirectory);

        if (string.IsNullOrEmpty(workspaceRoot))
        {
            _logger.LogError("No Angular workspace found. Make sure you're in an Angular project directory (should contain angular.json).");
            Console.WriteLine("Error: No Angular workspace found.");
            Console.WriteLine("Make sure you're in an Angular project directory (should contain angular.json).");
            return;
        }

        _logger.LogInformation("Found Angular workspace at: {WorkspaceRoot}", workspaceRoot);

        var options = new AngularAnalysisOptions
        {
            AnalyzeTemplates = !request.NoTemplates,
            AnalyzeStyles = !request.NoStyles,
            CheckIssues = !request.NoIssues,
            AnalyzeRouting = !request.NoRouting
        };

        var result = await _analysisService.AnalyzeAsync(workspaceRoot, options, cancellationToken);

        string output;
        if (request.Json)
        {
            output = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        else
        {
            output = FormatResults(result, request.Verbose);
        }

        if (!string.IsNullOrEmpty(request.Output))
        {
            await File.WriteAllTextAsync(request.Output, output, cancellationToken);
            _logger.LogInformation("Output written to: {OutputPath}", request.Output);
            Console.WriteLine($"Analysis results written to: {request.Output}");
        }
        else
        {
            Console.WriteLine(output);
        }
    }

    private static string FormatResults(AngularAnalysisResult result, bool verbose)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("================================================================================");
        sb.AppendLine("                        ANGULAR STATIC ANALYSIS REPORT");
        sb.AppendLine("================================================================================");
        sb.AppendLine();

        // Workspace info
        sb.AppendLine($"Workspace: {result.WorkspaceRoot}");
        sb.AppendLine($"Angular Version: {result.AngularVersion ?? "Unknown"}");
        sb.AppendLine($"Standalone Mode: {(result.IsStandalone ? "Yes" : "No")}");
        sb.AppendLine($"Analysis Date: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Summary
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("                                 SUMMARY");
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine($"  Components:    {result.Summary.TotalComponents,5}  (OnPush: {result.Summary.OnPushComponents}, Standalone: {result.Summary.StandaloneComponents})");
        sb.AppendLine($"  Services:      {result.Summary.TotalServices,5}");
        sb.AppendLine($"  Modules:       {result.Summary.TotalModules,5}");
        sb.AppendLine($"  Directives:    {result.Summary.TotalDirectives,5}");
        sb.AppendLine($"  Pipes:         {result.Summary.TotalPipes,5}");
        sb.AppendLine($"  Routes:        {result.Summary.TotalRoutes,5}");
        sb.AppendLine();
        sb.AppendLine($"  Issues:        {result.Summary.IssueCount,5}  (Errors: {result.Summary.ErrorCount}, Warnings: {result.Summary.WarningCount})");
        sb.AppendLine();

        // Projects
        if (result.Projects.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                                PROJECTS");
            sb.AppendLine("--------------------------------------------------------------------------------");
            foreach (var project in result.Projects)
            {
                sb.AppendLine($"  {project.Name}");
                sb.AppendLine($"    Type: {project.ProjectType}");
                sb.AppendLine($"    Root: {project.Root}");
                if (!string.IsNullOrEmpty(project.Prefix))
                    sb.AppendLine($"    Prefix: {project.Prefix}");
            }
            sb.AppendLine();
        }

        // Components
        if (result.Components.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                               COMPONENTS");
            sb.AppendLine("--------------------------------------------------------------------------------");
            foreach (var component in result.Components.OrderBy(c => c.FilePath))
            {
                var flags = new List<string>();
                if (component.IsStandalone) flags.Add("standalone");
                if (component.ChangeDetection == ChangeDetectionStrategy.OnPush) flags.Add("OnPush");
                var flagStr = flags.Count > 0 ? $" [{string.Join(", ", flags)}]" : "";

                sb.AppendLine($"  {component.Name}{flagStr}");
                sb.AppendLine($"    Selector: {component.Selector}");
                sb.AppendLine($"    File: {component.FilePath}");

                if (verbose)
                {
                    if (component.Inputs.Count > 0)
                        sb.AppendLine($"    Inputs: {string.Join(", ", component.Inputs)}");
                    if (component.Outputs.Count > 0)
                        sb.AppendLine($"    Outputs: {string.Join(", ", component.Outputs)}");
                    if (component.Imports.Count > 0)
                        sb.AppendLine($"    Imports: {string.Join(", ", component.Imports.Take(5))}{(component.Imports.Count > 5 ? "..." : "")}");
                }
            }
            sb.AppendLine();
        }

        // Services
        if (result.Services.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                                SERVICES");
            sb.AppendLine("--------------------------------------------------------------------------------");
            foreach (var service in result.Services.OrderBy(s => s.FilePath))
            {
                var providedIn = !string.IsNullOrEmpty(service.ProvidedIn) ? $" [providedIn: {service.ProvidedIn}]" : "";
                sb.AppendLine($"  {service.Name}{providedIn}");
                sb.AppendLine($"    File: {service.FilePath}");

                if (verbose)
                {
                    if (service.Dependencies.Count > 0)
                        sb.AppendLine($"    Dependencies: {string.Join(", ", service.Dependencies)}");
                    if (service.Methods.Count > 0)
                        sb.AppendLine($"    Methods: {string.Join(", ", service.Methods.Take(10))}{(service.Methods.Count > 10 ? "..." : "")}");
                }
            }
            sb.AppendLine();
        }

        // Modules
        if (result.Modules.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                                MODULES");
            sb.AppendLine("--------------------------------------------------------------------------------");
            foreach (var module in result.Modules.OrderBy(m => m.FilePath))
            {
                sb.AppendLine($"  {module.Name}");
                sb.AppendLine($"    File: {module.FilePath}");
                sb.AppendLine($"    Declarations: {module.Declarations.Count}");
                sb.AppendLine($"    Imports: {module.Imports.Count}");
                sb.AppendLine($"    Exports: {module.Exports.Count}");
                sb.AppendLine($"    Providers: {module.Providers.Count}");

                if (verbose)
                {
                    if (module.Bootstrap.Count > 0)
                        sb.AppendLine($"    Bootstrap: {string.Join(", ", module.Bootstrap)}");
                    if (module.Declarations.Count > 0)
                        sb.AppendLine($"    Declared: {string.Join(", ", module.Declarations.Take(10))}{(module.Declarations.Count > 10 ? "..." : "")}");
                }
            }
            sb.AppendLine();
        }

        // Directives
        if (result.Directives.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                               DIRECTIVES");
            sb.AppendLine("--------------------------------------------------------------------------------");
            foreach (var directive in result.Directives.OrderBy(d => d.FilePath))
            {
                var standalone = directive.IsStandalone ? " [standalone]" : "";
                sb.AppendLine($"  {directive.Name}{standalone}");
                sb.AppendLine($"    Selector: {directive.Selector}");
                sb.AppendLine($"    File: {directive.FilePath}");

                if (verbose)
                {
                    if (directive.Inputs.Count > 0)
                        sb.AppendLine($"    Inputs: {string.Join(", ", directive.Inputs)}");
                    if (directive.Outputs.Count > 0)
                        sb.AppendLine($"    Outputs: {string.Join(", ", directive.Outputs)}");
                }
            }
            sb.AppendLine();
        }

        // Pipes
        if (result.Pipes.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                                 PIPES");
            sb.AppendLine("--------------------------------------------------------------------------------");
            foreach (var pipe in result.Pipes.OrderBy(p => p.FilePath))
            {
                var flags = new List<string>();
                if (pipe.IsStandalone) flags.Add("standalone");
                if (!pipe.IsPure) flags.Add("impure");
                var flagStr = flags.Count > 0 ? $" [{string.Join(", ", flags)}]" : "";

                sb.AppendLine($"  {pipe.Name} ('{pipe.PipeName}'){flagStr}");
                sb.AppendLine($"    File: {pipe.FilePath}");
            }
            sb.AppendLine();
        }

        // Issues
        if (result.Issues.Count > 0)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("                                 ISSUES");
            sb.AppendLine("--------------------------------------------------------------------------------");

            // Group by severity
            var errors = result.Issues.Where(i => i.Severity == IssueSeverity.Error).ToList();
            var warnings = result.Issues.Where(i => i.Severity == IssueSeverity.Warning).ToList();
            var infos = result.Issues.Where(i => i.Severity == IssueSeverity.Info).ToList();

            if (errors.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("  ERRORS:");
                foreach (var issue in errors)
                {
                    FormatIssue(sb, issue, verbose);
                }
            }

            if (warnings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("  WARNINGS:");
                foreach (var issue in warnings)
                {
                    FormatIssue(sb, issue, verbose);
                }
            }

            if (infos.Count > 0 && verbose)
            {
                sb.AppendLine();
                sb.AppendLine("  INFO:");
                foreach (var issue in infos)
                {
                    FormatIssue(sb, issue, verbose);
                }
            }
            else if (infos.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"  INFO: {infos.Count} informational messages (use --verbose to see details)");
            }

            sb.AppendLine();
        }

        sb.AppendLine("================================================================================");
        sb.AppendLine("                           END OF ANALYSIS REPORT");
        sb.AppendLine("================================================================================");

        return sb.ToString();
    }

    private static void FormatIssue(StringBuilder sb, AngularIssue issue, bool verbose)
    {
        var prefix = issue.Severity switch
        {
            IssueSeverity.Error => "    [!]",
            IssueSeverity.Warning => "    [*]",
            _ => "    [-]"
        };

        sb.AppendLine($"{prefix} [{issue.Category}] {issue.Message}");
        if (!string.IsNullOrEmpty(issue.FilePath))
        {
            sb.AppendLine($"        File: {issue.FilePath}{(issue.Line.HasValue ? $":{issue.Line}" : "")}");
        }
        if (verbose && !string.IsNullOrEmpty(issue.Suggestion))
        {
            sb.AppendLine($"        Suggestion: {issue.Suggestion}");
        }
    }
}
