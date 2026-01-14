// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Angular;
using Endpoint.Engineering.StaticAnalysis.Models;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Acceptance tests for detecting components without OnPush change detection strategy.
/// </summary>
public class OnPushChangeDetectionViolationTests
{
    [Fact]
    public async Task Analyze_ComponentWithoutOnPush_ShouldReportInfoIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app"));

            // Create angular.json
            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                {
                  "projects": {
                    "test-app": {
                      "projectType": "application",
                      "root": "",
                      "sourceRoot": "src",
                      "prefix": "app"
                    }
                  }
                }
                """);

            // Create package.json
            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                {
                  "dependencies": {
                    "@angular/core": "^17.0.0"
                  }
                }
                """);

            // Create a component WITHOUT OnPush change detection
            var componentContent = """
                import { Component } from '@angular/core';

                @Component({
                  selector: 'app-example',
                  templateUrl: './example.component.html',
                  styleUrls: ['./example.component.scss']
                })
                export class ExampleComponent {
                  title = 'Example';
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "example.component.ts"), componentContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Components);
            Assert.Equal("ExampleComponent", result.Components[0].Name);
            Assert.NotEqual(ChangeDetectionStrategy.OnPush, result.Components[0].ChangeDetection);

            // Verify issue is reported
            var onPushIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Performance" &&
                i.Message.Contains("OnPush"));

            Assert.NotNull(onPushIssue);
            Assert.Equal(IssueSeverity.Info, onPushIssue.Severity);
            Assert.Contains("ExampleComponent", onPushIssue.Message);
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
    public async Task Analyze_ComponentWithOnPush_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                {
                  "projects": {
                    "test-app": {
                      "projectType": "application",
                      "root": "",
                      "sourceRoot": "src",
                      "prefix": "app"
                    }
                  }
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                {
                  "dependencies": {
                    "@angular/core": "^17.0.0"
                  }
                }
                """);

            // Create a component WITH OnPush change detection
            var componentContent = """
                import { Component, ChangeDetectionStrategy } from '@angular/core';

                @Component({
                  selector: 'app-optimized',
                  templateUrl: './optimized.component.html',
                  changeDetection: ChangeDetectionStrategy.OnPush
                })
                export class OptimizedComponent {
                  title = 'Optimized';
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "optimized.component.ts"), componentContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Components);
            Assert.Equal(ChangeDetectionStrategy.OnPush, result.Components[0].ChangeDetection);

            // Verify no OnPush-related issue is reported for this component
            var onPushIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Performance" &&
                i.Message.Contains("OnPush") &&
                i.Message.Contains("OptimizedComponent"));

            Assert.Null(onPushIssue);
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
    public async Task Analyze_MultipleComponentsMixedOnPush_ShouldReportOnlyViolations()
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

            // Component without OnPush
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "default.component.ts"), """
                import { Component } from '@angular/core';
                @Component({ selector: 'app-default', template: '' })
                export class DefaultComponent {}
                """);

            // Component with OnPush
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "onpush.component.ts"), """
                import { Component, ChangeDetectionStrategy } from '@angular/core';
                @Component({ selector: 'app-onpush', template: '', changeDetection: ChangeDetectionStrategy.OnPush })
                export class OnPushComponent {}
                """);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Equal(2, result.Components.Count);

            var onPushIssues = result.Issues.Where(i =>
                i.Category == "Performance" &&
                i.Message.Contains("OnPush")).ToList();

            // Only DefaultComponent should have an issue
            Assert.Single(onPushIssues);
            Assert.Contains("DefaultComponent", onPushIssues[0].Message);
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
