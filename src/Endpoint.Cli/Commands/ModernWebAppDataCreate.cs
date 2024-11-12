// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("mwa-data-create")]
public class ModernWebAppDataCreateRequest : IRequest
{
    [Option('n', "name", Required = true )]
    public string ProductName { get; set; }

    [Option('b', "bounded-context-name", Required = true)]
    public string BoundedContextName { get; set; }

    [Option('a', "aggregates", Required = true)]
    public string Aggregates { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ModernWebAppDataCreateRequestHandler : IRequestHandler<ModernWebAppDataCreateRequest>
{
    private readonly ILogger<ModernWebAppDataCreateRequestHandler> _logger;
    private readonly IFileSystem _fileSystem;

    public ModernWebAppDataCreateRequestHandler(ILogger<ModernWebAppDataCreateRequestHandler> logger, IArtifactGenerator artifactGenerator, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    public async Task Handle(ModernWebAppDataCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ModernWebAppDataCreateRequestHandler));

        var initialJson = $$"""
            {
                "productName": "{{request.ProductName}}",
                "boundedContexts": [
                    {
                        "name": "{{request.BoundedContextName}}"                        
                    }
                ]
            }
            """;

        var node = JsonNode.Parse(initialJson);

        var aggregates = new JsonArray();

        foreach (var aggregate in request.Aggregates.Split(','))
        {
            aggregates.Add(JsonNode.Parse($$"""
                { 
                    "name": "{{aggregate}}",
                    "properties": [
                        {
                            "name":"{{aggregate}}Id",
                            "kind": "Guid",
                            "key" : true
                        },
                        {
                            "name":"Name",
                            "kind": "String"
                        }                
                    ],
                    "commands": [
                        {
                            "name": "Create{{aggregate}}",
                            "kind" : "Create"
                        },
                        {
                            "name": "Update{{aggregate}}",
                            "kind" : "Update"
                        },
                        {
                            "name": "Delete{{aggregate}}",
                            "kind" : "Delete"
                        }               
                    ],
                    "queries": [
                        {
                            "name": "Get{{aggregate.Pluralize()}}",
                            "kind" : "Get"
                        },
                        {
                            "name": "Get{{aggregate}}ById",
                            "kind" : "GetById"
                        }   
                    ]
                }
                """));
        }

        var microservices = JsonNode.Parse($$"""
            [
                { 
                
                    "name": "{{request.ProductName}}.{{request.BoundedContextName}}.Api",
                    "productName": "{{request.ProductName}}",
                    "boundedContextName":"{{request.BoundedContextName}}"                
                }
            ]
            """);

        node["boundedContexts"][0]["aggregates"] = aggregates;

        node["microservices"] = microservices;

        var json = JsonSerializer.Serialize(node, new JsonSerializerOptions()
        {
            WriteIndented = true,
        });

        _fileSystem.File.WriteAllText(_fileSystem.Path.Combine(request.Directory, $"{request.ProductName.ToCamelCase()}.json"), json, System.Text.Encoding.UTF8);

    }
}