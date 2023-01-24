using Endpoint.Core.Abstractions;
using Endpoint.Core.Models.Artifacts.Files;
using Endpoint.Core.Models.Artifacts.Files.Factories;
using Endpoint.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Endpoint.Core.Models.WebArtifacts.Services;

public class AngularService : IAngularService
{
    private readonly ILogger<AngularService> _logger;
    private readonly IArtifactGenerationStrategyFactory _artifactGenerationStrategyFactory;
    private readonly ICommandService _commandService;
    private readonly IFileProvider _fileProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IFileModelFactory _fileModelFactory;
    public AngularService(
        ILogger<AngularService> logger,
        IArtifactGenerationStrategyFactory artifactGenerationStrategyFactory,
        ICommandService commandService,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IFileModelFactory fileModelFactory
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _artifactGenerationStrategyFactory = artifactGenerationStrategyFactory ?? throw new ArgumentNullException(nameof(artifactGenerationStrategyFactory));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _fileModelFactory = fileModelFactory ?? throw new ArgumentNullException(nameof(fileModelFactory));
    }

    public void KarmaRemove(string directory)
    {
        var workspaceDirectory = Path.GetDirectoryName(_fileProvider.Get("angular.json", directory));

        _commandService.Start("npm uninstall @types/jasmine", workspaceDirectory);

        _commandService.Start("npm uninstall karma", workspaceDirectory);

        _commandService.Start("npm uninstall karma-chrome-launcher", workspaceDirectory);

        _commandService.Start("npm uninstall karma-coverage", workspaceDirectory);

        _commandService.Start("npm uninstall karma-jasmine", workspaceDirectory);

        _commandService.Start("npm uninstall karma-jasmine-html-reporter", workspaceDirectory);
    }

    public void JestInstall(string directory)
    {
        var workspaceDirectory = Path.GetDirectoryName(_fileProvider.Get("angular.json", directory));

        _commandService.Start("npm install -D jest jest-preset-angular @angular-builders/jest @types/jest", workspaceDirectory);

        _fileSystem.WriteAllText($"{workspaceDirectory}{Path.DirectorySeparatorChar}setup-jest.ts", string.Empty);
    }

    public void NgxTranslateAdd(string projectName, string directory)
    {
        var workspaceDirectory = _fileSystem.GetDirectoryName(_fileProvider.Get("angular.json", directory));

        _commandService.Start("npm install -D @ngx-translate/core", workspaceDirectory);

        _commandService.Start("npm install -D @ngx-translate/http-loader", workspaceDirectory);

        var model = new AngularProjectModel(projectName, "", "", directory);

        _addImports(model);

        _addHttpLoaderFactory(model);

        _registerTranslateModule(model);

        var i18nDirectory = $"{model.Directory}src{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}i18n";

        _fileSystem.CreateDirectory(i18nDirectory);

        foreach (var lang in new string[] { "en", "fr" })
        {
            _fileSystem.WriteAllText($"{i18nDirectory}{Path.DirectorySeparatorChar}{lang}.json", "{}");
        }

        _setInitialLanguageInAppComponent(model);
    }

    public void CreateWorkspace(string name, string projectName, string projectType, string prefix, string rootDirectory)
    {
        var workspaceModel = new AngularWorkspaceModel(name, rootDirectory);

        _artifactGenerationStrategyFactory.CreateFor(workspaceModel);

        KarmaRemove(workspaceModel.Directory);

        JestInstall(workspaceModel.Directory);

        var angularProjectModel = new AngularProjectModel(projectName, projectType, prefix, workspaceModel.Directory);

        AddProject(angularProjectModel);

        _commandService.Start("code .", workspaceModel.Directory);
    }

