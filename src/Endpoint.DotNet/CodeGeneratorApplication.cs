// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using CommandLine;
using Endpoint.DotNet.Extensions;
using Endpoint.Core.Internal;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet;

public class CodeGeneratorApplication
{
    private readonly IMediator mediator;
    private readonly ILogger<CodeGeneratorApplication> logger;

    public CodeGeneratorApplication(IMediator mediator, ILogger<CodeGeneratorApplication> logger, Observable<INotification> notificationObservable)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(logger);

        this.mediator = mediator;
        this.logger = logger;
    }

    public static CodeGeneratorApplicationBuilder CreateBuilder()
    {
        var builder = new CodeGeneratorApplicationBuilder();
        return builder;
    }

    public async Task RunAsync(CancellationToken token = default)
    {
        await mediator.Send(Environment.GetCommandLineArgs().ParseArguments());
    }
}