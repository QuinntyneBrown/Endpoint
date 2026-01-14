// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Angular;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Acceptance tests for detecting impure pipes (pure: false).
/// </summary>
public class ImpurePipeViolationTests
{
    [Fact]
    public async Task Analyze_ImpurePipe_ShouldReportWarning()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "pipes"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                {
                  "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } }
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Create an IMPURE pipe
            var pipeContent = """
                import { Pipe, PipeTransform } from '@angular/core';

                @Pipe({
                  name: 'dynamicFilter',
                  pure: false
                })
                export class DynamicFilterPipe implements PipeTransform {
                  transform(items: any[], filter: string): any[] {
                    return items.filter(item => item.includes(filter));
                  }
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "pipes", "dynamic-filter.pipe.ts"), pipeContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Pipes);
            Assert.Equal("DynamicFilterPipe", result.Pipes[0].Name);
            Assert.Equal("dynamicFilter", result.Pipes[0].PipeName);
            Assert.False(result.Pipes[0].IsPure);

            // Verify warning is reported
            var impurePipeIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Performance" &&
                i.Message.Contains("impure") &&
                i.Message.Contains("DynamicFilterPipe"));

            Assert.NotNull(impurePipeIssue);
            Assert.Equal(AngularIssueSeverity.Warning, impurePipeIssue.Severity);
            Assert.Contains("change detection", impurePipeIssue.Suggestion ?? "", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Analyze_PurePipeExplicit_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "pipes"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                { "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } } }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Create a PURE pipe (explicit)
            var pipeContent = """
                import { Pipe, PipeTransform } from '@angular/core';

                @Pipe({
                  name: 'capitalize',
                  pure: true
                })
                export class CapitalizePipe implements PipeTransform {
                  transform(value: string): string {
                    return value.charAt(0).toUpperCase() + value.slice(1);
                  }
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "pipes", "capitalize.pipe.ts"), pipeContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Pipes);
            Assert.True(result.Pipes[0].IsPure);

            // Verify no impure pipe issue
            var impurePipeIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("CapitalizePipe") &&
                i.Message.Contains("impure"));

            Assert.Null(impurePipeIssue);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Analyze_PurePipeImplicit_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "pipes"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                { "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } } }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Create a PURE pipe (implicit - no pure property means pure: true by default)
            var pipeContent = """
                import { Pipe, PipeTransform } from '@angular/core';

                @Pipe({
                  name: 'truncate'
                })
                export class TruncatePipe implements PipeTransform {
                  transform(value: string, limit: number): string {
                    return value.length > limit ? value.substring(0, limit) + '...' : value;
                  }
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "pipes", "truncate.pipe.ts"), pipeContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Pipes);
            Assert.True(result.Pipes[0].IsPure); // Default is pure

            // Verify no impure pipe issue
            var impurePipeIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("TruncatePipe") &&
                i.Message.Contains("impure"));

            Assert.Null(impurePipeIssue);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Analyze_StandaloneImpurePipe_ShouldReportWarning()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "pipes"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                { "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } } }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Create a standalone impure pipe (Angular 17+)
            var pipeContent = """
                import { Pipe, PipeTransform } from '@angular/core';

                @Pipe({
                  name: 'asyncFilter',
                  standalone: true,
                  pure: false
                })
                export class AsyncFilterPipe implements PipeTransform {
                  transform(items: any[]): any[] {
                    return items;
                  }
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "pipes", "async-filter.pipe.ts"), pipeContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Pipes);
            Assert.True(result.Pipes[0].IsStandalone);
            Assert.False(result.Pipes[0].IsPure);

            // Verify warning is still reported for impure pipes
            var impurePipeIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Performance" &&
                i.Message.Contains("impure"));

            Assert.NotNull(impurePipeIssue);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