    public void AddProject(AngularProjectModel model)
    {
        var stringBuilder = new StringBuilder().Append($"ng generate {model.ProjectType} {model.Name} --prefix {model.Prefix}");

        if(model.ProjectType == "application")
            stringBuilder.Append(" --style=scss --strict=false --routing");

        _commandService.Start(stringBuilder.ToString(), model.RootDirectory);

        EnableDefaultStandaloneComponents(new AngularProjectReferenceModel(model.Name,model.Directory, model.ProjectType));

        if (model.ProjectType == "application")
        {            
            var srcDirectory = $"{model.Directory}{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}";

            var appDirectory = $"{srcDirectory}app{Path.DirectorySeparatorChar}";

            
            var files = new List<FileModel>
            {
                _fileModelFactory.CreateTemplate("Angular.app.component","app.component", appDirectory, "ts"),
                _fileModelFactory.CreateTemplate("Angular.app.component.spec","app.component.spec", appDirectory, "ts"),
                _fileModelFactory.CreateTemplate("Angular.main","main", srcDirectory, "ts"),
            };

            foreach(var file in files) {
                _artifactGenerationStrategyFactory.CreateFor(file);
            }

            _fileSystem.Delete($"{appDirectory}{Path.DirectorySeparatorChar}app.module.ts");

        }


        UpdateCompilerOptionsToUseJestTypes(model);

        JestConfigCreate(model);

        UpdateAngularJsonToUseJest(model);

    }

    public void UpdateAngularJsonToUseJest(AngularProjectModel model)
    {
        var angularJsonPath = _fileProvider.Get("angular.json", model.Directory);

        var angularJson = JObject.Parse(_fileSystem.ReadAllText(angularJsonPath));

        var testJObject = new JObject
        {
            { "builder", "@angular-builders/jest:run" }
        };

        angularJson["projects"][model.Name]["architect"]["test"] = testJObject;

        _fileSystem.WriteAllText(angularJsonPath, JsonConvert.SerializeObject(angularJson, Formatting.Indented));
    }

    public void JestConfigCreate(AngularProjectModel model)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("module.exports = {");

        stringBuilder.AppendLine("preset: 'jest-preset-angular',".Indent(1));

        stringBuilder.AppendLine("setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],".Indent(1));

        stringBuilder.AppendLine("globalSetup: 'jest-preset-angular/global-setup',".Indent(1));

        stringBuilder.AppendLine("};");

