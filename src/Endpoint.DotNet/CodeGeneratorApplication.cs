// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using Endpoint.Internal;
using Endpoint.DotNet.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet;

public class CodeGeneratorApplication
{
    private readonly IMediator mediator;
    private readonly ILogger<CodeGeneratorApplication> logger;
    private readonly IServiceProvider serviceProvider;

    public CodeGeneratorApplication(IMediator mediator, ILogger<CodeGeneratorApplication> logger, IServiceProvider serviceProvider, Observable<INotification> notificationObservable)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.mediator = mediator;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public static CodeGeneratorApplicationBuilder CreateBuilder()
    {
        var builder = new CodeGeneratorApplicationBuilder();
        return builder;
    }

    public async Task<int> RunAsync(CancellationToken token = default)
    {
        return await Environment.GetCommandLineArgs().ParseAndExecuteAsync(serviceProvider);
    }
}