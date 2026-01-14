// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Angular;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Acceptance tests for detecting modules with too many declarations (> 20).
/// </summary>
public class LargeModuleViolationTests
{
    [Fact]
    public async Task Analyze_ModuleWithMoreThan20Declarations_ShouldReportWarning()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                {
                  "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } }
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Generate 25 component declarations
            var declarations = string.Join(",\n    ", Enumerable.Range(1, 25).Select(i => $"Component{i}"));

            // Create a module WITH more than 20 declarations
            var moduleContent = $$"""
                import { NgModule } from '@angular/core';

                @NgModule({
                  declarations: [
                    {{declarations}}
                  ],
                  imports: [CommonModule],
                  exports: []
                })
                export class LargeModule {}
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "large.module.ts"), moduleContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Modules);
            Assert.Equal("LargeModule", result.Modules[0].Name);
            Assert.True(result.Modules[0].Declarations.Count > 20);

            // Verify warning is reported
            var largeModuleIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Architecture" &&
                i.Message.Contains("declarations") &&
                i.Message.Contains("LargeModule"));

            Assert.NotNull(largeModuleIssue);
            Assert.Equal(AngularIssueSeverity.Warning, largeModuleIssue.Severity);
            Assert.Contains("breaking", largeModuleIssue.Suggestion ?? "", StringComparison.OrdinalIgnoreCase);
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
    public async Task Analyze_ModuleWithExactly20Declarations_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                { "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } } }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Generate exactly 20 component declarations (boundary case)
            var declarations = string.Join(",\n    ", Enumerable.Range(1, 20).Select(i => $"Component{i}"));

            var moduleContent = $$"""
                import { NgModule } from '@angular/core';

                @NgModule({
                  declarations: [
                    {{declarations}}
                  ]
                })
                export class BoundaryModule {}
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "boundary.module.ts"), moduleContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Modules);
            Assert.Equal(20, result.Modules[0].Declarations.Count);

            // Verify no issue is reported for exactly 20 declarations
            var largeModuleIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("BoundaryModule") &&
                i.Message.Contains("declarations"));

            Assert.Null(largeModuleIssue);
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
    public async Task Analyze_ModuleWith5Declarations_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                { "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } } }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            var moduleContent = """
                import { NgModule } from '@angular/core';

                @NgModule({
                  declarations: [
                    HeaderComponent,
                    FooterComponent,
                    SidebarComponent,
                    MainComponent,
                    NavComponent
                  ]
                })
                export class SmallModule {}
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "small.module.ts"), moduleContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Modules);
            Assert.Equal(5, result.Modules[0].Declarations.Count);

            // Verify no large module issue
            var largeModuleIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("SmallModule") &&
                i.Message.Contains("declarations"));

            Assert.Null(largeModuleIssue);
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
