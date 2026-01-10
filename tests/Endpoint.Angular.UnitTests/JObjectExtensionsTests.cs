// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.UnitTests;

public class JObjectExtensionsTests
{
    #region AddBuildConfiguration Tests

    [Fact]
    public void AddBuildConfiguration_ShouldAddConfigurationToProject()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJson("test-project");
        var configurationName = "staging";
        var projectName = "test-project";
        var fileReplacements = new List<FileReplacementModel>
        {
            new("src/environments/environment.ts", "src/environments/environment.staging.ts")
        };

        // Act
        jObject.AddBuildConfiguration(configurationName, projectName, fileReplacements);

        // Assert
        var configuration = jObject["projects"]?[projectName]?["architect"]?["build"]?["configurations"]?[configurationName];
        Assert.NotNull(configuration);
        var replacements = configuration["fileReplacements"] as JArray;
        Assert.NotNull(replacements);
        Assert.Single(replacements);
        Assert.Equal("src/environments/environment.ts", replacements[0]?["replace"]?.ToString());
        Assert.Equal("src/environments/environment.staging.ts", replacements[0]?["with"]?.ToString());
    }

    [Fact]
    public void AddBuildConfiguration_WithMultipleReplacements_ShouldAddAllReplacements()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJson("my-app");
        var configurationName = "production";
        var projectName = "my-app";
        var fileReplacements = new List<FileReplacementModel>
        {
            new("src/environments/environment.ts", "src/environments/environment.prod.ts"),
            new("src/config/config.ts", "src/config/config.prod.ts")
        };

        // Act
        jObject.AddBuildConfiguration(configurationName, projectName, fileReplacements);

        // Assert
        var configuration = jObject["projects"]?[projectName]?["architect"]?["build"]?["configurations"]?[configurationName];
        var replacements = configuration?["fileReplacements"] as JArray;
        Assert.NotNull(replacements);
        Assert.Equal(2, replacements.Count);
    }

    [Fact]
    public void AddBuildConfiguration_WithEmptyReplacements_ShouldAddEmptyArray()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJson("test-project");
        var configurationName = "test";
        var projectName = "test-project";
        var fileReplacements = new List<FileReplacementModel>();

        // Act
        jObject.AddBuildConfiguration(configurationName, projectName, fileReplacements);

        // Assert
        var configuration = jObject["projects"]?[projectName]?["architect"]?["build"]?["configurations"]?[configurationName];
        var replacements = configuration?["fileReplacements"] as JArray;
        Assert.NotNull(replacements);
        Assert.Empty(replacements);
    }

    #endregion

    #region ExportsAssetsAndStyles Tests

    [Fact]
    public void ExportsAssetsAndStyles_ShouldSetAssetsArray()
    {
        // Arrange
        var jObject = new JObject();

        // Act
        jObject.ExportsAssetsAndStyles();

        // Assert
        var assets = jObject["assets"] as JArray;
        Assert.NotNull(assets);
        Assert.Equal(2, assets.Count);
        Assert.Equal("./scss/*.*", assets[0]?.ToString());
        Assert.Equal("./assets/**/*.*", assets[1]?.ToString());
    }

    [Fact]
    public void ExportsAssetsAndStyles_ShouldOverwriteExistingAssets()
    {
        // Arrange
        var jObject = new JObject
        {
            ["assets"] = new JArray("old-asset.ts")
        };

        // Act
        jObject.ExportsAssetsAndStyles();

        // Assert
        var assets = jObject["assets"] as JArray;
        Assert.NotNull(assets);
        Assert.Equal(2, assets.Count);
        Assert.DoesNotContain("old-asset.ts", assets.Select(a => a?.ToString()));
    }

    #endregion

    #region EnableDefaultStandalone Tests

    [Fact]
    public void EnableDefaultStandalone_ShouldSetSchematicsForProject()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJson("my-app");
        var projectName = "my-app";

        // Act
        jObject.EnableDefaultStandalone(projectName);

        // Assert
        var schematics = jObject["projects"]?[projectName]?["schematics"];
        Assert.NotNull(schematics);

        var componentSchematics = schematics["@schematics/angular:component"];
        Assert.NotNull(componentSchematics);
        Assert.True(componentSchematics["standalone"]?.Value<bool>());
        Assert.Equal("scss", componentSchematics["style"]?.ToString());

        var directiveSchematics = schematics["@schematics/angular:directive"];
        Assert.NotNull(directiveSchematics);
        Assert.True(directiveSchematics["standalone"]?.Value<bool>());
    }

    [Fact]
    public void EnableDefaultStandalone_ShouldOverwriteExistingSchematics()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJson("my-app");
        var projectName = "my-app";
        jObject["projects"]![projectName]!["schematics"] = new JObject
        {
            { "old-schematic", "old-value" }
        };

        // Act
        jObject.EnableDefaultStandalone(projectName);

        // Assert
        var schematics = jObject["projects"]?[projectName]?["schematics"];
        Assert.NotNull(schematics);
        Assert.Null(schematics["old-schematic"]);
        Assert.NotNull(schematics["@schematics/angular:component"]);
    }

    #endregion

    #region AddScript Tests

    [Fact]
    public void AddScript_ShouldAddNewScript()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject()
        };

        // Act
        jObject.AddScript("test", "jest");

        // Assert
        Assert.Equal("jest", jObject["scripts"]?["test"]?.ToString());
    }

    [Fact]
    public void AddScript_ShouldUpdateExistingScript()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject
            {
                ["test"] = "karma"
            }
        };

        // Act
        jObject.AddScript("test", "jest");

        // Assert
        Assert.Equal("jest", jObject["scripts"]?["test"]?.ToString());
    }

    [Fact]
    public void AddScript_ShouldAddMultipleScripts()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject()
        };

        // Act
        jObject.AddScript("build", "ng build");
        jObject.AddScript("test", "ng test");
        jObject.AddScript("lint", "ng lint");

        // Assert
        Assert.Equal("ng build", jObject["scripts"]?["build"]?.ToString());
        Assert.Equal("ng test", jObject["scripts"]?["test"]?.ToString());
        Assert.Equal("ng lint", jObject["scripts"]?["lint"]?.ToString());
    }

    #endregion

    #region AddScripts Tests

    [Fact]
    public void AddScripts_ShouldAddAllScripts()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject()
        };
        var scripts = new List<KeyValuePair<string, string>>
        {
            new("build", "ng build"),
            new("test", "ng test"),
            new("lint", "ng lint")
        };

        // Act
        jObject.AddScripts(scripts);

        // Assert
        Assert.Equal("ng build", jObject["scripts"]?["build"]?.ToString());
        Assert.Equal("ng test", jObject["scripts"]?["test"]?.ToString());
        Assert.Equal("ng lint", jObject["scripts"]?["lint"]?.ToString());
    }

    [Fact]
    public void AddScripts_WithEmptyCollection_ShouldNotModifyScripts()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject
            {
                ["existing"] = "value"
            }
        };
        var scripts = new List<KeyValuePair<string, string>>();

        // Act
        jObject.AddScripts(scripts);

        // Assert
        var scriptsObj = jObject["scripts"] as JObject;
        Assert.NotNull(scriptsObj);
        Assert.Single(scriptsObj.Properties());
        Assert.Equal("value", jObject["scripts"]?["existing"]?.ToString());
    }

    #endregion

    #region AddStyle Tests

    [Fact]
    public void AddStyle_ShouldPrependStyleToArray()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithStyles("my-app", new[] { "src/styles.scss" });
        var projectName = "my-app";
        var newStyle = "node_modules/bootstrap/dist/css/bootstrap.min.css";

        // Act
        jObject.AddStyle(projectName, newStyle);

        // Assert
        var styles = jObject["projects"]?[projectName]?["architect"]?["build"]?["options"]?["styles"] as JArray;
        Assert.NotNull(styles);
        Assert.Equal(2, styles.Count);
        Assert.Equal(newStyle, styles[0]?.ToString());
        Assert.Equal("src/styles.scss", styles[1]?.ToString());
    }

    [Fact]
    public void AddStyle_ToEmptyStylesArray_ShouldAddStyle()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithStyles("my-app", Array.Empty<string>());
        var projectName = "my-app";
        var newStyle = "src/custom-styles.scss";

        // Act
        jObject.AddStyle(projectName, newStyle);

        // Assert
        var styles = jObject["projects"]?[projectName]?["architect"]?["build"]?["options"]?["styles"] as JArray;
        Assert.NotNull(styles);
        Assert.Single(styles);
        Assert.Equal(newStyle, styles[0]?.ToString());
    }

    #endregion

    #region AddOrUpdate Tests

    [Fact]
    public void AddOrUpdate_ShouldAddNewKey()
    {
        // Arrange
        var jObject = new JObject();

        // Act
        jObject.AddOrUpdate("newKey", "newValue");

        // Assert
        Assert.Equal("newValue", jObject["newKey"]?.ToString());
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingKey()
    {
        // Arrange
        var jObject = new JObject
        {
            ["existingKey"] = "oldValue"
        };

        // Act
        jObject.AddOrUpdate("existingKey", "newValue");

        // Assert
        Assert.Equal("newValue", jObject["existingKey"]?.ToString());
    }

    [Fact]
    public void AddOrUpdate_WithJObjectValue_ShouldWork()
    {
        // Arrange
        var jObject = new JObject();
        var nestedObject = new JObject
        {
            ["nested"] = "value"
        };

        // Act
        jObject.AddOrUpdate("config", nestedObject);

        // Assert
        Assert.Equal("value", jObject["config"]?["nested"]?.ToString());
    }

    [Fact]
    public void AddOrUpdate_WithJArrayValue_ShouldWork()
    {
        // Arrange
        var jObject = new JObject();
        var array = new JArray("item1", "item2");

        // Act
        jObject.AddOrUpdate("items", array);

        // Assert
        var items = jObject["items"] as JArray;
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }

    #endregion

    #region GetSupportedLocales Tests

    [Fact]
    public void GetSupportedLocales_ShouldReturnLocalesList()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithLocales("my-app", new[] { "fr", "de", "es" });
        var projectName = "my-app";

        // Act
        var locales = jObject.GetSupportedLocales(projectName);

        // Assert
        Assert.NotNull(locales);
        Assert.Equal(3, locales.Count);
        Assert.Contains("fr", locales);
        Assert.Contains("de", locales);
        Assert.Contains("es", locales);
    }

    [Fact]
    public void GetSupportedLocales_WithSingleLocale_ShouldReturnSingleItem()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithLocales("my-app", new[] { "ja" });
        var projectName = "my-app";

        // Act
        var locales = jObject.GetSupportedLocales(projectName);

        // Assert
        Assert.Single(locales);
        Assert.Equal("ja", locales[0]);
    }

    #endregion

    #region AddSupportedLocales Tests

    [Fact]
    public void AddSupportedLocales_ShouldAddI18nConfiguration()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithRoot("my-app", "projects/my-app");
        var projectName = "my-app";
        var locales = new List<string> { "fr", "de" };

        // Act
        jObject.AddSupportedLocales(projectName, locales);

        // Assert
        var i18n = jObject["projects"]?[projectName]?["i18n"];
        Assert.NotNull(i18n);
        Assert.Equal("en-US", i18n["sourceLocale"]?.ToString());

        var localesConfig = i18n["locales"];
        Assert.NotNull(localesConfig);
        Assert.Equal("projects/my-app/src/locale/messages.fr.xlf", localesConfig["fr"]?.ToString());
        Assert.Equal("projects/my-app/src/locale/messages.de.xlf", localesConfig["de"]?.ToString());
    }

    [Fact]
    public void AddSupportedLocales_ShouldSetLocalizeInBuildOptions()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithRoot("my-app", "projects/my-app");
        var projectName = "my-app";
        var locales = new List<string> { "es", "pt" };

        // Act
        jObject.AddSupportedLocales(projectName, locales);

        // Assert
        var localizeArray = jObject["projects"]?[projectName]?["architect"]?["build"]?["options"]?["localize"] as JArray;
        Assert.NotNull(localizeArray);
        Assert.Equal(2, localizeArray.Count);
        Assert.Contains("es", localizeArray.Select(x => x?.ToString()));
        Assert.Contains("pt", localizeArray.Select(x => x?.ToString()));
    }

    #endregion

    #region GetProjectDirectory Tests

    [Fact]
    public void GetProjectDirectory_ShouldReturnRootPath()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithRoot("my-app", "projects/my-app");
        var projectName = "my-app";

        // Act
        var directory = jObject.GetProjectDirectory(projectName);

        // Assert
        Assert.Equal("projects/my-app", directory);
    }

    [Fact]
    public void GetProjectDirectory_WithEmptyRoot_ShouldReturnEmptyString()
    {
        // Arrange
        var jObject = CreateAngularWorkspaceJsonWithRoot("my-app", "");
        var projectName = "my-app";

        // Act
        var directory = jObject.GetProjectDirectory(projectName);

        // Assert
        Assert.Equal("", directory);
    }

    [Fact]
    public void GetProjectDirectory_WithDifferentProjectNames_ShouldReturnCorrectPath()
    {
        // Arrange
        var jObject = new JObject
        {
            ["projects"] = new JObject
            {
                ["app-one"] = new JObject { ["root"] = "projects/app-one" },
                ["app-two"] = new JObject { ["root"] = "projects/app-two" }
            }
        };

        // Act
        var directory1 = jObject.GetProjectDirectory("app-one");
        var directory2 = jObject.GetProjectDirectory("app-two");

        // Assert
        Assert.Equal("projects/app-one", directory1);
        Assert.Equal("projects/app-two", directory2);
    }

    #endregion

    #region RemoveAllScripts Tests

    [Fact]
    public void RemoveAllScripts_ShouldClearAllScripts()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject
            {
                ["build"] = "ng build",
                ["test"] = "ng test",
                ["lint"] = "ng lint"
            }
        };

        // Act
        jObject.RemoveAllScripts();

        // Assert
        var scripts = jObject["scripts"] as JObject;
        Assert.NotNull(scripts);
        Assert.Empty(scripts.Properties());
    }

    [Fact]
    public void RemoveAllScripts_WithEmptyScripts_ShouldNotThrow()
    {
        // Arrange
        var jObject = new JObject
        {
            ["scripts"] = new JObject()
        };

        // Act
        var exception = Record.Exception(() => jObject.RemoveAllScripts());

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region Helper Methods

    private static JObject CreateAngularWorkspaceJson(string projectName)
    {
        return new JObject
        {
            ["projects"] = new JObject
            {
                [projectName] = new JObject
                {
                    ["root"] = $"projects/{projectName}",
                    ["architect"] = new JObject
                    {
                        ["build"] = new JObject
                        {
                            ["options"] = new JObject
                            {
                                ["styles"] = new JArray()
                            },
                            ["configurations"] = new JObject()
                        }
                    }
                }
            }
        };
    }

    private static JObject CreateAngularWorkspaceJsonWithStyles(string projectName, string[] styles)
    {
        var jObject = CreateAngularWorkspaceJson(projectName);
        jObject["projects"]![projectName]!["architect"]!["build"]!["options"]!["styles"] = new JArray(styles);
        return jObject;
    }

    private static JObject CreateAngularWorkspaceJsonWithLocales(string projectName, string[] locales)
    {
        var jObject = CreateAngularWorkspaceJson(projectName);
        jObject["projects"]![projectName]!["architect"]!["build"]!["options"]!["localize"] = new JArray(locales);
        return jObject;
    }

    private static JObject CreateAngularWorkspaceJsonWithRoot(string projectName, string root)
    {
        var jObject = CreateAngularWorkspaceJson(projectName);
        jObject["projects"]![projectName]!["root"] = root;
        return jObject;
    }

    #endregion
}
