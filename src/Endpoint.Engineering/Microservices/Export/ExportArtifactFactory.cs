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

namespace Endpoint.Engineering.Microservices.Export;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class ExportArtifactFactory : IExportArtifactFactory
{
    private readonly ILogger<ExportArtifactFactory> logger;

    public ExportArtifactFactory(ILogger<ExportArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Export.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateExportJobFile(entitiesDir));
        project.Files.Add(CreateExportTemplateFile(entitiesDir));
        project.Files.Add(CreateExportResultFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIExportRepositoryFile(interfacesDir));
        project.Files.Add(CreateIExportServiceFile(interfacesDir));
        project.Files.Add(CreateIReportGeneratorFile(interfacesDir));

        // Events
        project.Files.Add(CreateExportCompletedEventFile(eventsDir));
        project.Files.Add(CreateExportFailedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateExportJobDtoFile(dtosDir));
        project.Files.Add(CreateGenerateExportRequestFile(dtosDir));
        project.Files.Add(CreateExportDownloadDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Export.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateExportDbContextFile(dataDir));
        project.Files.Add(CreateExportRepositoryFile(repositoriesDir));
        project.Files.Add(CreateExportServiceFile(servicesDir));
        project.Files.Add(CreateDefaultReportGeneratorFile(servicesDir));
        project.Files.Add(CreateConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Export.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateExportControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static FileModel CreateExportJobFile(string directory)
    {
        // Contains enum - keep as FileModel
        return new FileModel("ExportJob", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.Entities;

                public class ExportJob
                {
                    public Guid ExportJobId { get; set; }
                    public Guid UserId { get; set; }
                    public Guid? TemplateId { get; set; }
                    public required string Name { get; set; }
                    public required string Format { get; set; }
                    public ExportJobStatus Status { get; set; } = ExportJobStatus.Pending;
                    public string? Parameters { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAt { get; set; }
                    public DateTime? CompletedAt { get; set; }
                    public ExportTemplate? Template { get; set; }
                    public ExportResult? Result { get; set; }
                }

                public enum ExportJobStatus { Pending, Processing, Completed, Failed, Cancelled }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateExportTemplateFile(string directory)
    {
        var classModel = new ClassModel("ExportTemplate");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TemplateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Format", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TemplateDefinition", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "ExportTemplate", directory, CSharp)
        {
            Namespace = "Export.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateExportResultFile(string directory)
    {
        var classModel = new ClassModel("ExportResult");

        classModel.Usings.Add(new UsingModel("Export.Core.Entities"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExportResultId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ExportJob"), "ExportJob", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FilePath", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ExpiresAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "ExportResult", directory, CSharp)
        {
            Namespace = "Export.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIExportRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IExportRepository");

        interfaceModel.Usings.Add(new UsingModel("Export.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJob") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExportJob")] }] },
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
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExportJob")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJob")] },
            Params =
            [
                new ParamModel { Name = "exportJob", Type = new TypeModel("ExportJob") },
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
                new ParamModel { Name = "exportJob", Type = new TypeModel("ExportJob") },
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
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetResultByJobIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportResult") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IExportRepository", directory, CSharp)
        {
            Namespace = "Export.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIExportServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IExportService");

        interfaceModel.Usings.Add(new UsingModel("Export.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJobDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("GenerateExportRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetJobByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJobDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetJobsByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetDownloadAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportDownloadDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IExportService", directory, CSharp)
        {
            Namespace = "Export.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIReportGeneratorFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IReportGenerator");

        interfaceModel.Usings.Add(new UsingModel("Export.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateReportAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportResult")] },
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("ExportJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SupportsFormat",
            Interface = true,
            ReturnType = new TypeModel("bool"),
            Params =
            [
                new ParamModel { Name = "format", Type = new TypeModel("string") }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IReportGenerator", directory, CSharp)
        {
            Namespace = "Export.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateExportCompletedEventFile(string directory)
    {
        var classModel = new ClassModel("ExportCompletedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ExportCompletedEvent", directory, CSharp)
        {
            Namespace = "Export.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateExportFailedEventFile(string directory)
    {
        var classModel = new ClassModel("ExportFailedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Reason", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "FailedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ExportFailedEvent", directory, CSharp)
        {
            Namespace = "Export.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateExportJobDtoFile(string directory)
    {
        var classModel = new ClassModel("ExportJobDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Format", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "StartedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long") { Nullable = true }, "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "ExportJobDto", directory, CSharp)
        {
            Namespace = "Export.Core.DTOs"
        };
    }

    private static FileModel CreateGenerateExportRequestFile(string directory)
    {
        // Contains [Required] attributes - keep as FileModel
        return new FileModel("GenerateExportRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Export.Core.DTOs;

                public sealed class GenerateExportRequest
                {
                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string Format { get; init; }

                    public Guid? TemplateId { get; init; }

                    public string? Parameters { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateExportDownloadDtoFile(string directory)
    {
        var classModel = new ClassModel("ExportDownloadDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExportJobId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FilePath", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSize", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "ExportDownloadDto", directory, CSharp)
        {
            Namespace = "Export.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateExportDbContextFile(string directory)
    {
        var classModel = new ClassModel("ExportDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Export.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "ExportDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("ExportDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ExportJob")] }, "ExportJobs", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ExportTemplate")] }, "ExportTemplates", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ExportResult")] }, "ExportResults", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<ExportJob>(entity =>
        {
            entity.HasKey(e => e.ExportJobId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId);
            entity.HasOne(e => e.Result).WithOne(r => r.ExportJob).HasForeignKey<ExportResult>(r => r.ExportJobId);
        });

        modelBuilder.Entity<ExportTemplate>(entity =>
        {
            entity.HasKey(t => t.TemplateId);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<ExportResult>(entity =>
        {
            entity.HasKey(r => r.ExportResultId);
            entity.Property(r => r.FileName).IsRequired().HasMaxLength(255);
            entity.Property(r => r.ContentType).IsRequired().HasMaxLength(100);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "ExportDbContext", directory, CSharp)
        {
            Namespace = "Export.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateExportRepositoryFile(string directory)
    {
        var classModel = new ClassModel("ExportRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Export.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Export.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Export.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IExportRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("ExportDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "ExportRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("ExportDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJob") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ExportJobs.Include(e => e.Template).Include(e => e.Result).FirstOrDefaultAsync(e => e.ExportJobId == exportJobId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExportJob")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ExportJobs.Include(e => e.Template).Include(e => e.Result).Where(e => e.UserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExportJob")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ExportJobs.Include(e => e.Template).Include(e => e.Result).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJob")] },
            Params =
            [
                new ParamModel { Name = "exportJob", Type = new TypeModel("ExportJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"exportJob.ExportJobId = Guid.NewGuid();
        await context.ExportJobs.AddAsync(exportJob, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return exportJob;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "exportJob", Type = new TypeModel("ExportJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.ExportJobs.Update(exportJob);
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
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var exportJob = await context.ExportJobs.FindAsync(new object[] { exportJobId }, cancellationToken);
        if (exportJob != null)
        {
            context.ExportJobs.Remove(exportJob);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetResultByJobIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportResult") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.ExportResults.FirstOrDefaultAsync(r => r.ExportJobId == exportJobId, cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ExportRepository", directory, CSharp)
        {
            Namespace = "Export.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateExportServiceFile(string directory)
    {
        var classModel = new ClassModel("ExportService");

        classModel.Usings.Add(new UsingModel("Export.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Export.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Export.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IExportService"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IExportRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "reportGenerator", Type = new TypeModel("IReportGenerator"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "ExportService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("IExportRepository") },
                new ParamModel { Name = "reportGenerator", Type = new TypeModel("IReportGenerator") }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.reportGenerator = reportGenerator;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJobDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("GenerateExportRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var exportJob = new ExportJob
        {
            UserId = request.UserId,
            Name = request.Name,
            Format = request.Format,
            TemplateId = request.TemplateId,
            Parameters = request.Parameters,
            Status = ExportJobStatus.Processing,
            StartedAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(exportJob, cancellationToken);

        // Generate the report asynchronously
        try
        {
            var result = await reportGenerator.GenerateReportAsync(created, cancellationToken);
            created.Status = ExportJobStatus.Completed;
            created.CompletedAt = DateTime.UtcNow;
            created.Result = result;
            await repository.UpdateAsync(created, cancellationToken);
        }
        catch
        {
            created.Status = ExportJobStatus.Failed;
            await repository.UpdateAsync(created, cancellationToken);
            throw;
        }

        return new ExportJobDto
        {
            ExportJobId = created.ExportJobId,
            UserId = created.UserId,
            Name = created.Name,
            Format = created.Format,
            Status = created.Status.ToString(),
            CreatedAt = created.CreatedAt,
            StartedAt = created.StartedAt,
            CompletedAt = created.CompletedAt,
            FileName = created.Result?.FileName,
            FileSize = created.Result?.FileSize
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetJobByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportJobDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var exportJob = await repository.GetByIdAsync(exportJobId, cancellationToken);
        if (exportJob == null) return null;

        return new ExportJobDto
        {
            ExportJobId = exportJob.ExportJobId,
            UserId = exportJob.UserId,
            Name = exportJob.Name,
            Format = exportJob.Format,
            Status = exportJob.Status.ToString(),
            CreatedAt = exportJob.CreatedAt,
            StartedAt = exportJob.StartedAt,
            CompletedAt = exportJob.CompletedAt,
            FileName = exportJob.Result?.FileName,
            FileSize = exportJob.Result?.FileSize
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetJobsByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var exportJobs = await repository.GetByUserIdAsync(userId, cancellationToken);
        return exportJobs.Select(e => new ExportJobDto
        {
            ExportJobId = e.ExportJobId,
            UserId = e.UserId,
            Name = e.Name,
            Format = e.Format,
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt,
            FileName = e.Result?.FileName,
            FileSize = e.Result?.FileSize
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetDownloadAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportDownloadDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "exportJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var result = await repository.GetResultByJobIdAsync(exportJobId, cancellationToken);
        if (result == null) return null;

        return new ExportDownloadDto
        {
            ExportJobId = result.ExportJobId,
            FileName = result.FileName,
            ContentType = result.ContentType,
            FilePath = result.FilePath,
            FileSize = result.FileSize
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "ExportService", directory, CSharp)
        {
            Namespace = "Export.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateDefaultReportGeneratorFile(string directory)
    {
        var classModel = new ClassModel("DefaultReportGenerator");

        classModel.Usings.Add(new UsingModel("Export.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Export.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IReportGenerator"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "SupportedFormats",
            Type = new TypeModel("HashSet") { GenericTypeParameters = [new TypeModel("string")] },
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReadOnly = true,
            DefaultValue = "new(StringComparer.OrdinalIgnoreCase) { \"pdf\", \"csv\", \"xlsx\", \"json\" }"
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateReportAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ExportResult")] },
            Params =
            [
                new ParamModel { Name = "job", Type = new TypeModel("ExportJob") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Simulate report generation
        await Task.Delay(100, cancellationToken);

        var fileName = $""{job.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.{job.Format.ToLowerInvariant()}"";
        var contentType = GetContentType(job.Format);
        var filePath = Path.Combine(""exports"", job.UserId.ToString(), fileName);

        return new ExportResult
        {
            ExportResultId = Guid.NewGuid(),
            ExportJobId = job.ExportJobId,
            FileName = fileName,
            ContentType = contentType,
            FilePath = filePath,
            FileSize = 0,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SupportsFormat",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("bool"),
            Params =
            [
                new ParamModel { Name = "format", Type = new TypeModel("string") }
            ],
            Body = new ExpressionModel("return SupportedFormats.Contains(format);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetContentType",
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReturnType = new TypeModel("string"),
            Params =
            [
                new ParamModel { Name = "format", Type = new TypeModel("string") }
            ],
            Body = new ExpressionModel(@"return format.ToLowerInvariant() switch
        {
            ""pdf"" => ""application/pdf"",
            ""csv"" => ""text/csv"",
            ""xlsx"" => ""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"",
            ""json"" => ""application/json"",
            _ => ""application/octet-stream""
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "DefaultReportGenerator", directory, CSharp)
        {
            Namespace = "Export.Infrastructure.Services"
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
        classModel.Usings.Add(new UsingModel("Export.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Export.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Export.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Export.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddExportInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<ExportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""ExportDb"") ??
                @""Server=.\SQLEXPRESS;Database=ExportDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IExportRepository, ExportRepository>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IReportGenerator, DefaultReportGenerator>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateExportControllerFile(string directory)
    {
        var classModel = new ClassModel("ExportController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Export.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Export.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "service", Type = new TypeModel("IExportService"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "ExportController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "service", Type = new TypeModel("IExportService") }
            ],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var generateMethod = new MethodModel
        {
            Name = "Generate",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ExportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("GenerateExportRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var exportJob = await service.GenerateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetJob), new { id = exportJob.ExportJobId }, exportJob);")
        };
        generateMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"generate\"" });
        classModel.Methods.Add(generateMethod);

        var getJobMethod = new MethodModel
        {
            Name = "GetJob",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ExportJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var exportJob = await service.GetJobByIdAsync(id, cancellationToken);
        if (exportJob == null) return NotFound();
        return Ok(exportJob);")
        };
        getJobMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"jobs/{id:guid}\"" });
        classModel.Methods.Add(getJobMethod);

        var getDownloadMethod = new MethodModel
        {
            Name = "GetDownload",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ExportDownloadDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var download = await service.GetDownloadAsync(id, cancellationToken);
        if (download == null) return NotFound();
        return Ok(download);")
        };
        getDownloadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"download/{id:guid}\"" });
        classModel.Methods.Add(getDownloadMethod);

        return new CodeFileModel<ClassModel>(classModel, "ExportController", directory, CSharp)
        {
            Namespace = "Export.Api.Controllers"
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

                builder.Services.AddExportInfrastructure(builder.Configuration);
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
                    "ExportDb": "Server=.\\SQLEXPRESS;Database=ExportDb;Trusted_Connection=True;TrustServerCertificate=True"
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
