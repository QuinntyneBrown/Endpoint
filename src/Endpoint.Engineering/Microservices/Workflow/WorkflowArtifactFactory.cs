// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Workflow;

public class WorkflowArtifactFactory : IWorkflowArtifactFactory
{
    private readonly ILogger<WorkflowArtifactFactory> logger;

    public WorkflowArtifactFactory(ILogger<WorkflowArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Workflow.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Workflow", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class Workflow
                {
                    public Guid WorkflowId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public required string Version { get; set; }
                    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
                    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
                }

                public enum WorkflowStatus { Draft, Published, Archived }
                """
        });

        project.Files.Add(new FileModel("WorkflowInstance", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class WorkflowInstance
                {
                    public Guid InstanceId { get; set; }
                    public Guid WorkflowId { get; set; }
                    public Workflow Workflow { get; set; } = null!;
                    public Guid? CurrentStepId { get; set; }
                    public WorkflowStep? CurrentStep { get; set; }
                    public InstanceStatus Status { get; set; } = InstanceStatus.Running;
                    public string? ContextData { get; set; }
                    public Guid? InitiatedBy { get; set; }
                    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? CompletedAt { get; set; }
                    public ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
                    public ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
                }

                public enum InstanceStatus { Running, Paused, Completed, Failed, Cancelled }
                """
        });

        project.Files.Add(new FileModel("WorkflowStep", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class WorkflowStep
                {
                    public Guid StepId { get; set; }
                    public Guid WorkflowId { get; set; }
                    public Workflow Workflow { get; set; } = null!;
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public StepType Type { get; set; } = StepType.Task;
                    public int Order { get; set; }
                    public string? Configuration { get; set; }
                    public Guid? NextStepId { get; set; }
                    public bool RequiresApproval { get; set; } = false;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }

                public enum StepType { Task, Decision, Approval, Notification, Integration }
                """
        });

        project.Files.Add(new FileModel("ApprovalRequest", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class ApprovalRequest
                {
                    public Guid ApprovalId { get; set; }
                    public Guid InstanceId { get; set; }
                    public WorkflowInstance Instance { get; set; } = null!;
                    public Guid StepId { get; set; }
                    public WorkflowStep Step { get; set; } = null!;
                    public Guid? AssignedTo { get; set; }
                    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
                    public string? Comments { get; set; }
                    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? RespondedAt { get; set; }
                    public Guid? RespondedBy { get; set; }
                }

                public enum ApprovalStatus { Pending, Approved, Rejected, Escalated }
                """
        });

        project.Files.Add(new FileModel("WorkflowHistory", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class WorkflowHistory
                {
                    public Guid HistoryId { get; set; }
                    public Guid InstanceId { get; set; }
                    public WorkflowInstance Instance { get; set; } = null!;
                    public Guid? StepId { get; set; }
                    public required string Action { get; set; }
                    public required string FromState { get; set; }
                    public required string ToState { get; set; }
                    public string? Details { get; set; }
                    public Guid? PerformedBy { get; set; }
                    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IWorkflowRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Workflow.Core.Entities;

                namespace Workflow.Core.Interfaces;

                public interface IWorkflowRepository
                {
                    Task<Entities.Workflow?> GetByIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Workflow>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Workflow>> GetActiveAsync(CancellationToken cancellationToken = default);
                    Task<Entities.Workflow> AddAsync(Entities.Workflow workflow, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Entities.Workflow workflow, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid workflowId, CancellationToken cancellationToken = default);
                    Task<WorkflowInstance?> GetInstanceByIdAsync(Guid instanceId, CancellationToken cancellationToken = default);
                    Task<WorkflowInstance> AddInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
                    Task UpdateInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
                    Task<ApprovalRequest?> GetApprovalByIdAsync(Guid approvalId, CancellationToken cancellationToken = default);
                    Task UpdateApprovalAsync(ApprovalRequest approval, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IWorkflowEngine", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Workflow.Core.Entities;

                namespace Workflow.Core.Interfaces;

                public interface IWorkflowEngine
                {
                    Task<WorkflowInstance> StartWorkflowAsync(Guid workflowId, string? contextData = null, Guid? initiatedBy = null, CancellationToken cancellationToken = default);
                    Task<WorkflowInstance> TransitionAsync(Guid instanceId, string? transitionData = null, CancellationToken cancellationToken = default);
                    Task<WorkflowInstance> PauseAsync(Guid instanceId, CancellationToken cancellationToken = default);
                    Task<WorkflowInstance> ResumeAsync(Guid instanceId, CancellationToken cancellationToken = default);
                    Task<WorkflowInstance> CancelAsync(Guid instanceId, string? reason = null, CancellationToken cancellationToken = default);
                    Task<bool> CanTransitionAsync(Guid instanceId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IStateManager", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Workflow.Core.Entities;

                namespace Workflow.Core.Interfaces;

                public interface IStateManager
                {
                    Task<WorkflowStep?> GetNextStepAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
                    Task<bool> ValidateTransitionAsync(WorkflowInstance instance, Guid targetStepId, CancellationToken cancellationToken = default);
                    Task RecordStateChangeAsync(Guid instanceId, string fromState, string toState, string action, Guid? performedBy = null, string? details = null, CancellationToken cancellationToken = default);
                    Task<IEnumerable<WorkflowHistory>> GetHistoryAsync(Guid instanceId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("WorkflowStartedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class WorkflowStartedEvent
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string WorkflowName { get; init; }
                    public Guid? InitiatedBy { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class WorkflowCompletedEvent
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string WorkflowName { get; init; }
                    public required string FinalStatus { get; init; }
                    public DateTime StartedAt { get; init; }
                    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ApprovalRequestedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class ApprovalRequestedEvent
                {
                    public Guid ApprovalId { get; init; }
                    public Guid InstanceId { get; init; }
                    public Guid StepId { get; init; }
                    public required string StepName { get; init; }
                    public Guid? AssignedTo { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("StateChangedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class StateChangedEvent
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string FromState { get; init; }
                    public required string ToState { get; init; }
                    public Guid? StepId { get; init; }
                    public Guid? PerformedBy { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("WorkflowDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class WorkflowDto
                {
                    public Guid WorkflowId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public required string Version { get; init; }
                    public string Status { get; init; } = "Draft";
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public IEnumerable<WorkflowStepDto> Steps { get; init; } = [];
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowStepDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class WorkflowStepDto
                {
                    public Guid StepId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public string Type { get; init; } = "Task";
                    public int Order { get; init; }
                    public bool RequiresApproval { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowInstanceDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class WorkflowInstanceDto
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string WorkflowName { get; init; }
                    public Guid? CurrentStepId { get; init; }
                    public string? CurrentStepName { get; init; }
                    public string Status { get; init; } = "Running";
                    public DateTime StartedAt { get; init; }
                    public DateTime? CompletedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateWorkflowRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Workflow.Core.DTOs;

                public sealed class CreateWorkflowRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    [Required]
                    public required string Version { get; init; }

                    public IEnumerable<CreateWorkflowStepRequest> Steps { get; init; } = [];
                }

                public sealed class CreateWorkflowStepRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    public string Type { get; init; } = "Task";

                    public int Order { get; init; }

                    public bool RequiresApproval { get; init; }

                    public string? Configuration { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("TransitionRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class TransitionRequest
                {
                    public string? TransitionData { get; init; }
                    public Guid? PerformedBy { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("ApprovalResponse", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Workflow.Core.DTOs;

                public sealed class ApprovalResponse
                {
                    [Required]
                    public bool Approved { get; init; }

                    public string? Comments { get; init; }

                    public Guid? RespondedBy { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Workflow.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("WorkflowDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Workflow.Core.Entities;

                namespace Workflow.Infrastructure.Data;

                public class WorkflowDbContext : DbContext
                {
                    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

                    public DbSet<Core.Entities.Workflow> Workflows => Set<Core.Entities.Workflow>();
                    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
                    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
                    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
                    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Core.Entities.Workflow>(entity =>
                        {
                            entity.HasKey(w => w.WorkflowId);
                            entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
                            entity.Property(w => w.Version).IsRequired().HasMaxLength(50);
                            entity.HasMany(w => w.Steps).WithOne(s => s.Workflow).HasForeignKey(s => s.WorkflowId);
                            entity.HasMany(w => w.Instances).WithOne(i => i.Workflow).HasForeignKey(i => i.WorkflowId);
                        });

                        modelBuilder.Entity<WorkflowInstance>(entity =>
                        {
                            entity.HasKey(i => i.InstanceId);
                            entity.HasOne(i => i.CurrentStep).WithMany().HasForeignKey(i => i.CurrentStepId).OnDelete(DeleteBehavior.Restrict);
                            entity.HasMany(i => i.History).WithOne(h => h.Instance).HasForeignKey(h => h.InstanceId);
                            entity.HasMany(i => i.ApprovalRequests).WithOne(a => a.Instance).HasForeignKey(a => a.InstanceId);
                        });

                        modelBuilder.Entity<WorkflowStep>(entity =>
                        {
                            entity.HasKey(s => s.StepId);
                            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                        });

                        modelBuilder.Entity<ApprovalRequest>(entity =>
                        {
                            entity.HasKey(a => a.ApprovalId);
                            entity.HasOne(a => a.Step).WithMany().HasForeignKey(a => a.StepId).OnDelete(DeleteBehavior.Restrict);
                        });

                        modelBuilder.Entity<WorkflowHistory>(entity =>
                        {
                            entity.HasKey(h => h.HistoryId);
                            entity.Property(h => h.Action).IsRequired().HasMaxLength(100);
                            entity.Property(h => h.FromState).IsRequired().HasMaxLength(100);
                            entity.Property(h => h.ToState).IsRequired().HasMaxLength(100);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Workflow.Core.Entities;
                using Workflow.Core.Interfaces;
                using Workflow.Infrastructure.Data;

                namespace Workflow.Infrastructure.Repositories;

                public class WorkflowRepository : IWorkflowRepository
                {
                    private readonly WorkflowDbContext context;

                    public WorkflowRepository(WorkflowDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Core.Entities.Workflow?> GetByIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
                        => await context.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).FirstOrDefaultAsync(w => w.WorkflowId == workflowId, cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Workflow>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Workflow>> GetActiveAsync(CancellationToken cancellationToken = default)
                        => await context.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).Where(w => w.IsActive && w.Status == WorkflowStatus.Published).ToListAsync(cancellationToken);

                    public async Task<Core.Entities.Workflow> AddAsync(Core.Entities.Workflow workflow, CancellationToken cancellationToken = default)
                    {
                        workflow.WorkflowId = Guid.NewGuid();
                        foreach (var step in workflow.Steps)
                        {
                            step.StepId = Guid.NewGuid();
                            step.WorkflowId = workflow.WorkflowId;
                        }
                        await context.Workflows.AddAsync(workflow, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return workflow;
                    }

                    public async Task UpdateAsync(Core.Entities.Workflow workflow, CancellationToken cancellationToken = default)
                    {
                        workflow.UpdatedAt = DateTime.UtcNow;
                        context.Workflows.Update(workflow);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid workflowId, CancellationToken cancellationToken = default)
                    {
                        var workflow = await context.Workflows.FindAsync(new object[] { workflowId }, cancellationToken);
                        if (workflow != null)
                        {
                            context.Workflows.Remove(workflow);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<WorkflowInstance?> GetInstanceByIdAsync(Guid instanceId, CancellationToken cancellationToken = default)
                        => await context.WorkflowInstances.Include(i => i.Workflow).Include(i => i.CurrentStep).Include(i => i.History).FirstOrDefaultAsync(i => i.InstanceId == instanceId, cancellationToken);

                    public async Task<WorkflowInstance> AddInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
                    {
                        instance.InstanceId = Guid.NewGuid();
                        await context.WorkflowInstances.AddAsync(instance, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return instance;
                    }

                    public async Task UpdateInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
                    {
                        context.WorkflowInstances.Update(instance);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<ApprovalRequest?> GetApprovalByIdAsync(Guid approvalId, CancellationToken cancellationToken = default)
                        => await context.ApprovalRequests.Include(a => a.Instance).Include(a => a.Step).FirstOrDefaultAsync(a => a.ApprovalId == approvalId, cancellationToken);

                    public async Task UpdateApprovalAsync(ApprovalRequest approval, CancellationToken cancellationToken = default)
                    {
                        context.ApprovalRequests.Update(approval);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowEngine", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Workflow.Core.Entities;
                using Workflow.Core.Interfaces;

                namespace Workflow.Infrastructure.Services;

                public class WorkflowEngine : IWorkflowEngine
                {
                    private readonly IWorkflowRepository repository;
                    private readonly IStateManager stateManager;

                    public WorkflowEngine(IWorkflowRepository repository, IStateManager stateManager)
                    {
                        this.repository = repository;
                        this.stateManager = stateManager;
                    }

                    public async Task<WorkflowInstance> StartWorkflowAsync(Guid workflowId, string? contextData = null, Guid? initiatedBy = null, CancellationToken cancellationToken = default)
                    {
                        var workflow = await repository.GetByIdAsync(workflowId, cancellationToken)
                            ?? throw new InvalidOperationException($"Workflow {workflowId} not found");

                        var firstStep = workflow.Steps.OrderBy(s => s.Order).FirstOrDefault();

                        var instance = new WorkflowInstance
                        {
                            WorkflowId = workflowId,
                            CurrentStepId = firstStep?.StepId,
                            ContextData = contextData,
                            InitiatedBy = initiatedBy,
                            Status = InstanceStatus.Running
                        };

                        var created = await repository.AddInstanceAsync(instance, cancellationToken);

                        await stateManager.RecordStateChangeAsync(created.InstanceId, "None", "Started", "WorkflowStarted", initiatedBy, null, cancellationToken);

                        return created;
                    }

                    public async Task<WorkflowInstance> TransitionAsync(Guid instanceId, string? transitionData = null, CancellationToken cancellationToken = default)
                    {
                        var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                            ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                        if (instance.Status != InstanceStatus.Running)
                            throw new InvalidOperationException($"Instance is not in Running state");

                        var currentStepName = instance.CurrentStep?.Name ?? "Unknown";
                        var nextStep = await stateManager.GetNextStepAsync(instance, cancellationToken);

                        if (nextStep == null)
                        {
                            instance.Status = InstanceStatus.Completed;
                            instance.CompletedAt = DateTime.UtcNow;
                            instance.CurrentStepId = null;
                            await stateManager.RecordStateChangeAsync(instanceId, currentStepName, "Completed", "WorkflowCompleted", null, transitionData, cancellationToken);
                        }
                        else
                        {
                            instance.CurrentStepId = nextStep.StepId;
                            await stateManager.RecordStateChangeAsync(instanceId, currentStepName, nextStep.Name, "StepTransition", null, transitionData, cancellationToken);
                        }

                        await repository.UpdateInstanceAsync(instance, cancellationToken);
                        return instance;
                    }

                    public async Task<WorkflowInstance> PauseAsync(Guid instanceId, CancellationToken cancellationToken = default)
                    {
                        var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                            ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                        var previousStatus = instance.Status.ToString();
                        instance.Status = InstanceStatus.Paused;
                        await repository.UpdateInstanceAsync(instance, cancellationToken);
                        await stateManager.RecordStateChangeAsync(instanceId, previousStatus, "Paused", "WorkflowPaused", null, null, cancellationToken);

                        return instance;
                    }

                    public async Task<WorkflowInstance> ResumeAsync(Guid instanceId, CancellationToken cancellationToken = default)
                    {
                        var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                            ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                        if (instance.Status != InstanceStatus.Paused)
                            throw new InvalidOperationException("Instance is not paused");

                        instance.Status = InstanceStatus.Running;
                        await repository.UpdateInstanceAsync(instance, cancellationToken);
                        await stateManager.RecordStateChangeAsync(instanceId, "Paused", "Running", "WorkflowResumed", null, null, cancellationToken);

                        return instance;
                    }

                    public async Task<WorkflowInstance> CancelAsync(Guid instanceId, string? reason = null, CancellationToken cancellationToken = default)
                    {
                        var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                            ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                        var previousStatus = instance.Status.ToString();
                        instance.Status = InstanceStatus.Cancelled;
                        instance.CompletedAt = DateTime.UtcNow;
                        await repository.UpdateInstanceAsync(instance, cancellationToken);
                        await stateManager.RecordStateChangeAsync(instanceId, previousStatus, "Cancelled", "WorkflowCancelled", null, reason, cancellationToken);

                        return instance;
                    }

                    public async Task<bool> CanTransitionAsync(Guid instanceId, CancellationToken cancellationToken = default)
                    {
                        var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken);
                        if (instance == null || instance.Status != InstanceStatus.Running) return false;

                        if (instance.CurrentStep?.RequiresApproval == true)
                        {
                            var approval = instance.ApprovalRequests.FirstOrDefault(a => a.StepId == instance.CurrentStepId && a.Status == ApprovalStatus.Pending);
                            if (approval != null) return false;
                        }

                        return true;
                    }
                }
                """
        });

        project.Files.Add(new FileModel("StateManager", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Workflow.Core.Entities;
                using Workflow.Core.Interfaces;
                using Workflow.Infrastructure.Data;

                namespace Workflow.Infrastructure.Services;

                public class StateManager : IStateManager
                {
                    private readonly WorkflowDbContext context;

                    public StateManager(WorkflowDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<WorkflowStep?> GetNextStepAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
                    {
                        if (instance.CurrentStep?.NextStepId != null)
                        {
                            return await context.WorkflowSteps.FindAsync(new object[] { instance.CurrentStep.NextStepId }, cancellationToken);
                        }

                        var workflow = await context.Workflows.Include(w => w.Steps).FirstOrDefaultAsync(w => w.WorkflowId == instance.WorkflowId, cancellationToken);
                        if (workflow == null) return null;

                        var orderedSteps = workflow.Steps.OrderBy(s => s.Order).ToList();
                        var currentIndex = orderedSteps.FindIndex(s => s.StepId == instance.CurrentStepId);

                        if (currentIndex >= 0 && currentIndex < orderedSteps.Count - 1)
                        {
                            return orderedSteps[currentIndex + 1];
                        }

                        return null;
                    }

                    public async Task<bool> ValidateTransitionAsync(WorkflowInstance instance, Guid targetStepId, CancellationToken cancellationToken = default)
                    {
                        var targetStep = await context.WorkflowSteps.FindAsync(new object[] { targetStepId }, cancellationToken);
                        if (targetStep == null || targetStep.WorkflowId != instance.WorkflowId) return false;

                        if (instance.CurrentStep?.NextStepId == targetStepId) return true;

                        var orderedSteps = await context.WorkflowSteps.Where(s => s.WorkflowId == instance.WorkflowId).OrderBy(s => s.Order).ToListAsync(cancellationToken);
                        var currentIndex = orderedSteps.FindIndex(s => s.StepId == instance.CurrentStepId);
                        var targetIndex = orderedSteps.FindIndex(s => s.StepId == targetStepId);

                        return targetIndex == currentIndex + 1;
                    }

                    public async Task RecordStateChangeAsync(Guid instanceId, string fromState, string toState, string action, Guid? performedBy = null, string? details = null, CancellationToken cancellationToken = default)
                    {
                        var history = new WorkflowHistory
                        {
                            HistoryId = Guid.NewGuid(),
                            InstanceId = instanceId,
                            FromState = fromState,
                            ToState = toState,
                            Action = action,
                            PerformedBy = performedBy,
                            Details = details
                        };

                        await context.WorkflowHistories.AddAsync(history, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<WorkflowHistory>> GetHistoryAsync(Guid instanceId, CancellationToken cancellationToken = default)
                        => await context.WorkflowHistories.Where(h => h.InstanceId == instanceId).OrderBy(h => h.OccurredAt).ToListAsync(cancellationToken);
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
                using Workflow.Core.Interfaces;
                using Workflow.Infrastructure.Data;
                using Workflow.Infrastructure.Repositories;
                using Workflow.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddWorkflowInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<WorkflowDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("WorkflowDb") ??
                                @"Server=.\SQLEXPRESS;Database=WorkflowDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
                        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
                        services.AddScoped<IStateManager, StateManager>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Workflow.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("WorkflowsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Workflow.Core.DTOs;
                using Workflow.Core.Entities;
                using Workflow.Core.Interfaces;

                namespace Workflow.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class WorkflowsController : ControllerBase
                {
                    private readonly IWorkflowRepository repository;
                    private readonly IWorkflowEngine engine;

                    public WorkflowsController(IWorkflowRepository repository, IWorkflowEngine engine)
                    {
                        this.repository = repository;
                        this.engine = engine;
                    }

                    [HttpPost]
                    public async Task<ActionResult<WorkflowDto>> Create([FromBody] CreateWorkflowRequest request, CancellationToken cancellationToken)
                    {
                        var workflow = new Core.Entities.Workflow
                        {
                            Name = request.Name,
                            Description = request.Description,
                            Version = request.Version,
                            Steps = request.Steps.Select(s => new WorkflowStep
                            {
                                Name = s.Name,
                                Description = s.Description,
                                Type = Enum.TryParse<StepType>(s.Type, out var type) ? type : StepType.Task,
                                Order = s.Order,
                                RequiresApproval = s.RequiresApproval,
                                Configuration = s.Configuration
                            }).ToList()
                        };

                        var created = await repository.AddAsync(workflow, cancellationToken);

                        var dto = new WorkflowDto
                        {
                            WorkflowId = created.WorkflowId,
                            Name = created.Name,
                            Description = created.Description,
                            Version = created.Version,
                            Status = created.Status.ToString(),
                            IsActive = created.IsActive,
                            CreatedAt = created.CreatedAt,
                            Steps = created.Steps.Select(s => new WorkflowStepDto
                            {
                                StepId = s.StepId,
                                Name = s.Name,
                                Description = s.Description,
                                Type = s.Type.ToString(),
                                Order = s.Order,
                                RequiresApproval = s.RequiresApproval
                            })
                        };

                        return CreatedAtAction(nameof(GetById), new { id = dto.WorkflowId }, dto);
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<WorkflowDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var workflow = await repository.GetByIdAsync(id, cancellationToken);
                        if (workflow == null) return NotFound();

                        return Ok(new WorkflowDto
                        {
                            WorkflowId = workflow.WorkflowId,
                            Name = workflow.Name,
                            Description = workflow.Description,
                            Version = workflow.Version,
                            Status = workflow.Status.ToString(),
                            IsActive = workflow.IsActive,
                            CreatedAt = workflow.CreatedAt,
                            Steps = workflow.Steps.Select(s => new WorkflowStepDto
                            {
                                StepId = s.StepId,
                                Name = s.Name,
                                Description = s.Description,
                                Type = s.Type.ToString(),
                                Order = s.Order,
                                RequiresApproval = s.RequiresApproval
                            })
                        });
                    }

                    [HttpPost("{id:guid}/transition")]
                    public async Task<ActionResult<WorkflowInstanceDto>> Transition(Guid id, [FromBody] TransitionRequest request, CancellationToken cancellationToken)
                    {
                        try
                        {
                            var instance = await engine.TransitionAsync(id, request.TransitionData, cancellationToken);

                            return Ok(new WorkflowInstanceDto
                            {
                                InstanceId = instance.InstanceId,
                                WorkflowId = instance.WorkflowId,
                                WorkflowName = instance.Workflow?.Name ?? "Unknown",
                                CurrentStepId = instance.CurrentStepId,
                                CurrentStepName = instance.CurrentStep?.Name,
                                Status = instance.Status.ToString(),
                                StartedAt = instance.StartedAt,
                                CompletedAt = instance.CompletedAt
                            });
                        }
                        catch (InvalidOperationException ex)
                        {
                            return BadRequest(new { error = ex.Message });
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ApprovalsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Workflow.Core.DTOs;
                using Workflow.Core.Entities;
                using Workflow.Core.Interfaces;

                namespace Workflow.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class ApprovalsController : ControllerBase
                {
                    private readonly IWorkflowRepository repository;
                    private readonly IWorkflowEngine engine;

                    public ApprovalsController(IWorkflowRepository repository, IWorkflowEngine engine)
                    {
                        this.repository = repository;
                        this.engine = engine;
                    }

                    [HttpPost("{id:guid}/approve")]
                    public async Task<ActionResult> Approve(Guid id, [FromBody] ApprovalResponse response, CancellationToken cancellationToken)
                    {
                        var approval = await repository.GetApprovalByIdAsync(id, cancellationToken);
                        if (approval == null) return NotFound();

                        if (approval.Status != ApprovalStatus.Pending)
                            return BadRequest(new { error = "Approval request is no longer pending" });

                        approval.Status = response.Approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
                        approval.Comments = response.Comments;
                        approval.RespondedBy = response.RespondedBy;
                        approval.RespondedAt = DateTime.UtcNow;

                        await repository.UpdateApprovalAsync(approval, cancellationToken);

                        if (response.Approved)
                        {
                            await engine.TransitionAsync(approval.InstanceId, null, cancellationToken);
                        }

                        return Ok(new { status = approval.Status.ToString(), message = response.Approved ? "Approval granted" : "Approval rejected" });
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

                builder.Services.AddWorkflowInfrastructure(builder.Configuration);
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
                    "WorkflowDb": "Server=.\\SQLEXPRESS;Database=WorkflowDb;Trusted_Connection=True;TrustServerCertificate=True"
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
