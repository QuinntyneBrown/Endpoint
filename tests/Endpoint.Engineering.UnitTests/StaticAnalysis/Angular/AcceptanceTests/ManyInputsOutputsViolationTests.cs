// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Angular;
using Endpoint.Engineering.StaticAnalysis.Models;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Acceptance tests for detecting components with too many inputs/outputs (> 10).
/// </summary>
public class ManyInputsOutputsViolationTests
{
    [Fact]
    public async Task Analyze_ComponentWithMoreThan10InputsAndOutputs_ShouldReportWarning()
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

            // Create a component with many inputs and outputs (total > 10)
            var componentContent = """
                import { Component, Input, Output, EventEmitter } from '@angular/core';

                @Component({
                  selector: 'app-complex',
                  template: ''
                })
                export class ComplexComponent {
                  @Input() input1: string;
                  @Input() input2: string;
                  @Input() input3: string;
                  @Input() input4: string;
                  @Input() input5: string;
                  @Input() input6: string;
                  @Input() input7: string;
                  @Input() input8: string;

                  @Output() output1 = new EventEmitter<void>();
                  @Output() output2 = new EventEmitter<void>();
                  @Output() output3 = new EventEmitter<void>();
                  @Output() output4 = new EventEmitter<void>();
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "complex.component.ts"), componentContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Components);
            var component = result.Components[0];
            Assert.Equal("ComplexComponent", component.Name);
            Assert.True(component.Inputs.Count + component.Outputs.Count > 10);

            // Verify warning is reported
            var complexityIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Design" &&
                i.Message.Contains("ComplexComponent") &&
                i.Message.Contains("inputs") &&
                i.Message.Contains("outputs"));

            Assert.NotNull(complexityIssue);
            Assert.Equal(IssueSeverity.Warning, complexityIssue.Severity);
            Assert.Contains("refactor", complexityIssue.Suggestion ?? "", StringComparison.OrdinalIgnoreCase);
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
    public async Task Analyze_ComponentWithExactly10InputsAndOutputs_ShouldNotReportIssue()
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

            // Create a component with exactly 10 inputs/outputs (boundary case)
            var componentContent = """
                import { Component, Input, Output, EventEmitter } from '@angular/core';

                @Component({
                  selector: 'app-boundary',
                  template: ''
                })
                export class BoundaryComponent {
                  @Input() input1: string;
                  @Input() input2: string;
                  @Input() input3: string;
                  @Input() input4: string;
                  @Input() input5: string;
                  @Input() input6: string;
                  @Input() input7: string;
                  @Input() input8: string;

                  @Output() output1 = new EventEmitter<void>();
                  @Output() output2 = new EventEmitter<void>();
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "boundary.component.ts"), componentContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Components);
            var component = result.Components[0];
            Assert.Equal(10, component.Inputs.Count + component.Outputs.Count);

            // Verify no issue is reported for exactly 10 inputs/outputs
            var complexityIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("BoundaryComponent") &&
                i.Message.Contains("inputs") &&
                i.Message.Contains("outputs"));

            Assert.Null(complexityIssue);
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
    public async Task Analyze_ComponentWithFewInputsAndOutputs_ShouldNotReportIssue()
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

            // Create a simple component with few inputs/outputs
            var componentContent = """
                import { Component, Input, Output, EventEmitter } from '@angular/core';

                @Component({
                  selector: 'app-simple',
                  template: ''
                })
                export class SimpleComponent {
                  @Input() data: any;
                  @Input() config: any;

                  @Output() changed = new EventEmitter<void>();
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "simple.component.ts"), componentContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Components);
            var component = result.Components[0];
            Assert.Equal(3, component.Inputs.Count + component.Outputs.Count);

            // Verify no complexity issue
            var complexityIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("SimpleComponent") &&
                i.Category == "Design");

            Assert.Null(complexityIssue);
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
    public async Task Analyze_ComponentWithSignalInputsAndOutputs_ShouldReportWarningIfExceedsThreshold()
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

            // Create a component with many signal-based inputs/outputs (Angular 17+ syntax)
            var componentContent = """
                import { Component, input, output } from '@angular/core';

                @Component({
                  selector: 'app-signal-complex',
                  template: '',
                  standalone: true
                })
                export class SignalComplexComponent {
                  input1 = input<string>();
                  input2 = input<string>();
                  input3 = input<string>();
                  input4 = input<string>();
                  input5 = input<string>();
                  input6 = input<string>();
                  input7 = input<string>();

                  output1 = output<void>();
                  output2 = output<void>();
                  output3 = output<void>();
                  output4 = output<void>();
                  output5 = output<void>();
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "signal-complex.component.ts"), componentContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Components);
            var component = result.Components[0];
            Assert.Equal("SignalComplexComponent", component.Name);
            Assert.True(component.Inputs.Count + component.Outputs.Count > 10);

            // Verify warning is reported
            var complexityIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Design" &&
                i.Message.Contains("SignalComplexComponent"));

            Assert.NotNull(complexityIssue);
            Assert.Equal(IssueSeverity.Warning, complexityIssue.Severity);
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
