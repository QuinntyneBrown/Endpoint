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
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Import;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class ImportArtifactFactory : IImportArtifactFactory
{
    private readonly ILogger<ImportArtifactFactory> logger;

    public ImportArtifactFactory(ILogger<ImportArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Import.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateImportJobFile(entitiesDir));
        project.Files.Add(CreateImportMappingFile(entitiesDir));
        project.Files.Add(CreateImportErrorFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIImportRepositoryFile(interfacesDir));
        project.Files.Add(CreateIImportServiceFile(interfacesDir));
        project.Files.Add(CreateIDataValidatorFile(interfacesDir));

        // Events
        project.Files.Add(CreateImportStartedEventFile(eventsDir));
        project.Files.Add(CreateImportCompletedEventFile(eventsDir));
        project.Files.Add(CreateImportFailedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateImportJobDtoFile(dtosDir));
        project.Files.Add(CreateUploadImportRequestFile(dtosDir));
        project.Files.Add(CreateImportMappingDtoFile(dtosDir));
        project.Files.Add(CreateCreateMappingRequestFile(dtosDir));
        project.Files.Add(CreateImportErrorDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Import.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateImportDbContextFile(dataDir));
        project.Files.Add(CreateImportRepositoryFile(repositoriesDir));
        project.Files.Add(CreateImportServiceFile(servicesDir));
        project.Files.Add(CreateDefaultDataValidatorFile(servicesDir));
        project.Files.Add(CreateConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Import.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateImportControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files - Entities

    private static FileModel CreateImportJobFile(string directory)
    {
        // Contains enum - keep as FileModel
        return new FileModel("ImportJob", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Entities;

                public class ImportJob
                {
                    public Guid ImportJobId { get; set; }
                    public Guid UserId { get; set; }
                    public Guid? MappingId { get; set; }
                    public required string Name { get; set; }
                    public required string SourceFileName { get; set; }
                    public required string SourceType { get; set; }
                    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
                    public int TotalRecords { get; set; }
                    public int ProcessedRecords { get; set; }
                    public int SuccessfulRecords { get; set; }
                    public int FailedRecords { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAt { get; set; }
                    public DateTime? CompletedAt { get; set; }
                    public ImportMapping? Mapping { get; set; }
                    public ICollection<ImportError> Errors { get; set; } = new List<ImportError>();
                }

                public enum ImportJobStatus { Pending, Validating, Processing, Completed, Failed, Cancelled }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateImportMappingFile(string directory)
    {
        var classModel = new ClassModel("ImportMapping");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MappingId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "SourceType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TargetEntity", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "MappingDefinition", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "ImportMapping", directory, CSharp)
        {
            Namespace = "Import.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateImportErrorFile(string directory)
    {
        var classModel = new ClassModel("ImportError");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportErrorId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ImportJob"), "ImportJob", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RowNumber", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ColumnName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ErrorCode", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OriginalValue", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ImportError", directory, CSharp)
        {
            Namespace = "Import.Core.Entities"
        };
    }

    #endregion

    #region Core Layer Files - Interfaces

    private static CodeFileModel<InterfaceModel> CreateIImportRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IImportRepository");

        interfaceModel.Usings.Add(new UsingModel("Import.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJob") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportJob")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportJob")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJob")] },
            Params =
            [
                new ParamModel { Name = "importJob", Type = new TypeModel("ImportJob") },
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
                new ParamModel { Name = "importJob", Type = new TypeModel("ImportJob") },
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
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMappingByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportMapping") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "mappingId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMappingsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportMapping")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddMappingAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportMapping")] },
            Params =
            [
                new ParamModel { Name = "mapping", Type = new TypeModel("ImportMapping") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddErrorAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "error", Type = new TypeModel("ImportError") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetErrorsByJobIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportError")] }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IImportRepository", directory, CSharp)
        {
            Namespace = "Import.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIImportServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IImportService");

        interfaceModel.Usings.Add(new UsingModel("Import.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UploadAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJobDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("UploadImportRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetJobByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJobDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetJobsByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateMappingAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportMappingDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateMappingRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMappingsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportMappingDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetJobErrorsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportErrorDto")] }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IImportService", directory, CSharp)
        {
            Namespace = "Import.Core.Interfaces"
        };
    }

    private static FileModel CreateIDataValidatorFile(string directory)
    {
        // Contains both interface and class - keep as FileModel
        return new FileModel("IDataValidator", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Import.Core.Entities;

                namespace Import.Core.Interfaces;

                public interface IDataValidator
                {
                    Task<ValidationResult> ValidateAsync(ImportJob job, Stream dataStream, CancellationToken cancellationToken = default);
                    bool SupportsSourceType(string sourceType);
                }

                public class ValidationResult
                {
                    public bool IsValid { get; set; }
                    public int TotalRecords { get; set; }
                    public ICollection<ImportError> Errors { get; set; } = new List<ImportError>();
                }
                """
        };
    }

    #endregion

    #region Core Layer Files - Events

    private static CodeFileModel<ClassModel> CreateImportStartedEventFile(string directory)
    {
        var classModel = new ClassModel("ImportStartedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "SourceFileName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "SourceType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TotalRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StartedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ImportStartedEvent", directory, CSharp)
        {
            Namespace = "Import.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateImportCompletedEventFile(string directory)
    {
        var classModel = new ClassModel("ImportCompletedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TotalRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "SuccessfulRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "FailedRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ImportCompletedEvent", directory, CSharp)
        {
            Namespace = "Import.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateImportFailedEventFile(string directory)
    {
        var classModel = new ClassModel("ImportFailedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Reason", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "ErrorCount", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "FailedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ImportFailedEvent", directory, CSharp)
        {
            Namespace = "Import.Core.Events"
        };
    }

    #endregion

    #region Core Layer Files - DTOs

    private static CodeFileModel<ClassModel> CreateImportJobDtoFile(string directory)
    {
        var classModel = new ClassModel("ImportJobDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "SourceFileName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "SourceType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TotalRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "ProcessedRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "SuccessfulRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "FailedRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "StartedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "ImportJobDto", directory, CSharp)
        {
            Namespace = "Import.Core.DTOs"
        };
    }

    private static FileModel CreateUploadImportRequestFile(string directory)
    {
        // Contains [Required] attributes - keep as FileModel
        return new FileModel("UploadImportRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Import.Core.DTOs;

                public sealed class UploadImportRequest
                {
                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string SourceFileName { get; init; }

                    [Required]
                    public required string SourceType { get; init; }

                    public Guid? MappingId { get; init; }

                    public required Stream FileStream { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateImportMappingDtoFile(string directory)
    {
        var classModel = new ClassModel("ImportMappingDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MappingId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "SourceType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TargetEntity", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "ImportMappingDto", directory, CSharp)
        {
            Namespace = "Import.Core.DTOs"
        };
    }

    private static FileModel CreateCreateMappingRequestFile(string directory)
    {
        // Contains [Required] attributes - keep as FileModel
        return new FileModel("CreateMappingRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Import.Core.DTOs;

                public sealed class CreateMappingRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string Description { get; init; }

                    [Required]
                    public required string SourceType { get; init; }

                    [Required]
                    public required string TargetEntity { get; init; }

                    [Required]
                    public required string MappingDefinition { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateImportErrorDtoFile(string directory)
    {
        var classModel = new ClassModel("ImportErrorDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ImportErrorId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RowNumber", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ColumnName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ErrorCode", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OriginalValue", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "ImportErrorDto", directory, CSharp)
        {
            Namespace = "Import.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateImportDbContextFile(string directory)
    {
        var classModel = new ClassModel("ImportDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Import.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "ImportDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("ImportDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ImportJob")] }, "ImportJobs", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<ImportJob>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ImportMapping")] }, "ImportMappings", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<ImportMapping>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ImportError")] }, "ImportErrors", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<ImportError>()")]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.HasKey(e => e.ImportJobId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SourceFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SourceType).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Mapping).WithMany().HasForeignKey(e => e.MappingId);
            entity.HasMany(e => e.Errors).WithOne(e => e.ImportJob).HasForeignKey(e => e.ImportJobId);
        });

        modelBuilder.Entity<ImportMapping>(entity =>
        {
            entity.HasKey(m => m.MappingId);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(m => m.Name).IsUnique();
        });

        modelBuilder.Entity<ImportError>(entity =>
        {
            entity.HasKey(e => e.ImportErrorId);
            entity.Property(e => e.ErrorCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(1000);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "ImportDbContext", directory, CSharp)
        {
            Namespace = "Import.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateImportRepositoryFile(string directory)
    {
        var classModel = new ClassModel("ImportRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Import.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Import.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Import.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IImportRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("ImportDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "ImportRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("ImportDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJob") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ImportJobs.Include(e => e.Mapping).Include(e => e.Errors).FirstOrDefaultAsync(e => e.ImportJobId == importJobId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportJob")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ImportJobs.Include(e => e.Mapping).Where(e => e.UserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportJob")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ImportJobs.Include(e => e.Mapping).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJob")] },
            Params =
            [
                new ParamModel { Name = "importJob", Type = new TypeModel("ImportJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"importJob.ImportJobId = Guid.NewGuid();
        await context.ImportJobs.AddAsync(importJob, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return importJob;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "importJob", Type = new TypeModel("ImportJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.ImportJobs.Update(importJob);
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
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var importJob = await context.ImportJobs.FindAsync(new object[] { importJobId }, cancellationToken);
        if (importJob != null)
        {
            context.ImportJobs.Remove(importJob);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMappingByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportMapping") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "mappingId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ImportMappings.FirstOrDefaultAsync(m => m.MappingId == mappingId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMappingsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportMapping")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ImportMappings.Where(m => m.IsActive).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddMappingAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportMapping")] },
            Params =
            [
                new ParamModel { Name = "mapping", Type = new TypeModel("ImportMapping") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"mapping.MappingId = Guid.NewGuid();
        await context.ImportMappings.AddAsync(mapping, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return mapping;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddErrorAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "error", Type = new TypeModel("ImportError") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"error.ImportErrorId = Guid.NewGuid();
        await context.ImportErrors.AddAsync(error, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetErrorsByJobIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportError")] }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ImportErrors.Where(e => e.ImportJobId == importJobId).OrderBy(e => e.RowNumber).ToListAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ImportRepository", directory, CSharp)
        {
            Namespace = "Import.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateImportServiceFile(string directory)
    {
        var classModel = new ClassModel("ImportService");

        classModel.Usings.Add(new UsingModel("Import.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Import.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Import.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IImportService"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IImportRepository"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "dataValidator", Type = new TypeModel("IDataValidator"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "ImportService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("IImportRepository") },
                new ParamModel { Name = "dataValidator", Type = new TypeModel("IDataValidator") }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.dataValidator = dataValidator;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "UploadAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJobDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("UploadImportRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var importJob = new ImportJob
        {
            UserId = request.UserId,
            Name = request.Name,
            SourceFileName = request.SourceFileName,
            SourceType = request.SourceType,
            MappingId = request.MappingId,
            Status = ImportJobStatus.Validating,
            StartedAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(importJob, cancellationToken);

        // Validate the data
        try
        {
            var validationResult = await dataValidator.ValidateAsync(created, request.FileStream, cancellationToken);
            created.TotalRecords = validationResult.TotalRecords;

            if (!validationResult.IsValid)
            {
                created.Status = ImportJobStatus.Failed;
                created.FailedRecords = validationResult.Errors.Count;
                foreach (var error in validationResult.Errors)
                {
                    error.ImportJobId = created.ImportJobId;
                    await repository.AddErrorAsync(error, cancellationToken);
                }
            }
            else
            {
                created.Status = ImportJobStatus.Processing;
                // Processing would continue asynchronously
            }

            await repository.UpdateAsync(created, cancellationToken);
        }
        catch
        {
            created.Status = ImportJobStatus.Failed;
            await repository.UpdateAsync(created, cancellationToken);
            throw;
        }

        return new ImportJobDto
        {
            ImportJobId = created.ImportJobId,
            UserId = created.UserId,
            Name = created.Name,
            SourceFileName = created.SourceFileName,
            SourceType = created.SourceType,
            Status = created.Status.ToString(),
            TotalRecords = created.TotalRecords,
            ProcessedRecords = created.ProcessedRecords,
            SuccessfulRecords = created.SuccessfulRecords,
            FailedRecords = created.FailedRecords,
            CreatedAt = created.CreatedAt,
            StartedAt = created.StartedAt,
            CompletedAt = created.CompletedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetJobByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportJobDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var importJob = await repository.GetByIdAsync(importJobId, cancellationToken);
        if (importJob == null) return null;

        return new ImportJobDto
        {
            ImportJobId = importJob.ImportJobId,
            UserId = importJob.UserId,
            Name = importJob.Name,
            SourceFileName = importJob.SourceFileName,
            SourceType = importJob.SourceType,
            Status = importJob.Status.ToString(),
            TotalRecords = importJob.TotalRecords,
            ProcessedRecords = importJob.ProcessedRecords,
            SuccessfulRecords = importJob.SuccessfulRecords,
            FailedRecords = importJob.FailedRecords,
            CreatedAt = importJob.CreatedAt,
            StartedAt = importJob.StartedAt,
            CompletedAt = importJob.CompletedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetJobsByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var importJobs = await repository.GetByUserIdAsync(userId, cancellationToken);
        return importJobs.Select(e => new ImportJobDto
        {
            ImportJobId = e.ImportJobId,
            UserId = e.UserId,
            Name = e.Name,
            SourceFileName = e.SourceFileName,
            SourceType = e.SourceType,
            Status = e.Status.ToString(),
            TotalRecords = e.TotalRecords,
            ProcessedRecords = e.ProcessedRecords,
            SuccessfulRecords = e.SuccessfulRecords,
            FailedRecords = e.FailedRecords,
            CreatedAt = e.CreatedAt,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateMappingAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ImportMappingDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateMappingRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var mapping = new ImportMapping
        {
            Name = request.Name,
            Description = request.Description,
            SourceType = request.SourceType,
            TargetEntity = request.TargetEntity,
            MappingDefinition = request.MappingDefinition
        };

        var created = await repository.AddMappingAsync(mapping, cancellationToken);

        return new ImportMappingDto
        {
            MappingId = created.MappingId,
            Name = created.Name,
            Description = created.Description,
            SourceType = created.SourceType,
            TargetEntity = created.TargetEntity,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMappingsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportMappingDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var mappings = await repository.GetMappingsAsync(cancellationToken);
        return mappings.Select(m => new ImportMappingDto
        {
            MappingId = m.MappingId,
            Name = m.Name,
            Description = m.Description,
            SourceType = m.SourceType,
            TargetEntity = m.TargetEntity,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetJobErrorsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportErrorDto")] }] },
            Params =
            [
                new ParamModel { Name = "importJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var errors = await repository.GetErrorsByJobIdAsync(importJobId, cancellationToken);
        return errors.Select(e => new ImportErrorDto
        {
            ImportErrorId = e.ImportErrorId,
            RowNumber = e.RowNumber,
            ColumnName = e.ColumnName,
            ErrorCode = e.ErrorCode,
            ErrorMessage = e.ErrorMessage,
            OriginalValue = e.OriginalValue
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "ImportService", directory, CSharp)
        {
            Namespace = "Import.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateDefaultDataValidatorFile(string directory)
    {
        var classModel = new ClassModel("DefaultDataValidator");

        classModel.Usings.Add(new UsingModel("Import.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Import.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDataValidator"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "SupportedSourceTypes",
            Type = new TypeModel("HashSet") { GenericTypeParameters = [new TypeModel("string")] },
            AccessModifier = AccessModifier.Private,
            Static = true,
            Readonly = true,
            DefaultValue = "new(StringComparer.OrdinalIgnoreCase) { \"csv\", \"xlsx\", \"json\", \"xml\" }"
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ValidateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ValidationResult")] },
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("ImportJob") },
                new ParamModel { Name = "dataStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Simulate validation
        await Task.Delay(100, cancellationToken);

        var result = new ValidationResult
        {
            IsValid = true,
            TotalRecords = 0,
            Errors = new List<ImportError>()
        };

        // Basic validation - in production this would parse the file and validate each record
        if (dataStream.Length == 0)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ImportJobId = job.ImportJobId,
                RowNumber = 0,
                ErrorCode = ""EMPTY_FILE"",
                ErrorMessage = ""The uploaded file is empty""
            });
        }

        return result;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SupportsSourceType",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("bool"),
            Params =
            [
                new ParamModel { Name = "sourceType", Type = new TypeModel("string") }
            ],
            Body = new ExpressionModel("return SupportedSourceTypes.Contains(sourceType);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DefaultDataValidator", directory, CSharp)
        {
            Namespace = "Import.Infrastructure.Services"
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
        classModel.Usings.Add(new UsingModel("Import.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Import.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Import.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Import.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddImportInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<ImportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""ImportDb"") ??
                @""Server=.\SQLEXPRESS;Database=ImportDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IImportRepository, ImportRepository>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IDataValidator, DefaultDataValidator>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateImportControllerFile(string directory)
    {
        var classModel = new ClassModel("ImportController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Import.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Import.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "service", Type = new TypeModel("IImportService"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "ImportController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "service", Type = new TypeModel("IImportService") }],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var uploadMethod = new MethodModel
        {
            Name = "Upload",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ImportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid"), Attribute = "[FromForm]" },
                new ParamModel { Name = "name", Type = new TypeModel("string"), Attribute = "[FromForm]" },
                new ParamModel { Name = "sourceType", Type = new TypeModel("string"), Attribute = "[FromForm]" },
                new ParamModel { Name = "mappingId", Type = new TypeModel("Guid") { Nullable = true }, Attribute = "[FromForm]" },
                new ParamModel { Name = "file", Type = new TypeModel("IFormFile") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (file == null || file.Length == 0)
            return BadRequest(""No file uploaded"");

        using var stream = file.OpenReadStream();
        var request = new UploadImportRequest
        {
            UserId = userId,
            Name = name,
            SourceFileName = file.FileName,
            SourceType = sourceType,
            MappingId = mappingId,
            FileStream = stream
        };

        var importJob = await service.UploadAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetJob), new { id = importJob.ImportJobId }, importJob);")
        };
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"upload\"" });
        classModel.Methods.Add(uploadMethod);

        var getJobMethod = new MethodModel
        {
            Name = "GetJob",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ImportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var importJob = await service.GetJobByIdAsync(id, cancellationToken);
        if (importJob == null) return NotFound();
        return Ok(importJob);")
        };
        getJobMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"jobs/{id:guid}\"" });
        classModel.Methods.Add(getJobMethod);

        var getJobErrorsMethod = new MethodModel
        {
            Name = "GetJobErrors",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportErrorDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var errors = await service.GetJobErrorsAsync(id, cancellationToken);
        return Ok(errors);")
        };
        getJobErrorsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"jobs/{id:guid}/errors\"" });
        classModel.Methods.Add(getJobErrorsMethod);

        var createMappingMethod = new MethodModel
        {
            Name = "CreateMapping",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ImportMappingDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateMappingRequest"), Attribute = "[FromBody]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var mapping = await service.CreateMappingAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMappings), mapping);")
        };
        createMappingMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"mappings\"" });
        classModel.Methods.Add(createMappingMethod);

        var getMappingsMethod = new MethodModel
        {
            Name = "GetMappings",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ImportMappingDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var mappings = await service.GetMappingsAsync(cancellationToken);
        return Ok(mappings);")
        };
        getMappingsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"mappings\"" });
        classModel.Methods.Add(getMappingsMethod);

        return new CodeFileModel<ClassModel>(classModel, "ImportController", directory, CSharp)
        {
            Namespace = "Import.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Top-level statements - keep as FileModel
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddImportInfrastructure(builder.Configuration);
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
        // JSON file - keep as FileModel
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "ImportDb": "Server=.\\SQLEXPRESS;Database=ImportDb;Trusted_Connection=True;TrustServerCertificate=True"
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
