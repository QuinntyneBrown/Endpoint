// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.UnitTests;

public class ImportSyntaxGenerationStrategyTests
{
    private readonly Mock<ILogger<ImportSyntaxGenerationStrategy>> _mockLogger;
    private readonly ImportSyntaxGenerationStrategy _sut;

    public ImportSyntaxGenerationStrategyTests()
    {
        _mockLogger = new Mock<ILogger<ImportSyntaxGenerationStrategy>>();
        _sut = new ImportSyntaxGenerationStrategy(_mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ImportSyntaxGenerationStrategy(null!));
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new ImportSyntaxGenerationStrategy(_mockLogger.Object));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region GenerateAsync with Single Type Tests

    [Fact]
    public async Task GenerateAsync_WithSingleType_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel("Component", "@angular/core");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { Component } from \"@angular/core\";", result);
    }

    [Fact]
    public async Task GenerateAsync_WithSingleTypeAndRelativeModule_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel("UserService", "./services/user.service");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { UserService } from \"./services/user.service\";", result);
    }

    [Fact]
    public async Task GenerateAsync_WithSingleTypeAndIndexModule_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel("HttpClient", "@angular/common/http");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { HttpClient } from \"@angular/common/http\";", result);
    }

    #endregion

    #region GenerateAsync with Multiple Types Tests

    [Fact]
    public async Task GenerateAsync_WithMultipleTypes_ShouldGenerateCommaDelimitedImport()
    {
        // Arrange
        var model = new ImportModel
        {
            Module = "@angular/core",
            Types = new List<TypeModel>
            {
                new("Component"),
                new("OnInit"),
                new("Input")
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { Component,OnInit,Input } from \"@angular/core\";", result);
    }

    [Fact]
    public async Task GenerateAsync_WithTwoTypes_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel
        {
            Module = "@angular/forms",
            Types = new List<TypeModel>
            {
                new("FormGroup"),
                new("FormControl")
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { FormGroup,FormControl } from \"@angular/forms\";", result);
    }

    [Fact]
    public async Task GenerateAsync_WithManyTypes_ShouldGenerateAllTypes()
    {
        // Arrange
        var model = new ImportModel
        {
            Module = "@angular/core",
            Types = new List<TypeModel>
            {
                new("Component"),
                new("OnInit"),
                new("OnDestroy"),
                new("Input"),
                new("Output"),
                new("EventEmitter")
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("Component", result);
        Assert.Contains("OnInit", result);
        Assert.Contains("OnDestroy", result);
        Assert.Contains("Input", result);
        Assert.Contains("Output", result);
        Assert.Contains("EventEmitter", result);
    }

    #endregion

    #region Import Statement Format Tests

    [Fact]
    public async Task GenerateAsync_ShouldStartWithImportKeyword()
    {
        // Arrange
        var model = new ImportModel("Observable", "rxjs");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.StartsWith("import", result);
    }

    [Fact]
    public async Task GenerateAsync_ShouldContainCurlyBraces()
    {
        // Arrange
        var model = new ImportModel("Injectable", "@angular/core");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("{ ", result);
        Assert.Contains(" }", result);
    }

    [Fact]
    public async Task GenerateAsync_ShouldContainFromKeyword()
    {
        // Arrange
        var model = new ImportModel("Router", "@angular/router");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("from", result);
    }

    [Fact]
    public async Task GenerateAsync_ShouldEndWithSemicolon()
    {
        // Arrange
        var model = new ImportModel("ActivatedRoute", "@angular/router");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.EndsWith(";", result);
    }

    [Fact]
    public async Task GenerateAsync_ShouldUseDoubleQuotesForModule()
    {
        // Arrange
        var model = new ImportModel("BrowserModule", "@angular/platform-browser");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("\"@angular/platform-browser\"", result);
    }

    #endregion

    #region Module Path Tests

    [Fact]
    public async Task GenerateAsync_WithScopedPackage_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel("MatButtonModule", "@angular/material/button");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("@angular/material/button", result);
    }

    [Fact]
    public async Task GenerateAsync_WithRelativePath_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel("AppComponent", "./app.component");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("./app.component", result);
    }

    [Fact]
    public async Task GenerateAsync_WithParentRelativePath_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel("SharedModule", "../shared/shared.module");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Contains("../shared/shared.module", result);
    }

    [Fact]
    public async Task GenerateAsync_WithRxjsImport_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel
        {
            Module = "rxjs",
            Types = new List<TypeModel>
            {
                new("Observable"),
                new("Subject"),
                new("BehaviorSubject")
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { Observable,Subject,BehaviorSubject } from \"rxjs\";", result);
    }

    [Fact]
    public async Task GenerateAsync_WithRxjsOperators_ShouldGenerateCorrectImport()
    {
        // Arrange
        var model = new ImportModel
        {
            Module = "rxjs/operators",
            Types = new List<TypeModel>
            {
                new("map"),
                new("filter"),
                new("tap")
            }
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { map,filter,tap } from \"rxjs/operators\";", result);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GenerateAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var model = new ImportModel("Component", "@angular/core");
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _sut.GenerateAsync(model, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region ImportModel Tests

    [Fact]
    public async Task GenerateAsync_WithDefaultImportModel_ShouldHandleEmptyTypes()
    {
        // Arrange
        var model = new ImportModel
        {
            Module = "@angular/core",
            Types = new List<TypeModel>()
        };

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import {  } from \"@angular/core\";", result);
    }

    [Fact]
    public async Task GenerateAsync_WithConstructorOverload_ShouldWork()
    {
        // Arrange
        var model = new ImportModel("NgModule", "@angular/core");

        // Act
        var result = await _sut.GenerateAsync(model, CancellationToken.None);

        // Assert
        Assert.Equal("import { NgModule } from \"@angular/core\";", result);
    }

    #endregion

    #region Common Angular Import Tests

    [Fact]
    public async Task GenerateAsync_WithCommonAngularImports_ShouldGenerateCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("Component", "@angular/core"),
            ("Injectable", "@angular/core"),
            ("NgModule", "@angular/core"),
            ("HttpClientModule", "@angular/common/http"),
            ("FormsModule", "@angular/forms"),
            ("RouterModule", "@angular/router")
        };

        foreach (var (typeName, module) in testCases)
        {
            var model = new ImportModel(typeName, module);

            // Act
            var result = await _sut.GenerateAsync(model, CancellationToken.None);

            // Assert
            Assert.Equal($"import {{ {typeName} }} from \"{module}\";", result);
        }
    }

    #endregion
}
