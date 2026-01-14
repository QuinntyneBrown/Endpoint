// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Angular;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Acceptance tests for detecting services without providedIn specification.
/// </summary>
public class ServiceProvidedInViolationTests
{
    [Fact]
    public async Task Analyze_ServiceWithoutProvidedIn_ShouldReportWarning()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "services"));

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

            // Create a service WITHOUT providedIn
            var serviceContent = """
                import { Injectable } from '@angular/core';

                @Injectable()
                export class DataService {
                  getData() {
                    return [];
                  }
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "services", "data.service.ts"), serviceContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Services);
            Assert.Equal("DataService", result.Services[0].Name);
            Assert.True(string.IsNullOrEmpty(result.Services[0].ProvidedIn));

            // Verify warning is reported
            var providedInIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Architecture" &&
                i.Message.Contains("providedIn"));

            Assert.NotNull(providedInIssue);
            Assert.Equal(AngularIssueSeverity.Warning, providedInIssue.Severity);
            Assert.Contains("DataService", providedInIssue.Message);
            Assert.Contains("tree-shakable", providedInIssue.Suggestion ?? "");
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
    public async Task Analyze_ServiceWithProvidedInRoot_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "services"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                {
                  "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } }
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Create a service WITH providedIn: 'root'
            var serviceContent = """
                import { Injectable } from '@angular/core';

                @Injectable({
                  providedIn: 'root'
                })
                export class UserService {
                  getUser() {
                    return null;
                  }
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "services", "user.service.ts"), serviceContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Services);
            Assert.Equal("root", result.Services[0].ProvidedIn);

            // Verify no providedIn-related issue is reported
            var providedInIssue = result.Issues.FirstOrDefault(i =>
                i.Category == "Architecture" &&
                i.Message.Contains("providedIn") &&
                i.Message.Contains("UserService"));

            Assert.Null(providedInIssue);
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
    public async Task Analyze_ServiceWithProvidedInPlatform_ShouldNotReportIssue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"angular-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "src", "app", "services"));

            File.WriteAllText(Path.Combine(tempDir, "angular.json"), """
                {
                  "projects": { "test-app": { "projectType": "application", "root": "", "sourceRoot": "src" } }
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "package.json"), """
                { "dependencies": { "@angular/core": "^17.0.0" } }
                """);

            // Create a service WITH providedIn: 'platform'
            var serviceContent = """
                import { Injectable } from '@angular/core';

                @Injectable({
                  providedIn: 'platform'
                })
                export class PlatformService {
                  init() {}
                }
                """;
            File.WriteAllText(Path.Combine(tempDir, "src", "app", "services", "platform.service.ts"), serviceContent);

            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<AngularStaticAnalysisService>>();
            var service = new AngularStaticAnalysisService(loggerMock.Object);

            // Act
            var result = await service.AnalyzeAsync(tempDir);

            // Assert
            Assert.Single(result.Services);
            Assert.Equal("platform", result.Services[0].ProvidedIn);

            // Verify no issue is reported
            var providedInIssue = result.Issues.FirstOrDefault(i =>
                i.Message.Contains("PlatformService") &&
                i.Message.Contains("providedIn"));

            Assert.Null(providedInIssue);
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
