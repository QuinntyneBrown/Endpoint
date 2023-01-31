﻿using Endpoint.Core.Models.Syntax;
using Endpoint.Core.Services;

namespace Endpoint.Core.Strategies.Api;

public class WebApplicationBuilderGenerationStrategy: IWebApplicationBuilderGenerationStrategy
{
    private readonly ITemplateProcessor _templateProcessor;
    private readonly ITemplateLocator _templateLocator;

    public WebApplicationBuilderGenerationStrategy(ITemplateProcessor templateProcessor, ITemplateLocator templateLocator)
    {
        _templateProcessor = templateProcessor;
        _templateLocator = templateLocator;
    }

    public string Create(string @namespace, string dbContextName)
    {
        var template = _templateLocator.Get("WebApplicationBuilder");

        var tokens = new TokensBuilder()
            .With("Namespace", (SyntaxToken)@namespace)
            .With(nameof(dbContextName), (SyntaxToken)dbContextName)
            .Build();

        var contents = string.Join(Environment.NewLine,_templateProcessor.Process(template, tokens));

        return contents;
    }
}
