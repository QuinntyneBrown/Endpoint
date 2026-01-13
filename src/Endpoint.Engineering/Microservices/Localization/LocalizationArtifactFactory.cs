// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Localization;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class LocalizationArtifactFactory : ILocalizationArtifactFactory
{
    private readonly ILogger<LocalizationArtifactFactory> logger;

    public LocalizationArtifactFactory(ILogger<LocalizationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Localization.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateTranslationFile(entitiesDir));
        project.Files.Add(CreateLanguageFile(entitiesDir));
        project.Files.Add(CreateTranslationKeyFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateITranslationRepositoryFile(interfacesDir));
        project.Files.Add(CreateILocalizationServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateTranslationAddedEventFile(eventsDir));
        project.Files.Add(CreateLanguageEnabledEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateTranslationDtoFile(dtosDir));
        project.Files.Add(CreateLanguageDtoFile(dtosDir));
        project.Files.Add(CreateCreateTranslationRequestFile(dtosDir));
        project.Files.Add(CreateUpdateTranslationRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Localization.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateLocalizationDbContextFile(dataDir));
        project.Files.Add(CreateTranslationRepositoryFile(repositoriesDir));
        project.Files.Add(CreateLocalizationServiceFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Localization.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateTranslationsControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateTranslationFile(string directory)
    {
        var classModel = new ClassModel("Translation");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TranslationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TranslationKeyId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "LanguageId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsApproved", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("TranslationKey"), "TranslationKey", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Language"), "Language", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });

        return new CodeFileModel<ClassModel>(classModel, "Translation", directory, CSharp)
        {
            Namespace = "Localization.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateLanguageFile(string directory)
    {
        var classModel = new ClassModel("Language");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "LanguageId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Code", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "NativeName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsDefault", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Translation")] }, "Translations", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Translation>()" });

        return new CodeFileModel<ClassModel>(classModel, "Language", directory, CSharp)
        {
            Namespace = "Localization.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateTranslationKeyFile(string directory)
    {
        var classModel = new ClassModel("TranslationKey");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TranslationKeyId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Context", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Translation")] }, "Translations", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Translation>()" });

        return new CodeFileModel<ClassModel>(classModel, "TranslationKey", directory, CSharp)
        {
            Namespace = "Localization.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITranslationRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITranslationRepository");

        interfaceModel.Usings.Add(new UsingModel("Localization.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Translation") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "translationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByLanguageCodeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Translation")] }] },
            Params =
            [
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByKeyAndLanguageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Translation") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Translation")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Translation")] },
            Params =
            [
                new ParamModel { Name = "translation", Type = new TypeModel("Translation") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "translation", Type = new TypeModel("Translation") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "translationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITranslationRepository", directory, CSharp)
        {
            Namespace = "Localization.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateILocalizationServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ILocalizationService");

        interfaceModel.Usings.Add(new UsingModel("Localization.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetTranslationsByLanguageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TranslationDto")] }] },
            Params =
            [
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateTranslationAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TranslationDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTranslationRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateTranslationAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TranslationDto")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "request", Type = new TypeModel("UpdateTranslationRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetEnabledLanguagesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("LanguageDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "EnableLanguageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "languageId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ILocalizationService", directory, CSharp)
        {
            Namespace = "Localization.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateTranslationAddedEventFile(string directory)
    {
        var classModel = new ClassModel("TranslationAddedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TranslationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "LanguageCode", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "TranslationAddedEvent", directory, CSharp)
        {
            Namespace = "Localization.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateLanguageEnabledEventFile(string directory)
    {
        var classModel = new ClassModel("LanguageEnabledEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "LanguageId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Code", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "LanguageEnabledEvent", directory, CSharp)
        {
            Namespace = "Localization.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateTranslationDtoFile(string directory)
    {
        var classModel = new ClassModel("TranslationDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TranslationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "LanguageCode", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsApproved", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TranslationDto", directory, CSharp)
        {
            Namespace = "Localization.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateLanguageDtoFile(string directory)
    {
        var classModel = new ClassModel("LanguageDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "LanguageId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Code", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "NativeName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsDefault", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "LanguageDto", directory, CSharp)
        {
            Namespace = "Localization.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateCreateTranslationRequestFile(string directory)
    {
        var classModel = new ClassModel("CreateTranslationRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var keyProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        keyProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(keyProp);

        var languageCodeProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "LanguageCode", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        languageCodeProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(languageCodeProp);

        var valueProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        valueProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(valueProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Context", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "CreateTranslationRequest", directory, CSharp)
        {
            Namespace = "Localization.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateUpdateTranslationRequestFile(string directory)
    {
        var classModel = new ClassModel("UpdateTranslationRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var languageCodeProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "LanguageCode", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        languageCodeProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(languageCodeProp);

        var valueProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        valueProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(valueProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool") { Nullable = true }, "IsApproved", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "UpdateTranslationRequest", directory, CSharp)
        {
            Namespace = "Localization.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateLocalizationDbContextFile(string directory)
    {
        var classModel = new ClassModel("LocalizationDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "LocalizationDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("LocalizationDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Translation")] }, "Translations", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Language")] }, "Languages", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("TranslationKey")] }, "TranslationKeys", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Translation>(entity =>
        {
            entity.HasKey(t => t.TranslationId);
            entity.Property(t => t.Value).IsRequired();
            entity.HasOne(t => t.TranslationKey).WithMany(k => k.Translations).HasForeignKey(t => t.TranslationKeyId);
            entity.HasOne(t => t.Language).WithMany(l => l.Translations).HasForeignKey(t => t.LanguageId);
            entity.HasIndex(t => new { t.TranslationKeyId, t.LanguageId }).IsUnique();
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(l => l.LanguageId);
            entity.Property(l => l.Code).IsRequired().HasMaxLength(10);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(100);
            entity.Property(l => l.NativeName).IsRequired().HasMaxLength(100);
            entity.HasIndex(l => l.Code).IsUnique();
        });

        modelBuilder.Entity<TranslationKey>(entity =>
        {
            entity.HasKey(k => k.TranslationKeyId);
            entity.Property(k => k.Key).IsRequired().HasMaxLength(500);
            entity.HasIndex(k => k.Key).IsUnique();
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "LocalizationDbContext", directory, CSharp)
        {
            Namespace = "Localization.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateTranslationRepositoryFile(string directory)
    {
        var classModel = new ClassModel("TranslationRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Localization.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ITranslationRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("LocalizationDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "TranslationRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("LocalizationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Translation") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "translationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Translations
            .Include(t => t.TranslationKey)
            .Include(t => t.Language)
            .FirstOrDefaultAsync(t => t.TranslationId == translationId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByLanguageCodeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Translation")] }] },
            Params =
            [
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Translations
            .Include(t => t.TranslationKey)
            .Include(t => t.Language)
            .Where(t => t.Language.Code == languageCode)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByKeyAndLanguageAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Translation") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Translations
            .Include(t => t.TranslationKey)
            .Include(t => t.Language)
            .FirstOrDefaultAsync(t => t.TranslationKey.Key == key && t.Language.Code == languageCode, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Translation")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Translations
            .Include(t => t.TranslationKey)
            .Include(t => t.Language)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Translation")] },
            Params =
            [
                new ParamModel { Name = "translation", Type = new TypeModel("Translation") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"translation.TranslationId = Guid.NewGuid();
        await context.Translations.AddAsync(translation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return translation;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "translation", Type = new TypeModel("Translation") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"translation.UpdatedAt = DateTime.UtcNow;
        context.Translations.Update(translation);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "translationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var translation = await context.Translations.FindAsync(new object[] { translationId }, cancellationToken);
        if (translation != null)
        {
            context.Translations.Remove(translation);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "TranslationRepository", directory, CSharp)
        {
            Namespace = "Localization.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateLocalizationServiceFile(string directory)
    {
        var classModel = new ClassModel("LocalizationService");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Localization.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Localization.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ILocalizationService"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("ITranslationRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("LocalizationDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "LocalizationService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("ITranslationRepository") },
                new ParamModel { Name = "context", Type = new TypeModel("LocalizationDbContext") }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetTranslationsByLanguageAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TranslationDto")] }] },
            Params =
            [
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var translations = await repository.GetByLanguageCodeAsync(languageCode, cancellationToken);
        return translations.Select(t => new TranslationDto
        {
            TranslationId = t.TranslationId,
            Key = t.TranslationKey.Key,
            LanguageCode = t.Language.Code,
            Value = t.Value,
            IsApproved = t.IsApproved,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateTranslationAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TranslationDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTranslationRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var language = await context.Languages.FirstOrDefaultAsync(l => l.Code == request.LanguageCode, cancellationToken)
            ?? throw new InvalidOperationException($""Language '{request.LanguageCode}' not found"");

        var translationKey = await context.TranslationKeys.FirstOrDefaultAsync(k => k.Key == request.Key, cancellationToken);
        if (translationKey == null)
        {
            translationKey = new TranslationKey
            {
                TranslationKeyId = Guid.NewGuid(),
                Key = request.Key,
                Description = request.Description,
                Context = request.Context
            };
            await context.TranslationKeys.AddAsync(translationKey, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        var translation = new Translation
        {
            TranslationKeyId = translationKey.TranslationKeyId,
            LanguageId = language.LanguageId,
            Value = request.Value
        };

        var created = await repository.AddAsync(translation, cancellationToken);

        return new TranslationDto
        {
            TranslationId = created.TranslationId,
            Key = request.Key,
            LanguageCode = request.LanguageCode,
            Value = created.Value,
            IsApproved = created.IsApproved,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateTranslationAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TranslationDto")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "request", Type = new TypeModel("UpdateTranslationRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var translation = await repository.GetByKeyAndLanguageAsync(key, request.LanguageCode, cancellationToken)
            ?? throw new InvalidOperationException($""Translation not found for key '{key}' and language '{request.LanguageCode}'"");

        translation.Value = request.Value;
        if (request.IsApproved.HasValue)
        {
            translation.IsApproved = request.IsApproved.Value;
        }

        await repository.UpdateAsync(translation, cancellationToken);

        return new TranslationDto
        {
            TranslationId = translation.TranslationId,
            Key = translation.TranslationKey.Key,
            LanguageCode = translation.Language.Code,
            Value = translation.Value,
            IsApproved = translation.IsApproved,
            CreatedAt = translation.CreatedAt,
            UpdatedAt = translation.UpdatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetEnabledLanguagesAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("LanguageDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var languages = await context.Languages.Where(l => l.IsEnabled).ToListAsync(cancellationToken);
        return languages.Select(l => new LanguageDto
        {
            LanguageId = l.LanguageId,
            Code = l.Code,
            Name = l.Name,
            NativeName = l.NativeName,
            IsEnabled = l.IsEnabled,
            IsDefault = l.IsDefault
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "EnableLanguageAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "languageId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var language = await context.Languages.FindAsync(new object[] { languageId }, cancellationToken)
            ?? throw new InvalidOperationException($""Language with ID '{languageId}' not found"");

        language.IsEnabled = true;
        language.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "LocalizationService", directory, CSharp)
        {
            Namespace = "Localization.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Localization.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Localization.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Localization.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddLocalizationInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<LocalizationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""LocalizationDb"") ??
                @""Server=.\SQLEXPRESS;Database=LocalizationDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateTranslationsControllerFile(string directory)
    {
        var classModel = new ClassModel("TranslationsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Localization.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Localization.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "service", Type = new TypeModel("ILocalizationService"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "TranslationsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "service", Type = new TypeModel("ILocalizationService") }],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var getByLanguageMethod = new MethodModel
        {
            Name = "GetByLanguage",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TranslationDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "languageCode", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var translations = await service.GetTranslationsByLanguageAsync(languageCode, cancellationToken);
        return Ok(translations);")
        };
        getByLanguageMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{languageCode}\"" });
        classModel.Methods.Add(getByLanguageMethod);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("TranslationDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTranslationRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var translation = await service.CreateTranslationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByLanguage), new { languageCode = translation.LanguageCode }, translation);")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        var updateMethod = new MethodModel
        {
            Name = "Update",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("TranslationDto")] }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "request", Type = new TypeModel("UpdateTranslationRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var translation = await service.UpdateTranslationAsync(key, request, cancellationToken);
        return Ok(translation);")
        };
        updateMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPut", Template = "\"{key}\"" });
        classModel.Methods.Add(updateMethod);

        var getLanguagesMethod = new MethodModel
        {
            Name = "GetLanguages",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("LanguageDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var languages = await service.GetEnabledLanguagesAsync(cancellationToken);
        return Ok(languages);")
        };
        getLanguagesMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"languages\"" });
        classModel.Methods.Add(getLanguagesMethod);

        return new CodeFileModel<ClassModel>(classModel, "TranslationsController", directory, CSharp)
        {
            Namespace = "Localization.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddLocalizationInfrastructure(builder.Configuration);
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.MapControllers();
                app.MapHealthChecks("/health");
                app.Run();
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "LocalizationDb": "Server=.\\SQLEXPRESS;Database=LocalizationDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    #endregion
}
