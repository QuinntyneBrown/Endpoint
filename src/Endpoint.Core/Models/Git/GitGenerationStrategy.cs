﻿using Endpoint.Core.Abstractions;
using Endpoint.Core.Services;
using Microsoft.Extensions.Logging;
using Octokit;
using System.IO;

namespace Endpoint.Core.Models.Git;

public class GitGenerationStrategy : ArtifactGenerationStrategyBase<GitModel>
{
    private readonly ICommandService _commandService;
    private readonly ILogger<GitGenerationStrategy> _logger;
    private readonly ITemplateLocator _templateLocator;
    private readonly IFileSystem _fileSystem;

    public GitGenerationStrategy(IServiceProvider serviceProvider, ILogger<GitGenerationStrategy> logger, ICommandService commandService, ITemplateLocator templateLocator, IFileSystem fileSystem)
        :base(serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public override void Create(IArtifactGenerationStrategyFactory artifactGenerationStrategyFactory, GitModel model, dynamic configuration = null)
    {
        _logger.LogInformation($"{nameof(GitGenerationStrategy)}: Handled");

        var client = new GitHubClient(new ProductHeaderValue(model.Username))
        {
            Credentials = new Credentials(model.PersonalAccessToken)
        };

        client.Repository.Create(new NewRepository(model.RepositoryName)).GetAwaiter().GetResult();

        _commandService.Start($"git init", $@"{model.Directory}");

        _commandService.Start($"git config user.name {model.Username}", model.Directory);

        _commandService.Start($"git config user.email {model.Email}", model.Directory);

        _fileSystem.WriteAllText($@"{model.Directory}{Path.DirectorySeparatorChar}.gitignore", string.Join(Environment.NewLine, _templateLocator.Get("GitIgnoreFile")));

        _commandService.Start($"git remote add origin https://{model.Username}:{model.PersonalAccessToken}@github.com/{model.Username}/{model.RepositoryName}.git", model.Directory);

        _commandService.Start("git add -A", model.Directory);

        _commandService.Start("git commit -m intial", model.Directory);

        _commandService.Start("git push --set-upstream origin master", model.Directory);
    }

}
