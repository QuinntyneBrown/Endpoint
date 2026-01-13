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

namespace Endpoint.Engineering.Microservices.Calculation;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class CalculationArtifactFactory : ICalculationArtifactFactory
{
    private readonly ILogger<CalculationArtifactFactory> logger;

    public CalculationArtifactFactory(ILogger<CalculationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Calculation.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateCalculationFile(entitiesDir));
        project.Files.Add(CreateFormulaFile(entitiesDir));
        project.Files.Add(CreateScenarioFile(entitiesDir));
        project.Files.Add(CreateCalculationHistoryFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateICalculationRepositoryFile(interfacesDir));
        project.Files.Add(CreateIFormulaEngineFile(interfacesDir));
        project.Files.Add(CreateISimulationServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateCalculationCompletedEventFile(eventsDir));
        project.Files.Add(CreateScenarioCreatedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateCalculationDtoFile(dtosDir));
        project.Files.Add(CreateComputeCalculationRequestFile(dtosDir));
        project.Files.Add(CreateCreateScenarioRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Calculation.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");

        project.Files.Add(CreateCalculationDbContextFile(dataDir));
        project.Files.Add(CreateCalculationRepositoryFile(repositoriesDir));
        project.Files.Add(CreateConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Calculation.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateCalculationsControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files - Entities

    // Keep as FileModel because it contains enum CalculationStatus
    private static FileModel CreateCalculationFile(string directory)
    {
        return new FileModel("Calculation", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.Entities;

                public class Calculation
                {
                    public Guid CalculationId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public Guid FormulaId { get; set; }
                    public Formula Formula { get; set; } = null!;
                    public CalculationStatus Status { get; set; } = CalculationStatus.Pending;
                    public decimal? Result { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? CompletedAt { get; set; }
                    public ICollection<CalculationHistory> History { get; set; } = new List<CalculationHistory>();
                }

                public enum CalculationStatus { Pending, Running, Completed, Failed }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateFormulaFile(string directory)
    {
        var classModel = new ClassModel("Formula");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "FormulaId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Expression", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Calculation")] }, "Calculations", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Calculation>()" });

        return new CodeFileModel<ClassModel>(classModel, "Formula", directory, CSharp)
        {
            Namespace = "Calculation.Core.Entities"
        };
    }

    // Keep as FileModel because it contains enum ScenarioType
    private static FileModel CreateScenarioFile(string directory)
    {
        return new FileModel("Scenario", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.Entities;

                public class Scenario
                {
                    public Guid ScenarioId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public required string InputParameters { get; set; }
                    public ScenarioType Type { get; set; } = ScenarioType.Standard;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public ICollection<Calculation> Calculations { get; set; } = new List<Calculation>();
                }

                public enum ScenarioType { Standard, Simulation, WhatIf }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateCalculationHistoryFile(string directory)
    {
        var classModel = new ClassModel("CalculationHistory");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "HistoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CalculationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Calculation"), "Calculation", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "InputValues", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal") { Nullable = true }, "OutputValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "ExecutedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "CalculationHistory", directory, CSharp)
        {
            Namespace = "Calculation.Core.Entities"
        };
    }

    #endregion

    #region Core Layer Files - Interfaces

    private static CodeFileModel<InterfaceModel> CreateICalculationRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ICalculationRepository");

        interfaceModel.Usings.Add(new UsingModel("Calculation.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Calculation") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "calculationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Calculation")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Calculation")] },
            Params =
            [
                new ParamModel { Name = "calculation", Type = new TypeModel("Entities.Calculation") },
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
                new ParamModel { Name = "calculation", Type = new TypeModel("Entities.Calculation") },
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
                new ParamModel { Name = "calculationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ICalculationRepository", directory, CSharp)
        {
            Namespace = "Calculation.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIFormulaEngineFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IFormulaEngine");

        interfaceModel.Usings.Add(new UsingModel("Calculation.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "EvaluateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("decimal")] },
            Params =
            [
                new ParamModel { Name = "formula", Type = new TypeModel("Formula") },
                new ParamModel { Name = "variables", Type = new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("decimal")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ValidateExpressionAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "expression", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ExtractVariablesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] }] },
            Params =
            [
                new ParamModel { Name = "expression", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IFormulaEngine", directory, CSharp)
        {
            Namespace = "Calculation.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateISimulationServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ISimulationService");

        interfaceModel.Usings.Add(new UsingModel("Calculation.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateScenarioAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Scenario")] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "inputParameters", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RunScenarioAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Calculation")] }] },
            Params =
            [
                new ParamModel { Name = "scenarioId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetScenariosAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Scenario")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ISimulationService", directory, CSharp)
        {
            Namespace = "Calculation.Core.Interfaces"
        };
    }

    #endregion

    #region Core Layer Files - Events

    private static CodeFileModel<ClassModel> CreateCalculationCompletedEventFile(string directory)
    {
        var classModel = new ClassModel("CalculationCompletedEvent");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CalculationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal") { Nullable = true }, "Result", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "CalculationCompletedEvent", directory, CSharp)
        {
            Namespace = "Calculation.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateScenarioCreatedEventFile(string directory)
    {
        var classModel = new ClassModel("ScenarioCreatedEvent");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ScenarioId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ScenarioType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "ScenarioCreatedEvent", directory, CSharp)
        {
            Namespace = "Calculation.Core.Events"
        };
    }

    #endregion

    #region Core Layer Files - DTOs

    private static CodeFileModel<ClassModel> CreateCalculationDtoFile(string directory)
    {
        var classModel = new ClassModel("CalculationDto");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CalculationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal") { Nullable = true }, "Result", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "CalculationDto", directory, CSharp)
        {
            Namespace = "Calculation.Core.DTOs"
        };
    }

    // Keep as FileModel due to [Required] data annotation attributes
    private static FileModel CreateComputeCalculationRequestFile(string directory)
    {
        return new FileModel("ComputeCalculationRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Calculation.Core.DTOs;

                public sealed class ComputeCalculationRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public Guid FormulaId { get; init; }

                    [Required]
                    public required IDictionary<string, decimal> Variables { get; init; }
                }
                """
        };
    }

    // Keep as FileModel due to [Required] data annotation attributes
    private static FileModel CreateCreateScenarioRequestFile(string directory)
    {
        return new FileModel("CreateScenarioRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Calculation.Core.DTOs;

                public sealed class CreateScenarioRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    [Required]
                    public required string InputParameters { get; init; }

                    public string Type { get; init; } = "Standard";
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    // Keep as FileModel due to complex OnModelCreating configuration
    private static FileModel CreateCalculationDbContextFile(string directory)
    {
        return new FileModel("CalculationDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Calculation.Core.Entities;

                namespace Calculation.Infrastructure.Data;

                public class CalculationDbContext : DbContext
                {
                    public CalculationDbContext(DbContextOptions<CalculationDbContext> options) : base(options) { }

                    public DbSet<Core.Entities.Calculation> Calculations => Set<Core.Entities.Calculation>();
                    public DbSet<Formula> Formulas => Set<Formula>();
                    public DbSet<Scenario> Scenarios => Set<Scenario>();
                    public DbSet<CalculationHistory> CalculationHistories => Set<CalculationHistory>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Core.Entities.Calculation>(entity =>
                        {
                            entity.HasKey(c => c.CalculationId);
                            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                            entity.HasOne(c => c.Formula).WithMany(f => f.Calculations).HasForeignKey(c => c.FormulaId);
                        });

                        modelBuilder.Entity<Formula>(entity =>
                        {
                            entity.HasKey(f => f.FormulaId);
                            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
                            entity.Property(f => f.Expression).IsRequired().HasMaxLength(2000);
                        });

                        modelBuilder.Entity<Scenario>(entity =>
                        {
                            entity.HasKey(s => s.ScenarioId);
                            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                            entity.Property(s => s.InputParameters).IsRequired();
                        });

                        modelBuilder.Entity<CalculationHistory>(entity =>
                        {
                            entity.HasKey(h => h.HistoryId);
                            entity.HasOne(h => h.Calculation).WithMany(c => c.History).HasForeignKey(h => h.CalculationId);
                        });
                    }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateCalculationRepositoryFile(string directory)
    {
        var classModel = new ClassModel("CalculationRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Calculation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Calculation.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ICalculationRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("CalculationDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "CalculationRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("CalculationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Calculation") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "calculationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Calculations.Include(c => c.Formula).Include(c => c.History).FirstOrDefaultAsync(c => c.CalculationId == calculationId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Calculation")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Calculations.Include(c => c.Formula).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Calculation")] },
            Params =
            [
                new ParamModel { Name = "calculation", Type = new TypeModel("Core.Entities.Calculation") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"calculation.CalculationId = Guid.NewGuid();
        await context.Calculations.AddAsync(calculation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return calculation;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "calculation", Type = new TypeModel("Core.Entities.Calculation") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Calculations.Update(calculation);
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
                new ParamModel { Name = "calculationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var calculation = await context.Calculations.FindAsync(new object[] { calculationId }, cancellationToken);
        if (calculation != null)
        {
            context.Calculations.Remove(calculation);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "CalculationRepository", directory, CSharp)
        {
            Namespace = "Calculation.Infrastructure.Repositories"
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
        classModel.Usings.Add(new UsingModel("Calculation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Calculation.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Calculation.Infrastructure.Repositories"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddCalculationInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<CalculationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""CalculationDb"") ??
                @""Server=.\SQLEXPRESS;Database=CalculationDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ICalculationRepository, CalculationRepository>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateCalculationsControllerFile(string directory)
    {
        var classModel = new ClassModel("CalculationsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Calculation.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Calculation.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("ICalculationRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "formulaEngine", Type = new TypeModel("IFormulaEngine"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "simulationService", Type = new TypeModel("ISimulationService"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "CalculationsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("ICalculationRepository") },
                new ParamModel { Name = "formulaEngine", Type = new TypeModel("IFormulaEngine") },
                new ParamModel { Name = "simulationService", Type = new TypeModel("ISimulationService") }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.formulaEngine = formulaEngine;
        this.simulationService = simulationService;")
        };
        classModel.Constructors.Add(constructor);

        var computeMethod = new MethodModel
        {
            Name = "Compute",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("CalculationDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("ComputeCalculationRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var calculation = new Core.Entities.Calculation
        {
            Name = request.Name,
            FormulaId = request.FormulaId,
            Status = Core.Entities.CalculationStatus.Running
        };

        var created = await repository.AddAsync(calculation, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.CalculationId }, new CalculationDto
        {
            CalculationId = created.CalculationId,
            Name = created.Name,
            Status = created.Status.ToString(),
            CreatedAt = created.CreatedAt
        });")
        };
        computeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"compute\"" });
        classModel.Methods.Add(computeMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("CalculationDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var calculation = await repository.GetByIdAsync(id, cancellationToken);
        if (calculation == null) return NotFound();
        return Ok(new CalculationDto
        {
            CalculationId = calculation.CalculationId,
            Name = calculation.Name,
            Description = calculation.Description,
            Status = calculation.Status.ToString(),
            Result = calculation.Result,
            CreatedAt = calculation.CreatedAt,
            CompletedAt = calculation.CompletedAt
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var createScenarioMethod = new MethodModel
        {
            Name = "CreateScenario",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateScenarioRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var scenario = await simulationService.CreateScenarioAsync(request.Name, request.InputParameters, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = scenario.ScenarioId }, scenario);")
        };
        createScenarioMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"scenarios\"" });
        classModel.Methods.Add(createScenarioMethod);

        return new CodeFileModel<ClassModel>(classModel, "CalculationsController", directory, CSharp)
        {
            Namespace = "Calculation.Api.Controllers"
        };
    }

    // Keep as FileModel due to top-level statements
    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddCalculationInfrastructure(builder.Configuration);
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

    // Keep as FileModel for JSON format
    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "CalculationDb": "Server=.\\SQLEXPRESS;Database=CalculationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
