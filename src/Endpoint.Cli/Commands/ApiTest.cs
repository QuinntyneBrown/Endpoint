// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax;
using MediatR;

namespace Endpoint.Cli.Commands;

public class ApiTest
{
    [Verb("api-test")]
    public class Request : IRequest
    {
        [Option('n', "name")]
        public string EntityName { get; set; }

        [Option('d', Required = false)]
        public string Directory { get; private set; } = System.Environment.CurrentDirectory;
    }

    public class Handler : IRequestHandler<Request>
    {
        private readonly ITemplateLocator _templateLocator;
        private readonly ITemplateProcessor _templateProcessor;
        private readonly IFileSystem _fileSystem;

        public Handler(ITemplateLocator templateLocator, ITemplateProcessor templateProcessor, IFileSystem fileSystem)
        {
            ArgumentNullException.ThrowIfNull(templateLocator);
            ArgumentNullException.ThrowIfNull(fileSystem);
            ArgumentNullException.ThrowIfNull(templateProcessor);

            _templateLocator = templateLocator;
            _templateProcessor = templateProcessor;
            _fileSystem = fileSystem;
        }

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var template = _templateLocator.Get(nameof(ApiTest));

            var tokens = new TokensBuilder()
                .With(nameof(request.EntityName), (SyntaxToken)request.EntityName)
                .Build();

            var contents = string.Join(Environment.NewLine, _templateProcessor.Process(template, tokens));

            _fileSystem.File.WriteAllText($@"{request.Directory}/{((SyntaxToken)request.EntityName).PascalCase}ControllerTests.cs", contents);
        }
    }
}
