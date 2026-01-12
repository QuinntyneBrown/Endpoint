// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Endpoint.Engineering.Cli.Commands;


[Verb("code-parse")]
public class CodeParseRequest : IRequest {
    [Option('n',"name")]
    public string Name { get; set; }


    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class CodeParseRequestHandler : IRequestHandler<CodeParseRequest>
{
    private readonly ILogger<CodeParseRequestHandler> _logger;

    public CodeParseRequestHandler(ILogger<CodeParseRequestHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CodeParseRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(CodeParseRequestHandler));
    }
}