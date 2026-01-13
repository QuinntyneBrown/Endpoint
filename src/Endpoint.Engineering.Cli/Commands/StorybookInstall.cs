// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Installs Storybook in an Angular project with the appropriate version based on Angular version.
/// </summary>
[Verb("storybook-install")]
public class StorybookInstallRequest : IRequest
{
    [Option('v', "angular-version", Required = false, HelpText = "The Angular version of the project (e.g., 21, 20, 19). Defaults to 21.")]
    public int AngularVersion { get; set; } = 21;

    [Option('d', Required = false, HelpText = "The directory containing the Angular project.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    [Option('s', "skip-install", Required = false, HelpText = "Skip automatic npm install after Storybook initialization.")]
    public bool SkipInstall { get; set; } = false;
}

public class StorybookInstallRequestHandler : IRequestHandler<StorybookInstallRequest>
{
    private readonly ILogger<StorybookInstallRequestHandler> logger;
    private readonly ICommandService commandService;

    public StorybookInstallRequestHandler(
        ILogger<StorybookInstallRequestHandler> logger,
        ICommandService commandService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public async Task Handle(StorybookInstallRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(StorybookInstallRequestHandler));
        logger.LogInformation("Installing Storybook for Angular {AngularVersion} in {Directory}", request.AngularVersion, request.Directory);

        var storybookVersion = GetStorybookVersionForAngular(request.AngularVersion);

        logger.LogInformation("Using Storybook version: {StorybookVersion}", storybookVersion);

        var skipInstallFlag = request.SkipInstall ? " --skip-install" : string.Empty;
        var command = $"npx storybook@{storybookVersion} init --type angular{skipInstallFlag}";

        logger.LogInformation("Running command: {Command}", command);

        commandService.Start(command, request.Directory);

        logger.LogInformation("Storybook installation completed successfully.");

        await Task.CompletedTask;
    }

    private static string GetStorybookVersionForAngular(int angularVersion)
    {
        // Storybook version compatibility with Angular:
        // - Angular 15+ supports Storybook 8.x (latest)
        // - Angular 12-14 should use Storybook 7.x
        // - Angular 9-11 should use Storybook 6.x
        return angularVersion switch
        {
            >= 15 => "latest",
            >= 12 => "7",
            >= 9 => "6",
            _ => "6"
        };
    }
}
