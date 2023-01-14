using CommandLine;
using Endpoint.Core.Abstractions;
using Endpoint.Core.Models.Artifacts.Files;
using Endpoint.Core.Services;
using Endpoint.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;


[Verb("syntax-generation-strategy-create")]
public class SyntaxGenerationStrategyCreateRequest : IRequest<Unit> {
    [Option('n',"name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SyntaxGenerationStrategyCreateRequestHandler : IRequestHandler<SyntaxGenerationStrategyCreateRequest, Unit>
{
    private readonly IArtifactGenerationStrategyFactory _artifactGenerationStrategyFactory;
    private readonly ILogger<VerbRequestHandler> _logger;
    private readonly IFileModelFactory _fileFactory;
    private readonly INamespaceProvider _namespaceProvider;

    public SyntaxGenerationStrategyCreateRequestHandler(
        ILogger<VerbRequestHandler> logger,
        IArtifactGenerationStrategyFactory artifactGenerationStrategyFactory,
        IFileModelFactory fileFactory,
        INamespaceProvider namespaceProvider
        )
    {
        _artifactGenerationStrategyFactory = artifactGenerationStrategyFactory;
        _logger = logger;
        _fileFactory = fileFactory;
        _namespaceProvider = namespaceProvider;
    }

    public async Task<Unit> Handle(SyntaxGenerationStrategyCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handled: {nameof(SyntaxGenerationStrategyCreateRequestHandler)}");

        var @namespace = _namespaceProvider.Get(request.Directory);

        var tokens = new TokensBuilder()
            .With("ModelName", (Token)request.Name)
            .With("Namespace", (Token)@namespace)
            .Build();

        var model = _fileFactory.CreateTemplate("SyntaxGenerationStrategy", $"{request.Name}SyntaxGenerationStrategy", request.Directory, tokens: tokens);

        _artifactGenerationStrategyFactory.CreateFor(model);

        return new();
    }
}