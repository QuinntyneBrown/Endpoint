// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using Humanizer;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class ModernWebAppDataGenerationStrategy : GenericSyntaxGenerationStrategy<ModernWebAppDataModel>
{
    private readonly ILogger<ModernWebAppDataGenerationStrategy> _logger;

    public ModernWebAppDataGenerationStrategy(ILogger<ModernWebAppDataGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, ModernWebAppDataModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var initialJson = $$"""
            {
                "productName": "{{model.ProductName}}",
                "boundedContexts": [
                    {
                        "name": "{{model.BoundedContextName}}"                        
                    }
                ]
            }
            """;

        var node = JsonNode.Parse(initialJson);

        var aggregates = new JsonArray();

        var properties = new JsonArray();

        foreach (var property in model.Properties.Split(','))
        {
            var propertyName = property.Split(':')[0];

            var propertyKind = property.Split(':')[1];

            var key = propertyName == $"{model.Aggregates}Id";

            properties.Add(JsonNode.Parse($$"""
                {
                    "name": "{{propertyName}}",
                    "kind": "{{propertyKind}}",
                    "key": {{(key == true ? "true" : "false")}}
                }
                """));
        }

        foreach (var aggregate in model.Aggregates.Split(','))
        {
            aggregates.Add(JsonNode.Parse($$"""
                { 
                    "name": "{{aggregate}}",
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

        aggregates[0]["properties"] = properties;

        var microservices = JsonNode.Parse($$"""
            [
                { 
                
                    "name": "{{model.ProductName}}.{{model.BoundedContextName}}.Api",
                    "productName": "{{model.ProductName}}",
                    "boundedContextName":"{{model.BoundedContextName}}"                
                }
            ]
            """);

        node["boundedContexts"][0]["aggregates"] = aggregates;

        node["microservices"] = microservices;

        return JsonSerializer.Serialize(node, new JsonSerializerOptions()
        {
            WriteIndented = true,
        });
    }
}