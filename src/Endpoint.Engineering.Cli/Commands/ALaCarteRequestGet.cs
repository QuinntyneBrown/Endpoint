// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.ALaCarte.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("a-la-carte-request-get", HelpText = "Display all ALaCarteRequests from the database.")]
public class ALaCarteRequestGetRequest : IRequest
{
    [Option("no-color", Required = false, Default = false,
        HelpText = "Disable colorized output.")]
    public bool NoColor { get; set; }
}

public class ALaCarteRequestGetRequestHandler : IRequestHandler<ALaCarteRequestGetRequest>
{
    private readonly ILogger<ALaCarteRequestGetRequestHandler> _logger;
    private readonly IALaCarteContext _context;

    public ALaCarteRequestGetRequestHandler(
        ILogger<ALaCarteRequestGetRequestHandler> logger,
        IALaCarteContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task Handle(ALaCarteRequestGetRequest request, CancellationToken cancellationToken)
    {
        WriteHeader(request.NoColor);

        try
        {
            _logger.LogInformation("Retrieving all ALaCarte requests from database");

            var requests = await _context.ALaCarteRequests.ToListAsync(cancellationToken);

            if (requests.Count == 0)
            {
                Console.WriteLine("No ALaCarteRequests found in the database.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine($"Found {requests.Count} ALaCarteRequest(s):");
            Console.WriteLine();

            foreach (var alaCarteRequest in requests)
            {
                DisplayRequest(alaCarteRequest, request.NoColor);
            }
        }
        catch (Exception ex)
        {
            WriteError($"Error retrieving ALaCarteRequests: {ex.Message}", request.NoColor);
            _logger.LogError(ex, "Error retrieving ALaCarteRequests");
            Environment.ExitCode = 1;
        }
    }

    private static void WriteHeader(bool noColor)
    {
        Console.WriteLine();
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }

        Console.WriteLine("=== ALaCarte Requests ===");
        if (!noColor)
        {
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    private static void WriteError(string message, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine($"Error: {message}");
        if (!noColor)
        {
            Console.ResetColor();
        }
    }

    private static void DisplayRequest(Endpoint.Engineering.ALaCarte.Core.Models.ALaCarteRequest alaCarteRequest, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        Console.WriteLine($"ID: {alaCarteRequest.ALaCarteRequestId}");
        if (!noColor)
        {
            Console.ResetColor();
        }

        Console.WriteLine($"  Directory:     {alaCarteRequest.Directory}");
        Console.WriteLine($"  Solution Name: {alaCarteRequest.SolutionName}");
        Console.WriteLine($"  Output Type:   {alaCarteRequest.OutputType}");
        Console.WriteLine();
    }
}
