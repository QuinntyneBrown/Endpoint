using CommandLine;
using Endpoint.Core.Services;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Core.Commands
{
    internal class Default
    {
        [Verb("default")]
        internal class Request : IRequest<Unit>
        {
            [Option('p', "port")]
            public int Port { get; set; } = 5000;

            [Option("properties", Required = false)]
            public string Properties { get; set; }

            [Option('n', "name")]
            public string Name { get; set; } = "DefaultEndpoint";

            [Option('r', "resource")]
            public string Resource { get; set; } = "Foo";

            [Option('m', "isMonolithArchitecture")]
            public bool Monolith { get; set; } = false;

            [Option("dbContextName")]
            public string DbContextName { get; set; }

            [Option('s', "shortIdPropertyName")]
            public bool ShortIdPropertyName { get; set; }

            [Option('i', "numericIdPropertyDataType")]
            public bool NumericIdPropertyDataType { get; set; }

            [Option("plugins", Required = false)]
            public string Plugins { get; set; }

            [Option('d', "directory")]
            public string Directory { get; set; } = System.Environment.CurrentDirectory;
        }

        internal class Handler : IRequestHandler<Request, Unit>
        {
            private ISolutionTemplateService _solutionTemplateService;

            public Handler(ISolutionTemplateService solutionTemplateService)
            {
                _solutionTemplateService = solutionTemplateService;
            }
            public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                _solutionTemplateService.Build(request.Name, request.DbContextName, request.ShortIdPropertyName, request.Resource, request.Properties, request.Monolith, request.NumericIdPropertyDataType, request.Directory, request.Plugins?.Split(',').ToList());

                return Task.FromResult(new Unit());
            }
        }
    }
}
