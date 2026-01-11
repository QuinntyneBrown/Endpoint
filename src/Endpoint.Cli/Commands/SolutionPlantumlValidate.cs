// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;

[Verb("solution-plantuml-validate")]
public class SolutionPlantumlValidateRequest : IRequest {
    [Option('n',"name")]
    public string Name { get; set; }

    [Option('p', "plant-uml-source-path", Required = true, HelpText = "Path to the directory containing PlantUML files.")]
    public string PlantUmlSourcePath { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SolutionPlantumlValidateRequestHandler : IRequestHandler<SolutionPlantumlValidateRequest>
{
    private readonly ILogger<SolutionPlantumlValidateRequestHandler> _logger;

    public SolutionPlantumlValidateRequestHandler(ILogger<SolutionPlantumlValidateRequestHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(SolutionPlantumlValidateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(SolutionPlantumlValidateRequestHandler));
    }
}