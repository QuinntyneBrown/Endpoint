// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.Core.Syntax;

public class SyntaxReferenceSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<SyntaxReferenceModel>
{
    public static string SetInitialLanguageInAppComponent = nameof(SetInitialLanguageInAppComponent);

    private readonly ILogger<SyntaxReferenceSyntaxGenerationStrategy> _logger;
    private readonly IFileSystem _fileSystem;

    public SyntaxReferenceSyntaxGenerationStrategy(
        IServiceProvider serviceProvider,
        IFileSystem fileSystem,
        ILogger<SyntaxReferenceSyntaxGenerationStrategy> logger)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public bool CanHandle(object model)
        => true; //model is SyntaxReferenceModel && context.Request == SetInitialLanguageInAppComponent;

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, SyntaxReferenceModel model)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        var tabSize = 2;

        var lines = model.Syntax.Split(Environment.NewLine);

        var createConstructor = !model.Syntax.Contains("constructor");

        var ctor = new string[]
        {
            "constructor(private readonly _translateService: TranslateService) {",
            $"_translateService.setDefaultLang(\"en\");".Indent(1,tabSize),
            $"_translateService.use(localStorage.getItem(\"currentLanguage\") || \"en\");".Indent(1,tabSize),
            "}"
        };

        var importAdded = false;

        var constructorUpdated = false;

        foreach (var line in lines)
        {
            if (!line.StartsWith("import") && !importAdded)
            {
                builder.AppendLine("import { TranslateService } from '@ngx-translate/core';");

                importAdded = true;
            }

            builder.AppendLine(line);

            if (line.StartsWith("export class AppComponent {") && !constructorUpdated && createConstructor)
            {
                foreach (var newLine in ctor)
                {
                    builder.AppendLine(newLine.Indent(1, tabSize));
                }

                constructorUpdated = true;
            }

            if (line.Contains("constructor") && !constructorUpdated && !createConstructor)
            {
                //newLines.RemoveAt(newLines.Count - 1);

                var newLine = line.Replace("constructor(", "constructor(private readonly _translateService: TranslateService, ");

                builder.AppendLine(newLine);

                builder.AppendLine($"_translateService.setDefaultLang(\"en\");".Indent(2, tabSize));
                builder.AppendLine($"_translateService.use(localStorage.getItem(\"currentLanguage\") || \"en\");".Indent(2, tabSize));

                constructorUpdated = true;
            }
        }


        return builder.ToString();
    }
}
