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

namespace Endpoint.Engineering.Microservices.OcrVision;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating OcrVision microservice artifacts according to ocr-vision-microservice.spec.md.
/// </summary>
public class OcrVisionArtifactFactory : IOcrVisionArtifactFactory
{
    private readonly ILogger<OcrVisionArtifactFactory> logger;

    public OcrVisionArtifactFactory(ILogger<OcrVisionArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding OcrVision.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateOcrResultEntityFile(entitiesDir));
        project.Files.Add(CreateExtractedDataEntityFile(entitiesDir));
        project.Files.Add(CreateDocumentAnalysisEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIOcrServiceFile(interfacesDir));
        project.Files.Add(CreateIVisionServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateDocumentAnalyzedEventFile(eventsDir));
        project.Files.Add(CreateTextExtractedEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateOcrResultDtoFile(dtosDir));
        project.Files.Add(CreateExtractedDataDtoFile(dtosDir));
        project.Files.Add(CreateDocumentAnalysisDtoFile(dtosDir));
        project.Files.Add(CreateAnalyzeDocumentRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding OcrVision.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateOcrVisionDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateOcrResultConfigurationFile(configurationsDir));
        project.Files.Add(CreateExtractedDataConfigurationFile(configurationsDir));
        project.Files.Add(CreateDocumentAnalysisConfigurationFile(configurationsDir));

        // Services
        project.Files.Add(CreateOcrServiceFile(servicesDir));
        project.Files.Add(CreateVisionServiceFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding OcrVision.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateOcrControllerFile(controllersDir));

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
            Namespace = "OcrVision.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateOcrResultEntityFile(string directory)
    {
        var classModel = new ClassModel("OcrResult");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "OcrResultId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ExtractedText", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Confidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Language", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "PageCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("OcrStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "OcrStatus.Pending" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("ExtractedData")] }, "ExtractedDataItems", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<ExtractedData>()" });

