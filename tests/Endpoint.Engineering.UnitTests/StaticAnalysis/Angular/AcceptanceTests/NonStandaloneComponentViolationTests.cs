// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Angular;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Acceptance tests for detecting non-standalone components in standalone projects.
/// </summary>
public class NonStandaloneComponentViolationTests
{
    [Fact]
    public async Task Analyze_NonStandaloneComponentInStandaloneProject_ShouldReportInfo()
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

            // Create a STANDALONE component (marks project as standalone)
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "app.component.ts"), """
                import { Component } from '@angular/core';

                @Component({
                  selector: 'app-root',
                  template: '<router-outlet></router-outlet>',
                  standalone: true
                })
                export class AppComponent {}
                """);

            // Create a NON-STANDALONE component in the same project
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "legacy.component.ts"), """
                import { Component } from '@angular/core';

                @Component({
                  selector: 'app-legacy',
                  templateUrl: './legacy.component.html'
                })
                export class LegacyComponent {}
                """);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Equal(2, result.Components.Count);
            Assert.True(result.IsStandalone); // Project is marked as standalone

            var standaloneComponent = result.Components.First(c => c.Name == "AppComponent");
            var legacyComponent = result.Components.First(c => c.Name == "LegacyComponent");

            Assert.True(standaloneComponent.IsStandalone);
            Assert.False(legacyComponent.IsStandalone);

            // Verify info issue is reported for the non-standalone component
            var migrationIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Migration" &&
                i.Message.Contains("LegacyComponent") &&
                i.Message.Contains("not standalone"));

            Assert.NotNull(migrationIssue);
            Assert.Equal(AngularIssueSeverity.Info, migrationIssue.Severity);
            Assert.Contains("standalone", migrationIssue.Suggestion ?? "", StringComparison.OrdinalIgnoreCase);
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
    public async Task Analyze_AllStandaloneComponentsInStandaloneProject_ShouldNotReportMigrationIssue()
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

            // Create all standalone components
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "app.component.ts"), """
                import { Component } from '@angular/core';
                @Component({ selector: 'app-root', template: '', standalone: true })
                export class AppComponent {}
                """);

            File.WriteAllText(Path.Combine(tempDir, "src", "app", "header.component.ts"), """
                import { Component } from '@angular/core';
                @Component({ selector: 'app-header', template: '', standalone: true })
                export class HeaderComponent {}
                """);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Equal(2, result.Components.Count);
            Assert.True(result.IsStandalone);
            Assert.All(result.Components, c => Assert.True(c.IsStandalone));

            // Verify no migration issues are reported
            var migrationIssues = result.Issues.Where(i => i.Category == "Migration");
            Assert.Empty(migrationIssues);
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
    public async Task Analyze_NonStandaloneComponentsInNonStandaloneProject_ShouldNotReportMigrationIssue()
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
                { "dependencies": { "@angular/core": "^16.0.0" } }
                """);

            // Create only non-standalone components (traditional NgModule-based app)
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "app.component.ts"), """
                import { Component } from '@angular/core';
                @Component({ selector: 'app-root', templateUrl: './app.component.html' })
                export class AppComponent {}
                """);

            File.WriteAllText(Path.Combine(tempDir, "src", "app", "feature.component.ts"), """
                import { Component } from '@angular/core';
                @Component({ selector: 'app-feature', templateUrl: './feature.component.html' })
                export class FeatureComponent {}
                """);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Equal(2, result.Components.Count);
            Assert.False(result.IsStandalone); // Project is NOT standalone
            Assert.All(result.Components, c => Assert.False(c.IsStandalone));

            // Verify no migration issues are reported (project is not standalone)
            var migrationIssues = result.Issues.Where(i => i.Category == "Migration");
            Assert.Empty(migrationIssues);
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
