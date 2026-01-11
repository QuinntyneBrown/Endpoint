// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Endpoint.Cli.Commands;


[Verb("solution-create-from-sequence")]
public class SolutionCreateFromSequenceRequest : IRequest {
    [Option('n',"name")]
    public string Name { get; set; }


    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SolutionCreateFromSequenceRequestHandler : IRequestHandler<SolutionCreateFromSequenceRequest>
{
    private readonly ILogger<SolutionCreateFromSequenceRequestHandler> _logger;

    public SolutionCreateFromSequenceRequestHandler(ILogger<SolutionCreateFromSequenceRequestHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(SolutionCreateFromSequenceRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(SolutionCreateFromSequenceRequestHandler));
    }
}