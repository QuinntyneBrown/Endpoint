// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Calculation;

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
        project.Files.Add(new FileModel("Calculation", entitiesDir, CSharp)
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
        });

        project.Files.Add(new FileModel("Formula", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.Entities;

                public class Formula
                {
                    public Guid FormulaId { get; set; }
                    public required string Name { get; set; }
                    public required string Expression { get; set; }
                    public string? Description { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public ICollection<Calculation> Calculations { get; set; } = new List<Calculation>();
                }
                """
        });

        project.Files.Add(new FileModel("Scenario", entitiesDir, CSharp)
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
        });

        project.Files.Add(new FileModel("CalculationHistory", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.Entities;

                public class CalculationHistory
                {
                    public Guid HistoryId { get; set; }
                    public Guid CalculationId { get; set; }
                    public Calculation Calculation { get; set; } = null!;
                    public required string InputValues { get; set; }
                    public decimal? OutputValue { get; set; }
                    public required string Status { get; set; }
                    public string? ErrorMessage { get; set; }
                    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ICalculationRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Calculation.Core.Entities;

                namespace Calculation.Core.Interfaces;

                public interface ICalculationRepository
                {
                    Task<Entities.Calculation?> GetByIdAsync(Guid calculationId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Calculation>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Entities.Calculation> AddAsync(Entities.Calculation calculation, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Entities.Calculation calculation, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid calculationId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IFormulaEngine", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Calculation.Core.Entities;

                namespace Calculation.Core.Interfaces;

                public interface IFormulaEngine
                {
                    Task<decimal> EvaluateAsync(Formula formula, IDictionary<string, decimal> variables, CancellationToken cancellationToken = default);
                    Task<bool> ValidateExpressionAsync(string expression, CancellationToken cancellationToken = default);
                    Task<IEnumerable<string>> ExtractVariablesAsync(string expression, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ISimulationService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Calculation.Core.Entities;

                namespace Calculation.Core.Interfaces;

                public interface ISimulationService
                {
                    Task<Scenario> CreateScenarioAsync(string name, string inputParameters, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Calculation>> RunScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Scenario>> GetScenariosAsync(CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("CalculationCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.Events;

                public sealed class CalculationCompletedEvent
                {
                    public Guid CalculationId { get; init; }
                    public decimal? Result { get; init; }
                    public required string Status { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ScenarioCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.Events;

                public sealed class ScenarioCreatedEvent
                {
                    public Guid ScenarioId { get; init; }
                    public required string Name { get; init; }
                    public required string ScenarioType { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("CalculationDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Calculation.Core.DTOs;

                public sealed class CalculationDto
                {
                    public Guid CalculationId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public string Status { get; init; } = "Pending";
                    public decimal? Result { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? CompletedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("ComputeCalculationRequest", dtosDir, CSharp)
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
        });

        project.Files.Add(new FileModel("CreateScenarioRequest", dtosDir, CSharp)
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
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Calculation.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");

        project.Files.Add(new FileModel("CalculationDbContext", dataDir, CSharp)
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
        });

        project.Files.Add(new FileModel("CalculationRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Calculation.Core.Interfaces;
                using Calculation.Infrastructure.Data;

                namespace Calculation.Infrastructure.Repositories;

                public class CalculationRepository : ICalculationRepository
                {
                    private readonly CalculationDbContext context;

                    public CalculationRepository(CalculationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Core.Entities.Calculation?> GetByIdAsync(Guid calculationId, CancellationToken cancellationToken = default)
                        => await context.Calculations.Include(c => c.Formula).Include(c => c.History).FirstOrDefaultAsync(c => c.CalculationId == calculationId, cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Calculation>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Calculations.Include(c => c.Formula).ToListAsync(cancellationToken);

                    public async Task<Core.Entities.Calculation> AddAsync(Core.Entities.Calculation calculation, CancellationToken cancellationToken = default)
                    {
                        calculation.CalculationId = Guid.NewGuid();
                        await context.Calculations.AddAsync(calculation, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return calculation;
                    }

                    public async Task UpdateAsync(Core.Entities.Calculation calculation, CancellationToken cancellationToken = default)
                    {
                        context.Calculations.Update(calculation);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid calculationId, CancellationToken cancellationToken = default)
                    {
                        var calculation = await context.Calculations.FindAsync(new object[] { calculationId }, cancellationToken);
                        if (calculation != null)
                        {
                            context.Calculations.Remove(calculation);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;
                using Calculation.Core.Interfaces;
                using Calculation.Infrastructure.Data;
                using Calculation.Infrastructure.Repositories;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddCalculationInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<CalculationDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("CalculationDb") ??
                                @"Server=.\SQLEXPRESS;Database=CalculationDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ICalculationRepository, CalculationRepository>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Calculation.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("CalculationsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Calculation.Core.DTOs;
                using Calculation.Core.Interfaces;

                namespace Calculation.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class CalculationsController : ControllerBase
                {
                    private readonly ICalculationRepository repository;
                    private readonly IFormulaEngine formulaEngine;
                    private readonly ISimulationService simulationService;

                    public CalculationsController(ICalculationRepository repository, IFormulaEngine formulaEngine, ISimulationService simulationService)
                    {
                        this.repository = repository;
                        this.formulaEngine = formulaEngine;
                        this.simulationService = simulationService;
                    }

                    [HttpPost("compute")]
                    public async Task<ActionResult<CalculationDto>> Compute([FromBody] ComputeCalculationRequest request, CancellationToken cancellationToken)
                    {
                        var calculation = new Core.Entities.Calculation
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
                        });
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<CalculationDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var calculation = await repository.GetByIdAsync(id, cancellationToken);
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
                        });
                    }

                    [HttpPost("scenarios")]
                    public async Task<ActionResult> CreateScenario([FromBody] CreateScenarioRequest request, CancellationToken cancellationToken)
                    {
                        var scenario = await simulationService.CreateScenarioAsync(request.Name, request.InputParameters, cancellationToken);
                        return CreatedAtAction(nameof(GetById), new { id = scenario.ScenarioId }, scenario);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
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
        });

        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
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
        });
    }
}
