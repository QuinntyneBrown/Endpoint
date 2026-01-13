// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.OcrVision;

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

    private static FileModel CreateIAggregateRootFile(string directory)
    {
        return new FileModel("IAggregateRoot", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace OcrVision.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateOcrResultEntityFile(string directory)
    {
        return new FileModel("OcrResult", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace OcrVision.Core.Entities;

                /// <summary>
                /// OcrResult entity representing the result of OCR processing on a document.
                /// </summary>
                public class OcrResult : IAggregateRoot
                {
                    public Guid OcrResultId { get; set; }

                    public required string DocumentId { get; set; }

                    public required string FileName { get; set; }

                    public string? ContentType { get; set; }

                    public required string ExtractedText { get; set; }

                    public double Confidence { get; set; }

                    public string? Language { get; set; }

                    public int PageCount { get; set; }

                    public OcrStatus Status { get; set; } = OcrStatus.Pending;

                    public string? ErrorMessage { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? CompletedAt { get; set; }

                    public ICollection<ExtractedData> ExtractedDataItems { get; set; } = new List<ExtractedData>();
                }

                /// <summary>
                /// OCR processing status.
                /// </summary>
                public enum OcrStatus
                {
                    Pending,
                    Processing,
                    Completed,
                    Failed
                }
                """
        };
    }

    private static FileModel CreateExtractedDataEntityFile(string directory)
    {
        return new FileModel("ExtractedData", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace OcrVision.Core.Entities;

                /// <summary>
                /// ExtractedData entity representing structured data extracted from a document.
                /// </summary>
                public class ExtractedData : IAggregateRoot
                {
                    public Guid ExtractedDataId { get; set; }

                    public Guid OcrResultId { get; set; }

                    public required string FieldName { get; set; }

                    public required string FieldValue { get; set; }

                    public required string DataType { get; set; }

                    public double Confidence { get; set; }

                    public int? PageNumber { get; set; }

                    public BoundingBox? BoundingBox { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public OcrResult? OcrResult { get; set; }
                }

                /// <summary>
                /// Represents a bounding box for extracted data location.
                /// </summary>
                public class BoundingBox
                {
                    public double X { get; set; }

                    public double Y { get; set; }

                    public double Width { get; set; }

                    public double Height { get; set; }
                }
                """
        };
    }

    private static FileModel CreateDocumentAnalysisEntityFile(string directory)
    {
        return new FileModel("DocumentAnalysis", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace OcrVision.Core.Entities;

                /// <summary>
                /// DocumentAnalysis entity representing visual analysis results of a document.
                /// </summary>
                public class DocumentAnalysis : IAggregateRoot
                {
                    public Guid DocumentAnalysisId { get; set; }

                    public required string DocumentId { get; set; }

                    public required string FileName { get; set; }

                    public required string AnalysisType { get; set; }

                    public string? DocumentType { get; set; }

                    public Dictionary<string, object>? Metadata { get; set; }

                    public List<DetectedObject>? DetectedObjects { get; set; }

                    public List<DetectedTable>? DetectedTables { get; set; }

                    public double OverallConfidence { get; set; }

                    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;

                    public string? ErrorMessage { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? CompletedAt { get; set; }
                }

                /// <summary>
                /// Analysis processing status.
                /// </summary>
                public enum AnalysisStatus
                {
                    Pending,
                    Processing,
                    Completed,
                    Failed
                }

                /// <summary>
                /// Represents a detected object in the document.
                /// </summary>
                public class DetectedObject
                {
                    public required string ObjectType { get; set; }

                    public double Confidence { get; set; }

                    public BoundingBox? BoundingBox { get; set; }

                    public Dictionary<string, string>? Properties { get; set; }
                }

                /// <summary>
                /// Represents a detected table in the document.
                /// </summary>
                public class DetectedTable
                {
                    public int RowCount { get; set; }

                    public int ColumnCount { get; set; }

                    public List<List<string>>? Cells { get; set; }

                    public BoundingBox? BoundingBox { get; set; }

                    public int? PageNumber { get; set; }
                }
                """
        };
    }

    private static FileModel CreateIDomainEventFile(string directory)
    {
        return new FileModel("IDomainEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace OcrVision.Core.Interfaces;

                /// <summary>
                /// Interface for domain events.
                /// </summary>
                public interface IDomainEvent
                {
                    Guid AggregateId { get; }

                    string AggregateType { get; }

                    DateTime OccurredAt { get; }

                    string CorrelationId { get; }
                }
                """
        };
    }

    private static FileModel CreateIOcrServiceFile(string directory)
    {
        return new FileModel("IOcrService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;

                namespace OcrVision.Core.Interfaces;

                /// <summary>
                /// Service interface for OCR operations.
                /// </summary>
                public interface IOcrService
                {
                    Task<OcrResult> ProcessDocumentAsync(Stream documentStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default);

                    Task<OcrResult?> GetResultByIdAsync(Guid ocrResultId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<OcrResult>> GetResultsByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<ExtractedData>> ExtractStructuredDataAsync(Guid ocrResultId, IEnumerable<string> fieldNames, CancellationToken cancellationToken = default);

                    Task<OcrResult> UpdateStatusAsync(Guid ocrResultId, OcrStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIVisionServiceFile(string directory)
    {
        return new FileModel("IVisionService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;

                namespace OcrVision.Core.Interfaces;

                /// <summary>
                /// Service interface for document vision and analysis operations.
                /// </summary>
                public interface IVisionService
                {
                    Task<DocumentAnalysis> AnalyzeDocumentAsync(Stream documentStream, string fileName, string analysisType, CancellationToken cancellationToken = default);

                    Task<DocumentAnalysis?> GetAnalysisByIdAsync(Guid analysisId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<DocumentAnalysis>> GetAnalysesByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<DetectedObject>> DetectObjectsAsync(Stream documentStream, CancellationToken cancellationToken = default);

                    Task<IEnumerable<DetectedTable>> DetectTablesAsync(Stream documentStream, CancellationToken cancellationToken = default);

                    Task<string> ClassifyDocumentAsync(Stream documentStream, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateDocumentAnalyzedEventFile(string directory)
    {
        return new FileModel("DocumentAnalyzedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Interfaces;

                namespace OcrVision.Core.Events;

                /// <summary>
                /// Event raised when a document has been analyzed.
                /// </summary>
                public sealed class DocumentAnalyzedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "DocumentAnalysis";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string DocumentId { get; init; }

                    public required string AnalysisType { get; init; }

                    public string? DocumentType { get; init; }

                    public double OverallConfidence { get; init; }

                    public int DetectedObjectCount { get; init; }

                    public int DetectedTableCount { get; init; }
                }
                """
        };
    }

    private static FileModel CreateTextExtractedEventFile(string directory)
    {
        return new FileModel("TextExtractedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Interfaces;

                namespace OcrVision.Core.Events;

                /// <summary>
                /// Event raised when text has been extracted from a document.
                /// </summary>
                public sealed class TextExtractedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "OcrResult";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string DocumentId { get; init; }

                    public required string FileName { get; init; }

                    public int PageCount { get; init; }

                    public int ExtractedTextLength { get; init; }

                    public double Confidence { get; init; }

                    public string? Language { get; init; }
                }
                """
        };
    }

    private static FileModel CreateOcrResultDtoFile(string directory)
    {
        return new FileModel("OcrResultDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;

                namespace OcrVision.Core.DTOs;

                /// <summary>
                /// Data transfer object for OcrResult.
                /// </summary>
                public sealed class OcrResultDto
                {
                    public Guid OcrResultId { get; init; }

                    public required string DocumentId { get; init; }

                    public required string FileName { get; init; }

                    public string? ContentType { get; init; }

                    public required string ExtractedText { get; init; }

                    public double Confidence { get; init; }

                    public string? Language { get; init; }

                    public int PageCount { get; init; }

                    public OcrStatus Status { get; init; }

                    public string? ErrorMessage { get; init; }

                    public DateTime CreatedAt { get; init; }

                    public DateTime? CompletedAt { get; init; }

                    public IEnumerable<ExtractedDataDto>? ExtractedDataItems { get; init; }
                }
                """
        };
    }

    private static FileModel CreateExtractedDataDtoFile(string directory)
    {
        return new FileModel("ExtractedDataDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;

                namespace OcrVision.Core.DTOs;

                /// <summary>
                /// Data transfer object for ExtractedData.
                /// </summary>
                public sealed class ExtractedDataDto
                {
                    public Guid ExtractedDataId { get; init; }

                    public Guid OcrResultId { get; init; }

                    public required string FieldName { get; init; }

                    public required string FieldValue { get; init; }

                    public required string DataType { get; init; }

                    public double Confidence { get; init; }

                    public int? PageNumber { get; init; }

                    public BoundingBox? BoundingBox { get; init; }
                }
                """
        };
    }

    private static FileModel CreateDocumentAnalysisDtoFile(string directory)
    {
        return new FileModel("DocumentAnalysisDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;

                namespace OcrVision.Core.DTOs;

                /// <summary>
                /// Data transfer object for DocumentAnalysis.
                /// </summary>
                public sealed class DocumentAnalysisDto
                {
                    public Guid DocumentAnalysisId { get; init; }

                    public required string DocumentId { get; init; }

                    public required string FileName { get; init; }

                    public required string AnalysisType { get; init; }

                    public string? DocumentType { get; init; }

                    public Dictionary<string, object>? Metadata { get; init; }

                    public List<DetectedObject>? DetectedObjects { get; init; }

                    public List<DetectedTable>? DetectedTables { get; init; }

                    public double OverallConfidence { get; init; }

                    public AnalysisStatus Status { get; init; }

                    public string? ErrorMessage { get; init; }

                    public DateTime CreatedAt { get; init; }

                    public DateTime? CompletedAt { get; init; }
                }
                """
        };
    }

    private static FileModel CreateAnalyzeDocumentRequestFile(string directory)
    {
        return new FileModel("AnalyzeDocumentRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace OcrVision.Core.DTOs;

                /// <summary>
                /// Request model for analyzing a document.
                /// </summary>
                public sealed class AnalyzeDocumentRequest
                {
                    [Required]
                    public required string AnalysisType { get; init; }

                    public bool ExtractText { get; init; } = true;

                    public bool DetectObjects { get; init; } = false;

                    public bool DetectTables { get; init; } = false;

                    public bool ClassifyDocument { get; init; } = false;

                    public IEnumerable<string>? FieldsToExtract { get; init; }

                    public string? Language { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateOcrVisionDbContextFile(string directory)
    {
        return new FileModel("OcrVisionDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace OcrVision.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for OcrVision microservice.
                /// </summary>
                public class OcrVisionDbContext : DbContext
                {
                    public OcrVisionDbContext(DbContextOptions<OcrVisionDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<OcrResult> OcrResults => Set<OcrResult>();

                    public DbSet<ExtractedData> ExtractedDataItems => Set<ExtractedData>();

                    public DbSet<DocumentAnalysis> DocumentAnalyses => Set<DocumentAnalysis>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OcrVisionDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateOcrResultConfigurationFile(string directory)
    {
        return new FileModel("OcrResultConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace OcrVision.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for OcrResult.
                /// </summary>
                public class OcrResultConfiguration : IEntityTypeConfiguration<OcrResult>
                {
                    public void Configure(EntityTypeBuilder<OcrResult> builder)
                    {
                        builder.HasKey(o => o.OcrResultId);

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
                            .HasColumnType("nvarchar(max)");

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
                        builder.HasIndex(o => o.CreatedAt);
                    }
                }
                """
        };
    }

    private static FileModel CreateExtractedDataConfigurationFile(string directory)
    {
        return new FileModel("ExtractedDataConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace OcrVision.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for ExtractedData.
                /// </summary>
                public class ExtractedDataConfiguration : IEntityTypeConfiguration<ExtractedData>
                {
                    public void Configure(EntityTypeBuilder<ExtractedData> builder)
                    {
                        builder.HasKey(e => e.ExtractedDataId);

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
                            .HasColumnType("nvarchar(max)");

                        builder.Property(e => e.CreatedAt)
                            .IsRequired();

                        builder.HasIndex(e => e.OcrResultId);
                        builder.HasIndex(e => e.FieldName);
                    }
                }
                """
        };
    }

    private static FileModel CreateDocumentAnalysisConfigurationFile(string directory)
    {
        return new FileModel("DocumentAnalysisConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace OcrVision.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for DocumentAnalysis.
                /// </summary>
                public class DocumentAnalysisConfiguration : IEntityTypeConfiguration<DocumentAnalysis>
                {
                    public void Configure(EntityTypeBuilder<DocumentAnalysis> builder)
                    {
                        builder.HasKey(d => d.DocumentAnalysisId);

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
                            .HasColumnType("nvarchar(max)");

                        builder.Property(d => d.DetectedObjects)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(d => d.DetectedTables)
                            .HasColumnType("nvarchar(max)");

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
                        builder.HasIndex(d => d.CreatedAt);
                    }
                }
                """
        };
    }

    private static FileModel CreateOcrServiceFile(string directory)
    {
        return new FileModel("OcrService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;
                using OcrVision.Core.Interfaces;
                using OcrVision.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Logging;

                namespace OcrVision.Infrastructure.Services;

                /// <summary>
                /// Service implementation for OCR operations.
                /// </summary>
                public class OcrService : IOcrService
                {
                    private readonly OcrVisionDbContext context;
                    private readonly ILogger<OcrService> logger;

                    public OcrService(OcrVisionDbContext context, ILogger<OcrService> logger)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public async Task<OcrResult> ProcessDocumentAsync(Stream documentStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
                    {
                        var ocrResult = new OcrResult
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

                        logger.LogInformation("Started OCR processing for document: {FileName}", fileName);

                        try
                        {
                            // Simulate OCR processing - in production, integrate with actual OCR service
                            using var memoryStream = new MemoryStream();
                            await documentStream.CopyToAsync(memoryStream, cancellationToken);

                            ocrResult.ExtractedText = "Extracted text from document...";
                            ocrResult.Confidence = 0.95;
                            ocrResult.PageCount = 1;
                            ocrResult.Language = "en";
                            ocrResult.Status = OcrStatus.Completed;
                            ocrResult.CompletedAt = DateTime.UtcNow;

                            logger.LogInformation("OCR processing completed for document: {DocumentId}", ocrResult.DocumentId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "OCR processing failed for document: {FileName}", fileName);
                            ocrResult.Status = OcrStatus.Failed;
                            ocrResult.ErrorMessage = ex.Message;
                        }

                        context.OcrResults.Update(ocrResult);
                        await context.SaveChangesAsync(cancellationToken);

                        return ocrResult;
                    }

                    public async Task<OcrResult?> GetResultByIdAsync(Guid ocrResultId, CancellationToken cancellationToken = default)
                    {
                        return await context.OcrResults
                            .Include(o => o.ExtractedDataItems)
                            .FirstOrDefaultAsync(o => o.OcrResultId == ocrResultId, cancellationToken);
                    }

                    public async Task<IEnumerable<OcrResult>> GetResultsByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
                    {
                        return await context.OcrResults
                            .Include(o => o.ExtractedDataItems)
                            .Where(o => o.DocumentId == documentId)
                            .OrderByDescending(o => o.CreatedAt)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<ExtractedData>> ExtractStructuredDataAsync(Guid ocrResultId, IEnumerable<string> fieldNames, CancellationToken cancellationToken = default)
                    {
                        var ocrResult = await GetResultByIdAsync(ocrResultId, cancellationToken);

                        if (ocrResult == null)
                        {
                            throw new InvalidOperationException($"OCR result {ocrResultId} not found");
                        }

                        var extractedData = new List<ExtractedData>();

                        foreach (var fieldName in fieldNames)
                        {
                            var data = new ExtractedData
                            {
                                ExtractedDataId = Guid.NewGuid(),
                                OcrResultId = ocrResultId,
                                FieldName = fieldName,
                                FieldValue = "Extracted value",
                                DataType = "string",
                                Confidence = 0.9,
                                CreatedAt = DateTime.UtcNow
                            };

                            extractedData.Add(data);
                        }

                        await context.ExtractedDataItems.AddRangeAsync(extractedData, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        return extractedData;
                    }

                    public async Task<OcrResult> UpdateStatusAsync(Guid ocrResultId, OcrStatus status, string? errorMessage = null, CancellationToken cancellationToken = default)
                    {
                        var ocrResult = await context.OcrResults.FindAsync(new object[] { ocrResultId }, cancellationToken)
                            ?? throw new InvalidOperationException($"OCR result {ocrResultId} not found");

                        ocrResult.Status = status;
                        ocrResult.ErrorMessage = errorMessage;

                        if (status == OcrStatus.Completed)
                        {
                            ocrResult.CompletedAt = DateTime.UtcNow;
                        }

                        context.OcrResults.Update(ocrResult);
                        await context.SaveChangesAsync(cancellationToken);

                        return ocrResult;
                    }
                }
                """
        };
    }

    private static FileModel CreateVisionServiceFile(string directory)
    {
        return new FileModel("VisionService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Entities;
                using OcrVision.Core.Interfaces;
                using OcrVision.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Logging;

                namespace OcrVision.Infrastructure.Services;

                /// <summary>
                /// Service implementation for document vision and analysis operations.
                /// </summary>
                public class VisionService : IVisionService
                {
                    private readonly OcrVisionDbContext context;
                    private readonly ILogger<VisionService> logger;

                    public VisionService(OcrVisionDbContext context, ILogger<VisionService> logger)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public async Task<DocumentAnalysis> AnalyzeDocumentAsync(Stream documentStream, string fileName, string analysisType, CancellationToken cancellationToken = default)
                    {
                        var analysis = new DocumentAnalysis
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

                        logger.LogInformation("Started document analysis for: {FileName}, Type: {AnalysisType}", fileName, analysisType);

                        try
                        {
                            // Simulate document analysis - in production, integrate with actual vision service
                            using var memoryStream = new MemoryStream();
                            await documentStream.CopyToAsync(memoryStream, cancellationToken);

                            analysis.DocumentType = "invoice";
                            analysis.OverallConfidence = 0.92;
                            analysis.DetectedObjects = new List<DetectedObject>
                            {
                                new DetectedObject
                                {
                                    ObjectType = "logo",
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

                            logger.LogInformation("Document analysis completed for: {DocumentId}", analysis.DocumentId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Document analysis failed for: {FileName}", fileName);
                            analysis.Status = AnalysisStatus.Failed;
                            analysis.ErrorMessage = ex.Message;
                        }

                        context.DocumentAnalyses.Update(analysis);
                        await context.SaveChangesAsync(cancellationToken);

                        return analysis;
                    }

                    public async Task<DocumentAnalysis?> GetAnalysisByIdAsync(Guid analysisId, CancellationToken cancellationToken = default)
                    {
                        return await context.DocumentAnalyses
                            .FirstOrDefaultAsync(d => d.DocumentAnalysisId == analysisId, cancellationToken);
                    }

                    public async Task<IEnumerable<DocumentAnalysis>> GetAnalysesByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
                    {
                        return await context.DocumentAnalyses
                            .Where(d => d.DocumentId == documentId)
                            .OrderByDescending(d => d.CreatedAt)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<DetectedObject>> DetectObjectsAsync(Stream documentStream, CancellationToken cancellationToken = default)
                    {
                        logger.LogDebug("Detecting objects in document");

                        // Simulate object detection
                        await Task.Delay(100, cancellationToken);

                        return new List<DetectedObject>
                        {
                            new DetectedObject
                            {
                                ObjectType = "signature",
                                Confidence = 0.88,
                                BoundingBox = new BoundingBox { X = 400, Y = 600, Width = 150, Height = 50 }
                            },
                            new DetectedObject
                            {
                                ObjectType = "stamp",
                                Confidence = 0.92,
                                BoundingBox = new BoundingBox { X = 500, Y = 500, Width = 80, Height = 80 }
                            }
                        };
                    }

                    public async Task<IEnumerable<DetectedTable>> DetectTablesAsync(Stream documentStream, CancellationToken cancellationToken = default)
                    {
                        logger.LogDebug("Detecting tables in document");

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
                        };
                    }

                    public async Task<string> ClassifyDocumentAsync(Stream documentStream, CancellationToken cancellationToken = default)
                    {
                        logger.LogDebug("Classifying document");

                        // Simulate document classification
                        await Task.Delay(100, cancellationToken);

                        return "invoice";
                    }
                }
                """
        };
    }

    private static FileModel CreateInfrastructureConfigureServicesFile(string directory)
    {
        return new FileModel("ConfigureServices", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.Interfaces;
                using OcrVision.Infrastructure.Data;
                using OcrVision.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring OcrVision infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds OcrVision infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddOcrVisionInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<OcrVisionDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("OcrVisionDb") ??
                                @"Server=.\SQLEXPRESS;Database=OcrVisionDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IOcrService, OcrService>();
                        services.AddScoped<IVisionService, VisionService>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateOcrControllerFile(string directory)
    {
        return new FileModel("OcrController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using OcrVision.Core.DTOs;
                using OcrVision.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace OcrVision.Api.Controllers;

                /// <summary>
                /// Controller for OCR and document analysis operations.
                /// </summary>
                [ApiController]
                [Route("api/ocr")]
                [Authorize]
                public class OcrController : ControllerBase
                {
                    private readonly IOcrService ocrService;
                    private readonly IVisionService visionService;
                    private readonly ILogger<OcrController> logger;

                    public OcrController(
                        IOcrService ocrService,
                        IVisionService visionService,
                        ILogger<OcrController> logger)
                    {
                        this.ocrService = ocrService;
                        this.visionService = visionService;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Analyze a document with OCR and vision processing.
                    /// </summary>
                    [HttpPost("analyze")]
                    [ProducesResponseType(typeof(OcrResultDto), StatusCodes.Status202Accepted)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<OcrResultDto>> AnalyzeDocument(
                        IFormFile file,
                        [FromForm] AnalyzeDocumentRequest request,
                        CancellationToken cancellationToken)
                    {
                        if (file == null || file.Length == 0)
                        {
                            return BadRequest("No file provided");
                        }

                        logger.LogInformation("Received document for analysis: {FileName}", file.FileName);

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

                        return AcceptedAtAction(nameof(GetResultById), new { id = ocrResult.OcrResultId }, response);
                    }

                    /// <summary>
                    /// Get OCR result by ID.
                    /// </summary>
                    [HttpGet("results/{id:guid}")]
                    [ProducesResponseType(typeof(OcrResultDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<OcrResultDto>> GetResultById(Guid id, CancellationToken cancellationToken)
                    {
                        var ocrResult = await ocrService.GetResultByIdAsync(id, cancellationToken);

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

                        return Ok(response);
                    }

                    /// <summary>
                    /// Get document analysis by ID.
                    /// </summary>
                    [HttpGet("analysis/{id:guid}")]
                    [ProducesResponseType(typeof(DocumentAnalysisDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<DocumentAnalysisDto>> GetAnalysisById(Guid id, CancellationToken cancellationToken)
                    {
                        var analysis = await visionService.GetAnalysisByIdAsync(id, cancellationToken);

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

                        return Ok(response);
                    }

                    /// <summary>
                    /// Extract structured data from OCR result.
                    /// </summary>
                    [HttpPost("results/{id:guid}/extract")]
                    [ProducesResponseType(typeof(IEnumerable<ExtractedDataDto>), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<IEnumerable<ExtractedDataDto>>> ExtractData(
                        Guid id,
                        [FromBody] IEnumerable<string> fieldNames,
                        CancellationToken cancellationToken)
                    {
                        try
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
                        }
                    }
                }
                """
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
