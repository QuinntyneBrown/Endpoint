// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.CyclicRandomizr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("cyclic-randomizr")]
public class CyclicRandomizrCreateRequest : IRequest
{
    [Option('t', "type", Required = true, HelpText = "The fully qualified name of the .NET type (e.g., 'MyNamespace.MyClass').")]
    public string TypeName { get; set; }

    [Option('d', "directory", Required = false, HelpText = "The output directory for the generated file.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class CyclicRandomizrCreateRequestHandler : IRequestHandler<CyclicRandomizrCreateRequest>
{
    private readonly ILogger<CyclicRandomizrCreateRequestHandler> _logger;
    private readonly ICyclicRandomizrService _cyclicRandomizrService;

    public CyclicRandomizrCreateRequestHandler(
        ILogger<CyclicRandomizrCreateRequestHandler> logger,
        ICyclicRandomizrService cyclicRandomizrService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cyclicRandomizrService = cyclicRandomizrService ?? throw new ArgumentNullException(nameof(cyclicRandomizrService));
    }

    public async Task Handle(CyclicRandomizrCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Cyclic Randomizer for type: {TypeName}", request.TypeName);

        await _cyclicRandomizrService.GenerateRandomizerAsync(request.TypeName, request.Directory, cancellationToken);
    }
}
