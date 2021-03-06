using Endpoint.Application.Extensions;
using Endpoint.Application.Services;
using Endpoint.Application.ValueObjects;
using System.Collections.Generic;

namespace Endpoint.Application.Builders
{
    public class UpdateBuilder
    {
        private readonly List<string> _content;
        private readonly IContext _context;
        private readonly IFileSystem _fileSystem;
        private string _entity;
        private string _dbContext;
        private string _directory;
        private string _namespace;
        private string _domainNamespace;
        private string _applicationNamespace;

        public UpdateBuilder(IContext context, IFileSystem fileSystem)
        {
            _content = new();
            _context = context;
            _fileSystem = fileSystem;
        }

        public UpdateBuilder WithDomainNamespace(string domainNamespace)
        {
            _domainNamespace = domainNamespace;
            return this;
        }

        public UpdateBuilder WithApplicationNamespace(string applicationNamespace)
        {
            _applicationNamespace = applicationNamespace;
            return this;
        }

        public UpdateBuilder WithDirectory(string directory)
        {
            _directory = directory;
            return this;
        }

        public UpdateBuilder WithEntity(string entity)
        {
            _entity = entity;
            return this;
        }

        public UpdateBuilder WithDbContext(string dbContext)
        {
            _dbContext = dbContext;
            return this;
        }


        public UpdateBuilder WithNamespace(string @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        public void Build()
        {
            
            var validator = new ClassBuilder("Validator", _context, _fileSystem)
                .WithRuleFor(new RuleForBuilder().WithNotNull().WithEntity(_entity).Build())
                .WithRuleFor(new RuleForBuilder().WithValidator().WithEntity(_entity).Build())
                .WithBase(new TypeBuilder().WithGenericType("AbstractValidator", "Request").Build())
                .Class;

            var request = new ClassBuilder("Request", _context, _fileSystem)
                .WithInterface(new TypeBuilder().WithGenericType("IRequest", "Response").Build())
                .WithProperty(new PropertyBuilder().WithType($"{((Token)_entity).PascalCase}Dto").WithName($"{((Token)_entity).PascalCase}").WithAccessors(new AccessorsBuilder().Build()).Build())
                .Class;

            var response = new ClassBuilder("Response", _context, _fileSystem)
                .WithBase("ResponseBase")
                .WithProperty(new PropertyBuilder().WithType($"{((Token)_entity).PascalCase}Dto").WithName($"{((Token)_entity).PascalCase}").WithAccessors(new AccessorsBuilder().Build()).Build())
                .Class;

            var handler = new ClassBuilder("Handler", _context, _fileSystem)
                .WithBase(new TypeBuilder().WithGenericType("IRequestHandler", "Request", "Response").Build())
                .WithDependency($"I{((Token)_dbContext).PascalCase}", "context")
                .WithMethod(new MethodBuilder().WithName("Handle").WithAsync(true)
                .WithReturnType(new TypeBuilder().WithGenericType("Task", "Response").Build())
                .WithParameter(new ParameterBuilder("Request", "request").Build())
                .WithParameter(new ParameterBuilder("CancellationToken", "cancellationToken").Build())
                .WithBody(new List<string>() {
                $"var {((Token)_entity).CamelCase} = await _context.{((Token)_entity).PascalCasePlural}.SingleAsync(x => x.{((Token)_entity).PascalCase}Id == request.{((Token)_entity).PascalCase}.{((Token)_entity).PascalCase}Id);",
                "",
                "await _context.SaveChangesAsync(cancellationToken);",
                "",
                "return new Response()",
                "{",
                $"{((Token)_entity).PascalCase} = {((Token)_entity).CamelCase}.ToDto()".Indent(1),
                "};"
                }).Build())
                .Class;

            new ClassBuilder($"Update{((Token)_entity).PascalCase}", _context, _fileSystem)
                .WithDirectory(_directory)
                .WithNamespace(_namespace)
                .WithUsing("FluentValidation")
                .WithUsing("MediatR")
                .WithUsing("System.Threading")
                .WithUsing("System.Threading.Tasks")
                .WithUsing($"{_domainNamespace}.Core")
                .WithUsing($"{_applicationNamespace}.Interfaces")
                .WithUsing("Microsoft.EntityFrameworkCore")
                .WithClass(validator)
                .WithClass(request)
                .WithClass(response)
                .WithClass(handler)
                .Build();
        }
    }
}
