// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Files.Factories;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;

/// <summary>
/// Creates a Storybook story file for an Angular component.
/// </summary>
[Verb("story-create")]
public class StoryCreateRequest : IRequest
{
    [Option('n', "name", Required = true, HelpText = "The name of the component to create a story for (e.g., Button, UserProfile).")]
    public string Name { get; set; }

    [Option('v', "angular-version", Required = false, HelpText = "The Angular version of the project (e.g., 21, 20, 19). Defaults to 21.")]
    public int AngularVersion { get; set; } = 21;

    [Option('p', "path", Required = false, HelpText = "The import path for the component (e.g., ./button.component). If not provided, it will be auto-generated.")]
    public string ComponentPath { get; set; }

    [Option('t', "title", Required = false, HelpText = "The title/category for the story in Storybook (e.g., Components/Button). If not provided, defaults to 'Components/{Name}'.")]
    public string Title { get; set; }

    [Option('d', Required = false, HelpText = "The directory where the story file will be created.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class StoryCreateRequestHandler : IRequestHandler<StoryCreateRequest>
{
    private readonly ILogger<StoryCreateRequestHandler> logger;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly IFileFactory fileFactory;
    private readonly IArtifactGenerator artifactGenerator;

    public StoryCreateRequestHandler(
        ILogger<StoryCreateRequestHandler> logger,
        INamingConventionConverter namingConventionConverter,
        IFileFactory fileFactory,
        IArtifactGenerator artifactGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task Handle(StoryCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(StoryCreateRequestHandler));
        logger.LogInformation("Creating Storybook story for component: {Name}", request.Name);

        var nameSnakeCase = namingConventionConverter.Convert(NamingConvention.SnakeCase, request.Name);
        var namePascalCase = namingConventionConverter.Convert(NamingConvention.PascalCase, request.Name);
        var nameCamelCase = namingConventionConverter.Convert(NamingConvention.CamelCase, request.Name);

        var componentPath = string.IsNullOrEmpty(request.ComponentPath)
            ? $"./{nameSnakeCase}.component"
            : request.ComponentPath;

        var title = string.IsNullOrEmpty(request.Title)
            ? $"Components/{namePascalCase}"
            : request.Title;

        var templateName = GetTemplateNameForAngularVersion(request.AngularVersion);

        var tokens = new TokensBuilder()
            .With("name", request.Name)
            .With("nameSnakeCase", nameSnakeCase)
            .With("namePascalCase", namePascalCase)
            .With("nameCamelCase", nameCamelCase)
            .With("componentPath", componentPath)
            .With("title", title)
            .Build();

        var model = fileFactory.CreateTemplate(
            templateName,
            $"{nameSnakeCase}.component.stories",
            request.Directory,
            ".ts",
            tokens: tokens);

        await artifactGenerator.GenerateAsync(model);

        logger.LogInformation("Story file created: {FileName}.stories.ts", nameSnakeCase);
    }

    private static string GetTemplateNameForAngularVersion(int angularVersion)
    {
        // Use CSF3 format for Storybook 7+ (Angular 12+)
        // Use CSF2 format for older versions
        return angularVersion >= 12
            ? "Angular.Storybook.Story"
            : "Angular.Storybook.StoryLegacy";
    }
}
