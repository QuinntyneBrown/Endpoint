﻿using Endpoint.Core.Events;
using Endpoint.Core.Strategies.Global;
using MediatR;
using System.Collections.Generic;
using System.IO;

namespace Endpoint.Core.Services
{
    public interface ISolutionTemplateService
    {
        void Build(string name, string dbContextName, bool shortIdPropertyName, string resource, string properties, bool isMonolith, bool numericIdPropertyDataType, string directory, List<string> plugins, string prefix);
    }

    public class EndpointGenerationStrategy : ISolutionTemplateService, IEndpointGenerationStrategy
    {
        private ICommandService _commandService;
        private readonly ISolutionFilesGenerationStrategy _solutionFileService;
        private readonly ISharedKernelProjectFilesGenerationStrategy _domainFileService;
        private readonly IApplicationProjectFilesGenerationStrategy _applicationFileService;
        private readonly IInfrastructureProjectFilesGenerationStrategy _infrastructureFileService;
        private readonly IApiProjectFilesGenerationStrategy _apiFileService;
        private readonly IMediator _mediator;

        public EndpointGenerationStrategy(
            ICommandService commandService,
            ISolutionFilesGenerationStrategy solutionFileService,
            ISharedKernelProjectFilesGenerationStrategy domainFileService,
            IApplicationProjectFilesGenerationStrategy applicationFileService,
            IInfrastructureProjectFilesGenerationStrategy infrastructureFileService,
            IApiProjectFilesGenerationStrategy apiFileService,
            IMediator mediator)
        {
            _commandService = commandService;
            _solutionFileService = solutionFileService;
            _domainFileService = domainFileService;
            _applicationFileService = applicationFileService;
            _infrastructureFileService = infrastructureFileService;
            _apiFileService = apiFileService;
            _mediator = mediator;
        }

        public void Build(string name, string dbContextName, bool shortIdPropertyName, string resource, string properties, bool isMonolith, bool numericIdPropertyDataType, string directory, List<string> plugins, string prefix)
        {
            Build(name, dbContextName, shortIdPropertyName, new List<string>() { resource }, properties, isMonolith, numericIdPropertyDataType, directory, plugins, prefix);
        }

        public void Build(string name, string dbContextName, bool shortIdPropertyName, List<string> resources, string properties, bool isMonolith, bool numericIdPropertyDataType, string directory, List<string> plugins, string prefix)
        {
            int retries = 0;

            string originalName = name;

            while (true)
            {
                if (!Directory.Exists($"{directory}{Path.DirectorySeparatorChar}{name}"))
                {
                    var settings = _solutionFileService.Build(name, properties, dbContextName, shortIdPropertyName, numericIdPropertyDataType, resources, directory, isMicroserviceArchitecture: !isMonolith, plugins, prefix);

                    _domainFileService.Build(settings);

                    _applicationFileService.Build(settings);

                    _infrastructureFileService.Build(settings);

                    _apiFileService.Build(settings);

                    _mediator.Publish(new SolutionTemplateGenerated(settings.RootDirectory));

                    _commandService.Start($"start {settings.SolutionFileName}", settings.RootDirectory);

                    return;

                }

                retries++;

                name = $"{originalName}_{retries}";

            }
        }

        public bool CanHandle(Models.Settings model)
        {
            return !model.Minimal;
        }

        public void Create(Models.Settings model)
        {
            _solutionFileService.Create(model);

            _domainFileService.Build(model);

            _applicationFileService.Build(model);

            _infrastructureFileService.Build(model);

            _apiFileService.Build(model);

            _mediator.Publish(new SolutionTemplateGenerated(model.RootDirectory));

            _commandService.Start($"start {model.SolutionFileName}", model.RootDirectory);
        }
    }
}