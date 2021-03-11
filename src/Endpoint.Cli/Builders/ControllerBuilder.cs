using Endpoint.Cli.Services;
using System.Collections.Generic;

namespace Endpoint.Cli.Builders
{
    public class ControllerBuilder
    {
        private readonly ICommandService _commandService;
        private readonly ITokenBuilder _tokenBuilder;
        private readonly ITemplateProcessor _templateProcessor;
        private readonly ITemplateLocator _templateLocator;
        private readonly IFileSystem _fileSystem;
        private readonly INamingConventionConverter _namingConventionConverter;

        private string _directory = System.Environment.CurrentDirectory;
        private string _resourceName;
        private string[] _template;
        private string _templateName = nameof(ControllerBuilder);
        private string _rootNamespace;
        
        public ControllerBuilder(
            ICommandService commandService,
            ITokenBuilder tokenBuilder,
            ITemplateProcessor templateProcessor,
            ITemplateLocator templateLocator,
            IFileSystem fileSystem,
            INamingConventionConverter namingConventionConverter)
        {
            _commandService = commandService;
            _tokenBuilder = tokenBuilder;
            _templateProcessor = templateProcessor;
            _templateLocator = templateLocator;
            _fileSystem = fileSystem;
            _namingConventionConverter = namingConventionConverter;
        }

        public ControllerBuilder SetDirectory(string directory)
        {
            _directory = directory;
            return this;
        }

        public ControllerBuilder SetResourceName(string resourceName)
        {
            _resourceName = resourceName;
            return this;
        }

        public ControllerBuilder SetRootNamespace(string rootNamespace)
        {
            _rootNamespace = rootNamespace;
            return this;
        }

        public void Build()
        {
            _template = _templateLocator.Get(_templateName);

            var tokens = _tokenBuilder.Build(new Dictionary<string, string> {
                    { "ResourceName", $"{_resourceName}" },
                    { "RootNamespace", _rootNamespace }
                }, _directory);

            var contents = _templateProcessor.Process(_template, tokens);

            _fileSystem.WriteAllLines($@"{_directory}/{_namingConventionConverter.Convert(NamingConvention.PascalCase,_resourceName)}Controller.cs", contents);
        }
    }
}
