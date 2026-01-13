// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Tagging;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class TaggingArtifactFactory : ITaggingArtifactFactory
{
    private readonly ILogger<TaggingArtifactFactory> logger;

    public TaggingArtifactFactory(ILogger<TaggingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Tagging.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateTagEntityFile(entitiesDir));
        project.Files.Add(CreateCategoryEntityFile(entitiesDir));
        project.Files.Add(CreateEntityTagFile(entitiesDir));
        project.Files.Add(CreateTaxonomyEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateITagRepositoryFile(interfacesDir));
        project.Files.Add(CreateITaggingServiceFile(interfacesDir));
        project.Files.Add(CreateITaxonomyServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateTagCreatedEventFile(eventsDir));
        project.Files.Add(CreateEntityTaggedEventFile(eventsDir));
        project.Files.Add(CreateTagMergedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateTagDtoFile(dtosDir));
        project.Files.Add(CreateEntityTagDtoFile(dtosDir));
        project.Files.Add(CreateTaxonomyDtoFile(dtosDir));
        project.Files.Add(CreateCreateTagRequestFile(dtosDir));
        project.Files.Add(CreateTagEntityRequestFile(dtosDir));
        project.Files.Add(CreateCreateTaxonomyRequestFile(dtosDir));
        project.Files.Add(CreateUpdateTaxonomyRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tagging.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateTaggingDbContextFile(dataDir));
        project.Files.Add(CreateTagRepositoryFile(repositoriesDir));
        project.Files.Add(CreateTaggingServiceFile(servicesDir));
        project.Files.Add(CreateTaxonomyServiceFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tagging.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateTagsControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer - Entities

    private static CodeFileModel<ClassModel> CreateTagEntityFile(string directory)
    {
        var classModel = new ClassModel("Tag");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "CategoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Category") { Nullable = true }, "Category", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Color", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("EntityTag")] }, "EntityTags", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<EntityTag>()" });

        return new CodeFileModel<ClassModel>(classModel, "Tag", directory, CSharp)
        {
            Namespace = "Tagging.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateCategoryEntityFile(string directory)
    {
        var classModel = new ClassModel("Category");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CategoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "ParentCategoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Category") { Nullable = true }, "ParentCategory", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Tag")] }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Tag>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Category")] }, "SubCategories", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Category>()" });

        return new CodeFileModel<ClassModel>(classModel, "Category", directory, CSharp)
        {
            Namespace = "Tagging.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateEntityTagFile(string directory)
    {
        var classModel = new ClassModel("EntityTag");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityTagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Tag"), "Tag", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "TaggedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "TaggedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "EntityTag", directory, CSharp)
        {
            Namespace = "Tagging.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateTaxonomyEntityFile(string directory)
    {
        var classModel = new ClassModel("Taxonomy");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TaxonomyId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsHierarchical", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "AllowMultiple", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "Taxonomy", directory, CSharp)
        {
            Namespace = "Tagging.Core.Entities"
        };
    }

    #endregion

    #region Core Layer - Interfaces

    private static CodeFileModel<InterfaceModel> CreateITagRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITagRepository");

        interfaceModel.Usings.Add(new UsingModel("Tagging.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Tag") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByNameAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Tag") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Tag")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByCategoryIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Tag")] }] },
            Params =
            [
                new ParamModel { Name = "categoryId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Tag")] },
            Params =
            [
                new ParamModel { Name = "tag", Type = new TypeModel("Tag") },
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
                new ParamModel { Name = "tag", Type = new TypeModel("Tag") },
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
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITagRepository", directory, CSharp)
        {
            Namespace = "Tagging.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITaggingServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITaggingService");

        interfaceModel.Usings.Add(new UsingModel("Tagging.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateTagAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TagDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTagRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetTagByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TagDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllTagsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TagDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "TagEntityAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("EntityTagDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("TagEntityRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetTagsForEntityAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TagDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UntagEntityAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "MergeTagsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "sourceTagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "targetTagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITaggingService", directory, CSharp)
        {
            Namespace = "Tagging.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITaxonomyServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITaxonomyService");

        interfaceModel.Usings.Add(new UsingModel("Tagging.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateTaxonomyAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TaxonomyDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTaxonomyRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetTaxonomyByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TaxonomyDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "taxonomyId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllTaxonomiesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TaxonomyDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateTaxonomyAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "taxonomyId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("UpdateTaxonomyRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteTaxonomyAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "taxonomyId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITaxonomyService", directory, CSharp)
        {
            Namespace = "Tagging.Core.Interfaces"
        };
    }

    #endregion

    #region Core Layer - Events

    private static CodeFileModel<ClassModel> CreateTagCreatedEventFile(string directory)
    {
        var classModel = new ClassModel("TagCreatedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "CategoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "TagCreatedEvent", directory, CSharp)
        {
            Namespace = "Tagging.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateEntityTaggedEventFile(string directory)
    {
        var classModel = new ClassModel("EntityTaggedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityTagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "TaggedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "EntityTaggedEvent", directory, CSharp)
        {
            Namespace = "Tagging.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateTagMergedEventFile(string directory)
    {
        var classModel = new ClassModel("TagMergedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SourceTagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TargetTagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "EntitiesRetagged", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "TagMergedEvent", directory, CSharp)
        {
            Namespace = "Tagging.Core.Events"
        };
    }

    #endregion

    #region Core Layer - DTOs

    private static CodeFileModel<ClassModel> CreateTagDtoFile(string directory)
    {
        var classModel = new ClassModel("TagDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "CategoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "CategoryName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Color", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TagDto", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateEntityTagDtoFile(string directory)
    {
        var classModel = new ClassModel("EntityTagDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityTagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TagName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "TaggedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "EntityTagDto", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateTaxonomyDtoFile(string directory)
    {
        var classModel = new ClassModel("TaxonomyDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TaxonomyId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsHierarchical", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "AllowMultiple", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TaxonomyDto", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateCreateTagRequestFile(string directory)
    {
        var classModel = new ClassModel("CreateTagRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var nameProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "100" });
        classModel.Properties.Add(nameProperty);

        var descriptionProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        descriptionProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "500" });
        classModel.Properties.Add(descriptionProperty);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "CategoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        var colorProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Color", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        colorProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "7" });
        classModel.Properties.Add(colorProperty);

        return new CodeFileModel<ClassModel>(classModel, "CreateTagRequest", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateTagEntityRequestFile(string directory)
    {
        var classModel = new ClassModel("TagEntityRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var tagIdProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        tagIdProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(tagIdProperty);

        var entityTypeProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        entityTypeProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(entityTypeProperty);

        var entityIdProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        entityIdProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(entityIdProperty);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "TaggedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TagEntityRequest", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateCreateTaxonomyRequestFile(string directory)
    {
        var classModel = new ClassModel("CreateTaxonomyRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var nameProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "100" });
        classModel.Properties.Add(nameProperty);

        var descriptionProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        descriptionProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "500" });
        classModel.Properties.Add(descriptionProperty);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsHierarchical", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "AllowMultiple", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "true" });

        return new CodeFileModel<ClassModel>(classModel, "CreateTaxonomyRequest", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateUpdateTaxonomyRequestFile(string directory)
    {
        var classModel = new ClassModel("UpdateTaxonomyRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var nameProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "100" });
        classModel.Properties.Add(nameProperty);

        var descriptionProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        descriptionProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "MaxLength", Template = "500" });
        classModel.Properties.Add(descriptionProperty);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool") { Nullable = true }, "IsHierarchical", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool") { Nullable = true }, "AllowMultiple", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool") { Nullable = true }, "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "UpdateTaxonomyRequest", directory, CSharp)
        {
            Namespace = "Tagging.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer

    private static CodeFileModel<ClassModel> CreateTaggingDbContextFile(string directory)
    {
        var classModel = new ClassModel("TaggingDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "TaggingDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("TaggingDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Tag")] }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Category")] }, "Categories", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("EntityTag")] }, "EntityTags", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Taxonomy")] }, "Taxonomies", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.TagId);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
            entity.Property(t => t.Color).HasMaxLength(7);
            entity.HasOne(t => t.Category).WithMany(c => c.Tags).HasForeignKey(t => t.CategoryId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
            entity.HasOne(c => c.ParentCategory).WithMany(c => c.SubCategories).HasForeignKey(c => c.ParentCategoryId);
        });

        modelBuilder.Entity<EntityTag>(entity =>
        {
            entity.HasKey(et => et.EntityTagId);
            entity.Property(et => et.EntityType).IsRequired().HasMaxLength(100);
            entity.HasIndex(et => new { et.TagId, et.EntityType, et.EntityId }).IsUnique();
            entity.HasOne(et => et.Tag).WithMany(t => t.EntityTags).HasForeignKey(et => et.TagId);
        });

        modelBuilder.Entity<Taxonomy>(entity =>
        {
            entity.HasKey(t => t.TaxonomyId);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "TaggingDbContext", directory, CSharp)
        {
            Namespace = "Tagging.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateTagRepositoryFile(string directory)
    {
        var classModel = new ClassModel("TagRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Tagging.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ITagRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("TaggingDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "TagRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("TaggingDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Tag") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tags.Include(t => t.Category).FirstOrDefaultAsync(t => t.TagId == tagId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByNameAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Tag") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tags.Include(t => t.Category).FirstOrDefaultAsync(t => t.Name == name, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Tag")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tags.Include(t => t.Category).Where(t => t.IsActive).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByCategoryIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Tag")] }] },
            Params =
            [
                new ParamModel { Name = "categoryId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tags.Include(t => t.Category).Where(t => t.CategoryId == categoryId && t.IsActive).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Tag")] },
            Params =
            [
                new ParamModel { Name = "tag", Type = new TypeModel("Tag") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"tag.TagId = Guid.NewGuid();
        await context.Tags.AddAsync(tag, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return tag;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "tag", Type = new TypeModel("Tag") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"tag.UpdatedAt = DateTime.UtcNow;
        context.Tags.Update(tag);
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
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tag = await context.Tags.FindAsync(new object[] { tagId }, cancellationToken);
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "TagRepository", directory, CSharp)
        {
            Namespace = "Tagging.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateTaggingServiceFile(string directory)
    {
        var classModel = new ClassModel("TaggingService");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Tagging.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ITaggingService"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("ITagRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("TaggingDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "TaggingService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("ITagRepository") },
                new ParamModel { Name = "context", Type = new TypeModel("TaggingDbContext") }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateTagAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TagDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTagRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tag = new Tag
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Color = request.Color
        };

        var created = await repository.AddAsync(tag, cancellationToken);
        var category = created.CategoryId.HasValue
            ? await context.Categories.FindAsync(new object[] { created.CategoryId.Value }, cancellationToken)
            : null;

        return new TagDto
        {
            TagId = created.TagId,
            Name = created.Name,
            Description = created.Description,
            CategoryId = created.CategoryId,
            CategoryName = category?.Name,
            Color = created.Color,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetTagByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TagDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tag = await repository.GetByIdAsync(tagId, cancellationToken);
        if (tag == null) return null;

        return new TagDto
        {
            TagId = tag.TagId,
            Name = tag.Name,
            Description = tag.Description,
            CategoryId = tag.CategoryId,
            CategoryName = tag.Category?.Name,
            Color = tag.Color,
            IsActive = tag.IsActive,
            CreatedAt = tag.CreatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllTagsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TagDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tags = await repository.GetAllAsync(cancellationToken);
        return tags.Select(t => new TagDto
        {
            TagId = t.TagId,
            Name = t.Name,
            Description = t.Description,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name,
            Color = t.Color,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "TagEntityAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("EntityTagDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("TagEntityRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tag = await repository.GetByIdAsync(request.TagId, cancellationToken)
            ?? throw new InvalidOperationException($""Tag with ID {request.TagId} not found."");

        var entityTag = new EntityTag
        {
            EntityTagId = Guid.NewGuid(),
            TagId = request.TagId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            TaggedBy = request.TaggedBy
        };

        await context.EntityTags.AddAsync(entityTag, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new EntityTagDto
        {
            EntityTagId = entityTag.EntityTagId,
            TagId = entityTag.TagId,
            TagName = tag.Name,
            EntityType = entityTag.EntityType,
            EntityId = entityTag.EntityId,
            TaggedAt = entityTag.TaggedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetTagsForEntityAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TagDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entityTags = await context.EntityTags
            .Include(et => et.Tag)
            .ThenInclude(t => t.Category)
            .Where(et => et.EntityType == entityType && et.EntityId == entityId)
            .ToListAsync(cancellationToken);

        return entityTags.Select(et => new TagDto
        {
            TagId = et.Tag.TagId,
            Name = et.Tag.Name,
            Description = et.Tag.Description,
            CategoryId = et.Tag.CategoryId,
            CategoryName = et.Tag.Category?.Name,
            Color = et.Tag.Color,
            IsActive = et.Tag.IsActive,
            CreatedAt = et.Tag.CreatedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UntagEntityAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entityTag = await context.EntityTags
            .FirstOrDefaultAsync(et => et.EntityType == entityType && et.EntityId == entityId && et.TagId == tagId, cancellationToken);

        if (entityTag != null)
        {
            context.EntityTags.Remove(entityTag);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "MergeTagsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "sourceTagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "targetTagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var sourceTag = await repository.GetByIdAsync(sourceTagId, cancellationToken)
            ?? throw new InvalidOperationException($""Source tag with ID {sourceTagId} not found."");

        var targetTag = await repository.GetByIdAsync(targetTagId, cancellationToken)
            ?? throw new InvalidOperationException($""Target tag with ID {targetTagId} not found."");

        var entityTags = await context.EntityTags
            .Where(et => et.TagId == sourceTagId)
            .ToListAsync(cancellationToken);

        foreach (var entityTag in entityTags)
        {
            var existingTarget = await context.EntityTags
                .AnyAsync(et => et.TagId == targetTagId && et.EntityType == entityTag.EntityType && et.EntityId == entityTag.EntityId, cancellationToken);

            if (!existingTarget)
            {
                entityTag.TagId = targetTagId;
            }
            else
            {
                context.EntityTags.Remove(entityTag);
            }
        }

        sourceTag.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "TaggingService", directory, CSharp)
        {
            Namespace = "Tagging.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateTaxonomyServiceFile(string directory)
    {
        var classModel = new ClassModel("TaxonomyService");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Tagging.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ITaxonomyService"));

        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("TaggingDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "TaxonomyService")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("TaggingDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateTaxonomyAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TaxonomyDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTaxonomyRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var taxonomy = new Taxonomy
        {
            TaxonomyId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsHierarchical = request.IsHierarchical,
            AllowMultiple = request.AllowMultiple
        };

        await context.Taxonomies.AddAsync(taxonomy, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new TaxonomyDto
        {
            TaxonomyId = taxonomy.TaxonomyId,
            Name = taxonomy.Name,
            Description = taxonomy.Description,
            IsHierarchical = taxonomy.IsHierarchical,
            AllowMultiple = taxonomy.AllowMultiple,
            IsActive = taxonomy.IsActive,
            CreatedAt = taxonomy.CreatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetTaxonomyByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TaxonomyDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "taxonomyId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var taxonomy = await context.Taxonomies.FindAsync(new object[] { taxonomyId }, cancellationToken);
        if (taxonomy == null) return null;

        return new TaxonomyDto
        {
            TaxonomyId = taxonomy.TaxonomyId,
            Name = taxonomy.Name,
            Description = taxonomy.Description,
            IsHierarchical = taxonomy.IsHierarchical,
            AllowMultiple = taxonomy.AllowMultiple,
            IsActive = taxonomy.IsActive,
            CreatedAt = taxonomy.CreatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllTaxonomiesAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TaxonomyDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var taxonomies = await context.Taxonomies.Where(t => t.IsActive).ToListAsync(cancellationToken);
        return taxonomies.Select(t => new TaxonomyDto
        {
            TaxonomyId = t.TaxonomyId,
            Name = t.Name,
            Description = t.Description,
            IsHierarchical = t.IsHierarchical,
            AllowMultiple = t.AllowMultiple,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateTaxonomyAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "taxonomyId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("UpdateTaxonomyRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var taxonomy = await context.Taxonomies.FindAsync(new object[] { taxonomyId }, cancellationToken)
            ?? throw new InvalidOperationException($""Taxonomy with ID {taxonomyId} not found."");

        if (request.Name != null) taxonomy.Name = request.Name;
        if (request.Description != null) taxonomy.Description = request.Description;
        if (request.IsHierarchical.HasValue) taxonomy.IsHierarchical = request.IsHierarchical.Value;
        if (request.AllowMultiple.HasValue) taxonomy.AllowMultiple = request.AllowMultiple.Value;
        if (request.IsActive.HasValue) taxonomy.IsActive = request.IsActive.Value;
        taxonomy.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteTaxonomyAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "taxonomyId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var taxonomy = await context.Taxonomies.FindAsync(new object[] { taxonomyId }, cancellationToken);
        if (taxonomy != null)
        {
            context.Taxonomies.Remove(taxonomy);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "TaxonomyService", directory, CSharp)
        {
            Namespace = "Tagging.Infrastructure.Services"
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
        classModel.Usings.Add(new UsingModel("Tagging.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Tagging.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Tagging.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Tagging.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddTaggingInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<TaggingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""TaggingDb"") ??
                @""Server=.\SQLEXPRESS;Database=TaggingDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITaggingService, TaggingService>();
        services.AddScoped<ITaxonomyService, TaxonomyService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer

    private static CodeFileModel<ClassModel> CreateTagsControllerFile(string directory)
    {
        var classModel = new ClassModel("TagsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Tagging.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "service", Type = new TypeModel("ITaggingService"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "TagsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "service", Type = new TypeModel("ITaggingService") }],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("TagDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTagRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var tag = await service.CreateTagAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = tag.TagId }, tag);")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        var getAllMethod = new MethodModel
        {
            Name = "GetAll",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TagDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var tags = await service.GetAllTagsAsync(cancellationToken);
        return Ok(tags);")
        };
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        classModel.Methods.Add(getAllMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("TagDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var tag = await service.GetTagByIdAsync(id, cancellationToken);
        if (tag == null) return NotFound();
        return Ok(tag);")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var tagEntityMethod = new MethodModel
        {
            Name = "TagEntity",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("EntityTagDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "body", Type = new TypeModel("TagEntityBodyRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var request = new TagEntityRequest
        {
            TagId = body.TagId,
            EntityType = entityType,
            EntityId = entityId,
            TaggedBy = body.TaggedBy
        };
        var entityTag = await service.TagEntityAsync(request, cancellationToken);
        return Created($""/api/tags/{entityType}/{entityId}"", entityTag);")
        };
        tagEntityMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"{entityType}/{entityId:guid}\"" });
        classModel.Methods.Add(tagEntityMethod);

        var getTagsForEntityMethod = new MethodModel
        {
            Name = "GetTagsForEntity",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TagDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var tags = await service.GetTagsForEntityAsync(entityType, entityId, cancellationToken);
        return Ok(tags);")
        };
        getTagsForEntityMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{entityType}/{entityId:guid}\"" });
        classModel.Methods.Add(getTagsForEntityMethod);

        var untagEntityMethod = new MethodModel
        {
            Name = "UntagEntity",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "tagId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"await service.UntagEntityAsync(entityType, entityId, tagId, cancellationToken);
        return NoContent();")
        };
        untagEntityMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{entityType}/{entityId:guid}/{tagId:guid}\"" });
        classModel.Methods.Add(untagEntityMethod);

        var mergeTagsMethod = new MethodModel
        {
            Name = "MergeTags",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("MergeTagsRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"await service.MergeTagsAsync(request.SourceTagId, request.TargetTagId, cancellationToken);
        return NoContent();")
        };
        mergeTagsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"merge\"" });
        classModel.Methods.Add(mergeTagsMethod);

        return new CodeFileModel<ClassModel>(classModel, "TagsController", directory, CSharp)
        {
            Namespace = "Tagging.Api.Controllers"
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

                builder.Services.AddTaggingInfrastructure(builder.Configuration);
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
                    "TaggingDb": "Server=.\\SQLEXPRESS;Database=TaggingDb;Trusted_Connection=True;TrustServerCertificate=True"
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
