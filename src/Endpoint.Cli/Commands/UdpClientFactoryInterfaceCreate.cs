// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Files.Factories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;

[Verb("udp-client-factory-interface-create")]
public class UdpClientFactoryInterfaceCreateRequest : IRequest
{
    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class UdpClientFactoryInterfaceCreateRequestHandler : IRequestHandler<UdpClientFactoryInterfaceCreateRequest>
{
    private readonly ILogger<UdpClientFactoryInterfaceCreateRequestHandler> _logger;
    private readonly IFileFactory _fileFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    public UdpClientFactoryInterfaceCreateRequestHandler(ILogger<UdpClientFactoryInterfaceCreateRequestHandler> logger, IFileFactory fileFactory, IArtifactGenerator artifactGenerator)
    {
        _logger = logger;
        _fileFactory = fileFactory;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(UdpClientFactoryInterfaceCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(UdpClientFactoryInterfaceCreateRequestHandler));

        var model = await _fileFactory.CreateUdpClientFactoryInterfaceAsync(request.Directory);

        await _artifactGenerator.GenerateAsync(model);
    }
}