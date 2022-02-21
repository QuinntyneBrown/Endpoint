﻿using Endpoint.Core.Models;
using Endpoint.Core.Services;
using Endpoint.Core.Strategies.Api.FileGeneration;
using Endpoint.Core.Utilities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace Endpoint.Core.Strategies.Global
{
    internal class MinimalApiEndpointGenerationStrategy : IEndpointGenerationStrategy
    {
        private readonly ICommandService _commandService;
        private readonly IFileSystem _fileSystem;
        private readonly ITemplateLocator _templateLocator;
        private readonly ITemplateProcessor _templateProcessor;
        private readonly ILaunchSettingsGenerationStrategy _launchSettingsGenerationStrategy;
        private readonly IMinimalAppSettingsGenerationStrategy _minimalAppSettingsGenerationStrategy;
        private readonly ILogger _logger;

        public MinimalApiEndpointGenerationStrategy(ICommandService commandService, IFileSystem fileSystem, ITemplateLocator templateLocator, ITemplateProcessor templateProcessor, ILogger logger)
        {
            _commandService = commandService;
            _fileSystem = fileSystem;
            _templateLocator = templateLocator;
            _templateProcessor = templateProcessor;
            _launchSettingsGenerationStrategy = new LaunchSettingsGenerationStrategy(templateProcessor, fileSystem, templateLocator);
            _minimalAppSettingsGenerationStrategy = new MinimalAppSettingsGenerationStrategy(templateProcessor, fileSystem, templateLocator);
            _logger = logger;
        }

        public bool CanHandle(Settings model) => model.Minimal;

        public void Create(Settings model)
        {
            _logger.LogInformation($"Executing {nameof(MinimalApiEndpointGenerationStrategy)}");

            model.ApiDirectory = model.ApiDirectory.Replace(".Api", "");

            model.ApiNamespace = model.ApiNamespace.Replace(".Api", "");

            var json = Serialize(model, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            
            _fileSystem.CreateDirectory(model.RootDirectory);

            _fileSystem.WriteAllLines($"{model.RootDirectory}{Path.DirectorySeparatorChar}{CoreConstants.SettingsFileName}", new List<string> { json }.ToArray());

            _commandService.Start($"dotnet new sln -n {model.SolutionName}", model.RootDirectory);

            _fileSystem.CreateDirectory($"{model.RootDirectory}{Path.DirectorySeparatorChar}{model.SourceFolder}");

            _fileSystem.CreateDirectory($"{model.RootDirectory}{Path.DirectorySeparatorChar}{model.TestFolder}");

            _fileSystem.CreateDirectory($"{model.RootDirectory}{Path.DirectorySeparatorChar}deploy");

            _fileSystem.CreateDirectory(model.ApiDirectory);

            _commandService.Start("git init", model.RootDirectory);

            new GitIgnoreFileGenerationStrategy(_fileSystem, _templateLocator).Generate(model);

            _commandService.Start($"dotnet new webapi --framework net6.0", model.ApiDirectory);

            var parts = model.ApiDirectory.Split(Path.DirectorySeparatorChar);

            var name = parts[parts.Length - 1];

            _commandService.Start($"dotnet sln add {model.ApiDirectory}{Path.DirectorySeparatorChar}{name}.csproj", model.RootDirectory);

            _fileSystem.Delete($"{model.ApiDirectory}{Path.DirectorySeparatorChar}WeatherForecast.cs");

            _fileSystem.Delete($"{model.ApiDirectory}{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}WeatherForecastController.cs");

            _fileSystem.DeleteDirectory($"{model.ApiDirectory}{Path.DirectorySeparatorChar}Controllers");

            new BicepFileGenerationStrategy(_fileSystem, _templateLocator).Generate(model);

            new DeploySetupFileGenerationStrategy(_fileSystem, _templateLocator, _templateProcessor).Generate(model);

            var csProjFilePath = $"{model.ApiDirectory}{Path.DirectorySeparatorChar}{model.ApiNamespace}.csproj";

            CsProjectUtilities.AddGenerateDocumentationFile(csProjFilePath);

            CsProjectUtilities.RemoveDefaultWebApiFiles(model.ApiDirectory);

            _launchSettingsGenerationStrategy.Create(model);

            _minimalAppSettingsGenerationStrategy.Create(model);

            new MinimalApiProgramGenerationStratey(_fileSystem, _templateProcessor, _templateLocator).Create(new MinimalApiProgramModel(model.ApiNamespace,model.DbContextName, model.Resources), $"{model.ApiDirectory}");

            _commandService.Start($"dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 6.0.2", $@"{model.ApiDirectory}");

            _commandService.Start($"dotnet add package Swashbuckle.AspNetCore.Annotations --version 6.2.3", $@"{model.ApiDirectory}");

            _commandService.Start($"dotnet add package Swashbuckle.AspNetCore.Newtonsoft --version 6.2.3", $@"{model.ApiDirectory}");

            _commandService.Start($"dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 6.0.2", $@"{model.ApiDirectory}");

            _commandService.Start($"code .", model.RootDirectory);

        }
    }
}
