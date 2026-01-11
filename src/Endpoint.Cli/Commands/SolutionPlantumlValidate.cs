// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.PlantUml.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("solution-plantuml-validate")]
public class SolutionPlantumlValidateRequest : IRequest
{
    [Option('n', "name", Required = false, HelpText = "Optional name for the validation report.")]
    public string Name { get; set; }

    [Option('p', "plant-uml-source-path", Required = false, HelpText = "Path to the directory containing PlantUML files.")]
    public string PlantUmlSourcePath { get; set; } = Environment.CurrentDirectory;

    [Option('d', "directory", Required = false, HelpText = "Directory where the validation report will be saved.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class SolutionPlantumlValidateRequestHandler : IRequestHandler<SolutionPlantumlValidateRequest>
{
    private readonly ILogger<SolutionPlantumlValidateRequestHandler> logger;
    private readonly IPlantUmlValidationService validationService;

    public SolutionPlantumlValidateRequestHandler(
        ILogger<SolutionPlantumlValidateRequestHandler> logger,
        IPlantUmlValidationService validationService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    public async Task Handle(SolutionPlantumlValidateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating PlantUML solution architecture files");
        logger.LogInformation("PlantUML source path: {SourcePath}", request.PlantUmlSourcePath);
        logger.LogInformation("Output directory: {Directory}", request.Directory);

        // Resolve full paths
        var sourcePath = Path.GetFullPath(request.PlantUmlSourcePath);
        var outputDirectory = Path.GetFullPath(request.Directory);

        // Validate the PlantUML files
        var validationResult = await validationService.ValidateDirectoryAsync(sourcePath, cancellationToken);

        // Generate the markdown report
        var reportContent = validationResult.ToMarkdownReport();

        // Determine the report filename
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var reportName = string.IsNullOrWhiteSpace(request.Name)
            ? $"plantuml-validation-report-{timestamp}.md"
            : $"{SanitizeFileName(request.Name)}-validation-report-{timestamp}.md";

        var reportPath = Path.Combine(outputDirectory, reportName);

        // Ensure output directory exists
        if (!System.IO.Directory.Exists(outputDirectory))
        {
            System.IO.Directory.CreateDirectory(outputDirectory);
        }

        // Write the report
        await File.WriteAllTextAsync(reportPath, reportContent, cancellationToken);

        logger.LogInformation("Validation report saved to: {ReportPath}", reportPath);

        // Log summary to console
        LogValidationSummary(validationResult);

        if (validationResult.IsValid)
        {
            logger.LogInformation("Validation PASSED - PlantUML files are valid for solution generation");
        }
        else
        {
            logger.LogWarning("Validation FAILED - Found {ErrorCount} error(s) and {WarningCount} warning(s)",
                validationResult.TotalErrors, validationResult.TotalWarnings);
            logger.LogWarning("Review the validation report at: {ReportPath}", reportPath);
        }
    }

    private void LogValidationSummary(DotNet.Artifacts.PlantUml.Models.PlantUmlValidationResult result)
    {
        logger.LogInformation("========================================");
        logger.LogInformation("VALIDATION SUMMARY");
        logger.LogInformation("========================================");
        logger.LogInformation("Source Path: {SourcePath}", result.SourcePath);
        logger.LogInformation("Documents Analyzed: {Count}", result.DocumentResults.Count);
        logger.LogInformation("Status: {Status}", result.IsValid ? "PASSED" : "FAILED");
        logger.LogInformation("----------------------------------------");
        logger.LogInformation("Errors:   {Count}", result.TotalErrors);
        logger.LogInformation("Warnings: {Count}", result.TotalWarnings);
        logger.LogInformation("Info:     {Count}", result.TotalInfo);
        logger.LogInformation("========================================");

        // Log global issues (errors first)
        if (result.GlobalIssues.Count > 0)
        {
            logger.LogInformation("Global Issues:");
            foreach (var issue in result.GlobalIssues)
            {
                var logLevel = issue.Severity switch
                {
                    DotNet.Artifacts.PlantUml.Models.ValidationSeverity.Error => LogLevel.Error,
                    DotNet.Artifacts.PlantUml.Models.ValidationSeverity.Warning => LogLevel.Warning,
                    _ => LogLevel.Information
                };

                logger.Log(logLevel, "[{Severity}] {Code}: {Message}",
                    issue.Severity, issue.Code, issue.Message);
            }
        }

        // Log document-level summaries
        foreach (var docResult in result.DocumentResults)
        {
            if (docResult.HasErrors || docResult.HasWarnings)
            {
                var fileName = Path.GetFileName(docResult.FilePath);
                logger.LogInformation("Document: {FileName} - Errors: {Errors}, Warnings: {Warnings}",
                    fileName, docResult.ErrorCount, docResult.WarningCount);
            }
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return sanitized.ToLowerInvariant().Replace(' ', '-');
    }
}
