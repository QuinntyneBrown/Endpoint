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

namespace Endpoint.Engineering.Microservices.Search;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class SearchArtifactFactory : ISearchArtifactFactory
{
    private readonly ILogger<SearchArtifactFactory> logger;

    public SearchArtifactFactory(ILogger<SearchArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Search.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateSearchIndexFile(entitiesDir));
        project.Files.Add(CreateSearchQueryFile(entitiesDir));
        project.Files.Add(CreateSearchResultFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateISearchRepositoryFile(interfacesDir));
        project.Files.Add(CreateISearchServiceFile(interfacesDir));
        project.Files.Add(CreateIIndexerFile(interfacesDir));

        // Events
        project.Files.Add(CreateDocumentIndexedEventFile(eventsDir));
        project.Files.Add(CreateDocumentRemovedEventFile(eventsDir));
        project.Files.Add(CreateSearchPerformedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateSearchRequestDtoFile(dtosDir));
        project.Files.Add(CreateSearchResultDtoFile(dtosDir));
        project.Files.Add(CreateSearchResponseDtoFile(dtosDir));
        project.Files.Add(CreateIndexDocumentRequestDtoFile(dtosDir));
        project.Files.Add(CreateIndexInfoDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Search.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateSearchDbContextFile(dataDir));
        project.Files.Add(CreateSearchRepositoryFile(repositoriesDir));
        project.Files.Add(CreateSearchServiceFile(servicesDir));
        project.Files.Add(CreateIndexerFile(servicesDir));
        project.Files.Add(CreateConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Search.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateSearchControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static FileModel CreateSearchIndexFile(string directory)
    {
        // Using FileModel because this file contains both a class and an enum
        return new FileModel("SearchIndex", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Entities;

                public class SearchIndex
                {
                    public Guid IndexId { get; set; }
                    public required string Name { get; set; }
                    public required string DocumentType { get; set; }
                    public long DocumentCount { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? LastUpdatedAt { get; set; }
                    public IndexStatus Status { get; set; } = IndexStatus.Active;
                }

                public enum IndexStatus { Active, Building, Inactive }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchQueryFile(string directory)
    {
        var classModel = new ClassModel("SearchQuery");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "QueryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "QueryText", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Skip", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Take", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "10" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("string")] }, "Filters", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SortBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "SortDescending", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "ExecutedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "SearchQuery", directory, CSharp)
        {
            Namespace = "Search.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchResultFile(string directory)
    {
        var classModel = new ClassModel("SearchResult");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ResultId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Score", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Document", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] }] }, "Highlights", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new()" });

        return new CodeFileModel<ClassModel>(classModel, "SearchResult", directory, CSharp)
        {
            Namespace = "Search.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateISearchRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ISearchRepository");

        interfaceModel.Usings.Add(new UsingModel("Search.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetIndexByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchIndex") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "indexId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetIndexByNameAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchIndex") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllIndexesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("SearchIndex")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateIndexAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchIndex")] },
            Params =
            [
                new ParamModel { Name = "index", Type = new TypeModel("SearchIndex") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateIndexAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "index", Type = new TypeModel("SearchIndex") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteIndexAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ISearchRepository", directory, CSharp)
        {
            Namespace = "Search.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateISearchServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ISearchService");

        interfaceModel.Usings.Add(new UsingModel("Search.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SearchAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("SearchResult")] }] },
            Params =
            [
                new ParamModel { Name = "query", Type = new TypeModel("SearchQuery") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetDocumentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchResult") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ISearchService", directory, CSharp)
        {
            Namespace = "Search.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIIndexerFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IIndexer");

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "IndexDocumentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "document", Type = new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "IndexDocumentsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documents", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("(string documentId, Dictionary<string, object> document)")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RemoveDocumentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RemoveDocumentsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentIds", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RebuildIndexAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IIndexer", directory, CSharp)
        {
            Namespace = "Search.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentIndexedEventFile(string directory)
    {
        var classModel = new ClassModel("DocumentIndexedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "DocumentIndexedEvent", directory, CSharp)
        {
            Namespace = "Search.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentRemovedEventFile(string directory)
    {
        var classModel = new ClassModel("DocumentRemovedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "DocumentRemovedEvent", directory, CSharp)
        {
            Namespace = "Search.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchPerformedEventFile(string directory)
    {
        var classModel = new ClassModel("SearchPerformedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "QueryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "QueryText", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "ResultCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "ExecutionTimeMs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "SearchPerformedEvent", directory, CSharp)
        {
            Namespace = "Search.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchRequestDtoFile(string directory)
    {
        var classModel = new ClassModel("SearchRequestDto")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var queryProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Query", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        queryProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(queryProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Skip", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Take", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "10" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("string")], Nullable = true }, "Filters", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SortBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "SortDescending", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "SearchRequestDto", directory, CSharp)
        {
            Namespace = "Search.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchResultDtoFile(string directory)
    {
        var classModel = new ClassModel("SearchResultDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "IndexName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Score", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Document", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "new()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] }] }, "Highlights", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "new()" });

        return new CodeFileModel<ClassModel>(classModel, "SearchResultDto", directory, CSharp)
        {
            Namespace = "Search.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchResponseDtoFile(string directory)
    {
        var classModel = new ClassModel("SearchResponseDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IReadOnlyList") { GenericTypeParameters = [new TypeModel("SearchResultDto")] }, "Results", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "Array.Empty<SearchResultDto>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TotalCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Skip", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Take", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "ExecutionTimeMs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "SearchResponseDto", directory, CSharp)
        {
            Namespace = "Search.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateIndexDocumentRequestDtoFile(string directory)
    {
        var classModel = new ClassModel("IndexDocumentRequestDto")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var documentIdProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        documentIdProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(documentIdProp);

        var documentProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Document", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        documentProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(documentProp);

        return new CodeFileModel<ClassModel>(classModel, "IndexDocumentRequestDto", directory, CSharp)
        {
            Namespace = "Search.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateIndexInfoDtoFile(string directory)
    {
        var classModel = new ClassModel("IndexInfoDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IndexId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "DocumentCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Active\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "LastUpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "IndexInfoDto", directory, CSharp)
        {
            Namespace = "Search.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateSearchDbContextFile(string directory)
    {
        var classModel = new ClassModel("SearchDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Search.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "SearchDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("SearchDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("SearchIndex")] }, "SearchIndexes", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<SearchIndex>(entity =>
        {
            entity.HasKey(i => i.IndexId);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.DocumentType).IsRequired().HasMaxLength(100);
            entity.HasIndex(i => i.Name).IsUnique();
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "SearchDbContext", directory, CSharp)
        {
            Namespace = "Search.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchRepositoryFile(string directory)
    {
        var classModel = new ClassModel("SearchRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Search.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Search.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Search.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ISearchRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("SearchDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "SearchRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("SearchDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetIndexByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchIndex") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "indexId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.SearchIndexes.FirstOrDefaultAsync(i => i.IndexId == indexId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetIndexByNameAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchIndex") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.SearchIndexes.FirstOrDefaultAsync(i => i.Name == name, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllIndexesAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("SearchIndex")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.SearchIndexes.ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateIndexAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchIndex")] },
            Params =
            [
                new ParamModel { Name = "index", Type = new TypeModel("SearchIndex") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"index.IndexId = Guid.NewGuid();
        await context.SearchIndexes.AddAsync(index, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return index;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateIndexAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "index", Type = new TypeModel("SearchIndex") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"index.LastUpdatedAt = DateTime.UtcNow;
        context.SearchIndexes.Update(index);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteIndexAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var index = await context.SearchIndexes.FindAsync(new object[] { indexId }, cancellationToken);
        if (index != null)
        {
            context.SearchIndexes.Remove(index);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "SearchRepository", directory, CSharp)
        {
            Namespace = "Search.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateSearchServiceFile(string directory)
    {
        var classModel = new ClassModel("SearchService");

        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));
        classModel.Usings.Add(new UsingModel("Search.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Search.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ISearchService"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "logger",
            Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("SearchService")] },
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "SearchService")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("SearchService")] } }],
            Body = new ExpressionModel("this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "SearchAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("SearchResult")] }] },
            Params =
            [
                new ParamModel { Name = "query", Type = new TypeModel("SearchQuery") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Executing search query: {Query}"", query.QueryText);

        // Placeholder implementation - integrate with actual search engine (Elasticsearch, Azure Cognitive Search, etc.)
        await Task.CompletedTask;
        return Array.Empty<SearchResult>();")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetDocumentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SearchResult") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Getting document {DocumentId} from index {IndexName}"", documentId, indexName);

        // Placeholder implementation
        await Task.CompletedTask;
        return null;")
        });

        return new CodeFileModel<ClassModel>(classModel, "SearchService", directory, CSharp)
        {
            Namespace = "Search.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateIndexerFile(string directory)
    {
        var classModel = new ClassModel("Indexer");

        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));
        classModel.Usings.Add(new UsingModel("Search.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IIndexer"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "logger",
            Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("Indexer")] },
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "Indexer")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("Indexer")] } }],
            Body = new ExpressionModel("this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "IndexDocumentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "document", Type = new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Indexing document {DocumentId} in index {IndexName}"", documentId, indexName);

        // Placeholder implementation - integrate with actual search engine
        await Task.CompletedTask;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "IndexDocumentsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documents", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("(string documentId, Dictionary<string, object> document)")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Bulk indexing documents in index {IndexName}"", indexName);

        foreach (var (documentId, document) in documents)
        {
            await IndexDocumentAsync(indexName, documentId, document, cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RemoveDocumentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Removing document {DocumentId} from index {IndexName}"", documentId, indexName);

        // Placeholder implementation
        await Task.CompletedTask;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RemoveDocumentsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "documentIds", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Bulk removing documents from index {IndexName}"", indexName);

        foreach (var documentId in documentIds)
        {
            await RemoveDocumentAsync(indexName, documentId, cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RebuildIndexAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Rebuilding index {IndexName}"", indexName);

        // Placeholder implementation
        await Task.CompletedTask;")
        });

        return new CodeFileModel<ClassModel>(classModel, "Indexer", directory, CSharp)
        {
            Namespace = "Search.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Search.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Search.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Search.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Search.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddSearchInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<SearchDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""SearchDb"") ??
                @""Server=.\SQLEXPRESS;Database=SearchDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IIndexer, Indexer>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateSearchControllerFile(string directory)
    {
        var classModel = new ClassModel("SearchController");

        classModel.Usings.Add(new UsingModel("System.Diagnostics"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Search.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Search.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Search.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "searchService", Type = new TypeModel("ISearchService"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "indexer", Type = new TypeModel("IIndexer"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("ISearchRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("SearchController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "SearchController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "searchService", Type = new TypeModel("ISearchService") },
                new ParamModel { Name = "indexer", Type = new TypeModel("IIndexer") },
                new ParamModel { Name = "repository", Type = new TypeModel("ISearchRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("SearchController")] } }
            ],
            Body = new ExpressionModel(@"this.searchService = searchService;
        this.indexer = indexer;
        this.repository = repository;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        // Search method
        var searchMethod = new MethodModel
        {
            Name = "Search",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("SearchResponseDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SearchRequestDto"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var stopwatch = Stopwatch.StartNew();

        var query = new SearchQuery
        {
            QueryId = Guid.NewGuid(),
            QueryText = request.Query,
            IndexName = request.IndexName,
            Skip = request.Skip,
            Take = request.Take,
            Filters = request.Filters ?? new Dictionary<string, string>(),
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        var results = await searchService.SearchAsync(query, cancellationToken);
        stopwatch.Stop();

        var resultList = results.Select(r => new SearchResultDto
        {
            DocumentId = r.DocumentId,
            IndexName = r.IndexName,
            Score = r.Score,
            Document = r.Document,
            Highlights = r.Highlights
        }).ToList();

        return Ok(new SearchResponseDto
        {
            Results = resultList,
            TotalCount = resultList.Count,
            Skip = request.Skip,
            Take = request.Take,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        });")
        };
        searchMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        searchMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(SearchResponseDto), StatusCodes.Status200OK" });
        classModel.Methods.Add(searchMethod);

        // IndexDocument method
        var indexDocumentMethod = new MethodModel
        {
            Name = "IndexDocument",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "indexName", Type = new TypeModel("string"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "request", Type = new TypeModel("IndexDocumentRequestDto"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (string.IsNullOrWhiteSpace(indexName))
        {
            return BadRequest(""Index name is required"");
        }

        await indexer.IndexDocumentAsync(indexName, request.DocumentId, request.Document, cancellationToken);
        logger.LogInformation(""Document {DocumentId} indexed in {IndexName}"", request.DocumentId, indexName);

        return Created($""/api/search/index/{indexName}/{request.DocumentId}"", null);")
        };
        indexDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"index\"" });
        indexDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status201Created" });
        indexDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(indexDocumentMethod);

        // RemoveDocument method
        var removeDocumentMethod = new MethodModel
        {
            Name = "RemoveDocument",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("string") },
                new ParamModel { Name = "indexName", Type = new TypeModel("string"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"await indexer.RemoveDocumentAsync(indexName, id, cancellationToken);
        logger.LogInformation(""Document {DocumentId} removed from {IndexName}"", id, indexName);

        return NoContent();")
        };
        removeDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"index/{id}\"" });
        removeDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        classModel.Methods.Add(removeDocumentMethod);

        // GetIndexes method
        var getIndexesMethod = new MethodModel
        {
            Name = "GetIndexes",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("IndexInfoDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var indexes = await repository.GetAllIndexesAsync(cancellationToken);
        return Ok(indexes.Select(i => new IndexInfoDto
        {
            IndexId = i.IndexId,
            Name = i.Name,
            DocumentType = i.DocumentType,
            DocumentCount = i.DocumentCount,
            Status = i.Status.ToString(),
            CreatedAt = i.CreatedAt,
            LastUpdatedAt = i.LastUpdatedAt
        }));")
        };
        getIndexesMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"indexes\"" });
        getIndexesMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<IndexInfoDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getIndexesMethod);

        return new CodeFileModel<ClassModel>(classModel, "SearchController", directory, CSharp)
        {
            Namespace = "Search.Api.Controllers"
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

                builder.Services.AddSearchInfrastructure(builder.Configuration);
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
                    "SearchDb": "Server=.\\SQLEXPRESS;Database=SearchDb;Trusted_Connection=True;TrustServerCertificate=True"
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
