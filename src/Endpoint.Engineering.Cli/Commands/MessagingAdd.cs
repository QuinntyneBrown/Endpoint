// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Endpoint.Engineering.Cli.Commands;


[Verb("messaging-add")]
public class MessagingAddRequest : IRequest {
    [Option('n',"name")]
    public string Name { get; set; }


    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class MessagingAddRequestHandler : IRequestHandler<MessagingAddRequest>
{
    private readonly ILogger<MessagingAddRequestHandler> _logger;

    public MessagingAddRequestHandler(ILogger<MessagingAddRequestHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(MessagingAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(MessagingAddRequestHandler));
    }
}