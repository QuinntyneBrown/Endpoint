// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using Endpoint.Engineering.StaticAnalysis.Angular;
using Endpoint.Engineering.StaticAnalysis.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Angular.AcceptanceTests;

/// <summary>
/// Base class for Angular Static Analysis acceptance tests.
/// Provides common setup for test fixtures and Angular workspace creation.
/// </summary>
public abstract class AngularStaticAnalysisTestBase : IDisposable
{
    protected readonly Mock<ILogger<AngularStaticAnalysisService>> LoggerMock;
    protected readonly MockFileSystem FileSystem;
    protected readonly AngularStaticAnalysisService Service;
    protected readonly string WorkspaceRoot;

    protected AngularStaticAnalysisTestBase()
    {
        LoggerMock = new Mock<ILogger<AngularStaticAnalysisService>>();
        FileSystem = new MockFileSystem();
        Service = new AngularStaticAnalysisService(LoggerMock.Object);
        WorkspaceRoot = "/test/angular-app";
    }

    /// <summary>
    /// Creates a basic Angular workspace with angular.json and package.json.
    /// </summary>
    protected void SetupAngularWorkspace(string angularVersion = "17.0.0")
    {
        FileSystem.Directory.CreateDirectory(WorkspaceRoot);
        FileSystem.Directory.CreateDirectory($"{WorkspaceRoot}/src");
        FileSystem.Directory.CreateDirectory($"{WorkspaceRoot}/src/app");

        // Create angular.json
        var angularJson = """
        {
          "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
          "version": 1,
          "projects": {
            "test-app": {
              "projectType": "application",
              "root": "",
              "sourceRoot": "src",
              "prefix": "app"
            }
          }
        }
        """;
        FileSystem.File.WriteAllText($"{WorkspaceRoot}/angular.json", angularJson);

        // Create package.json
        var packageJson = $$"""
        {
          "name": "test-app",
          "version": "0.0.0",
          "dependencies": {
            "@angular/core": "^{{angularVersion}}"
          }
        }
        """;
        FileSystem.File.WriteAllText($"{WorkspaceRoot}/package.json", packageJson);
    }

    /// <summary>
    /// Creates a TypeScript file in the workspace.
    /// </summary>
    protected void CreateTypeScriptFile(string relativePath, string content)
    {
        var fullPath = $"{WorkspaceRoot}/{relativePath}";
        var directory = FileSystem.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !FileSystem.Directory.Exists(directory))
        {
            FileSystem.Directory.CreateDirectory(directory);
        }
        FileSystem.File.WriteAllText(fullPath, content);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
