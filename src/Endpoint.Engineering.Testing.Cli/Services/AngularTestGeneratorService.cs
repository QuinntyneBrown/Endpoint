// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text;
using Endpoint.Engineering.Testing.Cli.Models;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Testing.Cli.Services;

public class AngularTestGeneratorService : IAngularTestGeneratorService
{
    private readonly ILogger<AngularTestGeneratorService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ITypeScriptParser _parser;

    public AngularTestGeneratorService(
        ILogger<AngularTestGeneratorService> logger,
        IFileSystem fileSystem,
        ITypeScriptParser parser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public async Task<TestGenerationResult> GenerateTestAsync(string sourceFilePath, string? outputDirectory, bool overwrite, bool dryRun)
    {
        var result = new TestGenerationResult
        {
            SourceFilePath = sourceFilePath
        };

        try
        {
            var content = await _fileSystem.File.ReadAllTextAsync(sourceFilePath);
            var fileInfo = _parser.Parse(sourceFilePath, content);

            if (fileInfo.FileType == AngularFileType.Unknown ||
                fileInfo.FileType == AngularFileType.Interface ||
                fileInfo.FileType == AngularFileType.Enum)
            {
                result.SkipReason = $"Skipping {fileInfo.FileType} file - no tests needed";
                return result;
            }

            if (string.IsNullOrEmpty(fileInfo.ClassName))
            {
                result.SkipReason = "No class found in file";
                return result;
            }

            var testFilePath = GetTestFilePath(sourceFilePath, outputDirectory);
            result.TestFilePath = testFilePath;

            if (_fileSystem.File.Exists(testFilePath) && !overwrite)
            {
                result.SkipReason = "Test file already exists (use --overwrite to replace)";
                return result;
            }

            var testContent = GenerateTestContent(fileInfo);
            result.TestContent = testContent;

            if (!dryRun)
            {
                var directory = _fileSystem.Path.GetDirectoryName(testFilePath);
                if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
                {
                    _fileSystem.Directory.CreateDirectory(directory);
                }

                await _fileSystem.File.WriteAllTextAsync(testFilePath, testContent);
            }

            result.Generated = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test for {FilePath}", sourceFilePath);
            result.SkipReason = ex.Message;
            return result;
        }
    }

    private string GetTestFilePath(string sourceFilePath, string? outputDirectory)
    {
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(sourceFilePath);
        var testFileName = $"{fileName}.spec.ts";

        if (!string.IsNullOrEmpty(outputDirectory))
        {
            return _fileSystem.Path.Combine(outputDirectory, testFileName);
        }

        var sourceDirectory = _fileSystem.Path.GetDirectoryName(sourceFilePath) ?? ".";
        return _fileSystem.Path.Combine(sourceDirectory, testFileName);
    }

    private string GenerateTestContent(TypeScriptFileInfo fileInfo)
    {
        return fileInfo.FileType switch
        {
            AngularFileType.Component => GenerateComponentTest(fileInfo),
            AngularFileType.Service => GenerateServiceTest(fileInfo),
            AngularFileType.Directive => GenerateDirectiveTest(fileInfo),
            AngularFileType.Pipe => GeneratePipeTest(fileInfo),
            AngularFileType.Guard => GenerateGuardTest(fileInfo),
            AngularFileType.Interceptor => GenerateInterceptorTest(fileInfo),
            AngularFileType.Resolver => GenerateResolverTest(fileInfo),
            _ => GenerateClassTest(fileInfo)
        };
    }

    private string GenerateComponentTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine("import { ComponentFixture, TestBed } from '@angular/core/testing';");

        if (fileInfo.Dependencies.Any())
        {
            sb.AppendLine("import { of } from 'rxjs';");
        }

        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();

        // Generate mock classes for dependencies
        foreach (var dep in fileInfo.Dependencies)
        {
            sb.AppendLine($"const mock{dep} = {{");
            sb.AppendLine("  // Add mock methods as needed");
            sb.AppendLine("};");
            sb.AppendLine();
        }

        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let component: {className};");
        sb.AppendLine($"  let fixture: ComponentFixture<{className}>;");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"  let {camelCase}: jasmine.SpyObj<{dep}>;");
        }

        sb.AppendLine();
        sb.AppendLine("  beforeEach(async () => {");

        // Create spy objects for dependencies
        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"    {camelCase} = jasmine.createSpyObj('{dep}', ['/* add methods */']);");
        }

        sb.AppendLine();
        sb.AppendLine("    await TestBed.configureTestingModule({");

        if (fileInfo.IsStandalone)
        {
            sb.AppendLine($"      imports: [{className}],");
        }
        else
        {
            sb.AppendLine($"      declarations: [{className}],");
        }

        if (fileInfo.Dependencies.Any())
        {
            sb.AppendLine("      providers: [");
            foreach (var dep in fileInfo.Dependencies)
            {
                var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
                sb.AppendLine($"        {{ provide: {dep}, useValue: {camelCase} }},");
            }

            sb.AppendLine("      ],");
        }

        sb.AppendLine("    }).compileComponents();");
        sb.AppendLine();
        sb.AppendLine($"    fixture = TestBed.createComponent({className});");
        sb.AppendLine("    component = fixture.componentInstance;");
        sb.AppendLine("    fixture.detectChanges();");
        sb.AppendLine("  });");
        sb.AppendLine();

        // Basic creation test
        sb.AppendLine("  it('should create', () => {");
        sb.AppendLine("    expect(component).toBeTruthy();");
        sb.AppendLine("  });");

        // Generate tests for each public method
        foreach (var method in fileInfo.Methods.Where(m => m.AccessModifier != "private"))
        {
            sb.AppendLine();
            GenerateMethodTest(sb, method, "component");
        }

        // Generate tests for @Input properties
        foreach (var prop in fileInfo.Properties.Where(p => p.Decorator?.Contains("@Input") == true))
        {
            sb.AppendLine();
            sb.AppendLine($"  describe('{prop.Name} input', () => {{");
            sb.AppendLine($"    it('should accept {prop.Name} value', () => {{");
            sb.AppendLine($"      const testValue = {GetDefaultValue(prop.Type)};");
            sb.AppendLine($"      component.{prop.Name} = testValue;");
            sb.AppendLine($"      expect(component.{prop.Name}).toEqual(testValue);");
            sb.AppendLine("    });");
            sb.AppendLine("  });");
        }

        // Generate tests for @Output properties
        foreach (var prop in fileInfo.Properties.Where(p => p.Decorator?.Contains("@Output") == true))
        {
            sb.AppendLine();
            sb.AppendLine($"  describe('{prop.Name} output', () => {{");
            sb.AppendLine($"    it('should emit when triggered', () => {{");
            sb.AppendLine($"      const spy = jest.fn();");
            sb.AppendLine($"      component.{prop.Name}.subscribe(spy);");
            sb.AppendLine($"      // Trigger the output emission");
            sb.AppendLine($"      // expect(spy).toHaveBeenCalled();");
            sb.AppendLine("    });");
            sb.AppendLine("  });");
        }

        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GenerateServiceTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine("import { TestBed } from '@angular/core/testing';");

        if (fileInfo.Dependencies.Any(d => d.Contains("HttpClient")))
        {
            sb.AppendLine("import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';");
        }

        sb.AppendLine("import { of, throwError } from 'rxjs';");
        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();

        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let service: {className};");

        if (fileInfo.Dependencies.Any(d => d.Contains("HttpClient")))
        {
            sb.AppendLine("  let httpMock: HttpTestingController;");
        }

        foreach (var dep in fileInfo.Dependencies.Where(d => !d.Contains("HttpClient")))
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"  let {camelCase}Spy: jasmine.SpyObj<{dep}>;");
        }

        sb.AppendLine();
        sb.AppendLine("  beforeEach(() => {");

        foreach (var dep in fileInfo.Dependencies.Where(d => !d.Contains("HttpClient")))
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"    {camelCase}Spy = jasmine.createSpyObj('{dep}', ['/* add methods */']);");
        }

        sb.AppendLine();
        sb.AppendLine("    TestBed.configureTestingModule({");

        if (fileInfo.Dependencies.Any(d => d.Contains("HttpClient")))
        {
            sb.AppendLine("      imports: [HttpClientTestingModule],");
        }

        sb.AppendLine("      providers: [");
        sb.AppendLine($"        {className},");

        foreach (var dep in fileInfo.Dependencies.Where(d => !d.Contains("HttpClient")))
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"        {{ provide: {dep}, useValue: {camelCase}Spy }},");
        }

        sb.AppendLine("      ],");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine($"    service = TestBed.inject({className});");

        if (fileInfo.Dependencies.Any(d => d.Contains("HttpClient")))
        {
            sb.AppendLine("    httpMock = TestBed.inject(HttpTestingController);");
        }

        sb.AppendLine("  });");
        sb.AppendLine();

        if (fileInfo.Dependencies.Any(d => d.Contains("HttpClient")))
        {
            sb.AppendLine("  afterEach(() => {");
            sb.AppendLine("    httpMock.verify();");
            sb.AppendLine("  });");
            sb.AppendLine();
        }

        // Basic creation test
        sb.AppendLine("  it('should be created', () => {");
        sb.AppendLine("    expect(service).toBeTruthy();");
        sb.AppendLine("  });");

        // Generate tests for each public method
        foreach (var method in fileInfo.Methods.Where(m => m.AccessModifier != "private"))
        {
            sb.AppendLine();
            GenerateMethodTest(sb, method, "service");
        }

        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GenerateDirectiveTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine("import { Component } from '@angular/core';");
        sb.AppendLine("import { ComponentFixture, TestBed } from '@angular/core/testing';");
        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();
        sb.AppendLine("@Component({");
        sb.AppendLine($"  template: '<div {fileInfo.Selector ?? "appDirective"}>Test</div>',");
        sb.AppendLine("})");
        sb.AppendLine("class TestHostComponent {}");
        sb.AppendLine();
        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine("  let fixture: ComponentFixture<TestHostComponent>;");
        sb.AppendLine();
        sb.AppendLine("  beforeEach(async () => {");
        sb.AppendLine("    await TestBed.configureTestingModule({");
        sb.AppendLine($"      declarations: [TestHostComponent, {className}],");
        sb.AppendLine("    }).compileComponents();");
        sb.AppendLine();
        sb.AppendLine("    fixture = TestBed.createComponent(TestHostComponent);");
        sb.AppendLine("    fixture.detectChanges();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  it('should create an instance', () => {");
        sb.AppendLine($"    const directive = new {className}();");
        sb.AppendLine("    expect(directive).toBeTruthy();");
        sb.AppendLine("  });");

        foreach (var method in fileInfo.Methods.Where(m => m.AccessModifier != "private"))
        {
            sb.AppendLine();
            GenerateMethodTest(sb, method, "directive");
        }

        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GeneratePipeTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();
        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let pipe: {className};");
        sb.AppendLine();
        sb.AppendLine("  beforeEach(() => {");
        sb.AppendLine($"    pipe = new {className}();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  it('should create an instance', () => {");
        sb.AppendLine("    expect(pipe).toBeTruthy();");
        sb.AppendLine("  });");
        sb.AppendLine();

        // Find transform method
        var transformMethod = fileInfo.Methods.FirstOrDefault(m => m.Name == "transform");
        if (transformMethod != null)
        {
            sb.AppendLine("  describe('transform', () => {");
            sb.AppendLine("    it('should transform the value', () => {");
            sb.AppendLine("      // Arrange");
            sb.AppendLine($"      const input = {GetDefaultValue(transformMethod.Parameters.FirstOrDefault()?.Type ?? "string")};");
            sb.AppendLine();
            sb.AppendLine("      // Act");
            sb.AppendLine("      const result = pipe.transform(input);");
            sb.AppendLine();
            sb.AppendLine("      // Assert");
            sb.AppendLine("      expect(result).toBeDefined();");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    it('should handle null input', () => {");
            sb.AppendLine("      const result = pipe.transform(null as any);");
            sb.AppendLine("      expect(result).toBeDefined();");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    it('should handle undefined input', () => {");
            sb.AppendLine("      const result = pipe.transform(undefined as any);");
            sb.AppendLine("      expect(result).toBeDefined();");
            sb.AppendLine("    });");
            sb.AppendLine("  });");
        }

        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GenerateGuardTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine("import { TestBed } from '@angular/core/testing';");
        sb.AppendLine("import { RouterTestingModule } from '@angular/router/testing';");
        sb.AppendLine("import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';");
        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();
        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let guard: {className};");
        sb.AppendLine("  let mockRoute: ActivatedRouteSnapshot;");
        sb.AppendLine("  let mockState: RouterStateSnapshot;");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"  let {camelCase}Spy: jasmine.SpyObj<{dep}>;");
        }

        sb.AppendLine();
        sb.AppendLine("  beforeEach(() => {");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"    {camelCase}Spy = jasmine.createSpyObj('{dep}', ['/* add methods */']);");
        }

        sb.AppendLine("    mockRoute = {} as ActivatedRouteSnapshot;");
        sb.AppendLine("    mockState = {} as RouterStateSnapshot;");
        sb.AppendLine();
        sb.AppendLine("    TestBed.configureTestingModule({");
        sb.AppendLine("      imports: [RouterTestingModule],");
        sb.AppendLine("      providers: [");
        sb.AppendLine($"        {className},");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"        {{ provide: {dep}, useValue: {camelCase}Spy }},");
        }

        sb.AppendLine("      ],");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine($"    guard = TestBed.inject({className});");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  it('should be created', () => {");
        sb.AppendLine("    expect(guard).toBeTruthy();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  describe('canActivate', () => {");
        sb.AppendLine("    it('should return true when authorized', () => {");
        sb.AppendLine("      // Arrange - set up authorization");
        sb.AppendLine();
        sb.AppendLine("      // Act");
        sb.AppendLine("      const result = guard.canActivate(mockRoute, mockState);");
        sb.AppendLine();
        sb.AppendLine("      // Assert");
        sb.AppendLine("      expect(result).toBeTruthy();");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine("    it('should return false when not authorized', () => {");
        sb.AppendLine("      // Arrange - set up unauthorized state");
        sb.AppendLine();
        sb.AppendLine("      // Act");
        sb.AppendLine("      const result = guard.canActivate(mockRoute, mockState);");
        sb.AppendLine();
        sb.AppendLine("      // Assert");
        sb.AppendLine("      expect(result).toBeFalsy();");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GenerateInterceptorTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine("import { TestBed } from '@angular/core/testing';");
        sb.AppendLine("import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';");
        sb.AppendLine("import { HTTP_INTERCEPTORS, HttpClient, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';");
        sb.AppendLine("import { Observable, of } from 'rxjs';");
        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();
        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let interceptor: {className};");
        sb.AppendLine("  let httpMock: HttpTestingController;");
        sb.AppendLine("  let httpClient: HttpClient;");
        sb.AppendLine();
        sb.AppendLine("  beforeEach(() => {");
        sb.AppendLine("    TestBed.configureTestingModule({");
        sb.AppendLine("      imports: [HttpClientTestingModule],");
        sb.AppendLine("      providers: [");
        sb.AppendLine($"        {className},");
        sb.AppendLine("        {");
        sb.AppendLine("          provide: HTTP_INTERCEPTORS,");
        sb.AppendLine($"          useClass: {className},");
        sb.AppendLine("          multi: true,");
        sb.AppendLine("        },");
        sb.AppendLine("      ],");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine($"    interceptor = TestBed.inject({className});");
        sb.AppendLine("    httpMock = TestBed.inject(HttpTestingController);");
        sb.AppendLine("    httpClient = TestBed.inject(HttpClient);");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  afterEach(() => {");
        sb.AppendLine("    httpMock.verify();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  it('should be created', () => {");
        sb.AppendLine("    expect(interceptor).toBeTruthy();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  describe('intercept', () => {");
        sb.AppendLine("    it('should intercept HTTP requests', () => {");
        sb.AppendLine("      // Arrange");
        sb.AppendLine("      const testUrl = '/api/test';");
        sb.AppendLine();
        sb.AppendLine("      // Act");
        sb.AppendLine("      httpClient.get(testUrl).subscribe();");
        sb.AppendLine();
        sb.AppendLine("      // Assert");
        sb.AppendLine("      const req = httpMock.expectOne(testUrl);");
        sb.AppendLine("      expect(req.request.method).toBe('GET');");
        sb.AppendLine("      req.flush({});");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GenerateResolverTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine("import { TestBed } from '@angular/core/testing';");
        sb.AppendLine("import { RouterTestingModule } from '@angular/router/testing';");
        sb.AppendLine("import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';");
        sb.AppendLine("import { of } from 'rxjs';");
        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();
        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let resolver: {className};");
        sb.AppendLine("  let mockRoute: ActivatedRouteSnapshot;");
        sb.AppendLine("  let mockState: RouterStateSnapshot;");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"  let {camelCase}Spy: jasmine.SpyObj<{dep}>;");
        }

        sb.AppendLine();
        sb.AppendLine("  beforeEach(() => {");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"    {camelCase}Spy = jasmine.createSpyObj('{dep}', ['/* add methods */']);");
        }

        sb.AppendLine("    mockRoute = { params: {} } as any;");
        sb.AppendLine("    mockState = {} as RouterStateSnapshot;");
        sb.AppendLine();
        sb.AppendLine("    TestBed.configureTestingModule({");
        sb.AppendLine("      imports: [RouterTestingModule],");
        sb.AppendLine("      providers: [");
        sb.AppendLine($"        {className},");

        foreach (var dep in fileInfo.Dependencies)
        {
            var camelCase = char.ToLowerInvariant(dep[0]) + dep[1..];
            sb.AppendLine($"        {{ provide: {dep}, useValue: {camelCase}Spy }},");
        }

        sb.AppendLine("      ],");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine($"    resolver = TestBed.inject({className});");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  it('should be created', () => {");
        sb.AppendLine("    expect(resolver).toBeTruthy();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  describe('resolve', () => {");
        sb.AppendLine("    it('should resolve data', (done) => {");
        sb.AppendLine("      // Arrange - set up service responses");
        sb.AppendLine();
        sb.AppendLine("      // Act");
        sb.AppendLine("      const result = resolver.resolve(mockRoute, mockState);");
        sb.AppendLine();
        sb.AppendLine("      // Assert");
        sb.AppendLine("      if (result instanceof Observable) {");
        sb.AppendLine("        result.subscribe((data) => {");
        sb.AppendLine("          expect(data).toBeDefined();");
        sb.AppendLine("          done();");
        sb.AppendLine("        });");
        sb.AppendLine("      } else {");
        sb.AppendLine("        expect(result).toBeDefined();");
        sb.AppendLine("        done();");
        sb.AppendLine("      }");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("});");

        return sb.ToString();
    }

    private string GenerateClassTest(TypeScriptFileInfo fileInfo)
    {
        var sb = new StringBuilder();
        var className = fileInfo.ClassName;
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileInfo.FilePath);

        sb.AppendLine($"import {{ {className} }} from './{fileName}';");
        sb.AppendLine();
        sb.AppendLine($"describe('{className}', () => {{");
        sb.AppendLine($"  let instance: {className};");
        sb.AppendLine();
        sb.AppendLine("  beforeEach(() => {");
        sb.AppendLine($"    instance = new {className}();");
        sb.AppendLine("  });");
        sb.AppendLine();
        sb.AppendLine("  it('should create an instance', () => {");
        sb.AppendLine("    expect(instance).toBeTruthy();");
        sb.AppendLine("  });");

        foreach (var method in fileInfo.Methods.Where(m => m.AccessModifier != "private"))
        {
            sb.AppendLine();
            GenerateMethodTest(sb, method, "instance");
        }

        sb.AppendLine("});");

        return sb.ToString();
    }

    private void GenerateMethodTest(StringBuilder sb, TypeScriptMethod method, string instanceName)
    {
        sb.AppendLine($"  describe('{method.Name}', () => {{");

        // Success case
        sb.AppendLine($"    it('should {GetMethodTestDescription(method)}', {(method.IsAsync ? "async " : "")}() => {{");
        sb.AppendLine("      // Arrange");

        foreach (var param in method.Parameters)
        {
            sb.AppendLine($"      const {param.Name} = {GetDefaultValue(param.Type)};");
        }

        sb.AppendLine();
        sb.AppendLine("      // Act");

        var paramNames = string.Join(", ", method.Parameters.Select(p => p.Name));
        var awaitKeyword = method.IsAsync ? "await " : "";

        if (method.ReturnType != "void")
        {
            sb.AppendLine($"      const result = {awaitKeyword}{instanceName}.{method.Name}({paramNames});");
        }
        else
        {
            sb.AppendLine($"      {awaitKeyword}{instanceName}.{method.Name}({paramNames});");
        }

        sb.AppendLine();
        sb.AppendLine("      // Assert");

        if (method.ReturnType != "void")
        {
            sb.AppendLine("      expect(result).toBeDefined();");
        }
        else
        {
            sb.AppendLine($"      // Verify expected behavior");
        }

        sb.AppendLine("    });");

        // Edge case for methods with parameters
        if (method.Parameters.Any())
        {
            sb.AppendLine();
            sb.AppendLine($"    it('should handle edge cases', {(method.IsAsync ? "async " : "")}() => {{");
            sb.AppendLine("      // Test with boundary values or edge cases");
            sb.AppendLine("    });");
        }

        sb.AppendLine("  });");
    }

    private static string GetMethodTestDescription(TypeScriptMethod method)
    {
        var verb = method.IsAsync ? "asynchronously " : "";
        var action = method.ReturnType != "void" ? "return expected result" : "execute successfully";
        return $"{verb}{action}";
    }

    private static string GetDefaultValue(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "string" => "'test'",
            "number" => "0",
            "boolean" => "true",
            "any" => "{}",
            "void" => "undefined",
            "null" => "null",
            "undefined" => "undefined",
            "object" => "{}",
            "date" => "new Date()",
            _ when type.EndsWith("[]") => "[]",
            _ when type.StartsWith("Observable<") => "of({})",
            _ when type.StartsWith("Promise<") => "Promise.resolve({})",
            _ => "{}"
        };
    }
}
