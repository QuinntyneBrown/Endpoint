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

namespace Endpoint.Engineering.Microservices.Media;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Media microservice artifacts according to media-microservice.spec.md.
/// </summary>
public class MediaArtifactFactory : IMediaArtifactFactory
{
    private readonly ILogger<MediaArtifactFactory> logger;

    public MediaArtifactFactory(ILogger<MediaArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Media.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateMediaFileEntityFile(entitiesDir));
        project.Files.Add(CreateThumbnailEntityFile(entitiesDir));
        project.Files.Add(CreateTranscodingJobEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIMediaRepositoryFile(interfacesDir));
        project.Files.Add(CreateIImageProcessorFile(interfacesDir));
        project.Files.Add(CreateIVideoTranscoderFile(interfacesDir));

        // Events
        project.Files.Add(CreateMediaUploadedEventFile(eventsDir));
        project.Files.Add(CreateThumbnailGeneratedEventFile(eventsDir));
        project.Files.Add(CreateTranscodingCompletedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateMediaFileDtoFile(dtosDir));
        project.Files.Add(CreateThumbnailDtoFile(dtosDir));
        project.Files.Add(CreateUploadRequestFile(dtosDir));
        project.Files.Add(CreateUploadResponseFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Media.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateMediaDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateMediaFileConfigurationFile(configurationsDir));
        project.Files.Add(CreateThumbnailConfigurationFile(configurationsDir));
        project.Files.Add(CreateTranscodingJobConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateMediaRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateImageProcessorFile(servicesDir));
        project.Files.Add(CreateVideoTranscoderFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Media.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateMediaControllerFile(controllersDir));

        // Program.cs
        project.Files.Add(CreateProgramFile(project.Directory));

