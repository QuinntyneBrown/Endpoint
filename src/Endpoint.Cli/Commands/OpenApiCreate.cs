// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Services;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace Endpoint.Cli.Commands;

[Verb("open-api")]
public class OpenApiCreateRequest : IRequest
{
    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    [Option('o', "output")]
    public string OutputPath { get; set; }
}

public class OpenApiCreateRequestHandler : IRequestHandler<OpenApiCreateRequest>
{
    private readonly ILogger<OpenApiCreateRequestHandler> _logger;
    private readonly IFileProvider _fileProvider;

    public OpenApiCreateRequestHandler(
        ILogger<OpenApiCreateRequestHandler> logger,
        IFileProvider fileProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
    }

    public async Task Handle(OpenApiCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating OpenAPI specification for solution in directory: {directory}", request.Directory);

        var solutionPath = _fileProvider.Get("*.sln", request.Directory);

        if (string.IsNullOrEmpty(solutionPath))
        {
            _logger.LogError("No solution file found in directory: {directory}", request.Directory);
            return;
        }

        _logger.LogInformation("Found solution: {solutionPath}", solutionPath);

        var solutionDirectory = Path.GetDirectoryName(solutionPath);
        var solutionName = Path.GetFileNameWithoutExtension(solutionPath);

        var openApiDocument = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = $"{solutionName} API",
                Version = "v1",
                Description = $"OpenAPI specification for {solutionName}",
            },
            Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = "https://localhost:5001", Description = "Development server" },
            },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>(),
            },
        };

        var csFiles = System.IO.Directory.GetFiles(solutionDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
            .ToList();

        _logger.LogInformation("Found {count} C# files to analyze", csFiles.Count);

        foreach (var csFile in csFiles)
        {
            try
            {
                var sourceCode = await File.ReadAllTextAsync(csFile, cancellationToken);
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = await syntaxTree.GetRootAsync(cancellationToken);

                var classDeclarations = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();

                foreach (var classDecl in classDeclarations)
                {
                    if (IsControllerClass(classDecl))
                    {
                        ProcessController(classDecl, openApiDocument);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process file: {file}", csFile);
            }
        }

        var outputPath = request.OutputPath ?? Path.Combine(solutionDirectory, "openapi.json");

        var json = openApiDocument.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
        await File.WriteAllTextAsync(outputPath, json, cancellationToken);

        _logger.LogInformation("OpenAPI specification generated at: {outputPath}", outputPath);
    }

    private bool IsControllerClass(ClassDeclarationSyntax classDecl)
    {
        var className = classDecl.Identifier.Text;

        if (className.EndsWith("Controller"))
        {
            return true;
        }

        var attributes = classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString());

        return attributes.Any(a =>
            a.Contains("ApiController") ||
            a.Contains("Controller") ||
            a.Contains("Route"));
    }

    private void ProcessController(ClassDeclarationSyntax classDecl, OpenApiDocument document)
    {
        var className = classDecl.Identifier.Text;
        var controllerName = className.Replace("Controller", string.Empty);
        var controllerRoute = GetControllerRoute(classDecl, controllerName);

        _logger.LogDebug("Processing controller: {controller} with route: {route}", className, controllerRoute);

        var methods = classDecl.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)));

        foreach (var method in methods)
        {
            ProcessControllerMethod(method, controllerRoute, document);
        }
    }

    private string GetControllerRoute(ClassDeclarationSyntax classDecl, string controllerName)
    {
        var routeAttribute = classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains("Route"));

        if (routeAttribute?.ArgumentList?.Arguments.Count > 0)
        {
            var routeArg = routeAttribute.ArgumentList.Arguments[0].ToString().Trim('"', '\'');
            return routeArg.Replace("[controller]", controllerName.ToLowerInvariant());
        }

        return $"api/{controllerName.ToLowerInvariant()}";
    }

    private void ProcessControllerMethod(MethodDeclarationSyntax method, string controllerRoute, OpenApiDocument document)
    {
        var httpMethod = GetHttpMethod(method);
        if (httpMethod == null)
        {
            return;
        }

        var methodRoute = GetMethodRoute(method);
        var fullPath = $"/{controllerRoute}{methodRoute}".Replace("//", "/");

        if (!document.Paths.ContainsKey(fullPath))
        {
            document.Paths[fullPath] = new OpenApiPathItem();
        }

        var operation = new OpenApiOperation
        {
            Summary = GetMethodSummary(method),
            Description = GetMethodSummary(method),
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                            },
                        },
                    },
                },
            },
        };

        var parameters = method.ParameterList.Parameters;
        foreach (var param in parameters)
        {
            var paramName = param.Identifier.Text;
            var paramType = param.Type?.ToString() ?? "string";

            var fromBodyAttr = param.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("FromBody"));

            var fromRouteAttr = param.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("FromRoute"));

            var isInRoute = fullPath.Contains($"{{{paramName}}}");

            if (fromBodyAttr)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = MapCSharpTypeToOpenApiType(paramType),
                            },
                        },
                    },
                };
            }
            else
            {
                var paramLocation = (fromRouteAttr || isInRoute) ? ParameterLocation.Path : ParameterLocation.Query;
                var isRequired = paramLocation == ParameterLocation.Path;

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = paramName,
                    In = paramLocation,
                    Required = isRequired,
                    Schema = new OpenApiSchema
                    {
                        Type = MapCSharpTypeToOpenApiType(paramType),
                    },
                });
            }
        }

        SetOperation(document.Paths[fullPath], httpMethod.Value, operation);
    }

    private OperationType? GetHttpMethod(MethodDeclarationSyntax method)
    {
        var attributes = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString())
            .ToList();

        if (attributes.Any(a => a.Contains("HttpGet") || a.Contains("Get")))
        {
            return OperationType.Get;
        }

        if (attributes.Any(a => a.Contains("HttpPost") || a.Contains("Post")))
        {
            return OperationType.Post;
        }

        if (attributes.Any(a => a.Contains("HttpPut") || a.Contains("Put")))
        {
            return OperationType.Put;
        }

        if (attributes.Any(a => a.Contains("HttpDelete") || a.Contains("Delete")))
        {
            return OperationType.Delete;
        }

        if (attributes.Any(a => a.Contains("HttpPatch") || a.Contains("Patch")))
        {
            return OperationType.Patch;
        }

        return null;
    }

    private string GetMethodRoute(MethodDeclarationSyntax method)
    {
        var routeAttribute = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains("Route") || a.Name.ToString().Contains("Http"));

        if (routeAttribute?.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = routeAttribute.ArgumentList.Arguments[0];
            var route = firstArg.ToString().Trim('"', '\'');

            if (!string.IsNullOrWhiteSpace(route) && !route.StartsWith("\""))
            {
                return route.StartsWith("/") ? route : $"/{route}";
            }
        }

        return string.Empty;
    }

    private string GetMethodSummary(MethodDeclarationSyntax method)
    {
        var trivia = method.GetLeadingTrivia();
        var xmlComment = trivia.FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                                     t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

        if (!xmlComment.Equals(default(SyntaxTrivia)))
        {
            var xml = xmlComment.ToString();
            var summaryStart = xml.IndexOf("<summary>", StringComparison.Ordinal);
            var summaryEnd = xml.IndexOf("</summary>", StringComparison.Ordinal);

            if (summaryStart >= 0 && summaryEnd > summaryStart)
            {
                return xml.Substring(summaryStart + 9, summaryEnd - summaryStart - 9).Trim();
            }
        }

        return method.Identifier.Text;
    }

    private string MapCSharpTypeToOpenApiType(string csharpType)
    {
        return csharpType.ToLowerInvariant() switch
        {
            "int" or "int32" or "long" or "int64" => "integer",
            "bool" or "boolean" => "boolean",
            "float" or "double" or "decimal" => "number",
            "string" => "string",
            "datetime" or "datetimeoffset" => "string",
            "guid" => "string",
            _ => "object",
        };
    }

    private void SetOperation(OpenApiPathItem pathItem, OperationType operationType, OpenApiOperation operation)
    {
        switch (operationType)
        {
            case OperationType.Get:
                pathItem.Operations[OperationType.Get] = operation;
                break;
            case OperationType.Post:
                pathItem.Operations[OperationType.Post] = operation;
                break;
            case OperationType.Put:
                pathItem.Operations[OperationType.Put] = operation;
                break;
            case OperationType.Delete:
                pathItem.Operations[OperationType.Delete] = operation;
                break;
            case OperationType.Patch:
                pathItem.Operations[OperationType.Patch] = operation;
                break;
        }
    }
}