        _fileSystem.WriteAllText($"{model.Directory}{Path.DirectorySeparatorChar}jest.config.js",stringBuilder.ToString());

    }

    public void EnableDefaultStandaloneComponents(AngularProjectReferenceModel model)
    {
        var angularJsonPath = _fileProvider.Get("angular.json", model.ReferencedDirectory);

        var angularJson = JObject.Parse(_fileSystem.ReadAllText(angularJsonPath));

        angularJson.EnableDefaultStandaloneComponents(model.Name);

        _fileSystem.WriteAllText(angularJsonPath, JsonConvert.SerializeObject(angularJson, Formatting.Indented));
    }


    public void UpdateCompilerOptionsToUseJestTypes(AngularProjectModel model)
    {        
        var tsConfigSpecJsonPath = $"{model.Directory}{Path.DirectorySeparatorChar}tsconfig.spec.json";

        var tsConfigSpecJson = JObject.Parse(File.ReadAllText(tsConfigSpecJsonPath));

        tsConfigSpecJson.UpdateCompilerOptionsToUseJestTypes();

        _fileSystem.WriteAllText(tsConfigSpecJsonPath, JsonConvert.SerializeObject(tsConfigSpecJson, Formatting.Indented));
    }

    private readonly List<ImportModel> _importModels = new List<ImportModel>()
        {
            new ImportModel("HttpClient","@angular/common/http"),
            new ImportModel("TranslateLoader","@ngx-translate/core"),
            new ImportModel("TranslateModule ","@ngx-translate/core"),
            new ImportModel("TranslateHttpLoader ","@ngx-translate/http-loader")
        };
    private void _addImports(AngularProjectModel model)
    {
        var appModulePath = $"{model.Directory}src{Path.DirectorySeparatorChar}app{Path.DirectorySeparatorChar}app.module.ts";

        var lines = _fileSystem.ReadAllLines(appModulePath);

        var newLines = new List<string>();

        var added = false;

        foreach (var line in lines)
        {
            if (!line.StartsWith("import") && !added)
            {
                foreach (var importModel in _importModels)
                {
                    newLines.Add(new StringBuilder()
                        .Append("import { ")
                        .Append(importModel.Name)
                        .Append(" }")
                        .Append($" from '{importModel.ModuleName}';")
                        .ToString());
                }

                added = true;
            }

            newLines.Add(line);

        }

        _fileSystem.WriteAllLines(appModulePath, newLines.ToArray());
    }

    private void _addHttpLoaderFactory(AngularProjectModel model)
    {
        var appModulePath = $"{model.Directory}src{Path.DirectorySeparatorChar}app{Path.DirectorySeparatorChar}app.module.ts";

        var lines = _fileSystem.ReadAllLines(appModulePath);

        var newLines = new List<string>();

        var added = false;

        foreach (var line in lines)
        {
            if (!line.StartsWith("import") && !added)
            {
                foreach (var newLine in new string[3] {
                        "export function HttpLoaderFactory(httpClient: HttpClient) {",
                        "  return new TranslateHttpLoader(httpClient);",
                        "}"
                    })
                {
                    newLines.Add(newLine);
                }

                added = true;
            }

            newLines.Add(line);

        }

        _fileSystem.WriteAllLines(appModulePath, newLines.ToArray());
    }

    private void _registerTranslateModule(AngularProjectModel model)
    {
        var appModulePath = $"{model.Directory}src{Path.DirectorySeparatorChar}app{Path.DirectorySeparatorChar}app.module.ts";

        var lines = _fileSystem.ReadAllLines(appModulePath);

        var newLines = new List<string>();

        var added = false;

        foreach (var line in lines)
        {
            if (line.Contains("imports: [") && !added)
            {
                newLines.Add(line);

                foreach (var newLine in new string[7]
                {
                        "    TranslateModule.forRoot({",
                        "       loader: {",
                        "        provide: TranslateLoader,",
                        "        useFactory: HttpLoaderFactory,",
                        "        deps: [HttpClient]",
                        "      }",
                        "    }),"
                })
                {
                    newLines.Add(newLine);
                }

                added = true;
            }
            else
            {
                newLines.Add(line);
            }
        }

        _fileSystem.WriteAllLines(appModulePath, newLines.ToArray());
    }

    private void _setInitialLanguageInAppComponent(AngularProjectModel model)
    {
        var appComponentPath = $"{model.Directory}src{Path.DirectorySeparatorChar}app{Path.DirectorySeparatorChar}app.component.ts";

        var createConstructor = !_fileSystem.ReadAllText(appComponentPath).Contains("constructor");

        var ctor = new string[4]
        {
                "  constructor(private readonly _translateService: TranslateService) {",
                $"    _translateService.setDefaultLang(\"en\");",
                $"    _translateService.use(localStorage.getItem(\"currentLanguage\") || \"en\");",
                "  }"
        };

        var lines = _fileSystem.ReadAllLines(appComponentPath);

        var newLines = new List<string>();

        var importAdded = false;

        var constructorUpdated = false;

        foreach (var line in lines)
        {
            if (!line.StartsWith("import") && !importAdded)
            {
                newLines.Add("import { TranslateService } from '@ngx-translate/core';");

                importAdded = true;
            }

            newLines.Add(line);

            if (line.StartsWith("export class AppComponent {") && !constructorUpdated && createConstructor)
            {
                foreach (var newLine in ctor)
                {
                    newLines.Add(newLine);
                }

                constructorUpdated = true;
            }

            if (line.Contains("constructor") && !constructorUpdated && !createConstructor)
            {
                newLines.RemoveAt(newLines.Count - 1);

                var newLine = line.Replace("constructor(", "constructor(private readonly _translateService: TranslateService, ");

                newLines.Add(newLine);

                newLines.Add($"    _translateService.setDefaultLang(\"en\");");
                newLines.Add($"    _translateService.use(localStorage.getItem(\"currentLanguage\") || \"en\");");


                constructorUpdated = true;
            }
        }

        _fileSystem.WriteAllLines(appComponentPath, newLines.ToArray());
    }
}

public class TranslateModuleRegistration
{
    public string[] Create() => new []
    {
        "    TranslateModule.forRoot({",
        "       loader: {",
        "        provide: TranslateLoader,",
        "        useFactory: HttpLoaderFactory,",
        "        deps: [HttpClient]",
        "      }",
        "    }),"
    };
}

public class ImportModel
{
    public ImportModel(string name, string moduleName)
    {
        Name = name;
        ModuleName = moduleName;
    }
    public string Name { get; set; }
    public string ModuleName { get; set; }
}