// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.Services;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.Projects.Strategies;

public class AngularStandaloneProjectArtifactGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ILogger<AngularStandaloneProjectArtifactGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;
    private readonly ITemplateLocator templateLocator;
    private readonly ITemplateProcessor templateProcessor;
    private readonly ICommandService commandService;

    public AngularStandaloneProjectArtifactGenerationStrategy(
        ILogger<AngularStandaloneProjectArtifactGenerationStrategy> logger,
        IFileSystem fileSystem,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor,
        ICommandService commandService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        this.templateProcessor = templateProcessor ?? throw new ArgumentNullException(nameof(templateProcessor));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public async Task<bool> GenerateAsync(IArtifactGenerator generator, object target)
    {
        if (target is ProjectModel model && model.Extension == ".esproj")
        {
            await GenerateAsync(model);

            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual int GetPriority() => 1;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Generating Angular workspace for project {name}", model.Name);

        // Create the parent directory if it doesn't exist
        var parentDirectory = System.IO.Path.GetDirectoryName(model.Directory);
        if (!string.IsNullOrEmpty(parentDirectory) && !fileSystem.Directory.Exists(parentDirectory))
        {
            fileSystem.Directory.CreateDirectory(parentDirectory);
        }

        // Convert project name to kebab-case
        var kebabCaseProjectName = model.Name.Kebaberize();

        // Create Angular workspace without initial application
        logger.LogInformation("Creating Angular workspace: ng new {projectName} --no-create-application --directory ./", model.Name);
        commandService.Start($"ng new {model.Name} --no-create-application --directory ./", parentDirectory);

        // Create the single application with kebab-case name
        logger.LogInformation("Creating Angular application: ng g application {kebabCaseProjectName}", kebabCaseProjectName);
        commandService.Start($"ng g application {kebabCaseProjectName}", model.Directory);

        // Generate the .esproj file
        var template = string.Join(Environment.NewLine, templateLocator.Get("EsProj"));

        var result = templateProcessor.Process(template, new TokensBuilder()
            .With("projectName", model.Name)
            .Build());

        // Ensure the project directory exists before writing the file
        if (!fileSystem.Directory.Exists(model.Directory))
        {
            fileSystem.Directory.CreateDirectory(model.Directory);
        }

        fileSystem.File.WriteAllText(model.Path, result);
    }
}