        // appsettings.json
        project.Files.Add(CreateAppSettingsFile(project.Directory));
        project.Files.Add(CreateAppSettingsDevelopmentFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<InterfaceModel> CreateIAggregateRootFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IAggregateRoot");

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IAggregateRoot", directory, CSharp)
        {
            Namespace = "Media.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateMediaFileEntityFile(string directory)
    {
        var classModel = new ClassModel("MediaFile");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MediaFileId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "StoragePath", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OriginalFileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "UploadedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "UploadedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsProcessed", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ProcessedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Thumbnail")] }, "Thumbnails", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Thumbnail>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("TranscodingJob")] }, "TranscodingJobs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<TranscodingJob>()" });

        return new CodeFileModel<ClassModel>(classModel, "MediaFile", directory, CSharp)
        {
            Namespace = "Media.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateThumbnailEntityFile(string directory)
    {
        var classModel = new ClassModel("Thumbnail");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ThumbnailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MediaFileId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("MediaFile"), "MediaFile", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "StoragePath", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Width", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Height", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Format", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "GeneratedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "Thumbnail", directory, CSharp)
        {
            Namespace = "Media.Core.Entities"
        };
    }

    private static FileModel CreateTranscodingJobEntityFile(string directory)
    {
        // Keep as FileModel because it contains an enum which can't be expressed with syntax models
        return new FileModel("TranscodingJob", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.Entities;

                /// <summary>
                /// Entity representing a video transcoding job.
                /// </summary>
                public class TranscodingJob
                {
                    public Guid TranscodingJobId { get; set; }

                    public Guid MediaFileId { get; set; }

                    public MediaFile MediaFile { get; set; } = null!;

                    public required string TargetFormat { get; set; }

                    public required string TargetResolution { get; set; }

                    public TranscodingStatus Status { get; set; } = TranscodingStatus.Pending;

                    public string? OutputPath { get; set; }

                    public int Progress { get; set; }

                    public string? ErrorMessage { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? StartedAt { get; set; }

                    public DateTime? CompletedAt { get; set; }
                }

                /// <summary>
                /// Status of a transcoding job.
                /// </summary>
                public enum TranscodingStatus
                {
                    Pending,
                    InProgress,
                    Completed,
                    Failed,
                    Cancelled
                }
                """
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIDomainEventFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IDomainEvent");

        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IDomainEvent", directory, CSharp)
        {
            Namespace = "Media.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIMediaRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IMediaRepository");

        interfaceModel.Usings.Add(new UsingModel("Media.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("MediaFile") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdWithThumbnailsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("MediaFile") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("MediaFile")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUploaderAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("MediaFile")] }] },
            Params =
            [
                new ParamModel { Name = "uploaderId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("MediaFile")] },
            Params =
            [
                new ParamModel { Name = "mediaFile", Type = new TypeModel("MediaFile") },
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
                new ParamModel { Name = "mediaFile", Type = new TypeModel("MediaFile") },
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
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddThumbnailAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Thumbnail")] },
            Params =
            [
                new ParamModel { Name = "thumbnail", Type = new TypeModel("Thumbnail") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetThumbnailsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Thumbnail")] }] },
            Params =
            [
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddTranscodingJobAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TranscodingJob")] },
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("TranscodingJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateTranscodingJobAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("TranscodingJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IMediaRepository", directory, CSharp)
        {
            Namespace = "Media.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIImageProcessorFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IImageProcessor");

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateThumbnailAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "width", Type = new TypeModel("int") },
                new ParamModel { Name = "height", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ResizeImageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "maxWidth", Type = new TypeModel("int") },
                new ParamModel { Name = "maxHeight", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetImageDimensionsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("(int Width, int Height)")] },
            Params =
            [
                new ParamModel { Name = "imagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ConvertFormatAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "targetFormat", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IImageProcessor", directory, CSharp)
        {
            Namespace = "Media.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIVideoTranscoderFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IVideoTranscoder");

        interfaceModel.Usings.Add(new UsingModel("Media.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "TranscodeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "targetFormat", Type = new TypeModel("string") },
                new ParamModel { Name = "resolution", Type = new TypeModel("string") },
                new ParamModel { Name = "progress", Type = new TypeModel("IProgress") { GenericTypeParameters = [new TypeModel("int")], Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ExtractThumbnailAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "videoPath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "timestamp", Type = new TypeModel("TimeSpan") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetDurationAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TimeSpan")] },
            Params =
            [
                new ParamModel { Name = "videoPath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetResolutionAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("(int Width, int Height)")] },
            Params =
            [
                new ParamModel { Name = "videoPath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "IsFormatSupported",
            Interface = true,
            ReturnType = new TypeModel("bool"),
            Params =
            [
                new ParamModel { Name = "format", Type = new TypeModel("string") }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IVideoTranscoder", directory, CSharp)
        {
            Namespace = "Media.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateMediaUploadedEventFile(string directory)
    {
        var classModel = new ClassModel("MediaUploadedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "UploadedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "MediaUploadedEvent", directory, CSharp)
        {
            Namespace = "Media.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateThumbnailGeneratedEventFile(string directory)
    {
        var classModel = new ClassModel("ThumbnailGeneratedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ThumbnailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Width", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Height", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Format", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "ThumbnailGeneratedEvent", directory, CSharp)
        {
            Namespace = "Media.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateTranscodingCompletedEventFile(string directory)
    {
        var classModel = new ClassModel("TranscodingCompletedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TranscodingJobId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TargetFormat", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TargetResolution", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "OutputPath", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("TimeSpan"), "Duration", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TranscodingCompletedEvent", directory, CSharp)
        {
            Namespace = "Media.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateMediaFileDtoFile(string directory)
    {
        var classModel = new ClassModel("MediaFileDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MediaFileId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OriginalFileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "UploadedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "UploadedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsProcessed", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IReadOnlyList") { GenericTypeParameters = [new TypeModel("ThumbnailDto")] }, "Thumbnails", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "Array.Empty<ThumbnailDto>()" });

        return new CodeFileModel<ClassModel>(classModel, "MediaFileDto", directory, CSharp)
        {
            Namespace = "Media.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateThumbnailDtoFile(string directory)
    {
        var classModel = new ClassModel("ThumbnailDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ThumbnailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Width", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Height", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Format", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Url", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "ThumbnailDto", directory, CSharp)
        {
            Namespace = "Media.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateUploadRequestFile(string directory)
    {
        var classModel = new ClassModel("UploadRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "GenerateThumbnail", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "Transcode", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "TargetFormat", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "TargetResolution", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "UploadRequest", directory, CSharp)
        {
            Namespace = "Media.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateUploadResponseFile(string directory)
    {
        var classModel = new ClassModel("UploadResponse")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MediaFileId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Url", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "UploadResponse", directory, CSharp)
        {
            Namespace = "Media.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateMediaDbContextFile(string directory)
    {
        var classModel = new ClassModel("MediaDbContext");

        classModel.Usings.Add(new UsingModel("Media.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "MediaDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("MediaDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("MediaFile")] }, "MediaFiles", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Thumbnail")] }, "Thumbnails", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("TranscodingJob")] }, "TranscodingJobs", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "MediaDbContext", directory, CSharp)
        {
            Namespace = "Media.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateMediaFileConfigurationFile(string directory)
    {
        var classModel = new ClassModel("MediaFileConfiguration");

        classModel.Usings.Add(new UsingModel("Media.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("MediaFile")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("MediaFile")] } }],
            Body = new ExpressionModel(@"builder.HasKey(m => m.MediaFileId);

        builder.Property(m => m.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.OriginalFileName)
            .HasMaxLength(255);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.UploadedAt)
            .IsRequired();

        builder.HasIndex(m => m.UploadedBy);
        builder.HasIndex(m => m.ContentType);")
        });

        return new CodeFileModel<ClassModel>(classModel, "MediaFileConfiguration", directory, CSharp)
        {
            Namespace = "Media.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateThumbnailConfigurationFile(string directory)
    {
        var classModel = new ClassModel("ThumbnailConfiguration");

        classModel.Usings.Add(new UsingModel("Media.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Thumbnail")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Thumbnail")] } }],
            Body = new ExpressionModel(@"builder.HasKey(t => t.ThumbnailId);

        builder.Property(t => t.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Format)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(t => t.MediaFile)
            .WithMany(m => m.Thumbnails)
            .HasForeignKey(t => t.MediaFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.MediaFileId);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ThumbnailConfiguration", directory, CSharp)
        {
            Namespace = "Media.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateTranscodingJobConfigurationFile(string directory)
    {
        var classModel = new ClassModel("TranscodingJobConfiguration");

        classModel.Usings.Add(new UsingModel("Media.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("TranscodingJob")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("TranscodingJob")] } }],
            Body = new ExpressionModel(@"builder.HasKey(j => j.TranscodingJobId);

        builder.Property(j => j.TargetFormat)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(j => j.TargetResolution)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(j => j.OutputPath)
            .HasMaxLength(1000);

        builder.Property(j => j.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne(j => j.MediaFile)
            .WithMany(m => m.TranscodingJobs)
            .HasForeignKey(j => j.MediaFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => j.MediaFileId);
        builder.HasIndex(j => j.Status);")
        });

        return new CodeFileModel<ClassModel>(classModel, "TranscodingJobConfiguration", directory, CSharp)
        {
            Namespace = "Media.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateMediaRepositoryFile(string directory)
    {
        var classModel = new ClassModel("MediaRepository");

        classModel.Usings.Add(new UsingModel("Media.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Media.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("IMediaRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("MediaDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "MediaRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("MediaDbContext") }],
            Body = new ExpressionModel("this.context = context ?? throw new ArgumentNullException(nameof(context));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("MediaFile") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.MediaFiles
            .FirstOrDefaultAsync(m => m.MediaFileId == mediaFileId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdWithThumbnailsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("MediaFile") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.MediaFiles
            .Include(m => m.Thumbnails)
            .FirstOrDefaultAsync(m => m.MediaFileId == mediaFileId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("MediaFile")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.MediaFiles
            .Include(m => m.Thumbnails)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUploaderAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("MediaFile")] }] },
            Params =
            [
                new ParamModel { Name = "uploaderId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.MediaFiles
            .Include(m => m.Thumbnails)
            .Where(m => m.UploadedBy == uploaderId)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("MediaFile")] },
            Params =
            [
                new ParamModel { Name = "mediaFile", Type = new TypeModel("MediaFile") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"mediaFile.MediaFileId = Guid.NewGuid();
        mediaFile.UploadedAt = DateTime.UtcNow;
        await context.MediaFiles.AddAsync(mediaFile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return mediaFile;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "mediaFile", Type = new TypeModel("MediaFile") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.MediaFiles.Update(mediaFile);
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
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var mediaFile = await context.MediaFiles.FindAsync(new object[] { mediaFileId }, cancellationToken);
        if (mediaFile != null)
        {
            context.MediaFiles.Remove(mediaFile);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddThumbnailAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Thumbnail")] },
            Params =
            [
                new ParamModel { Name = "thumbnail", Type = new TypeModel("Thumbnail") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"thumbnail.ThumbnailId = Guid.NewGuid();
        thumbnail.GeneratedAt = DateTime.UtcNow;
        await context.Thumbnails.AddAsync(thumbnail, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return thumbnail;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetThumbnailsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Thumbnail")] }] },
            Params =
            [
                new ParamModel { Name = "mediaFileId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Thumbnails
            .Where(t => t.MediaFileId == mediaFileId)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddTranscodingJobAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("TranscodingJob")] },
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("TranscodingJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"job.TranscodingJobId = Guid.NewGuid();
        job.CreatedAt = DateTime.UtcNow;
        await context.TranscodingJobs.AddAsync(job, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return job;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateTranscodingJobAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("TranscodingJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.TranscodingJobs.Update(job);
        await context.SaveChangesAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "MediaRepository", directory, CSharp)
        {
            Namespace = "Media.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateImageProcessorFile(string directory)
    {
        var classModel = new ClassModel("ImageProcessor");

        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IImageProcessor"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "logger",
            Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("ImageProcessor")] },
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "ImageProcessor")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("ImageProcessor")] } }],
            Body = new ExpressionModel("this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateThumbnailAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "width", Type = new TypeModel("int") },
                new ParamModel { Name = "height", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Generating thumbnail for {SourcePath} at {Width}x{Height}"", sourcePath, width, height);

        // Implementation would use an image processing library like ImageSharp or SkiaSharp
        // This is a placeholder that would be replaced with actual implementation

        return Task.FromResult(outputPath);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ResizeImageAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "maxWidth", Type = new TypeModel("int") },
                new ParamModel { Name = "maxHeight", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Resizing image {SourcePath} to max {MaxWidth}x{MaxHeight}"", sourcePath, maxWidth, maxHeight);

        // Implementation would use an image processing library
        return Task.FromResult(outputPath);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetImageDimensionsAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("(int Width, int Height)")] },
            Params =
            [
                new ParamModel { Name = "imagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Getting dimensions for {ImagePath}"", imagePath);

        // Implementation would read image metadata
        return Task.FromResult((1920, 1080));")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ConvertFormatAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "sourcePath", Type = new TypeModel("string") },
                new ParamModel { Name = "outputPath", Type = new TypeModel("string") },
                new ParamModel { Name = "targetFormat", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Converting {SourcePath} to {TargetFormat}"", sourcePath, targetFormat);

        // Implementation would convert image format
        return Task.FromResult(outputPath);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ImageProcessor", directory, CSharp)
        {
            Namespace = "Media.Infrastructure.Services"
        };
    }

    private static FileModel CreateVideoTranscoderFile(string directory)
    {
        // Keep as FileModel because it has a static HashSet field initializer that's complex
        return new FileModel("VideoTranscoder", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace Media.Infrastructure.Services;

                /// <summary>
                /// Video transcoding service implementation.
                /// </summary>
                public class VideoTranscoder : IVideoTranscoder
                {
                    private readonly ILogger<VideoTranscoder> logger;
                    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
                    {
                        "mp4", "webm", "mkv", "avi", "mov", "wmv", "flv"
                    };

                    public VideoTranscoder(ILogger<VideoTranscoder> logger)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public Task<string> TranscodeAsync(string sourcePath, string outputPath, string targetFormat, string resolution, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Transcoding {SourcePath} to {TargetFormat} at {Resolution}", sourcePath, targetFormat, resolution);

                        // Implementation would use FFmpeg or similar library
                        // This is a placeholder that would be replaced with actual implementation

                        progress?.Report(100);
                        return Task.FromResult(outputPath);
                    }

                    public Task<string> ExtractThumbnailAsync(string videoPath, string outputPath, TimeSpan timestamp, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Extracting thumbnail from {VideoPath} at {Timestamp}", videoPath, timestamp);

                        // Implementation would use FFmpeg to extract frame
                        return Task.FromResult(outputPath);
                    }

                    public Task<TimeSpan> GetDurationAsync(string videoPath, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Getting duration for {VideoPath}", videoPath);

                        // Implementation would read video metadata
                        return Task.FromResult(TimeSpan.FromMinutes(5));
                    }

                    public Task<(int Width, int Height)> GetResolutionAsync(string videoPath, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Getting resolution for {VideoPath}", videoPath);

                        // Implementation would read video metadata
                        return Task.FromResult((1920, 1080));
                    }

                    public bool IsFormatSupported(string format)
                    {
                        return SupportedFormats.Contains(format);
                    }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Media.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Media.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Media.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddMediaInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<MediaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(""MediaDb"") ??
                @""Server=.\SQLEXPRESS;Database=MediaDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IImageProcessor, ImageProcessor>();
        services.AddScoped<IVideoTranscoder, VideoTranscoder>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateMediaControllerFile(string directory)
    {
        var classModel = new ClassModel("MediaController");

        classModel.Usings.Add(new UsingModel("Media.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Media.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Media.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "mediaRepository", Type = new TypeModel("IMediaRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "imageProcessor", Type = new TypeModel("IImageProcessor"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "videoTranscoder", Type = new TypeModel("IVideoTranscoder"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("MediaController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "MediaController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "mediaRepository", Type = new TypeModel("IMediaRepository") },
                new ParamModel { Name = "imageProcessor", Type = new TypeModel("IImageProcessor") },
                new ParamModel { Name = "videoTranscoder", Type = new TypeModel("IVideoTranscoder") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("MediaController")] } }
            ],
            Body = new ExpressionModel(@"this.mediaRepository = mediaRepository;
        this.imageProcessor = imageProcessor;
        this.videoTranscoder = videoTranscoder;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var uploadMethod = new MethodModel
        {
            Name = "Upload",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("UploadResponse")] }] },
            Params =
            [
                new ParamModel { Name = "file", Type = new TypeModel("IFormFile") },
                new ParamModel { Name = "request", Type = new TypeModel("UploadRequest") { Nullable = true }, Attribute = new AttributeModel() { Name = "FromForm" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = ""No file provided"" });
        }

        // Generate unique file name and storage path
        var fileName = $""{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"";
        var storagePath = Path.Combine(""uploads"", DateTime.UtcNow.ToString(""yyyy/MM/dd""), fileName);

        // Create the directory if it doesn't exist
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), ""wwwroot"", storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // Save the file
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var mediaFile = new MediaFile
        {
            FileName = fileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            StoragePath = storagePath,
            OriginalFileName = file.FileName,
            Description = request?.Description
        };

        var createdMedia = await mediaRepository.AddAsync(mediaFile, cancellationToken);

        logger.LogInformation(""Media file {FileName} uploaded successfully"", fileName);

        var response = new UploadResponse
        {
            MediaFileId = createdMedia.MediaFileId,
            FileName = createdMedia.FileName,
            ContentType = createdMedia.ContentType,
            FileSize = createdMedia.FileSize,
            Url = $""/uploads/{storagePath}""
        };

        return CreatedAtAction(nameof(GetById), new { id = createdMedia.MediaFileId }, response);")
        };
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"upload\"" });
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(UploadResponse), StatusCodes.Status201Created" });
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(uploadMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("MediaFileDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var mediaFile = await mediaRepository.GetByIdWithThumbnailsAsync(id, cancellationToken);

        if (mediaFile == null)
        {
            return NotFound();
        }

        return Ok(new MediaFileDto
        {
            MediaFileId = mediaFile.MediaFileId,
            FileName = mediaFile.FileName,
            ContentType = mediaFile.ContentType,
            FileSize = mediaFile.FileSize,
            OriginalFileName = mediaFile.OriginalFileName,
            Description = mediaFile.Description,
            UploadedAt = mediaFile.UploadedAt,
            UploadedBy = mediaFile.UploadedBy,
            IsProcessed = mediaFile.IsProcessed,
            Thumbnails = mediaFile.Thumbnails.Select(t => new ThumbnailDto
            {
                ThumbnailId = t.ThumbnailId,
                Width = t.Width,
                Height = t.Height,
                Format = t.Format,
                Url = $""/uploads/{t.StoragePath}""
            }).ToList()
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(MediaFileDto), StatusCodes.Status200OK" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getByIdMethod);

        var getThumbnailMethod = new MethodModel
        {
            Name = "GetThumbnail",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ThumbnailDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var thumbnails = await mediaRepository.GetThumbnailsAsync(id, cancellationToken);
        var thumbnail = thumbnails.FirstOrDefault();

        if (thumbnail == null)
        {
            return NotFound(new { error = ""No thumbnail found for this media file"" });
        }

        return Ok(new ThumbnailDto
        {
            ThumbnailId = thumbnail.ThumbnailId,
            Width = thumbnail.Width,
            Height = thumbnail.Height,
            Format = thumbnail.Format,
            Url = $""/uploads/{thumbnail.StoragePath}""
        });")
        };
        getThumbnailMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}/thumbnail\"" });
        getThumbnailMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(ThumbnailDto), StatusCodes.Status200OK" });
        getThumbnailMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getThumbnailMethod);

        var deleteMethod = new MethodModel
        {
            Name = "Delete",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var mediaFile = await mediaRepository.GetByIdAsync(id, cancellationToken);

        if (mediaFile == null)
        {
            return NotFound();
        }

        // Delete physical file
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), ""wwwroot"", mediaFile.StoragePath);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        await mediaRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation(""Media file {MediaFileId} deleted"", id);

        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{id:guid}\"" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(deleteMethod);

        return new CodeFileModel<ClassModel>(classModel, "MediaController", directory, CSharp)
        {
            Namespace = "Media.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Keep as FileModel because it's a top-level program statement
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.OpenApi.Models;
                using Serilog;

                var builder = WebApplication.CreateBuilder(args);

                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                builder.Host.UseSerilog();

                // Add services
                builder.Services.AddMediaInfrastructure(builder.Configuration);

                builder.Services.AddControllers();

                // Configure Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Media API",
                        Version = "v1",
                        Description = "Media microservice for file upload and processing"
                    });
                });

                // Configure CORS
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });

                // Add health checks
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                // Configure pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseCors();

                app.MapControllers();
                app.MapHealthChecks("/health");

                app.Run();
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        // Keep as FileModel because it's JSON
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "MediaDb": "Server=.\\SQLEXPRESS;Database=MediaDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Storage": {
                    "BasePath": "wwwroot/uploads",
                    "MaxFileSize": 104857600,
                    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".mp4", ".webm", ".mov", ".avi"]
                  },
                  "Thumbnail": {
                    "DefaultWidth": 150,
                    "DefaultHeight": 150,
                    "Format": "jpg"
                  },
                  "Serilog": {
                    "MinimumLevel": {
                      "Default": "Information",
                      "Override": {
                        "Microsoft": "Warning",
                        "Microsoft.Hosting.Lifetime": "Information"
                      }
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    private static FileModel CreateAppSettingsDevelopmentFile(string directory)
    {
        // Keep as FileModel because it's JSON
        return new FileModel("appsettings.Development", directory, ".json")
        {
            Body = """
                {
                  "Serilog": {
                    "MinimumLevel": {
                      "Default": "Debug"
                    }
                  }
                }
                """
        };
    }

    #endregion
}
