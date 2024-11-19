// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Endpoint.DotNet.Artifacts.Git;

public class GitGenerationStrategy : IArtifactGenerationStrategy<GitModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<GitGenerationStrategy> logger;
    private readonly ITemplateLocator templateLocator;
    private readonly IFileSystem fileSystem;

    public GitGenerationStrategy(ILogger<GitGenerationStrategy> logger, ICommandService commandService, ITemplateLocator templateLocator, IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task GenerateAsync( GitModel model)
    {
        logger.LogInformation("Generating Git Repository. {repositoryName}", model.RepositoryName);

        var client = new GitHubClient(new ProductHeaderValue(model.Username))
        {
            Credentials = new Credentials(Environment.GetEnvironmentVariable("GithubPersonalAccessToken", EnvironmentVariableTarget.Machine)),
        };

        client.Repository.Create(new NewRepository(model.RepositoryName)).GetAwaiter().GetResult();

        commandService.Start($"git init", $@"{model.Directory}");

        commandService.Start($"git config user.name {model.Username}", model.Directory);

        commandService.Start($"git config user.email {model.Email}", model.Directory);

        fileSystem.File.WriteAllText(Path.Combine(model.Directory, ".gitignore"), string.Join(Environment.NewLine, templateLocator.Get("GitIgnoreFile")));

        commandService.Start($"git remote add origin https://{model.Username}:{model.PersonalAccessToken}@github.com/{model.Username}/{model.RepositoryName}.git", model.Directory);

        commandService.Start("git add -A", model.Directory);

        commandService.Start("git commit -m initial", model.Directory);

        commandService.Start("git push --set-upstream origin master", model.Directory);

        commandService.Start("git checkout -b gh-pages", model.Directory);

        commandService.Start("git push --set-upstream origin gh-pages", model.Directory);

        commandService.Start("git checkout -", model.Directory);
    }
}