        return new CodeFileModel<ClassModel>(classModel, "OcrResult", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateExtractedDataEntityFile(string directory)
    {
        var classModel = new ClassModel("ExtractedData");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExtractedDataId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "OcrResultId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FieldName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FieldValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DataType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Confidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int") { Nullable = true }, "PageNumber", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("BoundingBox") { Nullable = true }, "BoundingBox", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("OcrResult") { Nullable = true }, "OcrResult", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "ExtractedData", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentAnalysisEntityFile(string directory)
    {
        var classModel = new ClassModel("DocumentAnalysis");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "DocumentAnalysisId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AnalysisType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "DocumentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { Nullable = true, GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Metadata", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { Nullable = true, GenericTypeParameters = [new TypeModel("DetectedObject")] }, "DetectedObjects", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { Nullable = true, GenericTypeParameters = [new TypeModel("DetectedTable")] }, "DetectedTables", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "OverallConfidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("AnalysisStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "AnalysisStatus.Pending" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "DocumentAnalysis", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Entities"
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
            Namespace = "OcrVision.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIOcrServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IOcrService");

        interfaceModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ProcessDocumentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("OcrResult")] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "fileName", Type = new TypeModel("string") },
                new ParamModel { Name = "contentType", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetResultByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("OcrResult") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "ocrResultId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetResultsByDocumentIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("OcrResult")] }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ExtractStructuredDataAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExtractedData")] }] },
            Params =
            [
                new ParamModel { Name = "ocrResultId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "fieldNames", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateStatusAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("OcrResult")] },
            Params =
            [
                new ParamModel { Name = "ocrResultId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "status", Type = new TypeModel("OcrStatus") },
                new ParamModel { Name = "errorMessage", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IOcrService", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIVisionServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IVisionService");

        interfaceModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AnalyzeDocumentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "fileName", Type = new TypeModel("string") },
                new ParamModel { Name = "analysisType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAnalysisByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("DocumentAnalysis") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "analysisId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAnalysesByDocumentIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DetectObjectsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DetectedObject")] }] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DetectTablesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DetectedTable")] }] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ClassifyDocumentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IVisionService", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentAnalyzedEventFile(string directory)
    {
        var classModel = new ClassModel("DocumentAnalyzedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AnalysisType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "DocumentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "OverallConfidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "DetectedObjectCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "DetectedTableCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "DocumentAnalyzedEvent", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateTextExtractedEventFile(string directory)
    {
        var classModel = new ClassModel("TextExtractedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "PageCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "ExtractedTextLength", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Confidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Language", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TextExtractedEvent", directory, CSharp)
        {
            Namespace = "OcrVision.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateOcrResultDtoFile(string directory)
    {
        var classModel = new ClassModel("OcrResultDto")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "OcrResultId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ExtractedText", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Confidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Language", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "PageCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("OcrStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IEnumerable") { Nullable = true, GenericTypeParameters = [new TypeModel("ExtractedDataDto")] }, "ExtractedDataItems", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "OcrResultDto", directory, CSharp)
        {
            Namespace = "OcrVision.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateExtractedDataDtoFile(string directory)
    {
        var classModel = new ClassModel("ExtractedDataDto")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ExtractedDataId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "OcrResultId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FieldName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FieldValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DataType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Confidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int") { Nullable = true }, "PageNumber", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("BoundingBox") { Nullable = true }, "BoundingBox", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "ExtractedDataDto", directory, CSharp)
        {
            Namespace = "OcrVision.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentAnalysisDtoFile(string directory)
    {
        var classModel = new ClassModel("DocumentAnalysisDto")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "DocumentAnalysisId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AnalysisType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "DocumentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { Nullable = true, GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Metadata", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { Nullable = true, GenericTypeParameters = [new TypeModel("DetectedObject")] }, "DetectedObjects", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { Nullable = true, GenericTypeParameters = [new TypeModel("DetectedTable")] }, "DetectedTables", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "OverallConfidence", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("AnalysisStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "DocumentAnalysisDto", directory, CSharp)
        {
            Namespace = "OcrVision.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateAnalyzeDocumentRequestFile(string directory)
    {
        var classModel = new ClassModel("AnalyzeDocumentRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var analysisTypeProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AnalysisType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        analysisTypeProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(analysisTypeProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "ExtractText", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "DetectObjects", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "DetectTables", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "ClassifyDocument", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IEnumerable") { Nullable = true, GenericTypeParameters = [new TypeModel("string")] }, "FieldsToExtract", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Language", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "AnalyzeDocumentRequest", directory, CSharp)
        {
            Namespace = "OcrVision.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateOcrVisionDbContextFile(string directory)
    {
        var classModel = new ClassModel("OcrVisionDbContext");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "OcrVisionDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("OcrVisionDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("OcrResult")] }, "OcrResults", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ExtractedData")] }, "ExtractedDataItems", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] }, "DocumentAnalyses", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OcrVisionDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "OcrVisionDbContext", directory, CSharp)
        {
            Namespace = "OcrVision.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateOcrResultConfigurationFile(string directory)
    {
        var classModel = new ClassModel("OcrResultConfiguration");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("OcrResult")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("OcrResult")] } }],
            Body = new ExpressionModel(@"builder.HasKey(o => o.OcrResultId);

        builder.Property(o => o.DocumentId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.ContentType)
            .HasMaxLength(100);

        builder.Property(o => o.ExtractedText)
            .IsRequired()
            .HasColumnType(""nvarchar(max)"");

        builder.Property(o => o.Confidence)
            .IsRequired();

        builder.Property(o => o.Language)
            .HasMaxLength(50);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.HasMany(o => o.ExtractedDataItems)
            .WithOne(e => e.OcrResult)
            .HasForeignKey(e => e.OcrResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.DocumentId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);")
        });

        return new CodeFileModel<ClassModel>(classModel, "OcrResultConfiguration", directory, CSharp)
        {
            Namespace = "OcrVision.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateExtractedDataConfigurationFile(string directory)
    {
        var classModel = new ClassModel("ExtractedDataConfiguration");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("ExtractedData")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("ExtractedData")] } }],
            Body = new ExpressionModel(@"builder.HasKey(e => e.ExtractedDataId);

        builder.Property(e => e.FieldName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.FieldValue)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.DataType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Confidence)
            .IsRequired();

        builder.Property(e => e.BoundingBox)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.OcrResultId);
        builder.HasIndex(e => e.FieldName);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ExtractedDataConfiguration", directory, CSharp)
        {
            Namespace = "OcrVision.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentAnalysisConfigurationFile(string directory)
    {
        var classModel = new ClassModel("DocumentAnalysisConfiguration");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] } }],
            Body = new ExpressionModel(@"builder.HasKey(d => d.DocumentAnalysisId);

        builder.Property(d => d.DocumentId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.AnalysisType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.DocumentType)
            .HasMaxLength(100);

        builder.Property(d => d.Metadata)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(d => d.DetectedObjects)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(d => d.DetectedTables)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(d => d.OverallConfidence)
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.HasIndex(d => d.DocumentId);
        builder.HasIndex(d => d.AnalysisType);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.CreatedAt);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentAnalysisConfiguration", directory, CSharp)
        {
            Namespace = "OcrVision.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateOcrServiceFile(string directory)
    {
        var classModel = new ClassModel("OcrService");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));
        classModel.Usings.Add(new UsingModel("OcrVision.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("OcrVision.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IOcrService"));

        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("OcrVisionDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("OcrService")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "OcrService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "context", Type = new TypeModel("OcrVisionDbContext") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("OcrService")] } }
            ],
            Body = new ExpressionModel(@"this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "ProcessDocumentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("OcrResult")] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "fileName", Type = new TypeModel("string") },
                new ParamModel { Name = "contentType", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var ocrResult = new OcrResult
        {
            OcrResultId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid().ToString(),
            FileName = fileName,
            ContentType = contentType,
            ExtractedText = string.Empty,
            Status = OcrStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await context.OcrResults.AddAsync(ocrResult, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(""Started OCR processing for document: {FileName}"", fileName);

        try
        {
            // Simulate OCR processing - in production, integrate with actual OCR service
            using var memoryStream = new MemoryStream();
            await documentStream.CopyToAsync(memoryStream, cancellationToken);

            ocrResult.ExtractedText = ""Extracted text from document..."";
            ocrResult.Confidence = 0.95;
            ocrResult.PageCount = 1;
            ocrResult.Language = ""en"";
            ocrResult.Status = OcrStatus.Completed;
            ocrResult.CompletedAt = DateTime.UtcNow;

            logger.LogInformation(""OCR processing completed for document: {DocumentId}"", ocrResult.DocumentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ""OCR processing failed for document: {FileName}"", fileName);
            ocrResult.Status = OcrStatus.Failed;
            ocrResult.ErrorMessage = ex.Message;
        }

        context.OcrResults.Update(ocrResult);
        await context.SaveChangesAsync(cancellationToken);

        return ocrResult;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetResultByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("OcrResult") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "ocrResultId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.OcrResults
            .Include(o => o.ExtractedDataItems)
            .FirstOrDefaultAsync(o => o.OcrResultId == ocrResultId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetResultsByDocumentIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("OcrResult")] }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.OcrResults
            .Include(o => o.ExtractedDataItems)
            .Where(o => o.DocumentId == documentId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ExtractStructuredDataAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExtractedData")] }] },
            Params =
            [
                new ParamModel { Name = "ocrResultId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "fieldNames", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var ocrResult = await GetResultByIdAsync(ocrResultId, cancellationToken);

        if (ocrResult == null)
        {
            throw new InvalidOperationException($""OCR result {ocrResultId} not found"");
        }

        var extractedData = new List<ExtractedData>();

        foreach (var fieldName in fieldNames)
        {
            var data = new ExtractedData
            {
                ExtractedDataId = Guid.NewGuid(),
                OcrResultId = ocrResultId,
                FieldName = fieldName,
                FieldValue = ""Extracted value"",
                DataType = ""string"",
                Confidence = 0.9,
                CreatedAt = DateTime.UtcNow
            };

            extractedData.Add(data);
        }

        await context.ExtractedDataItems.AddRangeAsync(extractedData, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return extractedData;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateStatusAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("OcrResult")] },
            Params =
            [
                new ParamModel { Name = "ocrResultId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "status", Type = new TypeModel("OcrStatus") },
                new ParamModel { Name = "errorMessage", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var ocrResult = await context.OcrResults.FindAsync(new object[] { ocrResultId }, cancellationToken)
            ?? throw new InvalidOperationException($""OCR result {ocrResultId} not found"");

        ocrResult.Status = status;
        ocrResult.ErrorMessage = errorMessage;

        if (status == OcrStatus.Completed)
        {
            ocrResult.CompletedAt = DateTime.UtcNow;
        }

        context.OcrResults.Update(ocrResult);
        await context.SaveChangesAsync(cancellationToken);

        return ocrResult;")
        });

        return new CodeFileModel<ClassModel>(classModel, "OcrService", directory, CSharp)
        {
            Namespace = "OcrVision.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateVisionServiceFile(string directory)
    {
        var classModel = new ClassModel("VisionService");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Entities"));
        classModel.Usings.Add(new UsingModel("OcrVision.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("OcrVision.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IVisionService"));

        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("OcrVisionDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("VisionService")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "VisionService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "context", Type = new TypeModel("OcrVisionDbContext") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("VisionService")] } }
            ],
            Body = new ExpressionModel(@"this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "AnalyzeDocumentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "fileName", Type = new TypeModel("string") },
                new ParamModel { Name = "analysisType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var analysis = new DocumentAnalysis
        {
            DocumentAnalysisId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid().ToString(),
            FileName = fileName,
            AnalysisType = analysisType,
            Status = AnalysisStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await context.DocumentAnalyses.AddAsync(analysis, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(""Started document analysis for: {FileName}, Type: {AnalysisType}"", fileName, analysisType);

        try
        {
            // Simulate document analysis - in production, integrate with actual vision service
            using var memoryStream = new MemoryStream();
            await documentStream.CopyToAsync(memoryStream, cancellationToken);

            analysis.DocumentType = ""invoice"";
            analysis.OverallConfidence = 0.92;
            analysis.DetectedObjects = new List<DetectedObject>
            {
                new DetectedObject
                {
                    ObjectType = ""logo"",
                    Confidence = 0.95,
                    BoundingBox = new BoundingBox { X = 10, Y = 10, Width = 100, Height = 50 }
                }
            };
            analysis.DetectedTables = new List<DetectedTable>
            {
                new DetectedTable
                {
                    RowCount = 5,
                    ColumnCount = 3,
                    PageNumber = 1
                }
            };
            analysis.Status = AnalysisStatus.Completed;
            analysis.CompletedAt = DateTime.UtcNow;

            logger.LogInformation(""Document analysis completed for: {DocumentId}"", analysis.DocumentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ""Document analysis failed for: {FileName}"", fileName);
            analysis.Status = AnalysisStatus.Failed;
            analysis.ErrorMessage = ex.Message;
        }

        context.DocumentAnalyses.Update(analysis);
        await context.SaveChangesAsync(cancellationToken);

        return analysis;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAnalysisByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("DocumentAnalysis") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "analysisId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.DocumentAnalyses
            .FirstOrDefaultAsync(d => d.DocumentAnalysisId == analysisId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAnalysesByDocumentIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DocumentAnalysis")] }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.DocumentAnalyses
            .Where(d => d.DocumentId == documentId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DetectObjectsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DetectedObject")] }] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogDebug(""Detecting objects in document"");

        // Simulate object detection
        await Task.Delay(100, cancellationToken);

        return new List<DetectedObject>
        {
            new DetectedObject
            {
                ObjectType = ""signature"",
                Confidence = 0.88,
                BoundingBox = new BoundingBox { X = 400, Y = 600, Width = 150, Height = 50 }
            },
            new DetectedObject
            {
                ObjectType = ""stamp"",
                Confidence = 0.92,
                BoundingBox = new BoundingBox { X = 500, Y = 500, Width = 80, Height = 80 }
            }
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DetectTablesAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DetectedTable")] }] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogDebug(""Detecting tables in document"");

        // Simulate table detection
        await Task.Delay(100, cancellationToken);

        return new List<DetectedTable>
        {
            new DetectedTable
            {
                RowCount = 10,
                ColumnCount = 4,
                PageNumber = 1,
                BoundingBox = new BoundingBox { X = 50, Y = 200, Width = 500, Height = 300 }
            }
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ClassifyDocumentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "documentStream", Type = new TypeModel("Stream") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogDebug(""Classifying document"");

        // Simulate document classification
        await Task.Delay(100, cancellationToken);

        return ""invoice"";")
        });

        return new CodeFileModel<ClassModel>(classModel, "VisionService", directory, CSharp)
        {
            Namespace = "OcrVision.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("OcrVision.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("OcrVision.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("OcrVision.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddOcrVisionInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<OcrVisionDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(""OcrVisionDb"") ??
                @""Server=.\SQLEXPRESS;Database=OcrVisionDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IVisionService, VisionService>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateOcrControllerFile(string directory)
    {
        var classModel = new ClassModel("OcrController");

        classModel.Usings.Add(new UsingModel("OcrVision.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("OcrVision.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/ocr\"" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });

        classModel.Fields.Add(new FieldModel { Name = "ocrService", Type = new TypeModel("IOcrService"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "visionService", Type = new TypeModel("IVisionService"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("OcrController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "OcrController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "ocrService", Type = new TypeModel("IOcrService") },
                new ParamModel { Name = "visionService", Type = new TypeModel("IVisionService") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("OcrController")] } }
            ],
            Body = new ExpressionModel(@"this.ocrService = ocrService;
        this.visionService = visionService;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var analyzeDocumentMethod = new MethodModel
        {
            Name = "AnalyzeDocument",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("OcrResultDto")] }] },
            Params =
            [
                new ParamModel { Name = "file", Type = new TypeModel("IFormFile") },
                new ParamModel { Name = "request", Type = new TypeModel("AnalyzeDocumentRequest"), Attribute = new AttributeModel() { Name = "FromForm" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (file == null || file.Length == 0)
        {
            return BadRequest(""No file provided"");
        }

        logger.LogInformation(""Received document for analysis: {FileName}"", file.FileName);

        using var stream = file.OpenReadStream();
        var ocrResult = await ocrService.ProcessDocumentAsync(
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        var response = new OcrResultDto
        {
            OcrResultId = ocrResult.OcrResultId,
            DocumentId = ocrResult.DocumentId,
            FileName = ocrResult.FileName,
            ContentType = ocrResult.ContentType,
            ExtractedText = ocrResult.ExtractedText,
            Confidence = ocrResult.Confidence,
            Language = ocrResult.Language,
            PageCount = ocrResult.PageCount,
            Status = ocrResult.Status,
            ErrorMessage = ocrResult.ErrorMessage,
            CreatedAt = ocrResult.CreatedAt,
            CompletedAt = ocrResult.CompletedAt
        };

        return AcceptedAtAction(nameof(GetResultById), new { id = ocrResult.OcrResultId }, response);")
        };
        analyzeDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"analyze\"" });
        analyzeDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(OcrResultDto), StatusCodes.Status202Accepted" });
        analyzeDocumentMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(analyzeDocumentMethod);

        var getResultByIdMethod = new MethodModel
        {
            Name = "GetResultById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("OcrResultDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var ocrResult = await ocrService.GetResultByIdAsync(id, cancellationToken);

        if (ocrResult == null)
        {
            return NotFound();
        }

        var response = new OcrResultDto
        {
            OcrResultId = ocrResult.OcrResultId,
            DocumentId = ocrResult.DocumentId,
            FileName = ocrResult.FileName,
            ContentType = ocrResult.ContentType,
            ExtractedText = ocrResult.ExtractedText,
            Confidence = ocrResult.Confidence,
            Language = ocrResult.Language,
            PageCount = ocrResult.PageCount,
            Status = ocrResult.Status,
            ErrorMessage = ocrResult.ErrorMessage,
            CreatedAt = ocrResult.CreatedAt,
            CompletedAt = ocrResult.CompletedAt,
            ExtractedDataItems = ocrResult.ExtractedDataItems?.Select(e => new ExtractedDataDto
            {
                ExtractedDataId = e.ExtractedDataId,
                OcrResultId = e.OcrResultId,
                FieldName = e.FieldName,
                FieldValue = e.FieldValue,
                DataType = e.DataType,
                Confidence = e.Confidence,
                PageNumber = e.PageNumber,
                BoundingBox = e.BoundingBox
            })
        };

        return Ok(response);")
        };
        getResultByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"results/{id:guid}\"" });
        getResultByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(OcrResultDto), StatusCodes.Status200OK" });
        getResultByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getResultByIdMethod);

        var getAnalysisByIdMethod = new MethodModel
        {
            Name = "GetAnalysisById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("DocumentAnalysisDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var analysis = await visionService.GetAnalysisByIdAsync(id, cancellationToken);

        if (analysis == null)
        {
            return NotFound();
        }

        var response = new DocumentAnalysisDto
        {
            DocumentAnalysisId = analysis.DocumentAnalysisId,
            DocumentId = analysis.DocumentId,
            FileName = analysis.FileName,
            AnalysisType = analysis.AnalysisType,
            DocumentType = analysis.DocumentType,
            Metadata = analysis.Metadata,
            DetectedObjects = analysis.DetectedObjects,
            DetectedTables = analysis.DetectedTables,
            OverallConfidence = analysis.OverallConfidence,
            Status = analysis.Status,
            ErrorMessage = analysis.ErrorMessage,
            CreatedAt = analysis.CreatedAt,
            CompletedAt = analysis.CompletedAt
        };

        return Ok(response);")
        };
        getAnalysisByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"analysis/{id:guid}\"" });
        getAnalysisByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(DocumentAnalysisDto), StatusCodes.Status200OK" });
        getAnalysisByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getAnalysisByIdMethod);

        var extractDataMethod = new MethodModel
        {
            Name = "ExtractData",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ExtractedDataDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "fieldNames", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] }, Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"try
        {
            var extractedData = await ocrService.ExtractStructuredDataAsync(id, fieldNames, cancellationToken);

            var response = extractedData.Select(e => new ExtractedDataDto
            {
                ExtractedDataId = e.ExtractedDataId,
                OcrResultId = e.OcrResultId,
                FieldName = e.FieldName,
                FieldValue = e.FieldValue,
                DataType = e.DataType,
                Confidence = e.Confidence,
                PageNumber = e.PageNumber,
                BoundingBox = e.BoundingBox
            });

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }")
        };
        extractDataMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"results/{id:guid}/extract\"" });
        extractDataMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<ExtractedDataDto>), StatusCodes.Status200OK" });
        extractDataMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(extractDataMethod);

        return new CodeFileModel<ClassModel>(classModel, "OcrController", directory, CSharp)
        {
            Namespace = "OcrVision.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text;
                using Microsoft.AspNetCore.Authentication.JwtBearer;
                using Microsoft.IdentityModel.Tokens;
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
                builder.Services.AddOcrVisionInfrastructure(builder.Configuration);

                // Configure JWT authentication
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")))
                        };
                    });

                builder.Services.AddAuthorization();
                builder.Services.AddControllers();

                // Configure Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "OcrVision API",
                        Version = "v1",
                        Description = "OcrVision microservice for OCR and document analysis"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
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
                app.UseCors();
                app.UseAuthentication();
                app.UseAuthorization();

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
                    "OcrVisionDb": "Server=.\\SQLEXPRESS;Database=OcrVisionDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "OcrVision.Api",
                    "Audience": "OcrVision.Api",
                    "ExpirationHours": "24"
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
